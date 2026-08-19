using System;
using System.Collections.Generic;
using System.Data.SQLite;

public class CPHInline
{
    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";

    public bool Execute()
    {
        int cacaID = CPH.GetGlobalVar<int>("cacaID");

        // Se a chamada veio do timer de 50s
        if (args.ContainsKey("lobbyTimerCacaID"))
        {
            int timerCacaID = Convert.ToInt32(args["lobbyTimerCacaID"]);
            
            // 1. Verificar se é o mesmo spawn
            if (timerCacaID != cacaID)
            {
                CPH.LogInfo(string.Format("[Lobby] Ignorando timer do spawn antigo {0} (Spawn atual: {1})", timerCacaID, cacaID));
                return true;
            }

            // 2. Verificar se o lobby já foi resolvido (por ter atingido 5 inscritos)
            bool jaResolvido = CPH.GetGlobalVar<bool>("lobbyResolvido");
            if (jaResolvido)
            {
                CPH.LogInfo("[Lobby] Ignorando timer de 50s pois o lobby ja foi resolvido antecipadamente.");
                return true;
            }
        }

        bool shouldProcess = false;
        bool isFirstResolution = false;
        int lobbyCount = 0;

        lock (typeof(CPHInline))
        {
            bool lobbyResolvido = CPH.GetGlobalVar<bool>("lobbyResolvido");
            if (!lobbyResolvido)
            {
                lobbyCount = CPH.GetGlobalVar<int>("lobbyCount");
                if (lobbyCount > 0)
                {
                    CPH.SetGlobalVar("lobbyResolvido", true);
                    CPH.SetGlobalVar("lobbyAtivo", false);
                    isFirstResolution = true;
                    shouldProcess = true;
                }
                else
                {
                    // Ninguém se inscreveu: desativa o lobby mas não o marca como resolvido para permitir arremessos livres (FCFS)
                    CPH.SetGlobalVar("lobbyAtivo", false);

                    CPH.SendMessage("⏳ Sorteio vazio! Lançamento livre ativo (por ordem de chegada).");
                    return true;
                }
            }
            else
            {
                shouldProcess = true;
            }
        }

        if (shouldProcess)
        {
            if (isFirstResolution)
            {
                int cacaSpritSleepyCount = CPH.GetGlobalVar<int>("cacaSpritSleepyCount");
                string cacaSpritSleepyUsers = CPH.GetGlobalVar<string>("cacaSpritSleepyUsers") ?? "";
                List<string> casters = new List<string>(cacaSpritSleepyUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                List<int> sleepIndices = new List<int>();
                Random rnd = new Random();

                if (cacaSpritSleepyCount > 0 && lobbyCount > 0)
                {
                    for (int s = 0; s < cacaSpritSleepyCount; s++)
                    {
                        List<int> candidateIndices = new List<int>();
                        for (int i = 1; i <= lobbyCount; i++)
                        {
                            if (sleepIndices.Contains(i)) continue;
                            
                            string uname = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", i)) ?? "";
                            bool isCaster = false;
                            foreach (var caster in casters)
                            {
                                if (uname.Equals(caster, StringComparison.OrdinalIgnoreCase))
                                {
                                    isCaster = true;
                                    break;
                                }
                            }

                            if (!isCaster)
                            {
                                candidateIndices.Add(i);
                            }
                        }

                        if (candidateIndices.Count > 0)
                        {
                            int randomIdx = rnd.Next(0, candidateIndices.Count);
                            int selectedSleepIdx = candidateIndices[randomIdx];
                            sleepIndices.Add(selectedSleepIdx);

                            string sleepUName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", selectedSleepIdx)) ?? "Viewer";
                            string sleepRwdId = CPH.GetGlobalVar<string>(string.Format("lobby_reward_id_{0}", selectedSleepIdx)) ?? "";
                            string sleepRedId = CPH.GetGlobalVar<string>(string.Format("lobby_redemption_id_{0}", selectedSleepIdx)) ?? "";

                            if (!string.IsNullOrEmpty(sleepRwdId) && !string.IsNullOrEmpty(sleepRedId))
                            {
                                try { CPH.TwitchRedemptionFulfill(sleepRwdId, sleepRedId); } catch {}
                            }

                            CPH.SendMessage(string.Format("💤 @{0} adormeceu! Arremesso cancelado (pontos gastos).", sleepUName));
                        }
                    }
                    
                    CPH.SetGlobalVar("cacaSpritSleepyCount", 0);
                    CPH.SetGlobalVar("cacaSpritSleepyUsers", "");
                    CPH.SetGlobalVar("cacaSpritSleepyAtiva", false);
                }

                // Criar lista de índices de 1 a lobbyCount (excluindo quem adormeceu) e baralhar usando Fisher-Yates
                List<int> indices = new List<int>();
                for (int i = 1; i <= lobbyCount; i++)
                {
                    if (sleepIndices.Contains(i)) continue;
                    indices.Add(i);
                }

                int n = indices.Count;
                while (n > 1)
                {
                    n--;
                    int k = rnd.Next(n + 1);
                    int value = indices[k];
                    indices[k] = indices[n];
                    indices[n] = value;
                }

                // Processar Prioridade de "Copinho de Leite" (< 1000 pontos)
                bool copinhoLeiteAtivo = CPH.GetGlobalVar<bool>("copinhoLeiteAtivo");
                if (copinhoLeiteAtivo && indices.Count > 1)
                {
                    Dictionary<string, int> userPoints = new Dictionary<string, int>();
                    try
                    {
                        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                        {
                            con.Open();
                            string queryPoints = @"
                                    SELECT c.user_id,
                                           COALESCE(u.passou_1000, 0) as passou_1000,
                                           SUM(
                                                CASE 
                                                    WHEN c.elemental_id = '1_8' THEN 300
                                                    WHEN c.elemental_id = '2_8' THEN 450
                                                    WHEN c.elemental_id = '3_8' THEN 600
                                                    WHEN c.elemental_id = '10_8' THEN 900
                                                    WHEN c.elemental_id LIKE 'u_%' THEN 100
                                                    ELSE COALESCE(p.pontos, e.pontos_custom, 0)
                                                END
                                            ) as total_pontos
                                    FROM capturas c
                                    LEFT JOIN cfg_especies e ON e.id = CAST(SUBSTR(c.elemental_id, 1, INSTR(c.elemental_id, '_') - 1) AS INTEGER)
                                    LEFT JOIN cfg_pontos p ON e.raridade = p.raridade AND p.variante_id = CAST(SUBSTR(c.elemental_id, INSTR(c.elemental_id, '_') + 1) AS INTEGER)
                                    LEFT JOIN utilizadores u ON c.user_id = u.user_id
                                    WHERE c.quantidade > 0
                                    GROUP BY c.user_id, u.passou_1000";
                            using (var cmdPoints = new SQLiteCommand(queryPoints, con))
                            {
                                using (var readerPoints = cmdPoints.ExecuteReader())
                                {
                                    while (readerPoints.Read())
                                    {
                                        string uid = readerPoints["user_id"].ToString();
                                        int pts = Convert.ToInt32(readerPoints["total_pontos"]);
                                        int passou1000 = Convert.ToInt32(readerPoints["passou_1000"]);
                                        
                                        // Apenas os que não passaram dos 1000 pontos contam como copinhos.
                                        // Mas se os pontos deles somados < 1000 e eles nunca passaram, marcamos.
                                        // Mas esperem! Eu armazeno num Dictionary<string, int> userPoints.
                                        // O dict vai guardar (passou1000 == 1 ? int.MaxValue : pts).
                                        // Desta forma pts será int.MaxValue e vai falhar o check if (pts < 1000).
                                        userPoints[uid] = (passou1000 == 1) ? int.MaxValue : pts;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CPH.LogWarn("[ResolverLobby] Erro ao calcular pontos para copinhos de leite: " + ex.Message);
                    }

                    List<int> copinhoIndices = new List<int>();
                    List<string> copinhoNames = new List<string>();

                    for (int i = indices.Count - 1; i >= 0; i--)
                    {
                        int pIdx = indices[i];
                        string uid = CPH.GetGlobalVar<string>(string.Format("lobby_user_id_{0}", pIdx)) ?? "";
                        string uName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", pIdx)) ?? "Viewer";
                        int pts = userPoints.ContainsKey(uid) ? userPoints[uid] : 0;

                        if (pts < 1000)
                        {
                            copinhoIndices.Add(pIdx);
                            copinhoNames.Add(uName);
                            indices.RemoveAt(i);
                        }
                    }

                    if (copinhoIndices.Count > 0)
                    {
                        // Baralhar os copinhos de leite entre si
                        int cn = copinhoIndices.Count;
                        while (cn > 1)
                        {
                            cn--;
                            int k = rnd.Next(cn + 1);
                            int tempVal = copinhoIndices[k];
                            copinhoIndices[k] = copinhoIndices[cn];
                            copinhoIndices[cn] = tempVal;

                            string tempName = copinhoNames[k];
                            copinhoNames[k] = copinhoNames[cn];
                            copinhoNames[cn] = tempName;
                        }

                        // Inserir no início da fila de indices (atrás das Auras!)
                        for (int i = copinhoIndices.Count - 1; i >= 0; i--)
                        {
                            indices.Insert(0, copinhoIndices[i]);
                        }

                        // Anunciar no chat
                        List<string> taggedNames = new List<string>();
                        foreach (string name in copinhoNames)
                        {
                            taggedNames.Add("@" + name);
                        }

                        if (copinhoIndices.Count == 1)
                        {
                            CPH.SendMessage(string.Format("🍼 Como @{0} é copinho de leite (< 1000 pts), passa à frente na fila!", copinhoNames[0]));
                        }
                        else
                        {
                            CPH.SendMessage(string.Format("🍼 Como {0} são copinhos de leite (< 1000 pts), passam à frente na fila!", string.Join(", ", taggedNames)));
                        }
                    }
                }

                // Mover o utilizador da Aura para a frente da fila, se estiver inscrito e não adormeceu
                bool cacaSpritAuraAtiva = CPH.GetGlobalVar<bool>("cacaSpritAuraAtiva");
                if (cacaSpritAuraAtiva)
                {
                    string cacaSpritAuraUserId = CPH.GetGlobalVar<string>("cacaSpritAuraUserId") ?? "";
                    string cacaSpritAuraUser = CPH.GetGlobalVar<string>("cacaSpritAuraUser") ?? "";

                    string[] auraIds = cacaSpritAuraUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] auraNames = cacaSpritAuraUser.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    List<int> foundLobbyIndices = new List<int>();
                    List<string> foundNames = new List<string>();

                    for (int a = 0; a < auraIds.Length; a++)
                    {
                        string targetId = auraIds[a].Trim();
                        string targetName = a < auraNames.Length ? auraNames[a].Trim() : "Viewer";

                        int idxInList = -1;
                        for (int i = 0; i < indices.Count; i++)
                        {
                            int pIdx = indices[i];
                            string uid = CPH.GetGlobalVar<string>(string.Format("lobby_user_id_{0}", pIdx)) ?? "";
                            if (uid.Equals(targetId, StringComparison.OrdinalIgnoreCase))
                            {
                                idxInList = i;
                                break;
                            }
                        }

                        if (idxInList != -1)
                        {
                            foundLobbyIndices.Add(indices[idxInList]);
                            foundNames.Add(targetName);
                            indices.RemoveAt(idxInList);
                        }
                    }

                    if (foundLobbyIndices.Count == 1)
                    {
                        indices.Insert(0, foundLobbyIndices[0]);
                        CPH.SendMessage(string.Format("✨ @{0}, como tens aura, passas à frente na fila!", foundNames[0]));
                    }
                    else if (foundLobbyIndices.Count >= 2)
                    {
                        Random randomObj = new Random();
                        int k = randomObj.Next(foundLobbyIndices.Count);
                        
                        int firstVal = foundLobbyIndices[k];
                        foundLobbyIndices.RemoveAt(k);
                        int secondVal = foundLobbyIndices[0];

                        string firstName = foundNames[k];
                        foundNames.RemoveAt(k);
                        string secondName = foundNames[0];

                        indices.Insert(0, secondVal);
                        indices.Insert(0, firstVal);

                        CPH.SendMessage(string.Format("✨ Auras duelam! Sorteio entre auras: @{0} fica em 1º e @{1} fica em 2º da fila!", firstName, secondName));
                    }
                    
                    CPH.SetGlobalVar("cacaSpritAuraAtiva", false);
                    CPH.SetGlobalVar("cacaSpritAuraUser", "");
                    CPH.SetGlobalVar("cacaSpritAuraUserId", "");
                }

                // Processar efeito do Elemental de Vento
                bool cacaSpritVentoAtiva = CPH.GetGlobalVar<bool>("cacaSpritVentoAtiva");
                if (cacaSpritVentoAtiva && indices.Count > 1)
                {
                    // Anunciar a ordem inicial antes do Vento atuar
                    List<string> nomesIniciais = new List<string>();
                    for (int i = 0; i < indices.Count; i++)
                    {
                        string uName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", indices[i])) ?? "Viewer";
                        nomesIniciais.Add("@" + uName);
                    }
                    CPH.SendMessage(string.Format("🎲 Ordem Inicial do Sorteio: {0}", string.Join(", ", nomesIniciais)));

                    string cacaSpritVentoUserId = CPH.GetGlobalVar<string>("cacaSpritVentoUserId") ?? "";
                    string cacaSpritVentoUser = CPH.GetGlobalVar<string>("cacaSpritVentoUser") ?? "";
                    string cacaSpritVentoSuper = CPH.GetGlobalVar<string>("cacaSpritVentoSuper") ?? "";

                    string[] ventoIds = cacaSpritVentoUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] ventoNames = cacaSpritVentoUser.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] ventoSuperIds = cacaSpritVentoSuper.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    List<string> processedUsers = new List<string>();

                    for (int v = 0; v < ventoIds.Length; v++)
                    {
                        string targetId = ventoIds[v].Trim();
                        string targetName = v < ventoNames.Length ? ventoNames[v].Trim() : "Viewer";

                        if (processedUsers.Contains(targetId)) continue;
                        processedUsers.Add(targetId);

                        bool noLobby = false;
                        int posActual = -1;
                        for (int i = 0; i < indices.Count; i++)
                        {
                            string uid = CPH.GetGlobalVar<string>(string.Format("lobby_user_id_{0}", indices[i])) ?? "";
                            if (uid.Equals(targetId, StringComparison.OrdinalIgnoreCase))
                            {
                                noLobby = true;
                                posActual = i + 1;
                                break;
                            }
                        }

                        if (!noLobby) continue;

                        bool userIsSuper = false;
                        foreach (string sid in ventoSuperIds)
                        {
                            if (sid.Trim().Equals(targetId, StringComparison.OrdinalIgnoreCase))
                            {
                                userIsSuper = true;
                                break;
                            }
                        }

                        if (posActual > 1)
                        {
                            // Baralhar 1ª vez
                            int shuffleN = indices.Count;
                            while (shuffleN > 1)
                            {
                                shuffleN--;
                                int k = rnd.Next(shuffleN + 1);
                                int val = indices[k];
                                indices[k] = indices[shuffleN];
                                indices[shuffleN] = val;
                            }

                            List<string> nomesNovos = new List<string>();
                            for (int i = 0; i < indices.Count; i++)
                            {
                                string uName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", indices[i])) ?? "Viewer";
                                nomesNovos.Add("@" + uName);
                            }

                            if (userIsSuper)
                            {
                                CPH.SendMessage(string.Format("🌪️ Vento [SUPER]: @{0} ficou em {1}º! O vento sopra e re-baralha a fila! Nova Ordem: {2}", targetName, posActual, string.Join(", ", nomesNovos)));
                            }
                            else
                            {
                                CPH.SendMessage(string.Format("🌪️ O Vento sopra! @{0} ficou em {1}º lugar e o vento baralha a fila novamente! Nova Ordem: {2}", targetName, posActual, string.Join(", ", nomesNovos)));
                            }

                            // Se for SUPER, verificar se continua fora do 1º lugar
                            if (userIsSuper)
                            {
                                string firstUidAfter1 = CPH.GetGlobalVar<string>(string.Format("lobby_user_id_{0}", indices[0])) ?? "";
                                if (!firstUidAfter1.Equals(targetId, StringComparison.OrdinalIgnoreCase))
                                {
                                    shuffleN = indices.Count;
                                    while (shuffleN > 1)
                                    {
                                        shuffleN--;
                                        int k = rnd.Next(shuffleN + 1);
                                        int val = indices[k];
                                        indices[k] = indices[shuffleN];
                                        indices[shuffleN] = val;
                                    }

                                    List<string> nomesNovos2 = new List<string>();
                                    for (int i = 0; i < indices.Count; i++)
                                    {
                                        string uName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", indices[i])) ?? "Viewer";
                                        nomesNovos2.Add("@" + uName);
                                    }
                                    CPH.SendMessage(string.Format("🌪️ Vento [SUPER]: @{0} ainda não ficou em 1º! O vento sopra 2ª vez e baralha novamente! Ordem Final: {1}", targetName, string.Join(", ", nomesNovos2)));
                                }
                            }
                        }
                        else
                        {
                            CPH.SendMessage(string.Format("🌪️ Vento: @{0} já ficou em 1º lugar na fila! O vento não precisou de soprar.", targetName));
                        }
                    }

                    CPH.SetGlobalVar("cacaSpritVentoAtiva", false);
                    CPH.SetGlobalVar("cacaSpritVentoUser", "");
                    CPH.SetGlobalVar("cacaSpritVentoUserId", "");
                    CPH.SetGlobalVar("cacaSpritVentoSuper", "");
                }

                bool peelyAtuou = false;
                // Processar efeito do Elemental Peely (Casca de Banana)
                bool cacaSpritPeelyAtiva = CPH.GetGlobalVar<bool>("cacaSpritPeelyAtiva");
                if (cacaSpritPeelyAtiva && indices.Count > 1)
                {
                    string cacaSpritPeelyUserId = CPH.GetGlobalVar<string>("cacaSpritPeelyUserId") ?? "";
                    bool cacaSpritPeelySuper = CPH.GetGlobalVar<bool>("cacaSpritPeelySuper");
                    string[] peelyIds = cacaSpritPeelyUserId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    int maxSlips = cacaSpritPeelySuper ? 2 : 1;
                    List<int> toSlip = new List<int>();
                    List<string> slippedNames = new List<string>();

                    int i = 0;
                    while (i < indices.Count && toSlip.Count < maxSlips)
                    {
                        int pIdx = indices[i];
                        string uid = CPH.GetGlobalVar<string>(string.Format("lobby_user_id_{0}", pIdx)) ?? "";
                        
                        bool isPeelyUser = false;
                        foreach (string pid in peelyIds)
                        {
                            if (uid.Equals(pid.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                isPeelyUser = true;
                                break;
                            }
                        }

                        if (!isPeelyUser)
                        {
                            toSlip.Add(pIdx);
                            string uName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", pIdx)) ?? "Viewer";
                            slippedNames.Add("@" + uName);
                            indices.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    foreach (int pIdx in toSlip)
                    {
                        indices.Add(pIdx);
                    }

                    if (slippedNames.Count > 0)
                    {
                        peelyAtuou = true;
                        if (cacaSpritPeelySuper)
                        {
                            CPH.SendMessage(string.Format("🍌 SUPER ESCORREGADELA! {0} pisaram a casca de banana e foram parar ao último lugar!", string.Join(" e ", slippedNames)));
                        }
                        else
                        {
                            CPH.SendMessage(string.Format("🍌 ESCORREGADELA! {0} pisou a casca de banana e escorregou para o último lugar!", slippedNames[0]));
                        }
                    }

                    CPH.SetGlobalVar("cacaSpritPeelyAtiva", false);
                    CPH.SetGlobalVar("cacaSpritPeelyUser", "");
                    CPH.SetGlobalVar("cacaSpritPeelyUserId", "");
                    CPH.SetGlobalVar("cacaSpritPeelySuper", false);
                }

                // Guardar a fila ordenada nos globais
                CPH.SetGlobalVar("lobbyFilaIndex", 1);
                int filaTotal = indices.Count;
                CPH.SetGlobalVar("lobbyFilaTotal", filaTotal);
                for (int i = 1; i <= filaTotal; i++)
                {
                    CPH.SetGlobalVar(string.Format("lobby_fila_item_{0}", i), indices[i - 1]);
                }

                // Anunciar a ordem sorteada no chat (se o Vento não tiver atuado, ou se o Peely tiver modificado a fila depois do Vento)
                List<string> nomesFila = new List<string>();
                for (int i = 1; i <= filaTotal; i++)
                {
                    int pIdx = CPH.GetGlobalVar<int>(string.Format("lobby_fila_item_{0}", i));
                    string userName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", pIdx));
                    nomesFila.Add("@" + userName);
                }
                if (!cacaSpritVentoAtiva || peelyAtuou)
                {
                    CPH.SendMessage(string.Format("🎲 Sorteio concluído! Ordem: {0}", string.Join(", ", nomesFila)));
                }

                int manuPos = -1;
                for (int i = 0; i < nomesFila.Count; i++)
                {
                    if (nomesFila[i].ToLower() == "@manu12321_")
                    {
                        manuPos = i;
                        break;
                    }
                }
                if (manuPos != -1)
                {
                    if (rnd.Next(0, 100) < 60)
                    {
                        int opt = rnd.Next(0, 3);
                        if (manuPos == 0)
                        {
                            if (opt == 0) CPH.SendMessage("Não sejas chorão @manu12321_, desta vez és o primeiro da fila! 🥳");
                            else if (opt == 1) CPH.SendMessage("Milagre! O @manu12321_ é o primeiro da fila! Agora não há desculpas! 🥇");
                            else CPH.SendMessage("Atenção chat, o @manu12321_ está em primeiro! Será que a probabilidade de falhar é de 100%? 😂");
                        }
                        else if (manuPos == nomesFila.Count - 1)
                        {
                            if (opt == 0) CPH.SendMessage("É verdade @manu12321_, eu (o bot do Z3) meti-te em último de propósito! 😈");
                            else if (opt == 1) CPH.SendMessage("O @manu12321_ ficou em último de novo! O algoritmo do bot nunca falha na escolha! 📉");
                            else CPH.SendMessage("Último lugar reservado com sucesso para @manu12321_. A tradição mantém-se! ☕");
                        }
                        else
                        {
                            int optMid = rnd.Next(0, 2);
                            if (optMid == 0) CPH.SendMessage(string.Format("Olha @manu12321_, ficaste em {0}º na fila. As probabilidades não batem certo, pois não? 🤪", manuPos + 1));
                            else CPH.SendMessage(string.Format("O @manu12321_ ficou em {0}º lugar. Já começou a fazer as contas às probabilidades? 🧮", manuPos + 1));
                        }
                    }
                }
            }

            // Processar a fila via loop iterativo para evitar recursividade síncrona e deadlocks
            ProcessarLobbyLoop();
        }

        return true;
    }

    private void ProcessarLobbyLoop()
    {
        while (true)
        {
            int filaIndex = CPH.GetGlobalVar<int>("lobbyFilaIndex");
            int filaTotal = CPH.GetGlobalVar<int>("lobbyFilaTotal");

            // Se o elemental já foi capturado ou fugiu, cancela o resto da fila
            bool cacaAtiva = CPH.GetGlobalVar<bool>("cacaAtiva");
            if (!cacaAtiva)
            {
                CancelarRestoDaFila(filaIndex, filaTotal);
                break;
            }

            if (filaIndex <= filaTotal)
            {
                int pIdx = CPH.GetGlobalVar<int>(string.Format("lobby_fila_item_{0}", filaIndex));
                if (pIdx <= 0)
                {
                    CPH.LogWarn(string.Format("[Lobby] Indice de fila invalido ({0}) na posicao {1}. Avancando para o proximo...", pIdx, filaIndex));
                    CPH.SetGlobalVar("lobbyFilaIndex", filaIndex + 1);
                    continue;
                }

                string uid = CPH.GetGlobalVar<string>(string.Format("lobby_user_id_{0}", pIdx)) ?? "";
                string uName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", pIdx)) ?? "Viewer";
                string rwdId = CPH.GetGlobalVar<string>(string.Format("lobby_reward_id_{0}", pIdx)) ?? "";
                string redId = CPH.GetGlobalVar<string>(string.Format("lobby_redemption_id_{0}", pIdx)) ?? "";
                string bola = CPH.GetGlobalVar<string>(string.Format("lobby_bola_{0}", pIdx)) ?? "normal";

                // Incrementa o índice da fila ANTES de chamar a action
                CPH.SetGlobalVar("lobbyFilaIndex", filaIndex + 1);

                // Executar o arremesso de forma forçada sync (bloqueante)
                CPH.SetArgument("userId", uid);
                CPH.SetArgument("userName", uName);
                CPH.SetArgument("rewardId", rwdId);
                CPH.SetArgument("redemptionId", redId);
                CPH.SetArgument("tipoBola", bola);
                CPH.SetArgument("forcarResgateLobby", "true");

                string actionName = "Elementais - Atirar Bola " + bola.ToUpper();
                try
                {
                    CPH.RunAction(actionName, true);
                }
                catch (Exception ex)
                {
                    CPH.LogWarn("[Lobby] Erro ao executar arremesso: " + ex.Message);
                }
            }
            else
            {
                // Fim da fila do lobby
                int tentativasGlobais = CPH.GetGlobalVar<int>("tentativasGlobais");
                if (tentativasGlobais < 5)
                {
                    CPH.SendMessage("📢 Arremessos do lobby falharam! Lançamento livre ativo (por ordem de chegada).");
                }
                break;
            }
        }
    }

    private void CancelarRestoDaFila(int filaIndex, int filaTotal)
    {
        int cacaID = CPH.GetGlobalVar<int>("cacaID");
        List<string> nomesCancelados = new List<string>();

        for (int i = filaIndex; i <= filaTotal; i++)
        {
            int pIdx = CPH.GetGlobalVar<int>(string.Format("lobby_fila_item_{0}", i));
            if (pIdx <= 0) continue;

            string uid = CPH.GetGlobalVar<string>(string.Format("lobby_user_id_{0}", pIdx));
            string uName = CPH.GetGlobalVar<string>(string.Format("lobby_user_name_{0}", pIdx));
            string rwdId = CPH.GetGlobalVar<string>(string.Format("lobby_reward_id_{0}", pIdx));
            string redId = CPH.GetGlobalVar<string>(string.Format("lobby_redemption_id_{0}", pIdx));
            
            if (!string.IsNullOrEmpty(rwdId) && !string.IsNullOrEmpty(redId))
            {
                try 
                { 
                    CPH.TwitchRedemptionCancel(rwdId, redId); 
                    if (!string.IsNullOrEmpty(uName) && !nomesCancelados.Contains(uName))
                    {
                        nomesCancelados.Add("@" + uName);
                    }
                } 
                catch {}
            }

            // Devolver/Restaurar a tentativa do utilizador já que não chegou a atirar
            int tent = CPH.GetGlobalVar<int>(string.Format("tentativas_{0}_{1}", cacaID, uid));
            if (tent > 0)
            {
                CPH.SetGlobalVar(string.Format("tentativas_{0}_{1}", cacaID, uid), tent - 1);
            }
        }

        if (nomesCancelados.Count > 0)
        {
            CPH.SendMessage(string.Format("🎟️ Lançamentos cancelados e reembolsados: {0}", string.Join(", ", nomesCancelados)));
        }
    }

}
