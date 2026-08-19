using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;

public class CPHInline
{
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";
    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";

    public bool Execute()
    {
        while (true)
        {
            // 1. Verifica se o bicho ainda está ativo ou se já foi capturado por alguém
            bool aindaAtiva = CPH.GetGlobalVar<bool>("cacaAtiva");
            if (!aindaAtiva) break; 

            // 2. BLINDAGEM DE FILA: Vai buscar o carimbo do último arremesso
            long ultimoTicks = CPH.GetGlobalVar<long>("ultimoArremessoTempo");
            if (ultimoTicks > 0)
            {
                TimeSpan decorrido = TimeSpan.FromTicks(DateTime.Now.Ticks - ultimoTicks);
                
                // Se uma bola correu nos últimos 9 segundos, a fila ainda está a processar!
                if (decorrido.TotalSeconds < 9) 
                {
                    CPH.LogInfo("[Elementais] Despawn adiado: Existem resgates a ser processados na fila.");
                    System.Threading.Thread.Sleep(10000); // Espera 10 segundos e reavalia o loop
                    continue;
                }
            }

            // 3. Se passou o teste de inatividade e ninguém jogou, o bicho foge de vez!
            CPH.SetGlobalVar("cacaAtiva", false);
            CPH.SetGlobalVar("cacaSpritAguaAtiva", false);
            CPH.SetGlobalVar("cacaSpritAguaUser", "");
            CPH.SetGlobalVar("cacaSpritDemonAtiva", false);
            CPH.SetGlobalVar("cacaSpritDemonUser", "");
            CPH.SetGlobalVar("cacaSpritKingAtiva", false);
            CPH.SetGlobalVar("cacaSpritKingUser", "");
            CPH.SetGlobalVar("cacaSpritKingUserId", "");
            CPH.SetGlobalVar("cacaSpritSleepyAtiva", false);
            CPH.SetGlobalVar("cacaSpritSleepyCount", 0);
            CPH.SetGlobalVar("cacaSpritSleepyUsers", "");
            CPH.SetGlobalVar("cacaSpritAuraAtiva", false);
            CPH.SetGlobalVar("cacaSpritAuraUser", "");
            CPH.SetGlobalVar("cacaSpritAuraUserId", "");
            CPH.SetGlobalVar("cacaSpritAtacanteAtiva", false);
            CPH.SetGlobalVar("cacaSpritAtacanteUser", "");
            CPH.SetGlobalVar("cacaSpritAtacanteUserId", "");
            CPH.SetGlobalVar("cacaSpritAtacanteSuper", "");
            CPH.SetGlobalVar("cacaSpritPeixeAtiva", false);
            CPH.SetGlobalVar("cacaSpritPeixeUser", "");
            CPH.SetGlobalVar("cacaSpritPeixeUserId", "");
            CPH.SetGlobalVar("cacaSpritPeixeSuper", false);
            CPH.SetGlobalVar("cacaSpritVentoAtiva", false);
            CPH.SetGlobalVar("cacaSpritVentoUser", "");
            CPH.SetGlobalVar("cacaSpritVentoUserId", "");
            CPH.SetGlobalVar("cacaSpritVentoSuper", "");
            ProcessarRouboPunk();
            CPH.SetGlobalVar("lobbyAtivo", false);
            CPH.SetGlobalVar("lobbyResolvido", false);
            string nomeDesteElemental = CPH.GetGlobalVar<string>("elementalAtivoNome");
            if (string.IsNullOrEmpty(nomeDesteElemental)) nomeDesteElemental = "Elemental";

            bool cacaSpritGhostAtiva = CPH.GetGlobalVar<bool>("cacaSpritGhostAtiva");
            if (cacaSpritGhostAtiva)
            {
                EscreverEstado("FUGIU;elemental mistério");
                CPH.SendMessage(string.Format("O elemental mistério fugiu! Afinal era um {0}! 🏃💨", nomeDesteElemental));
            }
            else
            {
                EscreverEstado($"FUGIU;{nomeDesteElemental}");
                CPH.SendMessage($"{nomeDesteElemental} fugiu! 🏃💨");
            }
            CPH.RunAction("Elementais - Desativar Caça");

            try
            {
                using (var con = new System.Data.SQLite.SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand("UPDATE utilizadores SET win_streak = 0", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    VerificarVitoriaAlbumCompleto(con);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[Spawn_2] Erro ao processar despawn: " + ex.Message);
            }

            break; // Sai do loop seguro
        }

        return true;
    }

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
                    CPH.SendMessage($"🎸 [ROUBO] @{conjuradorName} tentou roubar @{vitimaName}, mas a sua mala estava vazia!");
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

            CPH.SendMessage(string.Format("🎸 [ROUBO] @{0} roubou 1x {1} do @{2}! 🎒", conjuradorName, nomeBichoRoubado, vitimaName));
        }

        CPH.RunAction("Elementais - Colecao");
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
}