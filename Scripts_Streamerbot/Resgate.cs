using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

public class CPHInline
{
    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";

    public void InitDB(SQLiteConnection con)
    {
        using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS utilizadores (user_id TEXT PRIMARY KEY, username TEXT)", con)) cmd.ExecuteNonQuery();
        using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS capturas (user_id TEXT, elemental_id TEXT, quantidade INT)", con)) cmd.ExecuteNonQuery();
        using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS lancamentos (id INTEGER PRIMARY KEY AUTOINCREMENT, user_id TEXT, username TEXT, elemental_id TEXT, tipo_bola TEXT, sucesso INT, agua_ativa INT, created_at TEXT DEFAULT CURRENT_TIMESTAMP)", con)) cmd.ExecuteNonQuery();

        // DEPURADOR AUTOMÁTICO: Verifica se o índice de exclusividade já existe
        bool indexExiste = false;
        using (var cmd = new SQLiteCommand("PRAGMA index_list('capturas')", con))
        {
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "idx_user_elemental")
                    {
                        indexExiste = true;
                        break;
                    }
                }
            }
        }

        // Se o índice não existir, executa a migração de limpeza e consolidação histórica
        if (!indexExiste)
        {
            using (var cmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS capturas_temp (user_id TEXT, elemental_id TEXT, quantidade INT);
                
                /* CORREÇÃO HISTÓRICA: Limpa o bug matemático do passado e fixa o teto máximo em 2 cópias reais */
                INSERT INTO capturas_temp 
                SELECT user_id, elemental_id, MIN(2, SUM(quantidade)) 
                FROM capturas 
                GROUP BY user_id, elemental_id;
                
                DROP TABLE capturas;
                ALTER TABLE capturas_temp RENAME TO capturas;
                CREATE UNIQUE INDEX IF NOT EXISTS idx_user_elemental ON capturas (user_id, elemental_id);
            ", con))
            {
                try { cmd.ExecuteNonQuery(); CPH.LogInfo("[Elementais] Base de dados otimizada e duplicados corrigidos para o máximo de 2!"); } catch {}
            }
        }
    }

    public bool ExecutarResgate()
    {
        string userId = args.ContainsKey("userId") ? args["userId"].ToString() : "12345";
        string userName = args.ContainsKey("userName") ? args["userName"].ToString() : "Z3RGtv_Teste";
        string rewardId = args.ContainsKey("rewardId") ? args["rewardId"].ToString() : "";
        string redemptionId = args.ContainsKey("redemptionId") ? args["redemptionId"].ToString() : "";

        // 1. VERIFICAÇÃO DA FILA: O bicho ainda está ativo no ecrã?
        bool cacaAtiva = CPH.GetGlobalVar<bool>("cacaAtiva");
        if (!cacaAtiva)
        {
            if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId)) CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{userName}, esse elemental já fugiu ou foi capturado! Pontos devolvidos.");
            return true; 
        }

        string elementalAtivoId = CPH.GetGlobalVar<string>("elementalAtivoId");
        string elementalAtivoNome = CPH.GetGlobalVar<string>("elementalAtivoNome");

        // RESTRIÇÃO DO ELEMENTAL DE KING: apenas o conjurador pode arremessar Pokébolas neste spawn
        bool cacaSpritKingAtiva = CPH.GetGlobalVar<bool>("cacaSpritKingAtiva");
        if (cacaSpritKingAtiva)
        {
            string cacaSpritKingUserId = CPH.GetGlobalVar<string>("cacaSpritKingUserId") ?? "";
            string[] allowedIds = cacaSpritKingUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool allowed = false;
            foreach (string id in allowedIds)
            {
                if (id.Trim().Equals(userId, StringComparison.OrdinalIgnoreCase))
                {
                    allowed = true;
                    break;
                }
            }

            if (!allowed)
            {
                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                string cacaSpritKingUser = CPH.GetGlobalVar<string>("cacaSpritKingUser") ?? "";
                CPH.SendMessage(string.Format("@{0}, King ativo! Só quem usou o Rei ({1}) pode arremessar Pokébolas neste spawn. Pontos devolvidos.", userName, cacaSpritKingUser.Replace(",", ", @")));
                return true;
            }

            // SE O SLEEPY ESTIVER ATIVO TAMBÉM: o King adormece no seu arremesso!
            int cacaSpritSleepyCount = CPH.GetGlobalVar<int>("cacaSpritSleepyCount");
            if (cacaSpritSleepyCount > 0)
            {
                string cacaSpritSleepyUsers = CPH.GetGlobalVar<string>("cacaSpritSleepyUsers") ?? "";
                List<string> casters = new List<string>(cacaSpritSleepyUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                
                bool isCaster = false;
                foreach (var caster in casters)
                {
                    if (userName.Equals(caster, StringComparison.OrdinalIgnoreCase))
                    {
                        isCaster = true;
                        break;
                    }
                }

                if (!isCaster)
                {
                    if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                        CPH.TwitchRedemptionFulfill(rewardId, redemptionId);

                    CPH.SendMessage(string.Format("💤 @{0} adormeceu! Arremesso cancelado (pontos gastos).", userName));
                    
                    cacaSpritSleepyCount--;
                    CPH.SetGlobalVar("cacaSpritSleepyCount", cacaSpritSleepyCount);
                    if (cacaSpritSleepyCount <= 0)
                    {
                        CPH.SetGlobalVar("cacaSpritSleepyAtiva", false);
                        CPH.SetGlobalVar("cacaSpritSleepyUsers", "");
                    }
                    return true;
                }
            }
        }

        // 2. TRANCA RIGOROSA DE INVENTÁRIO: Verifica se o jogador já atingiu o teto máximo (2 ou mais)
        int quantidadePossuida = 0;
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            InitDB(con); // Garante a saúde e limpeza da tabela a cada execução

            using (var cmd = new SQLiteCommand("SELECT SUM(quantidade) FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@eid", elementalAtivoId);
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) quantidadePossuida = Convert.ToInt32(res);
            }
        }

        if (quantidadePossuida >= 2)
        {
            if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId)) CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            bool cacaSpritGhostAtiva = CPH.GetGlobalVar<bool>("cacaSpritGhostAtiva");
            if (cacaSpritGhostAtiva)
            {
                CPH.SendMessage($"@{userName}, já tens o limite de 2 cópias deste elemental mistério! Pontos devolvidos.");
            }
            else
            {
                CPH.SendMessage($"@{userName}, já tens o limite de 2 cópias de {elementalAtivoNome}! Pontos devolvidos.");
            }
            return true;
        }

        bool forcarResgateLobby = args.ContainsKey("forcarResgateLobby") && args["forcarResgateLobby"].ToString() == "true";

        // 3. LIMITE DE LANÇAMENTOS POR PESSOA: Máximo 2 jogadas por Spawn (Bypass se for arremesso ordenado do lobby)
        int cacaID = CPH.GetGlobalVar<int>("cacaID");
        int tentativasUser = CPH.GetGlobalVar<int>($"tentativas_{cacaID}_{userId}");

        int limiteTentativas = 2;
        bool cacaSpritKingAtivaTemp = CPH.GetGlobalVar<bool>("cacaSpritKingAtiva");
        if (cacaSpritKingAtivaTemp)
        {
            string cacaSpritKingUserIdTemp = CPH.GetGlobalVar<string>("cacaSpritKingUserId") ?? "";
            string[] allowedIds = cacaSpritKingUserIdTemp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in allowedIds)
            {
                if (id.Trim().Equals(userId, StringComparison.OrdinalIgnoreCase))
                {
                    limiteTentativas = 5;
                    break;
                }
            }
        }

        if (!forcarResgateLobby && tentativasUser >= limiteTentativas)
        {
            if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId)) CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{userName}, já esgotaste o limite de {limiteTentativas} tentativas! Pontos devolvidos.");
            return true; 
        }

        // Identificar tipo de bola utilizada no resgate através de argumento ou rawInput
        string bola = "normal";
        if (args.ContainsKey("tipoBola")) bola = args["tipoBola"].ToString().ToLower();
        else if (args.ContainsKey("rawInput"))
        {
            string rawInput = args["rawInput"].ToString().ToLower();
            if (rawInput.Contains("super")) bola = "super";
            else if (rawInput.Contains("ultra")) bola = "ultra";
            else if (rawInput.Contains("master")) bola = "master";
        }

        // RESTRIÇÃO DO ELEMENTAL DE DEMON: apenas o conjurador pode usar Master Ball (e Ultra Ball se for SUPER) (Bypass se o King estiver ativo)
        bool cacaSpritDemonAtiva = CPH.GetGlobalVar<bool>("cacaSpritDemonAtiva");
        bool cacaSpritDemonSuper = CPH.GetGlobalVar<bool>("cacaSpritDemonSuper");
        if (((bola == "master" || (bola == "ultra" && cacaSpritDemonSuper)) && !cacaSpritKingAtiva) && cacaSpritDemonAtiva)
        {
            string cacaSpritDemonUser = CPH.GetGlobalVar<string>("cacaSpritDemonUser") ?? "";
            string[] allowedUsers = cacaSpritDemonUser.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool allowed = false;
            foreach (string u in allowedUsers)
            {
                if (u.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    allowed = true;
                    break;
                }
            }

            if (!allowed)
            {
                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                string ballType = (bola == "master") ? "Master Ball" : "Ultra Ball";
                if (cacaSpritDemonSuper)
                {
                    CPH.SendMessage(string.Format("@{0}, Demon [SUPER] ativo! Só quem usou o Demónio ({1}) pode usar a {2}. Pontos devolvidos.", userName, cacaSpritDemonUser.Replace(",", ", @"), ballType));
                }
                else
                {
                    CPH.SendMessage(string.Format("@{0}, Demon ativo! Só quem usou o Demónio ({1}) pode usar a Master Ball. Pontos devolvidos.", userName, cacaSpritDemonUser.Replace(",", ", @")));
                }
                return true;
            }
        }

        if (!forcarResgateLobby)
        {
            bool lobbyResolvido = CPH.GetGlobalVar<bool>("lobbyResolvido");
            if (lobbyResolvido)
            {
                int filaIndex = CPH.GetGlobalVar<int>("lobbyFilaIndex");
                int filaTotal = CPH.GetGlobalVar<int>("lobbyFilaTotal");
                if (filaIndex <= filaTotal)
                {
                    if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                        CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                    CPH.SendMessage(string.Format("@{0}, aguarda pelo fim dos arremessos do sorteio! Pontos devolvidos.", userName));
                    return true;
                }
            }

            bool lobbyAtivo = CPH.GetGlobalVar<bool>("lobbyAtivo");
            if (lobbyAtivo)
            {
                bool jaNoLobby = CPH.GetGlobalVar<bool>($"lobby_inscrito_{cacaID}_{userId}");
                if (jaNoLobby)
                {
                    if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                        CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                    CPH.SendMessage($"@{userName}, já estás inscrito no sorteio! Pontos devolvidos.");
                    return true;
                }

                int lobbyCount = CPH.GetGlobalVar<int>("lobbyCount");
                if (lobbyCount >= 5)
                {
                    if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                        CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                    CPH.SendMessage(string.Format("@{0}, lobby cheio (máx 5 tentativas)! Pontos devolvidos.", userName));
                    return true;
                }

                // Reservar a tentativa do utilizador imediatamente
                tentativasUser++;
                CPH.SetGlobalVar(string.Format("tentativas_{0}_{1}", cacaID, userId), tentativasUser);
                CPH.SetGlobalVar($"lobby_inscrito_{cacaID}_{userId}", true);

                // Registar dados no lobby
                lobbyCount++;
                CPH.SetGlobalVar("lobbyCount", lobbyCount);
                CPH.SetGlobalVar(string.Format("lobby_user_id_{0}", lobbyCount), userId);
                CPH.SetGlobalVar(string.Format("lobby_user_name_{0}", lobbyCount), userName);
                CPH.SetGlobalVar(string.Format("lobby_bola_{0}", lobbyCount), bola);
                CPH.SetGlobalVar(string.Format("lobby_reward_id_{0}", lobbyCount), rewardId);
                CPH.SetGlobalVar(string.Format("lobby_redemption_id_{0}", lobbyCount), redemptionId);

                // REGISTAR PARTICIPAÇÃO PARA O ROUBO DO PUNK
                bool cacaSpritPunkAtiva = CPH.GetGlobalVar<bool>("cacaSpritPunkAtiva");
                if (cacaSpritPunkAtiva)
                {
                    string cacaSpritPunkUserId = CPH.GetGlobalVar<string>("cacaSpritPunkUserId") ?? "";
                    string[] conjuradores = cacaSpritPunkUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    bool isConjurador = false;
                    foreach (string cId in conjuradores)
                    {
                        if (userId.Equals(cId.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            isConjurador = true;
                            break;
                        }
                    }

                    if (!isConjurador)
                    {
                        string candidatos = CPH.GetGlobalVar<string>("cacaSpritPunkCandidatos") ?? "";
                        if (!candidatos.Contains(userId))
                        {
                            if (!string.IsNullOrEmpty(candidatos)) candidatos += ",";
                            candidatos += $"{userId}:{userName}";
                            CPH.SetGlobalVar("cacaSpritPunkCandidatos", candidatos);
                        }
                    }
                }

                if (lobbyCount >= 5)
                {
                    CPH.RunAction("Elementais - Resolver Lobby", true);
                }

                return true;
            }
        }

        // Incrementar contadores de segurança do utilizador (se não for do lobby) e globais
        if (!forcarResgateLobby)
        {
            tentativasUser++;
            CPH.SetGlobalVar(string.Format("tentativas_{0}_{1}", cacaID, userId), tentativasUser);

            // REGISTAR PARTICIPAÇÃO PARA O ROUBO DO PUNK
            bool cacaSpritPunkAtiva = CPH.GetGlobalVar<bool>("cacaSpritPunkAtiva");
            if (cacaSpritPunkAtiva)
            {
                string cacaSpritPunkUserId = CPH.GetGlobalVar<string>("cacaSpritPunkUserId") ?? "";
                string[] conjuradores = cacaSpritPunkUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                bool isConjurador = false;
                foreach (string cId in conjuradores)
                {
                    if (userId.Equals(cId.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        isConjurador = true;
                        break;
                    }
                }

                if (!isConjurador)
                {
                    string candidatos = CPH.GetGlobalVar<string>("cacaSpritPunkCandidatos") ?? "";
                    if (!candidatos.Contains(userId))
                    {
                        if (!string.IsNullOrEmpty(candidatos)) candidatos += ",";
                        candidatos += $"{userId}:{userName}";
                        CPH.SetGlobalVar("cacaSpritPunkCandidatos", candidatos);
                    }
                }
            }
        }

        int tentativasGlobais = CPH.GetGlobalVar<int>("tentativasGlobais");
        bool cacaSpritAtacanteAtiva = CPH.GetGlobalVar<bool>("cacaSpritAtacanteAtiva");
        string cacaSpritAtacanteUserId = CPH.GetGlobalVar<string>("cacaSpritAtacanteUserId") ?? "";
        string cacaSpritAtacanteUser = CPH.GetGlobalVar<string>("cacaSpritAtacanteUser") ?? "";
        
        bool usarRebound = false;
        if (cacaSpritAtacanteAtiva)
        {
            string[] allowedIds = cacaSpritAtacanteUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in allowedIds)
            {
                if (id.Trim().Equals(userId, StringComparison.OrdinalIgnoreCase))
                {
                    usarRebound = true;
                    break;
                }
            }
        }

        int maxAttempts = usarRebound ? 2 : 1;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (attempt == 1)
            {
                tentativasGlobais = CPH.GetGlobalVar<int>("tentativasGlobais") + 1;
                CPH.SetGlobalVar("tentativasGlobais", tentativasGlobais);
            }
            
            // Blindagem de fila: Regista o exato milissegundo deste arremesso
            CPH.SetGlobalVar("ultimoArremessoTempo", DateTime.Now.Ticks);

            // Fulfill a redenção na Twitch apenas no primeiro arremesso (se veio de resgate real)
            if (attempt == 1 && !string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
            {
                CPH.TwitchRedemptionFulfill(rewardId, redemptionId);
            }

            if (attempt == 2)
            {
                string cacaSpritAtacanteSuperStr = CPH.GetGlobalVar<string>("cacaSpritAtacanteSuper") ?? "";
                string[] superIds = cacaSpritAtacanteSuperStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                bool hasSuper = false;
                foreach (string sId in superIds)
                {
                    if (sId.Trim().Equals(userId, StringComparison.OrdinalIgnoreCase))
                    {
                        hasSuper = true;
                        break;
                    }
                }

                if (hasSuper)
                {
                    CPH.SendMessage(string.Format("⚽ [REBOUND SUPER] @{0} falhou o 1º remate! O Atacante [SUPER] ganha o ressalto e chuta com +20% de taxa! 🥅🔥", userName));
                }
                else
                {
                    CPH.SendMessage(string.Format("⚽ [REBOUND] @{0} falhou o 1º remate! O Atacante ganha o ressalto e chuta a segunda bola! 🥅", userName));
                }
                
                // Remover o utilizador que usou o seu rebound da lista
                string[] allowedIds = cacaSpritAtacanteUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] allowedNames = cacaSpritAtacanteUser.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> newIds = new List<string>();
                List<string> newNames = new List<string>();
                
                for (int idx = 0; idx < allowedIds.Length; idx++)
                {
                    if (!allowedIds[idx].Trim().Equals(userId, StringComparison.OrdinalIgnoreCase))
                    {
                        newIds.Add(allowedIds[idx].Trim());
                        if (idx < allowedNames.Length) newNames.Add(allowedNames[idx].Trim());
                    }
                }
                
                if (newIds.Count > 0)
                {
                    CPH.SetGlobalVar("cacaSpritAtacanteUserId", string.Join(",", newIds));
                    CPH.SetGlobalVar("cacaSpritAtacanteUser", string.Join(",", newNames));
                }
                else
                {
                    CPH.SetGlobalVar("cacaSpritAtacanteAtiva", false);
                    CPH.SetGlobalVar("cacaSpritAtacanteUser", "");
                    CPH.SetGlobalVar("cacaSpritAtacanteUserId", "");
                }
            }

            // Envia comando de animação para o OBS e aguarda os 5 segundos regulamentares da animação de abano
            if (attempt == 2)
            {
                EscreverEstado(string.Format("REBOUND;{0};{1}", userName, bola));
            }
            else
            {
                EscreverEstado(string.Format("ATIRAR;{0};{1}", userName, bola));
            }
            Thread.Sleep(5000);

        // Definição das probabilidades calibradas por tipo de bola
        int probabilidade = 25;
        if (bola == "super") probabilidade = 45;
        else if (bola == "ultra") probabilidade = 65;
        else if (bola == "master") probabilidade = 96; // Ajustado de 94 para 96 por balanceamento de RNG

        // BONUS DO ATACANTE SUPER NO REBOUND (apenas no 2º arremesso)
        if (attempt == 2)
        {
            string cacaSpritAtacanteSuperStr = CPH.GetGlobalVar<string>("cacaSpritAtacanteSuper") ?? "";
            string[] superIds = cacaSpritAtacanteSuperStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string sId in superIds)
            {
                if (sId.Trim().Equals(userId, StringComparison.OrdinalIgnoreCase))
                {
                    probabilidade += 20;
                    if (probabilidade > 100) probabilidade = 100;
                    break;
                }
            }
        }

        // EFEITO DO ELEMENTAL DE ÁGUA: reduz a probabilidade de captura em 40% (x0.6) ou 60% (x0.4) para todos exceto quem o usou
        bool cacaSpritAguaAtiva = CPH.GetGlobalVar<bool>("cacaSpritAguaAtiva");
        if (cacaSpritAguaAtiva)
        {
            string cacaSpritAguaUser = CPH.GetGlobalVar<string>("cacaSpritAguaUser") ?? "";
            string[] allowedUsers = cacaSpritAguaUser.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool isAllowed = false;
            foreach (string u in allowedUsers)
            {
                if (u.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed)
            {
                bool cacaSpritAguaSuper = CPH.GetGlobalVar<bool>("cacaSpritAguaSuper");
                double mult = cacaSpritAguaSuper ? 0.4 : 0.6;
                probabilidade = (int)(probabilidade * mult);
                if (probabilidade < 1) probabilidade = 1; // probabilidade mínima de 1%
            }
        }

        // EFEITO DO ELEMENTAL DE BOSS: reduz a probabilidade de captura de todas as bolas em 60% (x0.4)
        bool cacaSpritBossAtiva = CPH.GetGlobalVar<bool>("cacaSpritBossAtiva");
        if (cacaSpritBossAtiva)
        {
            probabilidade = (int)(probabilidade * 0.4);
            if (probabilidade < 1) probabilidade = 1; // probabilidade mínima de 1%
        }

        Random rnd = new Random();
        bool sucesso = rnd.Next(1, 101) <= probabilidade;

        if (sucesso)
        {
            // BLOQUEIO DE SEGURANÇA MULTI-THREAD: Evita capturas duplas em arremessos simultâneos
            lock (typeof(CPHInline))
            {
                bool cacaAtivaCheck = CPH.GetGlobalVar<bool>("cacaAtiva");
                if (!cacaAtivaCheck)
                {
                    if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                        CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                    return true;
                }
                CPH.SetGlobalVar("cacaAtiva", false);
            }

            // =========================================================================
            // CASO DE SUCESSO: CAPTURA EFETUADA
            // =========================================================================
            CPH.SetGlobalVar("cacaSpritAguaAtiva", false); 
            CPH.SetGlobalVar("cacaSpritAguaUser", "");
            CPH.SetGlobalVar("cacaSpritAguaSuper", false);
            CPH.SetGlobalVar("cacaSpritDemonAtiva", false);
            CPH.SetGlobalVar("cacaSpritDemonUser", "");
            CPH.SetGlobalVar("cacaSpritDemonSuper", false);
            CPH.SetGlobalVar("cacaSpritKingAtiva", false);
            CPH.SetGlobalVar("cacaSpritKingUser", "");
            CPH.SetGlobalVar("cacaSpritKingUserId", "");
            CPH.SetGlobalVar("cacaSpritBossAtiva", false);
            CPH.SetGlobalVar("cacaSpritBossUser", "");
            CPH.SetGlobalVar("cacaSpritBossUserId", "");
            CPH.SetGlobalVar("cacaSpritBossSuper", false);
            CPH.SetGlobalVar("cacaSpritVentoAtiva", false);
            CPH.SetGlobalVar("cacaSpritVentoUser", "");
            CPH.SetGlobalVar("cacaSpritVentoUserId", "");
            CPH.SetGlobalVar("cacaSpritVentoSuper", "");

            // Processar Pesca Extra do Peixoto
            bool cacaSpritPeixeAtiva = CPH.GetGlobalVar<bool>("cacaSpritPeixeAtiva");
            if (cacaSpritPeixeAtiva)
            {
                string cacaSpritPeixeUserId = CPH.GetGlobalVar<string>("cacaSpritPeixeUserId") ?? "";
                string[] allowedIds = cacaSpritPeixeUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                bool allowed = false;
                foreach (string id in allowedIds)
                {
                    if (id.Trim().Equals(userId, StringComparison.OrdinalIgnoreCase))
                    {
                        allowed = true;
                        break;
                    }
                }

                if (allowed)
                {
                    ProcessarPescaExtra(userId, userName);
                }
            }
            CPH.SetGlobalVar("cacaSpritPeixeAtiva", false);
            CPH.SetGlobalVar("cacaSpritPeixeUser", "");
            CPH.SetGlobalVar("cacaSpritPeixeUserId", "");
            CPH.SetGlobalVar("cacaSpritPeixeSuper", false);

            ProcessarRouboPunk();
            EscreverEstado($"SUCESSO;{userName}");
            
            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO utilizadores (user_id, username) VALUES (@userId, @userName)", con))
                { cmd.Parameters.AddWithValue("@userId", userId); cmd.Parameters.AddWithValue("@userName", userName); cmd.ExecuteNonQuery(); }
                using (var cmd = new SQLiteCommand("UPDATE utilizadores SET username=@userName WHERE user_id=@userId", con))
                { cmd.Parameters.AddWithValue("@userId", userId); cmd.Parameters.AddWithValue("@userName", userName); cmd.ExecuteNonQuery(); }
                
                // Graças ao novo INDEX ÚNICO criado no InitDB, o INSERT OR IGNORE agora funciona a 100% por célula
                using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@userId, @elemId, 0)", con))
                { cmd.Parameters.AddWithValue("@userId", userId); cmd.Parameters.AddWithValue("@elemId", elementalAtivoId); cmd.ExecuteNonQuery(); }
                using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@userId AND elemental_id=@elemId", con))
                { cmd.Parameters.AddWithValue("@userId", userId); cmd.Parameters.AddWithValue("@elemId", elementalAtivoId); cmd.ExecuteNonQuery(); }

                // Registar estatísticas de lançamento
                using (var cmd = new SQLiteCommand("INSERT INTO lancamentos (user_id, username, elemental_id, tipo_bola, sucesso, agua_ativa) VALUES (@userId, @userName, @elemId, @tipoBola, 1, @agua)", con))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@userName", userName);
                    cmd.Parameters.AddWithValue("@elemId", elementalAtivoId);
                    cmd.Parameters.AddWithValue("@tipoBola", bola);
                    cmd.Parameters.AddWithValue("@agua", cacaSpritAguaAtiva ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }

                // Processar Win Streak em caso de sucesso
                ProcessarWinStreak(con, userId, userName, true);

                // Processar Milestones da variante Quack (20, 40, 75, 100 capturas acumuladas)
                ProcessarMilestonesQuack(con, userId, userName);
            }

            CPH.SendMessage($"@{userName} capturou {elementalAtivoNome} com bola {bola}! 🎉");
            if (userName.ToLower() == "manu12321_")
            {
                if (rnd.Next(0, 100) < 60)
                {
                    int jokeIdx = rnd.Next(0, 2);
                    if (jokeIdx == 0)
                        CPH.SendMessage("Milagre! O @manu12321_ conseguiu capturar um bicho! As probabilidades mentiram! 😱");
                    else
                        CPH.SendMessage("Olha só, @manu12321_! Afinal a tua taxa de captura não é zero! 🎉");
                }
            }
            
            // Sincroniza instantaneamente o novo estado do inventário e pontos com o repositório do teu site GitHub
            CPH.RunAction("Elementais - Exportar Site", true);

            // VERIFICAÇÃO DE VITÓRIA: Se o jogador completou o álbum de 100% de todos os Elementais
            using (var conVitoria = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                conVitoria.Open();
                VerificarVitoriaAlbumCompleto(conVitoria);
            }
            
            CPH.RunAction("Elementais - Desativar Caça");

        }
        else
        {
            // =========================================================================
            // CASO DE FALHA: ARREMESSO FALHOU
            // =========================================================================
            CPH.SendMessage($"A bola {bola} de @{userName} falhou! 💨 ({tentativasGlobais}/5)");
            if (userName.ToLower() == "manu12321_")
            {
                if (rnd.Next(0, 100) < 60)
                {
                    int jokeIdx = rnd.Next(0, 3);
                    if (jokeIdx == 0)
                        CPH.SendMessage("Calma @manu12321_, as probabilidades estão corretas, tu é que tens azar! 📉");
                    else if (jokeIdx == 1)
                        CPH.SendMessage("Outra bola falhada, @manu12321_? Deve ser culpa das 'probabilidades'... 🤫");
                    else
                        CPH.SendMessage("O bot garantiu com sucesso que a taxa de captura para o @manu12321_ é de 0%! 🤖");
                }
            }

            // Registar estatísticas de lançamento falhado
            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand("INSERT INTO lancamentos (user_id, username, elemental_id, tipo_bola, sucesso, agua_ativa) VALUES (@userId, @userName, @elemId, @tipoBola, 0, @agua)", con))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@userName", userName);
                    cmd.Parameters.AddWithValue("@elemId", elementalAtivoId);
                    cmd.Parameters.AddWithValue("@tipoBola", bola);
                    cmd.Parameters.AddWithValue("@agua", cacaSpritAguaAtiva ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }

                // Processar Win Streak em caso de falha (reseta streak para 0)
                ProcessarWinStreak(con, userId, userName, false);

                // Processar compensação por falhas (Piedade)
                ProcessarPiedadeFalhas(con, userId, userName, bola);
            }

            // VERIFICAÇÃO DE FUGA: Se foi a 5ª tentativa global do chat, o elemental foge permanentemente
            bool cacaSpritGhostAtiva = CPH.GetGlobalVar<bool>("cacaSpritGhostAtiva");
            if (tentativasGlobais >= 5)
            {
                CPH.SetGlobalVar("cacaAtiva", false); 
                CPH.SetGlobalVar("cacaSpritAguaAtiva", false); 
                CPH.SetGlobalVar("cacaSpritAguaUser", "");
                CPH.SetGlobalVar("cacaSpritAguaSuper", false);
                CPH.SetGlobalVar("cacaSpritDemonAtiva", false);
                CPH.SetGlobalVar("cacaSpritDemonUser", "");
                CPH.SetGlobalVar("cacaSpritDemonSuper", false);
                CPH.SetGlobalVar("cacaSpritKingAtiva", false);
                CPH.SetGlobalVar("cacaSpritKingUser", "");
                CPH.SetGlobalVar("cacaSpritKingUserId", ""); 
                CPH.SetGlobalVar("cacaSpritBossAtiva", false);
                CPH.SetGlobalVar("cacaSpritBossUser", "");
                CPH.SetGlobalVar("cacaSpritBossUserId", "");
                CPH.SetGlobalVar("cacaSpritBossSuper", false);
                CPH.SetGlobalVar("cacaSpritVentoAtiva", false);
                CPH.SetGlobalVar("cacaSpritVentoUser", "");
                CPH.SetGlobalVar("cacaSpritVentoUserId", "");
                CPH.SetGlobalVar("cacaSpritVentoSuper", "");
                if (cacaSpritGhostAtiva)
                {
                    EscreverEstado("FUGIU;elemental mistério");
                    CPH.SendMessage("O elemental mistério fugiu! 🏃💨");
                }
                else
                {
                    EscreverEstado($"FUGIU;{elementalAtivoNome}");
                    CPH.SendMessage($"O {elementalAtivoNome} fugiu! 🏃💨");
                }
                ProcessarRouboPunk();
                using (var conVitoria = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    conVitoria.Open();
                    VerificarVitoriaAlbumCompleto(conVitoria);
                }
                CPH.RunAction("Elementais - Desativar Caça");
            }
            else
            {
                // Liberta o bicho e força o re-spawn imediato do mesmo exemplar no ecrã do OBS para a próxima tentativa
                EscreverEstado($"FALHA;{userName}");
                Thread.Sleep(3000); // Aguarda a animação visual da explosão da bola passar
                
                if (attempt < maxAttempts)
                {
                    continue;
                }

                string elementalAtivoFicheiro = CPH.GetGlobalVar<string>("elementalAtivoFicheiro");
                EscreverEstado(string.Format("SPAWN;{0};{1};False;False;False;False;{2};False", elementalAtivoFicheiro, elementalAtivoNome, cacaSpritGhostAtiva));
            }
        }
        }

        return true;
    }

    private void EscreverEstado(string cmd)
    {
        try
        {
            bool cacaAtiva = CPH.GetGlobalVar<bool>("cacaAtiva");
            bool aguaAtiva = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAguaAtiva") : CPH.GetGlobalVar<bool>("spritAguaAtivo");
            bool aguaSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAguaSuper") : CPH.GetGlobalVar<bool>("spritAguaSuper");
            string aguaVal = aguaAtiva ? (aguaSuper ? "Super" : "True") : "False";

            bool terraAtiva = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritTerraAtiva") : CPH.GetGlobalVar<bool>("spritTerraAtivo");
            bool terraSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritTerraSuper") : CPH.GetGlobalVar<bool>("spritTerraSuper");
            string terraVal = terraAtiva ? (terraSuper ? "Super" : "True") : "False";

            bool fogo = CPH.GetGlobalVar<bool>("spritFogoAtivo");

            bool patoAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPatoAtiva") : CPH.GetGlobalVar<bool>("spritPatoAtivo");
            bool patoSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPatoSuper") : CPH.GetGlobalVar<bool>("spritPatoSuper");
            string patoVal = patoAtivo ? (patoSuper ? "Super" : "True") : "False";

            bool ghost = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritGhostAtiva") : CPH.GetGlobalVar<bool>("spritGhostAtivo");

            bool sleepyAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritSleepyAtiva") : CPH.GetGlobalVar<bool>("spritSleepyAtivo");
            int sleepyCount = CPH.GetGlobalVar<int>(cacaAtiva ? "cacaSpritSleepyCount" : "spritSleepyCount");
            string sleepyVal = sleepyAtivo ? (sleepyCount > 1 ? "Super" : "True") : "False";

            bool demonAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritDemonAtiva") : CPH.GetGlobalVar<bool>("spritDemonAtivo");
            bool demonSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritDemonSuper") : CPH.GetGlobalVar<bool>("spritDemonSuper");
            string demonVal = demonAtivo ? (demonSuper ? "Super" : "True") : "False";

            bool punkAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPunkAtiva") : CPH.GetGlobalVar<bool>("spritPunkAtivo");
            bool punkSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPunkSuper") : CPH.GetGlobalVar<bool>("spritPunkSuper");
            string punkVal = punkAtivo ? (punkSuper ? "Super" : "True") : "False";

            bool king = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritKingAtiva") : CPH.GetGlobalVar<bool>("spritKingAtivo");
            bool aura = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAuraAtiva") : CPH.GetGlobalVar<bool>("spritAuraAtivo");
            bool bossAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritBossAtiva") : CPH.GetGlobalVar<bool>("spritBossAtivo");
            bool bossSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritBossSuper") : CPH.GetGlobalVar<bool>("spritBossSuper");
            string bossVal = bossAtivo ? (bossSuper ? "Super" : "True") : "False";

            bool peixeAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPeixeAtiva") : CPH.GetGlobalVar<bool>("spritPeixeAtivo");
            bool peixeSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPeixeSuper") : CPH.GetGlobalVar<bool>("spritPeixeSuper");
            string peixeVal = peixeAtivo ? (peixeSuper ? "Super" : "True") : "False";

            bool atacanteAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAtacanteAtiva") : CPH.GetGlobalVar<bool>("spritAtacanteAtivo");
            string cacaAtacanteSuperStr = CPH.GetGlobalVar<string>(cacaAtiva ? "cacaSpritAtacanteSuper" : "spritAtacanteSuper") ?? "";
            bool atacanteSuper = !string.IsNullOrEmpty(cacaAtacanteSuperStr);
            string atacanteVal = atacanteAtivo ? (atacanteSuper ? "Super" : "True") : "False";

            bool ventoAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritVentoAtiva") : CPH.GetGlobalVar<bool>("spritVentoAtivo");
            string cacaVentoSuperStr = CPH.GetGlobalVar<string>(cacaAtiva ? "cacaSpritVentoSuper" : "spritVentoSuper") ?? "";
            bool ventoSuper = !string.IsNullOrEmpty(cacaVentoSuperStr);
            string ventoVal = ventoAtivo ? (ventoSuper ? "Super" : "True") : "False";

            string suffix = string.Format("|agua={0};terra={1};fogo={2};pato={3};ghost={4};sleepy={5};demon={6};punk={7};king={8};aura={9};boss={10};peixe={11};atacante={12};vento={13}",
                aguaVal, terraVal, fogo, patoVal, ghost, sleepyVal, demonVal, punkVal, king, aura, bossVal, peixeVal, atacanteVal, ventoVal);
            File.WriteAllText(caminhoEstado, cmd + suffix);
        }
        catch {}
    }
    public bool Execute() { return ExecutarResgate(); }

    private void ProcessarRouboPunk()
    {
        bool cacaSpritPunkAtiva = CPH.GetGlobalVar<bool>("cacaSpritPunkAtiva");
        if (!cacaSpritPunkAtiva) return;

        string conjuradoresIdsRaw = CPH.GetGlobalVar<string>("cacaSpritPunkUserId") ?? "";
        string conjuradoresNamesRaw = CPH.GetGlobalVar<string>("cacaSpritPunkUser") ?? "Viewer";
        string candidatosRaw = CPH.GetGlobalVar<string>("cacaSpritPunkCandidatos") ?? "";

        // Resetar flags globais de imediato
        CPH.SetGlobalVar("cacaSpritPunkAtiva", false);
        CPH.SetGlobalVar("cacaSpritPunkUserId", "");
        CPH.SetGlobalVar("cacaSpritPunkUser", "");
        CPH.SetGlobalVar("cacaSpritPunkCandidatos", "");

        string[] conjuradoresIds = conjuradoresIdsRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        string[] conjuradoresNames = conjuradoresNamesRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (conjuradoresIds.Length == 0) return;

        if (string.IsNullOrEmpty(candidatosRaw))
        {
            string allNames = string.Join(" e @", conjuradoresNames);
            CPH.SendMessage($"🎸 [ROUBO] Ninguém participou além de @{allNames}, por isso o Punk não roubou nada!");
            return;
        }

        string[] candidatos = candidatosRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (candidatos.Length == 0)
        {
            string allNames = string.Join(" e @", conjuradoresNames);
            CPH.SendMessage($"🎸 [ROUBO] Ninguém participou além de @{allNames}, por isso o Punk não roubou nada!");
            return;
        }

        Random rnd = new Random();
        List<string> listaCandidatos = new List<string>(candidatos);

        for (int i = 0; i < conjuradoresIds.Length; i++)
        {
            if (listaCandidatos.Count == 0)
            {
                break;
            }

            string conjuradorId = conjuradoresIds[i].Trim();
            string conjuradorName = i < conjuradoresNames.Length ? conjuradoresNames[i].Trim() : "Viewer";

            // Verificar bónus de coleção de punk completa para este conjurador
            bool temTodosPunk = false;
            try
            {
                using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    string qCheck = "SELECT COUNT(DISTINCT elemental_id) FROM capturas WHERE user_id=@uid AND elemental_id IN ('8_1','8_2','8_3','8_4','8_6') AND quantidade > 0";
                    using (var cmdCheck = new SQLiteCommand(qCheck, con))
                    {
                        cmdCheck.Parameters.AddWithValue("@uid", conjuradorId);
                        object countObj = cmdCheck.ExecuteScalar();
                        if (countObj != null && countObj != DBNull.Value)
                        {
                            int count = Convert.ToInt32(countObj);
                            if (count == 5) temTodosPunk = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[Resgate] Erro ao verificar colecao de punk: " + ex.Message);
            }

            int stealAttempts = (temTodosPunk && listaCandidatos.Count >= 2) ? 2 : 1;
            for (int attempt = 0; attempt < stealAttempts; attempt++)
            {
                if (listaCandidatos.Count == 0) break;

                int victimIdx = rnd.Next(0, listaCandidatos.Count);
                string[] parts = listaCandidatos[victimIdx].Split(':');
                if (parts.Length < 2) continue;
                string vitimaId = parts[0];
                string vitimaName = parts[1];

                listaCandidatos.RemoveAt(victimIdx);

                string roubadoId = null;
                using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    
                    List<string> inventarioVitima = new List<string>();
                    using (var cmd = new SQLiteCommand("SELECT elemental_id FROM capturas WHERE user_id=@uid AND quantidade > 0", con))
                    {
                        cmd.Parameters.AddWithValue("@uid", vitimaId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                inventarioVitima.Add(reader[0].ToString());
                            }
                        }
                    }

                    if (inventarioVitima.Count == 0)
                    {
                        if (temTodosPunk && stealAttempts == 2)
                        {
                            CPH.SendMessage($"🎸 [ROUBO SUPER] @{conjuradorName} tentou roubar @{vitimaName}, mas a sua mala estava vazia!");
                        }
                        else
                        {
                            CPH.SendMessage($"🎸 [ROUBO] @{conjuradorName} tentou roubar @{vitimaName}, mas a sua mala estava vazia!");
                        }
                        continue;
                    }

                    int elemIdx = rnd.Next(0, inventarioVitima.Count);
                    roubadoId = inventarioVitima[elemIdx];

                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade - 1 WHERE user_id=@uid AND elemental_id=@eid", con, trans))
                            {
                                cmd.Parameters.AddWithValue("@uid", vitimaId);
                                cmd.Parameters.AddWithValue("@eid", roubadoId);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd = new SQLiteCommand("DELETE FROM capturas WHERE user_id=@uid AND elemental_id=@eid AND quantidade <= 0", con, trans))
                            {
                                cmd.Parameters.AddWithValue("@uid", vitimaId);
                                cmd.Parameters.AddWithValue("@eid", roubadoId);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@uid, @eid, 0)", con, trans))
                            {
                                cmd.Parameters.AddWithValue("@uid", conjuradorId);
                                cmd.Parameters.AddWithValue("@eid", roubadoId);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@uid AND elemental_id=@eid", con, trans))
                            {
                                cmd.Parameters.AddWithValue("@uid", conjuradorId);
                                cmd.Parameters.AddWithValue("@eid", roubadoId);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            CPH.LogWarn("Erro na transação de roubo do Punk: " + ex.Message);
                            continue;
                        }
                    }
                }

                string nomeBichoRoubado = roubadoId;
                try { nomeBichoRoubado = ObterNomeBichoPorId(roubadoId); } catch {}

                if (temTodosPunk && stealAttempts == 2)
                {
                    CPH.SendMessage(string.Format("🎸 [ROUBO SUPER] @{0} roubou 1x {1} do @{2}! 🎒", conjuradorName, nomeBichoRoubado, vitimaName));
                }
                else
                {
                    CPH.SendMessage(string.Format("🎸 [ROUBO] @{0} roubou 1x {1} do @{2}! 🎒", conjuradorName, nomeBichoRoubado, vitimaName));
                }
            }
        }

        CPH.RunAction("Elementais - Exportar Site", true);
    }

    private string ObterNomeBichoPorId(string id)
    {
        try
        {
            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                return ObterNomeBichoPorId(con, id);
            }
        }
        catch { return id; }
    }

    private string ObterNomeBichoPorId(SQLiteConnection con, string id)
    {
        try
        {
            if (id.StartsWith("u_")) return id.Substring(2) + " (Especial)";

            string[] partes = id.Split('_');
            int especieId = int.Parse(partes[0]);
            int varianteId = int.Parse(partes[1]);

            using (var cmd = new SQLiteCommand(@"
                SELECT e.nome, v.nome
                FROM cfg_especies e
                LEFT JOIN cfg_variantes v ON v.id = @varId
                WHERE e.id = @espId", con))
            {
                cmd.Parameters.AddWithValue("@varId", varianteId);
                cmd.Parameters.AddWithValue("@espId", especieId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string espNome = reader[0].ToString();
                        string varNome = reader[1] != DBNull.Value ? reader[1].ToString() : "Normal";
                        return string.Format("{0} ({1})", espNome, varNome);
                    }
                }
            }
        }
        catch {}
        return id;
    }

    private void ProcessarPescaExtra(string userId, string userName)
    {
        try
        {
            bool isSuper = CPH.GetGlobalVar<bool>("cacaSpritPeixeSuper");
            int loops = isSuper ? 2 : 1;
            
            for (int l = 0; l < loops; l++)
            {
                Random rnd = new Random();
                List<int> especiesValidasPesca = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19, 22, 23, 24, 25 };
                int especieSorteada = especiesValidasPesca[rnd.Next(0, especiesValidasPesca.Count)];
                
                string extraElemId = $"{especieSorteada}_1";
                string nomeBichoPesca = ObterNomeBichoPorId(extraElemId);

                using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    
                    // Inserir ou atualizar na base de dados
                    using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@userId, @elemId, 0)", con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@elemId", extraElemId);
                        cmd.ExecuteNonQuery();
                    }
                    
                    using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@userId AND elemental_id=@elemId", con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@elemId", extraElemId);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                if (isSuper)
                {
                    CPH.SendMessage(string.Format("🎣 [PESCA EXTRA SUPER] @{0} também pescou um {1} extra! 🐟", userName, nomeBichoPesca));
                }
                else
                {
                    CPH.SendMessage(string.Format("🎣 [PESCA EXTRA] @{0} também pescou um {1} extra! 🐟", userName, nomeBichoPesca));
                }
                
                EscreverEstado(string.Format("PESCA;{0};{1}", userName, extraElemId));
            }
            
            CPH.SetGlobalVar("cacaSpritPeixeSuper", false);
            
            // Sincroniza o site online instantaneamente
            CPH.RunAction("Elementais - Exportar Site", true);
        }
        catch (Exception ex)
        {
            CPH.SendMessage("Erro ao processar pesca extra: " + ex.Message);
        }
    }

    // =========================================================================
    // SISTEMA DE WIN STREAK (CAPTURAS SEGUIDAS)
    // =========================================================================
    private void ProcessarWinStreak(SQLiteConnection con, string userId, string userName, bool sucesso)
    {
        try
        {
            if (!sucesso)
            {
                using (var cmd = new SQLiteCommand("UPDATE utilizadores SET win_streak = 0 WHERE user_id=@uid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                }
                return;
            }

            int currentStreak = 0;
            using (var cmd = new SQLiteCommand("SELECT win_streak FROM utilizadores WHERE user_id=@uid", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) currentStreak = Convert.ToInt32(res);
            }

            int newStreak = currentStreak + 1;

            using (var cmd = new SQLiteCommand("UPDATE utilizadores SET win_streak = @streak WHERE user_id=@uid", con))
            {
                cmd.Parameters.AddWithValue("@streak", newStreak);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();
            }

            // NOVA REGRA: Apanhar quebra a streak de todos os outros!
            using (var cmd = new SQLiteCommand("UPDATE utilizadores SET win_streak = 0 WHERE user_id != @uid", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();
            }

            Random rnd = new Random();

            if (newStreak == 1)
            {
                CPH.SendMessage($"🔥 @{userName} Apanha mais 1 seguido e ganha 1 extra! (Streak:1)");
            }
            else if (newStreak == 2)
            {
                Dictionary<string, int> counts = new Dictionary<string, int>();
                using (var cmd = new SQLiteCommand("SELECT elemental_id, quantidade FROM capturas WHERE user_id=@uid AND quantidade > 0", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            counts[reader[0].ToString()] = Convert.ToInt32(reader[1]);
                        }
                    }
                }

                List<string> unownedPool = new List<string>();
                using (var cmd = new SQLiteCommand("SELECT id FROM cfg_especies WHERE raridade != 'custom'", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int espId = Convert.ToInt32(reader[0]);
                            for (int v = 1; v <= 3; v++)
                            {
                                string eid = $"{espId}_{v}";
                                int qty = counts.ContainsKey(eid) ? counts[eid] : 0;
                                if (qty < 2) unownedPool.Add(eid);
                            }
                        }
                    }
                }

                if (unownedPool.Count > 0)
                {
                    string rewardId = unownedPool[rnd.Next(0, unownedPool.Count)];
                    AdicionarElementalSemPerder(con, userId, rewardId);
                    string bichoNome = ObterNomeBichoPorId(con, rewardId);
                    CPH.SendMessage($"🔥 STREAK 2! @{userName} ganhou +1 {bichoNome} extra! Apanha mais 1 seguido e ganha 1 RARO extra! 🔥 (Streak:2)");
                }
                else
                {
                    CPH.SendMessage($"🔥 STREAK 2! @{userName} já tem pelo menos 2x de todos os Normal, Gold e Gummy! Apanha mais 1 seguido e ganha 1 RARO extra! 🔥 (Streak:2)");
                }
            }
            else if (newStreak == 3)
            {
                Dictionary<string, int> counts = new Dictionary<string, int>();
                using (var cmd = new SQLiteCommand("SELECT elemental_id, quantidade FROM capturas WHERE user_id=@uid AND quantidade > 0", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            counts[reader[0].ToString()] = Convert.ToInt32(reader[1]);
                        }
                    }
                }

                List<string> unownedRares = new List<string>();
                using (var cmd = new SQLiteCommand("SELECT id FROM cfg_especies WHERE raridade != 'custom'", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int espId = Convert.ToInt32(reader[0]);
                            int q3 = counts.ContainsKey($"{espId}_3") ? counts[$"{espId}_3"] : 0;
                            int q4 = counts.ContainsKey($"{espId}_4") ? counts[$"{espId}_4"] : 0;
                            int q5 = counts.ContainsKey($"{espId}_5") ? counts[$"{espId}_5"] : 0;
                            int q6 = counts.ContainsKey($"{espId}_6") ? counts[$"{espId}_6"] : 0;
                            int q7 = counts.ContainsKey($"{espId}_7") ? counts[$"{espId}_7"] : 0;

                            if (q3 < 2) unownedRares.Add($"{espId}_3");
                            if (q4 < 2) unownedRares.Add($"{espId}_4");
                            if (SupportsHolofoil(espId) && q5 < 2) unownedRares.Add($"{espId}_5");
                            if (SupportsCube(espId) && q6 < 2) unownedRares.Add($"{espId}_6");
                            if (SupportsGem(espId) && q7 < 2) unownedRares.Add($"{espId}_7");
                        }
                    }
                }

                if (unownedRares.Count > 0)
                {
                    string rewardId = unownedRares[rnd.Next(0, unownedRares.Count)];
                    AdicionarElementalSemPerder(con, userId, rewardId);
                    string bichoNome = ObterNomeBichoPorId(con, rewardId);
                    CPH.SendMessage($"⚡ STREAK 3! @{userName} ganhou +1 {bichoNome} RARO extra! 🏆 Apanha mais 1 seguido e ganha 1 Elemental INÉDITO para a tua coleção! 👑 (Streak:3)");
                }
                else
                {
                    CPH.SendMessage($"⚡ STREAK 3! @{userName} já tem pelo menos 2x de todas as cartas Raras! Apanha mais 1 seguido e ganha 1 Elemental INÉDITO para a tua coleção! 👑 (Streak:3)");
                }
            }
            else if (newStreak >= 4)
            {
                Dictionary<string, int> counts = new Dictionary<string, int>();
                using (var cmd = new SQLiteCommand("SELECT elemental_id, quantidade FROM capturas WHERE user_id=@uid AND quantidade > 0", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            counts[reader[0].ToString()] = Convert.ToInt32(reader[1]);
                        }
                    }
                }

                // OBRIGATORIAMENTE UMA CARTA QUE O JOGADOR NÃO TENHA (quantidade == 0)
                List<string> unownedStrict = new List<string>();

                // 1. Cartas regulares que o jogador tem 0 unidades
                using (var cmd = new SQLiteCommand("SELECT id FROM cfg_especies WHERE raridade != 'custom'", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int espId = Convert.ToInt32(reader[0]);
                            for (int v = 1; v <= 7; v++)
                            {
                                if (v == 5 && !SupportsHolofoil(espId)) continue;
                                if (v == 6 && !SupportsCube(espId)) continue;
                                if (v == 7 && !SupportsGem(espId)) continue;

                                string eid = $"{espId}_{v}";
                                int qty = counts.ContainsKey(eid) ? counts[eid] : 0;
                                if (qty == 0) unownedStrict.Add(eid);
                            }
                        }
                    }
                }

                // 2. Especiais e Comunidade que o jogador tem 0 unidades
                List<string> todosEspeciais = ObterTodosEspeciaisIds();
                foreach (var sId in todosEspeciais)
                {
                    int qty = counts.ContainsKey(sId) ? counts[sId] : 0;
                    if (qty == 0) unownedStrict.Add(sId);
                }

                if (unownedStrict.Count > 0)
                {
                    string rewardId = unownedStrict[rnd.Next(0, unownedStrict.Count)];
                    AdicionarElementalSemPerder(con, userId, rewardId);
                    string bichoNome = ObterNomeBichoPorId(con, rewardId);
                    CPH.SendMessage($"👑 STREAK 4 MÁXIMA! @{userName} completou 4 capturas seguidas e desbloqueou +1 {bichoNome} INÉDITO para a sua coleção! 🏆🎉");
                }
                else
                {
                    // Fallback se o jogador já tem 100% de todas as cartas do jogo
                    List<string> backupRares = new List<string> { "1_4", "1_5", "1_7", "2_4", "2_6", "2_7", "3_4", "3_5", "3_6", "4_4", "4_7", "7_4", "7_7", "8_4", "8_6", "9_4", "9_5", "10_4", "10_5", "10_6", "10_7", "12_4", "12_6", "13_4", "13_5", "14_4", "14_7", "15_4", "15_6", "16_4", "16_5", "16_6", "16_7", "17_4", "17_5", "18_4", "18_5", "19_4", "19_5", "19_6", "22_4", "22_7", "23_4", "23_5" };
                    string rewardId = backupRares[rnd.Next(0, backupRares.Count)];
                    AdicionarElementalSemPerder(con, userId, rewardId);
                    string bichoNome = ObterNomeBichoPorId(con, rewardId);
                    CPH.SendMessage($"👑 STREAK 4 MÁXIMA! @{userName} já tem todos os Elementais do jogo, por isso ganhou +1 {bichoNome} duplicado! 🏆👑");
                }

                using (var cmd = new SQLiteCommand("UPDATE utilizadores SET win_streak = 0 WHERE user_id=@uid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogWarn("[WinStreak] Erro ao processar streak: " + ex.Message);
        }
    }

    private List<string> ObterTodosEspeciaisIds()
    {
        List<string> list = new List<string> { "11_1", "20_1", "21_1", "24_1", "25_1" };
        string userDir = @"I:\Twitch\Games\elementais\Sprites\Users";
        if (Directory.Exists(userDir))
        {
            foreach (var f in Directory.GetFiles(userDir, "*.png"))
            {
                string name = Path.GetFileNameWithoutExtension(f);
                list.Add($"u_{name}");
            }
            foreach (var f in Directory.GetFiles(userDir, "*.webp"))
            {
                string name = Path.GetFileNameWithoutExtension(f);
                string id = $"u_{name}";
                if (!list.Contains(id)) list.Add(id);
            }
        }
        return list;
    }

    private void AdicionarElementalSemPerder(SQLiteConnection con, string userId, string elemId)
    {
        using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@uid, @eid, 0)", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elemId);
            cmd.ExecuteNonQuery();
        }
        using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@uid AND elemental_id=@eid", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elemId);
            cmd.ExecuteNonQuery();
        }
    }

    private static bool SupportsHolofoil(int especieId)
    {
        return (especieId == 1 || especieId == 3 || especieId == 5 || especieId == 9 || especieId == 10 || especieId == 13 || especieId == 16 || especieId == 17 || especieId == 18 || especieId == 19 || especieId == 23);
    }

    private static bool SupportsCube(int especieId)
    {
        return (especieId == 2 || especieId == 3 || especieId == 6 || especieId == 8 || especieId == 10 || especieId == 12 || especieId == 15 || especieId == 16 || especieId == 19);
    }

    private static bool SupportsGem(int especieId)
    {
        return (especieId == 1 || especieId == 2 || especieId == 4 || especieId == 7 || especieId == 10 || especieId == 14 || especieId == 16 || especieId == 22);
    }

    private void VerificarVitoriaAlbumCompleto(SQLiteConnection con)
    {
        try
        {
            // 1. Obter total de cartas existentes no jogo
            List<int> speciesList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19, 22, 23 };
            int totalJogo = 0;

            foreach (int espId in speciesList)
            {
                for (int v = 1; v <= 7; v++)
                {
                    if (v == 5 && !SupportsHolofoil(espId)) continue;
                    if (v == 6 && !SupportsCube(espId)) continue;
                    if (v == 7 && !SupportsGem(espId)) continue;
                    totalJogo++;
                }
            }

            totalJogo += 4; // Quack milestones (1_8, 2_8, 3_8, 10_8)
            List<string> todosEspeciais = ObterTodosEspeciaisIds();
            totalJogo += todosEspeciais.Count;

            // 2. Verificar se algum jogador completou 100% e ainda não tem posicao_vitoria
            using (var cmd = new SQLiteCommand(@"
                SELECT u.user_id, u.username, COUNT(DISTINCT c.elemental_id) as total_distinct
                FROM capturas c
                JOIN utilizadores u ON u.user_id = c.user_id
                WHERE c.quantidade > 0 AND (u.posicao_vitoria IS NULL OR u.posicao_vitoria = 0)
                GROUP BY c.user_id
                HAVING total_distinct >= @total", con))
            {
                cmd.Parameters.AddWithValue("@total", totalJogo);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string winUid = reader["user_id"].ToString();
                        string winName = reader["username"].ToString();
                        int winDistinct = Convert.ToInt32(reader["total_distinct"]);

                        // Determinar a próxima posição de vitória no Hall da Fama (1º, 2º, 3º...)
                        int nextPos = 1;
                        using (var cmdPos = new SQLiteCommand("SELECT COALESCE(MAX(posicao_vitoria), 0) + 1 FROM utilizadores", con))
                        {
                            object posRes = cmdPos.ExecuteScalar();
                            if (posRes != null && posRes != DBNull.Value) nextPos = Convert.ToInt32(posRes);
                        }

                        // Marcar vitória na BD para nunca mais sair dessa posição
                        using (var cmdWin = new SQLiteCommand("UPDATE utilizadores SET vitoria_anunciada = 1, posicao_vitoria = @pos, data_vitoria = datetime('now') WHERE user_id=@uid", con))
                        {
                            cmdWin.Parameters.AddWithValue("@pos", nextPos);
                            cmdWin.Parameters.AddWithValue("@uid", winUid);
                            cmdWin.ExecuteNonQuery();
                        }

                        // Enviar comando visual para o OBS (exatamente no elemental-area com posição) e mensagem no chat
                        EscreverEstado($"VITORIA;{winName};{winDistinct};{nextPos}");
                        CPH.SendMessage($"👑🏆 HISTÓRICO! @{winName} É O #{nextPos}º JOGADOR A COMPLETAR 100% DO ÁLBUM DE ELEMENTAIS ({winDistinct}/{totalJogo})! CONQUISTOU ETERNAMENTE O TOP {nextPos} NO HALL DA FAMA! 🌟🎉👑");

                        // Sincronizar o site GitHub de imediato com o novo Campeão no topo
                        CPH.RunAction("Elementais - Exportar Site");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogWarn("[Vitoria] Erro ao verificar álbum completo: " + ex.Message);
        }
    }

    private void ProcessarMilestonesQuack(SQLiteConnection con, string userId, string userName)
    {
        try
        {
            int totalCapturas = 0;
            using (var cmd = new SQLiteCommand("SELECT COUNT(DISTINCT elemental_id) FROM capturas WHERE user_id=@uid AND quantidade > 0 AND elemental_id NOT LIKE '%_8'", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) totalCapturas = Convert.ToInt32(res);
            }

            var milestones = new (int req, string elemId, string nome, int pts)[]
            {
                (20, "1_8", "Água Quack", 300),
                (40, "2_8", "Terra Quack", 450),
                (75, "3_8", "Fogo Quack", 600),
                (100, "10_8", "Ponto Zero Quack", 900)
            };

            foreach (var m in milestones)
            {
                if (totalCapturas >= m.req)
                {
                    bool jaPossui = false;
                    using (var cmdCheck = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid AND quantidade > 0", con))
                    {
                        cmdCheck.Parameters.AddWithValue("@uid", userId);
                        cmdCheck.Parameters.AddWithValue("@eid", m.elemId);
                        object countObj = cmdCheck.ExecuteScalar();
                        if (countObj != null && countObj != DBNull.Value && Convert.ToInt32(countObj) > 0)
                        {
                            jaPossui = true;
                        }
                    }

                    if (!jaPossui)
                    {
                        AdicionarElementalSemPerder(con, userId, m.elemId);

                        if (m.elemId == "10_8")
                        {
                            CPH.SendMessage($"👑 MILESTONE {m.req}! @{userName} alcançou {m.req} capturas e ganhou o Ponto Zero Quack! 🦆👑");
                        }
                        else
                        {
                            CPH.SendMessage($"🦆 MILESTONE {m.req}! @{userName} alcançou {m.req} capturas e ganhou o {m.nome}! 🦆🎉");
                        }

                        EscreverEstado($"QUACK;{userName};{m.elemId};{m.req}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogWarn("[QuackMilestone] Erro ao processar milestone: " + ex.Message);
        }
    }

    private void ProcessarPiedadeFalhas(SQLiteConnection con, string userId, string userName, string bola)
    {
        try
        {
            // Garante que o jogador está na BD
            using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO utilizadores (user_id, username, passou_1000, win_streak, falhas_normal, falhas_super, falhas_ultra, falhas_master) VALUES (@uid, @uname, 0, 0, 0, 0, 0, 0)", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@uname", userName);
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new SQLiteCommand("UPDATE utilizadores SET username=@uname WHERE user_id=@uid", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@uname", userName);
                cmd.ExecuteNonQuery();
            }

            // Incrementa o contador da bola usada
            string colName = "falhas_normal";
            string bolaLower = bola.ToLower();
            if (bolaLower == "super") colName = "falhas_super";
            else if (bolaLower == "ultra") colName = "falhas_ultra";
            else if (bolaLower == "master") colName = "falhas_master";

            using (var cmd = new SQLiteCommand(string.Format("UPDATE utilizadores SET {0} = {0} + 1 WHERE user_id=@uid", colName), con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();
            }

            // Lê as falhas acumuladas
            int fn = 0, fs = 0, fu = 0, fm = 0;
            using (var cmd = new SQLiteCommand("SELECT falhas_normal, falhas_super, falhas_ultra, falhas_master FROM utilizadores WHERE user_id=@uid", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        fn = Convert.ToInt32(reader["falhas_normal"]);
                        fs = Convert.ToInt32(reader["falhas_super"]);
                        fu = Convert.ToInt32(reader["falhas_ultra"]);
                        fm = Convert.ToInt32(reader["falhas_master"]);
                    }
                }
            }

            int totalFalhas = fn + fs + fu + fm;
            if (totalFalhas >= 8)
            {
                // Determina a maioria
                string majorityBall = "normal";
                int maxCount = fn;
                if (fs >= maxCount) { majorityBall = "super"; maxCount = fs; }
                if (fu >= maxCount) { majorityBall = "ultra"; maxCount = fu; }
                if (fm >= maxCount) { majorityBall = "master"; maxCount = fm; }

                // Determina a variante e o nome da bola
                int varianteId = 1; // normal
                string varNome = "Normal";
                string bolaUsadaNome = "Normal Ball";
                if (majorityBall == "super") { varianteId = 2; varNome = "Gold"; bolaUsadaNome = "Super Ball"; }
                else if (majorityBall == "ultra") { varianteId = 3; varNome = "Gummy"; bolaUsadaNome = "Ultra Ball"; }
                else if (majorityBall == "master") { varianteId = 4; varNome = "Galaxy"; bolaUsadaNome = "Master Ball"; }

                // Filtra as espécies válidas onde o utilizador tem menos de 2 unidades desta variante
                List<int> eligibleSpecies = new List<int>();
                using (var cmdFilter = new SQLiteCommand(@"
                    SELECT e.id
                    FROM cfg_especies e
                    LEFT JOIN capturas c ON c.elemental_id = (CAST(e.id AS TEXT) || '_' || @varId) AND c.user_id = @uid
                    WHERE e.id IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19, 22, 23)
                      AND (c.quantidade IS NULL OR c.quantidade < 2)", con))
                {
                    cmdFilter.Parameters.AddWithValue("@varId", varianteId.ToString());
                    cmdFilter.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmdFilter.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            eligibleSpecies.Add(Convert.ToInt32(reader[0]));
                        }
                    }
                }

                if (eligibleSpecies.Count > 0)
                {
                    // Escolhe um elemental aleatório da lista de elegíveis
                    Random rnd = new Random();
                    int especieSorteada = eligibleSpecies[rnd.Next(0, eligibleSpecies.Count)];
                    string elemId = string.Format("{0}_{1}", especieSorteada, varianteId);

                    // Adiciona o elemental
                    AdicionarElementalSemPerder(con, userId, elemId);
                    string bichoNome = ObterNomeBichoPorId(con, elemId);

                    CPH.SendMessage(string.Format("🍀 @{0} falhou 8 lançamentos! A maioria com {1}, ganhou um {2} extra! 🎒🎉", 
                        userName, bolaUsadaNome, bichoNome));
                    
                    EscreverEstado(string.Format("PIEDADE;{0};{1}", userName, elemId));
                }
                else
                {
                    CPH.SendMessage(string.Format("🍀 @{0} falhou 8 lançamentos! A maioria com {1}, mas já tem pelo menos 2 exemplares de todos os ({2})! 🎒", 
                        userName, bolaUsadaNome, varNome));
                }

                // Reseta os contadores de falhas
                using (var cmd = new SQLiteCommand("UPDATE utilizadores SET falhas_normal=0, falhas_super=0, falhas_ultra=0, falhas_master=0 WHERE user_id=@uid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogWarn("[PiedadeFalhas] Erro ao processar: " + ex.Message);
        }
    }
}