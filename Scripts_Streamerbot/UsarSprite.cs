using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

public class CPHInline
{
    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";

    public bool Execute()
    {
        string userId = args.ContainsKey("userId") ? args["userId"].ToString() : "12345";
        string userName = args.ContainsKey("userName") ? args["userName"].ToString() : "Viewer";
        string rewardId = args.ContainsKey("rewardId") ? args["rewardId"].ToString() : "";
        string redemptionId = args.ContainsKey("redemptionId") ? args["redemptionId"].ToString() : "";
        
        string input = args.ContainsKey("rawInput") ? args["rawInput"].ToString().Trim() : "";

        InitDB();
        int restante = 0;

        // 1. VALIDAÇÃO DE INPUT
        int numeroSprit = 0;
        int[] validSprits = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
        if (!int.TryParse(input, out numeroSprit) || Array.IndexOf(validSprits, numeroSprit) < 0) 
        {
            CancelarRedemption(rewardId, redemptionId, $"@{userName}, escolhe um número de Elemental válido (ex: 1 a 10, 12 a 25)!");
            return true;
        }

        // 2. TRANCA DE CAÇA ATIVA
        bool cacaAtiva = CPH.GetGlobalVar<bool>("cacaAtiva");
        if (cacaAtiva)
        {
            CancelarRedemption(rewardId, redemptionId, $"@{userName}, não podes usar Habilidades enquanto um Elemental estiver no ecrã!");
            return true;
        }

        // 3. TRANCA DE SIMULTÂNEO (Baseada em tempo para evitar trancas permanentes)
        long ultimoSpritTempo = CPH.GetGlobalVar<long>("ultimoSpritTempo");
        if (ultimoSpritTempo > 0)
        {
            TimeSpan tempoDecorrido = new TimeSpan(DateTime.Now.Ticks - ultimoSpritTempo);
            if (tempoDecorrido.TotalSeconds < 10.0)
            {
                double segundosRestantes = 10.0 - tempoDecorrido.TotalSeconds;
                CancelarRedemption(rewardId, redemptionId, string.Format("@{0}, aguarda {1:F1}s para usar outro Elemental!", userName, segundosRestantes));
                return true;
            }
        }

        // 4. TRANCA DE LIMITE MÁXIMO DO ROUND
        int spritsUsados = CPH.GetGlobalVar<int>("spritsUsados");
        if (spritsUsados >= 2)
        {
            CancelarRedemption(rewardId, redemptionId, $"@{userName}, limite de 2 Habilidades por spawn atingido!");
            return true;
        }

        // 4.5. TRANCA DO ELEMENTAL DE FOGO ATIVO (COOLDOWN DE 1 HORA)
        if (numeroSprit == 3)
        {
            bool spritFogoAtivo = CPH.GetGlobalVar<bool>("spritFogoAtivo");
            if (spritFogoAtivo)
            {
                long ultimoFogoTempo = CPH.GetGlobalVar<long>("ultimoFogoTempo");
                if (ultimoFogoTempo > 0)
                {
                    TimeSpan tempoPassado = new TimeSpan(DateTime.Now.Ticks - ultimoFogoTempo);
                    if (tempoPassado.TotalMinutes < 60.0)
                    {
                        double minutosRestantes = 60.0 - tempoPassado.TotalMinutes;
                        CancelarRedemption(rewardId, redemptionId, string.Format("@{0}, Elemental de Fogo já ativo! Restam {1}m. Pontos devolvidos.", userName, (int)minutosRestantes));
                        return true;
                    }
                    else
                    {
                        CPH.SetGlobalVar("spritFogoAtivo", false);
                    }
                }
            }
        }

        // TRANCA MUTUAMENTE EXCLUSIVA: DEMON E KING
        if (numeroSprit == 7)
        {
            bool spritKingAtivo = CPH.GetGlobalVar<bool>("spritKingAtivo");
            if (spritKingAtivo)
            {
                int llamaSegredo = CPH.GetGlobalVar<int>("spritLlamaSegredo");
                if (llamaSegredo > 0)
                {
                    CancelarRedemption(rewardId, redemptionId, $"@{userName}, esse Elemental entra em conflito com uma Habilidade misteriosa que já está ativa neste spawn! Pontos devolvidos.");
                }
                else
                {
                    CancelarRedemption(rewardId, redemptionId, $"@{userName}, não podes usar o Demónio porque o Rei já está ativo!");
                }
                return true;
            }
        }
        else if (numeroSprit == 9)
        {
            bool spritDemonAtivo = CPH.GetGlobalVar<bool>("spritDemonAtivo");
            if (spritDemonAtivo)
            {
                int llamaSegredo = CPH.GetGlobalVar<int>("spritLlamaSegredo");
                if (llamaSegredo > 0)
                {
                    CancelarRedemption(rewardId, redemptionId, $"@{userName}, esse Elemental entra em conflito com uma Habilidade misteriosa que já está ativa neste spawn! Pontos devolvidos.");
                }
                else
                {
                    CancelarRedemption(rewardId, redemptionId, $"@{userName}, não podes usar o Rei porque o Demónio já está ativo!");
                }
                return true;
            }
        }

        // LIMITES DE ELEMENTAIS ATIVOS POR SPAWN (MÁXIMO 2)
        if (numeroSprit == 1)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritAguaUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Água ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 14)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritAuraUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Aura ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 7)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritDemonUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Demónio ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 9)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritKingUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Rei ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 8)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritPunkUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Punk ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 12)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritPeixeUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Peixoto ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 13)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritAtacanteUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Atacante ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 17)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritVentoUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais de Vento ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }
        else if (numeroSprit == 23)
        {
            string currentUsers = CPH.GetGlobalVar<string>("spritPeelyUser") ?? "";
            string[] parts = currentUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                CancelarRedemption(rewardId, redemptionId, $"@{userName}, o limite de 2 Elementais Peely ativos já foi atingido! Pontos devolvidos.");
                return true;
            }
        }

        // 5. VERIFICAÇÃO E DEDUÇÃO DE INVENTÁRIO (USANDO ELEMENTAIS CAPTURADOS DA COLEÇÃO)
        string prefix = numeroSprit + "_";
        string foundElementalId = null;
        int quantidadePossuida = 0;
        bool lockedInTrade = false;

        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();

            // 1. Primeiro Passo: Procurar por duplicados (quantidade >= 2) de Normal para Quack (1 a 8)
            for (int varId = 1; varId <= 8; varId++)
            {
                string checkId = prefix + varId;
                
                int total = 0;
                using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@eid", checkId);
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) total = Convert.ToInt32(res);
                }

                if (total >= 2)
                {
                    int disponivel = ObterQuantidadeDisponivel(con, userId, checkId);
                    if (disponivel > 0)
                    {
                        foundElementalId = checkId;
                        quantidadePossuida = total;
                        break;
                    }
                    else
                    {
                        lockedInTrade = true;
                    }
                }
            }

            // 2. Segundo Passo: Se não encontrou duplicados, procurar por cópias únicas (quantidade == 1) de Normal para Quack (1 a 8)
            if (string.IsNullOrEmpty(foundElementalId))
            {
                for (int varId = 1; varId <= 8; varId++)
                {
                    string checkId = prefix + varId;
                    
                    int total = 0;
                    using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        cmd.Parameters.AddWithValue("@eid", checkId);
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value) total = Convert.ToInt32(res);
                    }

                    if (total == 1)
                    {
                        int disponivel = ObterQuantidadeDisponivel(con, userId, checkId);
                        if (disponivel > 0)
                        {
                            foundElementalId = checkId;
                            quantidadePossuida = total;
                            break;
                        }
                        else
                        {
                            lockedInTrade = true;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(foundElementalId))
            {
                if (lockedInTrade)
                {
                    CancelarRedemption(rewardId, redemptionId, string.Format("@{0}, o teu Elemental de {1} está bloqueado numa troca ativa!", userName, GetNomeElemento(numeroSprit)));
                }
                else
                {
                    CancelarRedemption(rewardId, redemptionId, string.Format("@{0}, não tens nenhum Elemental de {1} para gastar!", userName, GetNomeElemento(numeroSprit)));
                }
                return true;
            }

            // Decrementar a quantidade na coleção
            if (quantidadePossuida == 1)
            {
                using (var cmd = new SQLiteCommand("DELETE FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@eid", foundElementalId);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade - 1 WHERE user_id=@uid AND elemental_id=@eid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@eid", foundElementalId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =========================================================================
        // SUCESSO: ITEM CONSUMIDO
        // =========================================================================
        CPH.SetGlobalVar("spritsUsados", spritsUsados + 1);
        
        // REGISTO CRUCIAL: Guarda o exato milissegundo em que este Sprit foi ativado
        CPH.SetGlobalVar("ultimoSpritTempo", DateTime.Now.Ticks);

        string nomeElemental = string.Format("Elemental {0}", numeroSprit);
        if (numeroSprit == 1)
        {
            nomeElemental = "Elemental de Água";
            CPH.SetGlobalVar("spritAguaAtivo", true);
            string currentUsers = CPH.GetGlobalVar<string>("spritAguaUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritAguaUser", currentUsers);

            // Verificar bónus de coleção de água completa
            bool temTodosAgua = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosAgua = VerificarColecaoCompleta(con, userId, 1);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de agua: " + ex.Message);
            }

            if (temTodosAgua)
            {
                CPH.SetGlobalVar("spritAguaSuper", true);
            }
        }
        else if (numeroSprit == 2)
        {
            nomeElemental = "Elemental de Terra";
            CPH.SetGlobalVar("spritTerraAtivo", true);

            // Verificar bónus de coleção de terra completa
            bool temTodosTerra = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosTerra = VerificarColecaoCompleta(con, userId, 2);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de terra: " + ex.Message);
            }

            if (temTodosTerra)
            {
                CPH.SetGlobalVar("spritTerraSuper", true);
            }
        }
        else if (numeroSprit == 3)
        {
            nomeElemental = "Elemental de Fogo";
            CPH.SetGlobalVar("spritFogoAtivo", true);
            CPH.SetGlobalVar("ultimoFogoTempo", DateTime.Now.Ticks);
            var cph = CPH;
            new System.Threading.Thread(() => {
                cph.RunAction("Elementais - Fogo Timer");
            }).Start();
        }
        else if (numeroSprit == 4)
        {
            nomeElemental = "Elemental de Pato";
            CPH.SetGlobalVar("spritPatoAtivo", true);

            // Verificar bónus de coleção de pato completa
            bool temTodosPato = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosPato = VerificarColecaoCompleta(con, userId, 4);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de pato: " + ex.Message);
            }

            if (temTodosPato)
            {
                CPH.SetGlobalVar("spritPatoSuper", true);
            }
        }
        else if (numeroSprit == 5)
        {
            nomeElemental = "Elemental de Fantasma";
            CPH.SetGlobalVar("spritGhostAtivo", true);
        }
        else if (numeroSprit == 6)
        {
            nomeElemental = "Elemental dos Sonhos";
            CPH.SetGlobalVar("spritSleepyAtivo", true);

            // Verificar bónus de coleção de sonhos completa
            bool temTodosSonhos = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosSonhos = VerificarColecaoCompleta(con, userId, 6);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de sonhos: " + ex.Message);
            }

            int increment = temTodosSonhos ? 2 : 1;
            int spritSleepyCount = CPH.GetGlobalVar<int>("spritSleepyCount");
            CPH.SetGlobalVar("spritSleepyCount", spritSleepyCount + increment);
            
            string spritSleepyUsers = CPH.GetGlobalVar<string>("spritSleepyUsers") ?? "";
            if (!string.IsNullOrEmpty(spritSleepyUsers)) spritSleepyUsers += ",";
            spritSleepyUsers += userName;
            CPH.SetGlobalVar("spritSleepyUsers", spritSleepyUsers);

            if (temTodosSonhos)
            {
                CPH.SetGlobalVar("spritSleepySuper", true);
            }
        }
        else if (numeroSprit == 7)
        {
            nomeElemental = "Elemental de Demónio";
            CPH.SetGlobalVar("spritDemonAtivo", true);
            string currentUsers = CPH.GetGlobalVar<string>("spritDemonUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritDemonUser", currentUsers);

            // Verificar bónus de coleção de demónio completa
            bool temTodosDemon = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosDemon = VerificarColecaoCompleta(con, userId, 7);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de demonio: " + ex.Message);
            }

            if (temTodosDemon)
            {
                CPH.SetGlobalVar("spritDemonSuper", true);
            }
        }
        else if (numeroSprit == 8)
        {
            nomeElemental = "Elemental de Punk";
            CPH.SetGlobalVar("spritPunkAtivo", true);
            string currentUsers = CPH.GetGlobalVar<string>("spritPunkUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritPunkUser", currentUsers);

            string currentIds = CPH.GetGlobalVar<string>("spritPunkUserId") ?? "";
            if (!string.IsNullOrEmpty(currentIds)) currentIds += ",";
            currentIds += userId;
            CPH.SetGlobalVar("spritPunkUserId", currentIds);

            // Verificar bónus de coleção de punk completa
            bool temTodosPunk = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosPunk = VerificarColecaoCompleta(con, userId, 8);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de punk: " + ex.Message);
            }

            if (temTodosPunk)
            {
                CPH.SetGlobalVar("spritPunkSuper", true);
            }
        }
        else if (numeroSprit == 9)
        {
            nomeElemental = "Elemental de Rei";
            CPH.SetGlobalVar("spritKingAtivo", true);
            string currentUsers = CPH.GetGlobalVar<string>("spritKingUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritKingUser", currentUsers);

            string currentIds = CPH.GetGlobalVar<string>("spritKingUserId") ?? "";
            if (!string.IsNullOrEmpty(currentIds)) currentIds += ",";
            currentIds += userId;
            CPH.SetGlobalVar("spritKingUserId", currentIds);
        }
        else if (numeroSprit == 14)
        {
            nomeElemental = "Elemental de Aura";
            CPH.SetGlobalVar("spritAuraAtivo", true);
            string currentUsers = CPH.GetGlobalVar<string>("spritAuraUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritAuraUser", currentUsers);

            string currentIds = CPH.GetGlobalVar<string>("spritAuraUserId") ?? "";
            if (!string.IsNullOrEmpty(currentIds)) currentIds += ",";
            currentIds += userId;
            CPH.SetGlobalVar("spritAuraUserId", currentIds);
        }
        else if (numeroSprit == 15)
        {
            nomeElemental = "Elemental de Boss";
            CPH.SetGlobalVar("spritBossAtivo", true);
            CPH.SetGlobalVar("spritBossUser", userName);
            CPH.SetGlobalVar("spritBossUserId", userId);

            // Verificar bónus de coleção de boss completa
            bool temTodosBoss = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosBoss = VerificarColecaoCompleta(con, userId, 15);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de boss: " + ex.Message);
            }

            if (temTodosBoss)
            {
                CPH.SetGlobalVar("spritBossSuper", true);
            }
        }
        else if (numeroSprit == 10)
        {
            nomeElemental = "Elemental de Ponto Zero";
            string chosenElemId = null;

            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();

                // Procurar último capturado elegível (sucesso=1, não-custom, não-burntpeanut)
                string queryLanc = "SELECT elemental_id FROM lancamentos WHERE user_id=@uid AND sucesso=1 ORDER BY id DESC";
                using (var cmd = new SQLiteCommand(queryLanc, con))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string eid = reader["elemental_id"].ToString();
                            if (eid.StartsWith("u_") || eid.StartsWith("11_")) continue;

                            string[] partesEid = eid.Split('_');
                            if (partesEid.Length > 1)
                            {
                                int esp = int.Parse(partesEid[0]);
                                int v = int.Parse(partesEid[1]);
                                bool hasCube = SupportsCube(esp);
                                bool hasHolo = SupportsHolofoil(esp);
                                bool hasGem = SupportsGem(esp);
                                int maxV = hasGem ? 7 : (hasCube ? 6 : (hasHolo ? 5 : 4));
                                if (v >= maxV) continue;
                            }

                            // Verificar se o jogador ainda possui este bicho
                            using (var cmdCheck = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid AND quantidade > 0", con))
                            {
                                cmdCheck.Parameters.AddWithValue("@uid", userId);
                                cmdCheck.Parameters.AddWithValue("@eid", eid);
                                object qtyObj = cmdCheck.ExecuteScalar();
                                if (qtyObj != null && qtyObj != DBNull.Value && Convert.ToInt32(qtyObj) > 0)
                                {
                                    chosenElemId = eid;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(chosenElemId))
                {
                    // Devolver o elemental consumido na BD
                    using (var cmdRefund = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@uid, @eid, 0)", con))
                    {
                        cmdRefund.Parameters.AddWithValue("@uid", userId);
                        cmdRefund.Parameters.AddWithValue("@eid", foundElementalId);
                        cmdRefund.ExecuteNonQuery();
                    }
                    using (var cmdAdd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@uid AND elemental_id=@eid", con))
                    {
                        cmdAdd.Parameters.AddWithValue("@uid", userId);
                        cmdAdd.Parameters.AddWithValue("@eid", foundElementalId);
                        cmdAdd.ExecuteNonQuery();
                    }

                    CancelarRedemption(rewardId, redemptionId, string.Format("@{0}, não tens nenhum elemental elegível recentemente capturado no teu inventário para evoluir!", userName));
                    return true;
                }

                // Calcular ID promovido
                string[] partes = chosenElemId.Split('_');
                int especie = int.Parse(partes[0]);
                int variante = int.Parse(partes[1]);
                int novaVariante = variante + 1;

                bool temCube = SupportsCube(especie);
                bool temHolofoil = SupportsHolofoil(especie);
                bool temGem = SupportsGem(especie);
                
                while (novaVariante <= 7)
                {
                    if (novaVariante == 5 && !temHolofoil) { novaVariante++; continue; }
                    if (novaVariante == 6 && !temCube) { novaVariante++; continue; }
                    if (novaVariante == 7 && !temGem) { novaVariante++; continue; }
                    break;
                }

                int maxVariantePermitida = temGem ? 7 : (temCube ? 6 : (temHolofoil ? 5 : 4));

                if (novaVariante > maxVariantePermitida)
                {
                    // Devolver o elemental consumido na BD
                    using (var cmdRefund = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@uid, @eid, 0)", con))
                    {
                        cmdRefund.Parameters.AddWithValue("@uid", userId);
                        cmdRefund.Parameters.AddWithValue("@eid", foundElementalId);
                        cmdRefund.ExecuteNonQuery();
                    }
                    using (var cmdAdd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@uid AND elemental_id=@eid", con))
                    {
                        cmdAdd.Parameters.AddWithValue("@uid", userId);
                        cmdAdd.Parameters.AddWithValue("@eid", foundElementalId);
                        cmdAdd.ExecuteNonQuery();
                    }

                    CancelarRedemption(rewardId, redemptionId, string.Format("@{0}, o teu {1} já está no nível máximo!", userName, ObterNomeBichoPorId(chosenElemId)));
                    return true;
                }

                string newElemId = string.Format("{0}_{1}", especie, novaVariante);
                string nomeAntigo = ObterNomeBichoPorId(chosenElemId);
                string nomeNovo = ObterNomeBichoPorId(newElemId);

                // Executar a transação de substituição no inventário
                using (var trans = con.BeginTransaction())
                {
                    try
                    {
                        // 1. Remover 1x do antigo
                        using (var cmdSub = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade - 1 WHERE user_id=@uid AND elemental_id=@eid", con, trans))
                        {
                            cmdSub.Parameters.AddWithValue("@uid", userId);
                            cmdSub.Parameters.AddWithValue("@eid", chosenElemId);
                            cmdSub.ExecuteNonQuery();
                        }
                        using (var cmdDel = new SQLiteCommand("DELETE FROM capturas WHERE user_id=@uid AND elemental_id=@eid AND quantidade <= 0", con, trans))
                        {
                            cmdDel.Parameters.AddWithValue("@uid", userId);
                            cmdDel.Parameters.AddWithValue("@eid", chosenElemId);
                            cmdDel.ExecuteNonQuery();
                        }

                        // 2. Adicionar 1x do novo
                        using (var cmdIns = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@uid, @newEid, 0)", con, trans))
                        {
                            cmdIns.Parameters.AddWithValue("@uid", userId);
                            cmdIns.Parameters.AddWithValue("@newEid", newElemId);
                            cmdIns.ExecuteNonQuery();
                        }
                        using (var cmdAdd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@uid AND elemental_id=@newEid", con, trans))
                        {
                            cmdAdd.Parameters.AddWithValue("@uid", userId);
                            cmdAdd.Parameters.AddWithValue("@newEid", newElemId);
                            cmdAdd.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        
                        // Devolver o elemental consumido na BD
                        using (var cmdRefund = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@uid, @eid, 0)", con))
                        {
                            cmdRefund.Parameters.AddWithValue("@uid", userId);
                            cmdRefund.Parameters.AddWithValue("@eid", foundElementalId);
                            cmdRefund.ExecuteNonQuery();
                        }
                        using (var cmdAdd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + 1 WHERE user_id=@uid AND elemental_id=@eid", con))
                        {
                            cmdAdd.Parameters.AddWithValue("@uid", userId);
                            cmdAdd.Parameters.AddWithValue("@eid", foundElementalId);
                            cmdAdd.ExecuteNonQuery();
                        }

                        CancelarRedemption(rewardId, redemptionId, string.Format("@{0}, erro ao processar a evolução do elemental: {1}", userName, ex.Message));
                        return true;
                    }
                }

                // Sucesso: Anunciar evolução no chat Twitch
                CPH.SendMessage(string.Format("🌌 [PONTO ZERO] O elemental {0} de @{1} foi transformado pelo Ponto Zero e evoluiu para {2}! ☄️", nomeAntigo, userName, nomeNovo));

                // Escrever estado para efeitos visuais na overlay (Sprit 10)
                EscreverEstado(string.Format("SPRIT;10;{0}", userName));

                // Exportar site em background
                var cph = CPH;
                new System.Threading.Thread(() => {
                    System.Threading.Thread.Sleep(8000);
                    cph.RunAction("Elementais - Exportar Site", true);
                }).Start();

                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionFulfill(rewardId, redemptionId);

                return true;
            }
        }
        else if (numeroSprit == 13)
        {
            nomeElemental = "Elemental Atacante";
            CPH.SetGlobalVar("spritAtacanteAtivo", true);
            string currentUsers = CPH.GetGlobalVar<string>("spritAtacanteUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritAtacanteUser", currentUsers);

            string currentIds = CPH.GetGlobalVar<string>("spritAtacanteUserId") ?? "";
            if (!string.IsNullOrEmpty(currentIds)) currentIds += ",";
            currentIds += userId;
            CPH.SetGlobalVar("spritAtacanteUserId", currentIds);

            // Verificar bónus de coleção de atacante completa
            bool temTodosAtacante = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosAtacante = VerificarColecaoCompleta(con, userId, 13);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de atacante: " + ex.Message);
            }

            if (temTodosAtacante)
            {
                string superIds = CPH.GetGlobalVar<string>("spritAtacanteSuper") ?? "";
                if (!string.IsNullOrEmpty(superIds)) superIds += ",";
                superIds += userId;
                CPH.SetGlobalVar("spritAtacanteSuper", superIds);
            }
        }
        else if (numeroSprit == 12)
        {
            nomeElemental = "Elemental de Peixoto";
            CPH.SetGlobalVar("spritPeixeAtivo", true);
            string currentUsers = CPH.GetGlobalVar<string>("spritPeixeUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritPeixeUser", currentUsers);

            string currentIds = CPH.GetGlobalVar<string>("spritPeixeUserId") ?? "";
            if (!string.IsNullOrEmpty(currentIds)) currentIds += ",";
            currentIds += userId;
            CPH.SetGlobalVar("spritPeixeUserId", currentIds);

            // Verificar bónus de coleção de peixoto completa
            bool temTodosPeixe = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosPeixe = VerificarColecaoCompleta(con, userId, 12);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de peixoto: " + ex.Message);
            }

            if (temTodosPeixe)
            {
                CPH.SetGlobalVar("spritPeixeSuper", true);
            }
        }
        else if (numeroSprit == 17)
        {
            nomeElemental = "Elemental de Vento";
            CPH.SetGlobalVar("spritVentoAtivo", true);

            string currentUsers = CPH.GetGlobalVar<string>("spritVentoUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritVentoUser", currentUsers);

            string currentIds = CPH.GetGlobalVar<string>("spritVentoUserId") ?? "";
            if (!string.IsNullOrEmpty(currentIds)) currentIds += ",";
            currentIds += userId;
            CPH.SetGlobalVar("spritVentoUserId", currentIds);

            // Verificar bónus de coleção de vento completa
            bool temTodosVento = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosVento = VerificarColecaoCompleta(con, userId, 17);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de vento: " + ex.Message);
            }

            if (temTodosVento)
            {
                string superIds = CPH.GetGlobalVar<string>("spritVentoSuper") ?? "";
                if (!string.IsNullOrEmpty(superIds)) superIds += ",";
                superIds += userId;
                CPH.SetGlobalVar("spritVentoSuper", superIds);
            }
        }
        else if (numeroSprit == 23)
        {
            nomeElemental = "Elemental Peely";
            CPH.SetGlobalVar("spritPeelyAtivo", true);

            string currentUsers = CPH.GetGlobalVar<string>("spritPeelyUser") ?? "";
            if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
            currentUsers += userName;
            CPH.SetGlobalVar("spritPeelyUser", currentUsers);

            string currentIds = CPH.GetGlobalVar<string>("spritPeelyUserId") ?? "";
            if (!string.IsNullOrEmpty(currentIds)) currentIds += ",";
            currentIds += userId;
            CPH.SetGlobalVar("spritPeelyUserId", currentIds);

            // Verificar bónus de coleção de Peely completa
            bool temTodosPeely = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosPeely = VerificarColecaoCompleta(con, userId, 23);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de peely: " + ex.Message);
            }

            if (temTodosPeely)
            {
                CPH.SetGlobalVar("spritPeelySuper", true);
            }
        }
        else if (numeroSprit == 18)
        {
            nomeElemental = "Elemental Seven";
            AtivarEfeitoElemental(18, userId, userName, caminhoBD);

            int vIdx = CPH.GetGlobalVar<int>("spritSevenTargetVariantIndex");
            string vNameFormatted = GetVariantNameFormatted(vIdx);
            bool isSuper = CPH.GetGlobalVar<bool>("spritSevenSuper");

            if (isSuper)
            {
                CPH.SendMessage(string.Format("⚡ @{0} usou [Elemental Seven] [SUPER]! O número 7 invocou um elemental do passado com upgrade duplo de variante [{1}]! (Restantes: {2})", userName, vNameFormatted, quantidadePossuida - 1));
            }
            else
            {
                CPH.SendMessage(string.Format("⚡ @{0} usou [Elemental Seven]! O número 7 invocou um elemental do passado com a variante [{1}]! (Restantes: {2})", userName, vNameFormatted, quantidadePossuida - 1));
            }
        }
        else if (numeroSprit == 22)
        {
            nomeElemental = "Elemental Llama";

            // Verificar se o jogador tem a coleção completa da Llama (Super Llama)
            bool temTodosLlama = false;
            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    temTodosLlama = VerificarColecaoCompleta(con, userId, 22);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de llama: " + ex.Message);
            }

            // Construir lista de candidatos elegíveis
            List<int> candidatos = new List<int>();
            if (temTodosLlama)
            {
                // Super Llama: Apenas Lendários e Míticos elegíveis
                candidatos.AddRange(new int[] { 6, 8, 10, 15, 16, 18, 23 });
            }
            else
            {
                // Llama Normal: Todos os elementais ativos elegíveis
                candidatos.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 23 });
            }

            // Filtrar candidatos com base em trancas ativas
            List<int> elegiveis = new List<int>();
            foreach (int cId in candidatos)
            {
                if (cId == 3 && CPH.GetGlobalVar<bool>("spritFogoAtivo")) continue;
                if (cId == 7 && CPH.GetGlobalVar<bool>("spritKingAtivo")) continue;
                if (cId == 9 && CPH.GetGlobalVar<bool>("spritDemonAtivo")) continue;
                
                if (cId == 13)
                {
                    string u = CPH.GetGlobalVar<string>("spritAtacanteUser") ?? "";
                    if (u.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length >= 2) continue;
                }
                if (cId == 17)
                {
                    string u = CPH.GetGlobalVar<string>("spritVentoUser") ?? "";
                    if (u.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length >= 2) continue;
                }
                if (cId == 23)
                {
                    string u = CPH.GetGlobalVar<string>("spritPeelyUser") ?? "";
                    if (u.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length >= 2) continue;
                }

                elegiveis.Add(cId);
            }

            if (elegiveis.Count == 0) elegiveis.Add(1); // Fallback de segurança para Água

            Random rndLlama = new Random();
            int chosenSprit = elegiveis[rndLlama.Next(elegiveis.Count)];

            // Ativar o efeito do elemental sorteado
            AtivarEfeitoElemental(chosenSprit, userId, userName, caminhoBD);

            if (chosenSprit == 16) // Grim Reaper
            {
                CPH.SendMessage(string.Format("🦙 @{0} usou [Elemental Llama]! A Llama libertou o Ceifeiro Grim! 💀", userName));
                return ExecutarGrimReaper(userId, userName, rewardId, redemptionId, quantidadePossuida, spritsUsados, caminhoBD);
            }
            else if (chosenSprit == 3) // Fogo
            {
                CPH.SendMessage(string.Format("🦙 @{0} usou [Elemental Llama]! A Llama ativou o modo FOGO! Spawns rápidos a cada 5m ativos durante 1h! 🔥 (Restantes: {1})", userName, quantidadePossuida - 1));
            }
            else
            {
                // Guardar segredo para revelar no Spawn
                CPH.SetGlobalVar("spritLlamaSegredo", chosenSprit);
                CPH.SetGlobalVar("spritLlamaUser", userName);
                CPH.SetGlobalVar("spritLlamaUserId", userId);

                if (temTodosLlama)
                {
                    CPH.SendMessage(string.Format("🦙 @{0} usou [Elemental Llama] [SUPER]! Um poder LENDÁRIO/MÍTICO misterioso foi invocado para o próximo spawn... 🎲 (Restantes: {1})", userName, quantidadePossuida - 1));
                }
                else
                {
                    CPH.SendMessage(string.Format("🦙 @{0} usou [Elemental Llama]! Um poder elemental misterioso foi invocado para o próximo spawn... 🎲 (Restantes: {1})", userName, quantidadePossuida - 1));
                }
            }
        }
        else if (numeroSprit == 16)
        {
            return ExecutarGrimReaper(userId, userName, rewardId, redemptionId, quantidadePossuida, spritsUsados, caminhoBD);
        }

        EscreverEstado(string.Format("SPRIT;{0};{1}", numeroSprit, userName));
        
        string msg = "";
        restante = quantidadePossuida - 1;
        string nomeLegivelGasto = ObterNomeBichoPorId(foundElementalId);

        if (numeroSprit == 1)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritAguaSuper");
            if (isSuper)
            {
                msg = string.Format("💧 @{0} usou [{1}] [SUPER]! Próximo spawn: captura -60% para os outros. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("💧 @{0} usou [{1}]! Próximo spawn: captura -40% para os outros. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 2)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritTerraSuper");
            if (isSuper)
            {
                msg = string.Format("🌍 @{0} usou [{1}] [SUPER]! Próximo spawn: lendário ou mítico. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("🌍 @{0} usou [{1}]! Próximo spawn: épico ou superior. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 3)
        {
            msg = string.Format("🔥 @{0} usou [{1}]! Spawns a cada 5m por 1h. (Restantes: {2})", userName, nomeLegivelGasto, restante);
        }
        else if (numeroSprit == 4)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritPatoSuper");
            if (isSuper)
            {
                msg = string.Format("🦆 @{0} usou [{1}] [SUPER]! Próximo spawn: Gummy ou superior. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("🦆 @{0} usou [{1}]! Próximo spawn: Gold ou superior. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 5)
        {
            msg = string.Format("👻 @{0} usou [{1}]! Próximo spawn: mistério/fumaça. (Restantes: {2})", userName, nomeLegivelGasto, restante);
        }
        else if (numeroSprit == 6)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritSleepySuper");
            if (isSuper)
            {
                msg = string.Format("💤 @{0} usou [{1}] [SUPER]! Vai adormecer 2 pessoas no sorteio. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("💤 @{0} usou [{1}]! Vai adormecer 1 pessoa no sorteio. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 7)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritDemonSuper");
            if (isSuper)
            {
                msg = string.Format("😈 @{0} usou [{1}] [SUPER]! Apenas @{0} poderá usar Master Ball e Ultra Ball. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("😈 @{0} usou [{1}]! Apenas @{0} poderá usar Master Ball. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 8)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritPunkSuper");
            if (isSuper)
            {
                msg = string.Format("🎸 @{0} usou [{1}] [SUPER]! Vai roubar de 2 pessoas diferentes no sorteio. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("🎸 @{0} usou [{1}]! Vai roubar um elemental de quem participar na próxima caçada. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 9)
        {
            msg = string.Format("👑 @{0} usou [{1}]! Apenas @{0} poderá tentar apanhar (até 5 bolas). (Restantes: {2})", userName, nomeLegivelGasto, restante);
        }
        else if (numeroSprit == 12)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritPeixeSuper");
            if (isSuper)
            {
                msg = string.Format("🎣 @{0} usou [{1}] [SUPER]! Se conseguires capturar o próximo elemental, recebes 2 elementais normais aleatórios. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("🎣 @{0} usou [{1}]! Se conseguires capturar o próximo elemental, recebes um elemental normal aleatório. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 13)
        {
            string superIds = CPH.GetGlobalVar<string>("spritAtacanteSuper") ?? "";
            bool isSuper = superIds.Contains(userId);
            if (isSuper)
            {
                msg = string.Format("⚽ @{0} usou [{1}] [SUPER]! Terá direito a até 2 ressaltos extras gratuitos se os remates falharem! (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("⚽ @{0} usou [{1}]! Terá direito a um ressalto extra gratuito se o seu remate falhar! (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 14)
        {
            msg = string.Format("✨ @{0} usou [{1}]! Próximo spawn: passará à frente na fila do sorteio. (Restantes: {2})", userName, nomeLegivelGasto, restante);
        }
        else if (numeroSprit == 15)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritBossSuper");
            if (isSuper)
            {
                msg = string.Format("👑 @{0} usou [{1}] [SUPER]! Próximo spawn: garante variante Galaxy ou superior, com -60% de captura. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("👑 @{0} usou [{1}]! Próximo spawn: garante variante Gummy ou superior, mas todas as taxas de captura caem 60%. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 17)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritVentoSuper");
            if (isSuper)
            {
                msg = string.Format("🌪️ @{0} usou [{1}] [SUPER]! Se não ficares em 1º, o vento re-baralha a fila (até 2x). (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("🌪️ @{0} usou [{1}]! Se não ficares em 1º, o vento re-baralha a fila. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 23)
        {
            bool isSuper = CPH.GetGlobalVar<bool>("spritPeelySuper");
            if (isSuper)
            {
                msg = string.Format("🍌 @{0} usou [{1}] [SUPER]! As 2 primeiras pessoas da fila vão pisar na casca e escorregar para último. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
            else
            {
                msg = string.Format("🍌 @{0} usou [{1}]! A primeira pessoa da fila vai pisar na casca e escorregar para último. (Restantes: {2})", userName, nomeLegivelGasto, restante);
            }
        }
        else if (numeroSprit == 22)
        {
            // Mensagem já enviada na invocação do Llama
            return true;
        }
        else
        {
            msg = string.Format("✨ @{0} usou [{1}]! (Restantes: {2})", userName, nomeLegivelGasto, restante);
        }
        CPH.SendMessage(msg);
        if (userName.ToLower() == "manu12321_")
        {
            Random rnd = new Random();
            if (rnd.Next(0, 100) < 60)
            {
                int opt = rnd.Next(0, 3);
                if (opt == 0) CPH.SendMessage("Será que este elemental vai ajudar o @manu12321_ a sair do último lugar? Veremos... 👀");
                else if (opt == 1) CPH.SendMessage("O @manu12321_ gastou um elemental... As probabilidades de correr bem são baixas! 🎲");
                else CPH.SendMessage("Investimento de alto risco do @manu12321_ ao usar este elemental! 📉");
            }
        }
        
        CPH.RunAction("Elementais - Exportar Site", true);

        if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId)) 
            CPH.TwitchRedemptionFulfill(rewardId, redemptionId);

        return true;
    }

    private void CancelarRedemption(string rewardId, string redemptionId, string mensagem)
    {
        if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId)) 
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
        CPH.SendMessage(mensagem);
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

    private static int GetMaxVariantForSpecies(int especieId)
    {
        int max = 4;
        if (SupportsHolofoil(especieId)) max = Math.Max(max, 5);
        if (SupportsCube(especieId)) max = Math.Max(max, 6);
        if (SupportsGem(especieId)) max = Math.Max(max, 7);
        return max;
    }

    private bool VerificarColecaoCompleta(SQLiteConnection con, string userId, int especieId)
    {
        List<string> requiredIds = new List<string> { $"{especieId}_1", $"{especieId}_2", $"{especieId}_3", $"{especieId}_4" };
        if (SupportsHolofoil(especieId)) requiredIds.Add($"{especieId}_5");
        if (SupportsCube(especieId)) requiredIds.Add($"{especieId}_6");
        if (SupportsGem(especieId)) requiredIds.Add($"{especieId}_7");

        string inClause = "'" + string.Join("','", requiredIds) + "'";
        string qCheck = $"SELECT COUNT(DISTINCT elemental_id) FROM capturas WHERE user_id=@uid AND elemental_id IN ({inClause}) AND quantidade > 0";

        using (var cmdCheck = new SQLiteCommand(qCheck, con))
        {
            cmdCheck.Parameters.AddWithValue("@uid", userId);
            object countObj = cmdCheck.ExecuteScalar();
            if (countObj != null && countObj != DBNull.Value)
            {
                int count = Convert.ToInt32(countObj);
                return count == requiredIds.Count;
            }
        }
        return false;
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

            bool peelyAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPeelyAtiva") : CPH.GetGlobalVar<bool>("spritPeelyAtivo");
            bool peelySuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPeelySuper") : CPH.GetGlobalVar<bool>("spritPeelySuper");
            string peelyVal = peelyAtivo ? (peelySuper ? "Super" : "True") : "False";

            bool sevenAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritSevenAtiva") : CPH.GetGlobalVar<bool>("spritSevenAtivo");
            bool sevenSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritSevenSuper") : CPH.GetGlobalVar<bool>("spritSevenSuper");
            string sevenVal = sevenAtivo ? (sevenSuper ? "Super" : "True") : "False";

            string suffix = string.Format("|agua={0};terra={1};fogo={2};pato={3};ghost={4};sleepy={5};demon={6};punk={7};king={8};aura={9};boss={10};peixe={11};atacante={12};vento={13};peely={14};seven={15}",
                aguaVal, terraVal, fogo, patoVal, ghost, sleepyVal, demonVal, punkVal, king, aura, bossVal, peixeVal, atacanteVal, ventoVal, peelyVal, sevenVal);
            File.WriteAllText(caminhoEstado, cmd + suffix);
        }
        catch {}
    }

    private string GetNomeElemento(int num)
    {
        if (num == 1) return "Água";
        if (num == 2) return "Terra";
        if (num == 3) return "Fogo";
        if (num == 4) return "Pato";
        if (num == 5) return "Fantasma";
        if (num == 6) return "Dos Sonhos";
        if (num == 7) return "Demónio";
        if (num == 8) return "Punk";
        if (num == 9) return "Rei";
        if (num == 10) return "Ponto Zero";
        if (num == 12) return "Peixoto";
        if (num == 13) return "Atacante";
        if (num == 14) return "Aura";
        if (num == 15) return "Boss";
        if (num == 16) return "Grim";
        if (num == 17) return "Vento";
        if (num == 18) return "Seven";
        if (num == 22) return "Llama";
        if (num == 23) return "Peely";
        if (num == 24) return "John Wick";
        if (num == 25) return "Ironmouse";
        return "Desconhecido";
    }

    private string GetVariantCode(int index)
    {
        switch (index)
        {
            case 1: return "normal";
            case 2: return "gold";
            case 3: return "gummy";
            case 4: return "galaxy";
            case 5: return "holofoil";
            case 6: return "cube";
            case 7: return "gem";
            case 8: return "quack";
            default: return "normal";
        }
    }

    private void AtivarEfeitoElemental(int chosenSprit, string userId, string userName, string caminhoBD)
    {
        using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            if (chosenSprit == 1) // Água
            {
                CPH.SetGlobalVar("spritAguaAtivo", true);
                string currentUsers = CPH.GetGlobalVar<string>("spritAguaUser") ?? "";
                if (!string.IsNullOrEmpty(currentUsers)) currentUsers += ",";
                currentUsers += userName;
                CPH.SetGlobalVar("spritAguaUser", currentUsers);
                if (VerificarColecaoCompleta(con, userId, 1)) CPH.SetGlobalVar("spritAguaSuper", true);
            }
            else if (chosenSprit == 2) // Terra
            {
                CPH.SetGlobalVar("spritTerraAtivo", true);
                if (VerificarColecaoCompleta(con, userId, 2)) CPH.SetGlobalVar("spritTerraSuper", true);
            }
            else if (chosenSprit == 3) // Fogo
            {
                CPH.SetGlobalVar("spritFogoAtivo", true);
                CPH.SetGlobalVar("ultimoFogoTempo", DateTime.Now.Ticks);
                var cph = CPH;
                new System.Threading.Thread(() => { cph.RunAction("Elementais - Fogo Timer"); }).Start();
            }
            else if (chosenSprit == 4) // Pato
            {
                CPH.SetGlobalVar("spritPatoAtivo", true);
                if (VerificarColecaoCompleta(con, userId, 4)) CPH.SetGlobalVar("spritPatoSuper", true);
            }
            else if (chosenSprit == 5) // Fantasma
            {
                CPH.SetGlobalVar("spritGhostAtivo", true);
            }
            else if (chosenSprit == 6) // Dos Sonhos
            {
                CPH.SetGlobalVar("spritSleepyAtivo", true);
                bool temTodos = VerificarColecaoCompleta(con, userId, 6);
                int inc = temTodos ? 2 : 1;
                CPH.SetGlobalVar("spritSleepyCount", CPH.GetGlobalVar<int>("spritSleepyCount") + inc);
                string users = CPH.GetGlobalVar<string>("spritSleepyUsers") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritSleepyUsers", users);
                if (temTodos) CPH.SetGlobalVar("spritSleepySuper", true);
            }
            else if (chosenSprit == 7) // Demónio
            {
                CPH.SetGlobalVar("spritDemonAtivo", true);
                string users = CPH.GetGlobalVar<string>("spritDemonUser") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritDemonUser", users);
                if (VerificarColecaoCompleta(con, userId, 7)) CPH.SetGlobalVar("spritDemonSuper", true);
            }
            else if (chosenSprit == 8) // Punk
            {
                CPH.SetGlobalVar("spritPunkAtivo", true);
                string users = CPH.GetGlobalVar<string>("spritPunkUser") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritPunkUser", users);
                if (VerificarColecaoCompleta(con, userId, 8)) CPH.SetGlobalVar("spritPunkSuper", true);
            }
            else if (chosenSprit == 9) // Rei
            {
                CPH.SetGlobalVar("spritKingAtivo", true);
                string users = CPH.GetGlobalVar<string>("spritKingUser") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritKingUser", users);
                string ids = CPH.GetGlobalVar<string>("spritKingUserId") ?? "";
                if (!string.IsNullOrEmpty(ids)) ids += ",";
                ids += userId;
                CPH.SetGlobalVar("spritKingUserId", ids);
            }
            else if (chosenSprit == 10) // Ponto Zero
            {
                CPH.SetGlobalVar("spritZeroAtivo", true);
                if (VerificarColecaoCompleta(con, userId, 10)) CPH.SetGlobalVar("spritZeroSuper", true);
            }
            else if (chosenSprit == 12) // Peixoto
            {
                CPH.SetGlobalVar("spritPeixeAtivo", true);
                if (VerificarColecaoCompleta(con, userId, 12)) CPH.SetGlobalVar("spritPeixeSuper", true);
            }
            else if (chosenSprit == 13) // Atacante
            {
                CPH.SetGlobalVar("spritAtacanteAtivo", true);
                string users = CPH.GetGlobalVar<string>("spritAtacanteUser") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritAtacanteUser", users);
                string ids = CPH.GetGlobalVar<string>("spritAtacanteUserId") ?? "";
                if (!string.IsNullOrEmpty(ids)) ids += ",";
                ids += userId;
                CPH.SetGlobalVar("spritAtacanteUserId", ids);
                if (VerificarColecaoCompleta(con, userId, 13)) CPH.SetGlobalVar("spritAtacanteSuper", true);
            }
            else if (chosenSprit == 14) // Aura
            {
                CPH.SetGlobalVar("spritAuraAtivo", true);
                string users = CPH.GetGlobalVar<string>("spritAuraUser") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritAuraUser", users);
                string ids = CPH.GetGlobalVar<string>("spritAuraUserId") ?? "";
                if (!string.IsNullOrEmpty(ids)) ids += ",";
                ids += userId;
                CPH.SetGlobalVar("spritAuraUserId", ids);
            }
            else if (chosenSprit == 15) // Boss
            {
                CPH.SetGlobalVar("spritBossAtivo", true);
                if (VerificarColecaoCompleta(con, userId, 15)) CPH.SetGlobalVar("spritBossSuper", true);
            }
            else if (chosenSprit == 17) // Vento
            {
                CPH.SetGlobalVar("spritVentoAtivo", true);
                string users = CPH.GetGlobalVar<string>("spritVentoUser") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritVentoUser", users);
                string ids = CPH.GetGlobalVar<string>("spritVentoUserId") ?? "";
                if (!string.IsNullOrEmpty(ids)) ids += ",";
                ids += userId;
                CPH.SetGlobalVar("spritVentoUserId", ids);
                if (VerificarColecaoCompleta(con, userId, 17))
                {
                    string superIds = CPH.GetGlobalVar<string>("spritVentoSuper") ?? "";
                    if (!string.IsNullOrEmpty(superIds)) superIds += ",";
                    superIds += userId;
                    CPH.SetGlobalVar("spritVentoSuper", superIds);
                }
            }
            else if (chosenSprit == 18) // Seven
            {
                CPH.SetGlobalVar("spritSevenAtivo", true);
                CPH.SetGlobalVar("spritSevenUser", userName);
                bool temTodos = VerificarColecaoCompleta(con, userId, 18);
                if (temTodos) CPH.SetGlobalVar("spritSevenSuper", true);

                int targetSpecies = 1;
                int targetVariantIndex = 2;
                string targetVariantName = "gold";

                List<KeyValuePair<int, int>> lastCaptures = new List<KeyValuePair<int, int>>();
                using (var cmd = new SQLiteCommand("SELECT elemental_id FROM lancamentos WHERE sucesso = 1 ORDER BY id DESC LIMIT 7", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string elemIdStr = reader.GetString(0);
                            if (!string.IsNullOrEmpty(elemIdStr) && elemIdStr.Contains("_"))
                            {
                                string[] parts = elemIdStr.Split('_');
                                int sId, vId;
                                if (int.TryParse(parts[0], out sId) && int.TryParse(parts[1], out vId))
                                {
                                    lastCaptures.Add(new KeyValuePair<int, int>(sId, vId));
                                }
                            }
                        }
                    }
                }

                if (lastCaptures.Count > 0)
                {
                    Random rndSeven = new Random();
                    var chosenCapture = lastCaptures[rndSeven.Next(lastCaptures.Count)];
                    targetSpecies = chosenCapture.Key;
                    int baseVariant = chosenCapture.Value;
                    int step = temTodos ? 2 : 1;
                    int maxAllowed = GetMaxVariantForSpecies(targetSpecies);
                    targetVariantIndex = Math.Min(maxAllowed, baseVariant + step);
                    targetVariantName = GetVariantCode(targetVariantIndex);
                }

                CPH.SetGlobalVar("spritSevenTargetSpecies", targetSpecies);
                CPH.SetGlobalVar("spritSevenTargetVariantIndex", targetVariantIndex);
                CPH.SetGlobalVar("spritSevenTargetVariantName", targetVariantName);
            }
            else if (chosenSprit == 23) // Peely
            {
                CPH.SetGlobalVar("spritPeelyAtivo", true);
                string users = CPH.GetGlobalVar<string>("spritPeelyUser") ?? "";
                if (!string.IsNullOrEmpty(users)) users += ",";
                users += userName;
                CPH.SetGlobalVar("spritPeelyUser", users);
                string ids = CPH.GetGlobalVar<string>("spritPeelyUserId") ?? "";
                if (!string.IsNullOrEmpty(ids)) ids += ",";
                ids += userId;
                CPH.SetGlobalVar("spritPeelyUserId", ids);
                if (VerificarColecaoCompleta(con, userId, 23)) CPH.SetGlobalVar("spritPeelySuper", true);
            }
        }
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

    private void InitDB()
    {
        Directory.CreateDirectory(@"I:\Twitch\Games\elementais");
        if (!File.Exists(caminhoBD)) SQLiteConnection.CreateFile(caminhoBD);
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS propostas_troca (proposer_id TEXT, proposer_name TEXT, target_id TEXT, target_name TEXT, elem_proposer TEXT, elem_target TEXT, reward_id TEXT, redemption_id TEXT, created_at TEXT)", con)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS historico_trocas (user_id TEXT, username TEXT, parceiro_id TEXT, parceiro_name TEXT, elem_dado TEXT, elem_recebido TEXT, data_troca TEXT, recuperacao_anunciada INT DEFAULT 0)", con)) cmd.ExecuteNonQuery();
        }
    }

    private int ObterQuantidadeDisponivel(SQLiteConnection con, string userId, string elemId)
    {
        int total = 0;
        using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elemId);
            object res = cmd.ExecuteScalar();
            if (res != null && res != DBNull.Value) total = Convert.ToInt32(res);
        }
        if (total == 0) return 0;

        int bloqueadoProponente = 0;
        using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM propostas_troca WHERE proposer_id=@uid AND elem_proposer=@eid", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elemId);
            bloqueadoProponente = Convert.ToInt32(cmd.ExecuteScalar());
        }

        int bloqueadoAlvo = 0;
        using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM propostas_troca WHERE target_id=@uid AND elem_target=@eid", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elemId);
            bloqueadoAlvo = Convert.ToInt32(cmd.ExecuteScalar());
        }

        int disponivel = total - (bloqueadoProponente + bloqueadoAlvo);
        return Math.Max(0, disponivel);
    }

    private string GetVariantNameFormatted(int vIndex)
    {
        switch (vIndex)
        {
            case 1: return "NORMAL";
            case 2: return "GOLD";
            case 3: return "GUMMY";
            case 4: return "GALAXY";
            case 5: return "HOLOFOIL";
            case 6: return "CUBE";
            case 7: return "GEM";
            case 8: return "QUACK";
            default: return "GOLD";
        }
    }

    private bool ExecutarGrimReaper(string userId, string userName, string rewardId, string redemptionId, int quantidadePossuida, int spritsUsados, string caminhoBD)
    {
        string nomeElemental = "Elemental de Grim";
        int restante = quantidadePossuida - 1;
        Dictionary<string, string> nomesPorUser = new Dictionary<string, string>();
        List<KeyValuePair<string, int>> rankingOrdenado = new List<KeyValuePair<string, int>>();

        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            
            string query = @"
                SELECT c.user_id, u.username,
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
                INNER JOIN utilizadores u ON c.user_id = u.user_id
                LEFT JOIN cfg_especies e ON e.id = CAST(SUBSTR(c.elemental_id, 1, INSTR(c.elemental_id, '_') - 1) AS INTEGER)
                LEFT JOIN cfg_pontos p ON e.raridade = p.raridade AND p.variante_id = CAST(SUBSTR(c.elemental_id, INSTR(c.elemental_id, '_') + 1) AS INTEGER)
                WHERE c.quantidade > 0
                GROUP BY c.user_id, u.username
                ORDER BY total_pontos DESC";

            using (var cmd = new SQLiteCommand(query, con))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string uid = reader["user_id"].ToString();
                        string uname = reader["username"].ToString();
                        int pts = Convert.ToInt32(reader["total_pontos"]);

                        nomesPorUser[uid] = uname;
                        rankingOrdenado.Add(new KeyValuePair<string, int>(uid, pts));
                    }
                }
            }
        }

        // Obtém os candidatos do Top 3
        List<string> top3Candidates = new List<string>();
        List<string> top3Names = new List<string>();
        for (int i = 0; i < Math.Min(3, rankingOrdenado.Count); i++)
        {
            top3Candidates.Add(rankingOrdenado[i].Key);
            top3Names.Add(nomesPorUser[rankingOrdenado[i].Key]);
        }

        // Se não houver candidatos válidos no ranking, cancela o resgate
        if (top3Candidates.Count == 0)
        {
            CancelarRedemption(rewardId, redemptionId, $"@{userName}, não há jogadores no ranking para o Grim eliminar!");
            return true;
        }

        // Verificar bónus de coleção de Grim completa (SUPER = 2 vítimas)
        bool grimSuper = false;
        try
        {
            using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                grimSuper = VerificarColecaoCompleta(con, userId, 16);
            }
        }
        catch (Exception ex)
        {
            CPH.LogWarn("[UsarSprite] Erro ao verificar colecao de grim: " + ex.Message);
        }

        int numVitimas = (grimSuper && top3Candidates.Count >= 2) ? 2 : 1;

        // Escolhe vítima(s) aleatória(s) únicas do Top 3
        Random rnd = new Random();
        List<string> victims = new List<string>();
        List<string> candidatesPool = new List<string>(top3Candidates);
        for (int v = 0; v < numVitimas && candidatesPool.Count > 0; v++)
        {
            int idx = rnd.Next(0, candidatesPool.Count);
            victims.Add(candidatesPool[idx]);
            candidatesPool.RemoveAt(idx);
        }

        // Processar cada vítima
        List<string> grimResults = new List<string>();
        string firstVictimName = "";
        string firstChosenElemId = "";
        bool firstResult = true;

        foreach (string victimUserId in victims)
        {
            string victimUserName = nomesPorUser[victimUserId];
            if (firstResult) firstVictimName = victimUserName;

            // Obter os elementais que a vítima tem
            List<string> victimElementals = new List<string>();
            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand("SELECT elemental_id FROM capturas WHERE user_id=@uid AND quantidade > 0", con))
                {
                    cmd.Parameters.AddWithValue("@uid", victimUserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string eid = reader["elemental_id"].ToString();
                            if (!eid.EndsWith("_8"))
                            {
                                victimElementals.Add(eid);
                            }
                        }
                    }
                }
            }

            if (victimElementals.Count == 0)
            {
                grimResults.Add(string.Format("@{0} não tinha elementais!", victimUserName));
                continue;
            }

            // Escolhe um elemental aleatório da vítima para ser deletado
            string chosenElemId = victimElementals[rnd.Next(0, victimElementals.Count)];
            if (firstResult) { firstChosenElemId = chosenElemId; firstResult = false; }

            // Deleta/decrementa na base de dados
            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                int qtd = 0;
                using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", victimUserId);
                    cmd.Parameters.AddWithValue("@eid", chosenElemId);
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) qtd = Convert.ToInt32(res);
                }

                if (qtd == 1)
                {
                    using (var cmd = new SQLiteCommand("DELETE FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
                    {
                        cmd.Parameters.AddWithValue("@uid", victimUserId);
                        cmd.Parameters.AddWithValue("@eid", chosenElemId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else if (qtd > 1)
                {
                    using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade - 1 WHERE user_id=@uid AND elemental_id=@eid", con))
                    {
                        cmd.Parameters.AddWithValue("@uid", victimUserId);
                        cmd.Parameters.AddWithValue("@eid", chosenElemId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            string nomeBicho = chosenElemId;
            try { nomeBicho = ObterNomeBichoPorId(chosenElemId); } catch {}
            grimResults.Add(string.Format("1x {0} do @{1}", nomeBicho, victimUserName));
        }

        // Escrever o estado para o OBS ler e despoletar a roleta e animação (usa a 1ª vítima para a animação)
        EscreverEstado(string.Format("GRIM;{0};{1};{2};{3}", userName, firstVictimName, firstChosenElemId, string.Join(",", top3Names)));
        
        // Consumir o elemental
        CPH.SetGlobalVar("spritsUsados", spritsUsados + 1);
        CPH.SetGlobalVar("ultimoSpritTempo", DateTime.Now.Ticks);
        
        if (grimSuper && numVitimas == 2)
        {
            CPH.SendMessage(string.Format("💀 @{0} libertou o Ceifeiro Grim [SUPER]! Duas almas do Top 3 estão em perigo... (Restantes: {1})", userName, restante));
        }
        else
        {
            CPH.SendMessage(string.Format("💀 @{0} libertou o Ceifeiro Grim! O Top 3 está sob ameaça... (Restantes: {1})", userName, restante));
        }

        var cph = CPH;
        string resultsStr = string.Join(" | ", grimResults);
        bool isGrimSuperFinal = grimSuper && numVitimas == 2;
        new System.Threading.Thread(() => {
            System.Threading.Thread.Sleep(8000);
            cph.RunAction("Elementais - Exportar Site", true);
            if (isGrimSuperFinal)
                cph.SendMessage(string.Format("💀 [GRIM SUPER] O ceifeiro de @{0} ceifou: {1}! 🪦🪦", userName, resultsStr));
            else
                cph.SendMessage(string.Format("💀 [GRIM] O ceifeiro enviado por @{0} eliminou {1}! 🪦", userName, resultsStr));
        }).Start();

        if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId)) 
            CPH.TwitchRedemptionFulfill(rewardId, redemptionId);

        return true;
    }
}