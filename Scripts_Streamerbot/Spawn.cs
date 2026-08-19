using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

public class CPHInline
{
    public class Species
    {
        public string Name { get; set; }
        public int Weight { get; set; }
        public Dictionary<string, string> Files { get; set; }
    }

    public class MissingCard
    {
        public bool IsSpecial { get; set; }
        public int ListIndex { get; set; }
        public int SpeciesId { get; set; }
        public string VariantName { get; set; }
        public int VariantIndex { get; set; }
        public string DbId { get; set; }
        public string FileName { get; set; }
        public string DisplayName { get; set; }
    }

    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";

    private static readonly List<Species> speciesList = new List<Species>
    {
        new Species { Name = "Água", Weight = 55, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Water_Unvault_Ch7S3_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Water_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Water_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Water_Galaxy_ui_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_Water_Holofoil_ui_L.webp" },
            { "gem", "T_Icon_BR_Creature_Sprite_Water_Gem_ui_L.webp" }
        }},
        new Species { Name = "Terra", Weight = 55, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Earth_Ch7S3_UI_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Earth_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Earth_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Earth_Galaxy_ui_L.webp" },
            { "cube", "T_Icon_BR_Creature_Sprite_Earth_Cube_ui_L.webp" },
            { "gem", "T_Icon_BR_Creature_Sprite_Earth_Gem_ui_L.webp" }
        }},
        new Species { Name = "Fogo", Weight = 55, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Fire_Unvault_Ch7S3_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Fire_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Fire_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Fire_Galaxy_ui_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_Fire_Holofoil_ui_L.webp" },
            { "cube", "T_Icon_BR_Creature_Sprite_Fire_Cube_ui_L.webp" }
        }},
        new Species { Name = "Pato", Weight = 30, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Duck_Default_L.webp" },
            { "gold", "T_Icon_BR_Duck_Gold_L.webp" },
            { "gummy", "T_Icon_BR_Duck_Candy_L.webp" }, 
            { "galaxy", "T_Icon_BR_Duck_Galaxy_L.webp" },
            { "gem", "T_Icon_BR_Duck_Gem_L.webp" }
        }},
        new Species { Name = "Fantasma", Weight = 30, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Ghost_Unvault_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Ghost_Gold_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Ghost_Candy_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Ghost_Galaxy_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_Ghost_Holo_L.webp" }
        }},
        new Species { Name = "Dos Sonhos", Weight = 25, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Sleepy_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Sleepy_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Sleepy_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Sleepy_Galaxy_ui_L.webp" },
            { "cube", "T_Icon_BR_Creature_Sprite_Sleepy_Cube_ui_L.webp" }
        }},
        new Species { Name = "Demónio", Weight = 30, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_RedDemon_Default_L.webp" },
            { "gold", "T_Icon_BR_RedDemon_Gold_L.webp" },
            { "gummy", "T_Icon_BR_RedDemon_Candy_L.webp" }, 
            { "galaxy", "T_Icon_BR_RedDemon_Galaxy_L.webp" },
            { "gem", "T_Icon_BR_RedDemon_Gem_L.webp" }
        }},
        new Species { Name = "Punk", Weight = 25, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Punk_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Punk_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Punk_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Punk_Galaxy_ui_L.webp" },
            { "cube", "T_Icon_BR_Creature_Sprite_Punk_Cube_ui_L.webp" }
        }},
        new Species { Name = "Rei", Weight = 30, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_King_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_King_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_King_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_King_Galaxy_ui_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_King_Holofoil_ui_L.webp" }
        }},
        new Species { Name = "Ponto Zero", Weight = 20, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_ZeroPoint_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_ZeroPoint_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_ZeroPoint_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_ZeroPoint_Galaxy_ui_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_ZeroPoint_Holofoil_ui_L.webp" },
            { "cube", "T_Icon_BR_Creature_Sprite_ZeroPoint_Cube_ui_L.webp" },
            { "gem", "T_Icon_BR_Creature_Sprite_ZeroPoint_Gem_ui_L.webp" }
        }},
        new Species { Name = "Peixoto", Weight = 55, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Fishy_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Fishy_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Fishy_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Fishy_Galaxy_ui_L.webp" },
            { "cube", "T_Icon_BR_Creature_Sprite_Fishy_Cube_L.webp" }
        }},
        new Species { Name = "Atacante", Weight = 30, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Soccer_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Soccer_Gold_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Soccer_Candy_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Soccer_Galaxy_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_Soccer_Holofoil_L.webp" }
        }},
        new Species { Name = "Aura", Weight = 30, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Drifter_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Drifter_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Drifter_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Drifter_Galaxy_ui_L.webp" },
            { "gem", "T_Icon_BR_Creature_Sprite_Drifter_Gem_ui_L.webp" }
        }},
        new Species { Name = "Boss", Weight = 25, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Boss_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Boss_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Boss_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Boss_Galaxy_ui_L.webp" },
            { "cube", "T_Icon_BR_Creature_Sprite_Boss_Cube_ui_L.webp" }
        }},
        new Species { Name = "Grim", Weight = 20, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_GrimReaper_Default_L.webp" },
            { "gold", "T_Icon_BR_GrimReaper_Gold_L.webp" },
            { "gummy", "T_Icon_BR_GrimReaper_Candy_L.webp" }, 
            { "galaxy", "T_Icon_BR_GrimReaper_Galaxy_L.webp" },
            { "holofoil", "T_Icon_BR_GrimReaper_Holofoil_L.webp" },
            { "cube", "T_Icon_BR_GrimReaper_Cube_L.webp" },
            { "gem", "T_Icon_BR_GrimReaper_Gem_L.webp" }
        }},
        new Species { Name = "Ar", Weight = 55, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Air_Default_L.webp" },
            { "gold", "T_Icon_BR_Air_Gold_L.webp" },
            { "gummy", "T_Icon_BR_Air_Candy_L.webp" }, 
            { "galaxy", "T_Icon_BR_Air_Galaxy_L.webp" },
            { "holofoil", "T_Icon_BR_Air_Holo_L.webp" }
        }},
        new Species { Name = "Seven", Weight = 25, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Seven_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Seven_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Seven_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Seven_Galaxy_ui_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_Seven_Holofoil_ui_L.webp" }
        }},
        new Species { Name = "Batman", Weight = 20, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_FossilMeal_Default_L.webp" },
            { "gold", "T_Icon_BR_FossilMeal_Gold_L.webp" },
            { "gummy", "T_Icon_BR_FossilMeal_Candy_L.webp" }, 
            { "galaxy", "T_Icon_BR_FossilMeal_Galaxy_L.webp" },
            { "holofoil", "T_Icon_BR_FossilMeal_Holofoil_L.webp" },
            { "cube", "T_Icon_BR_FossilMeal_Cube_L.webp" }
        }},
        new Species { Name = "Llama", Weight = 25, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Llama_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Llama_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Llama_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Llama_Galaxy_ui_L.webp" },
            { "gem", "T_Icon_BR_Creature_Sprite_Llama_Gem_ui_L.webp" }
        }},
        new Species { Name = "Peely", Weight = 25, Files = new Dictionary<string, string>{
            { "normal", "T_Icon_BR_Creature_Sprite_Peely_ui_L.webp" },
            { "gold", "T_Icon_BR_Creature_Sprite_Peely_Gold_ui_L.webp" },
            { "gummy", "T_Icon_BR_Creature_Sprite_Peely_Candy_ui_L.webp" }, 
            { "galaxy", "T_Icon_BR_Creature_Sprite_Peely_Galaxy_ui_L.webp" },
            { "holofoil", "T_Icon_BR_Creature_Sprite_Peely_Holofoil_ui_L.webp" }
        }}
    };

    public void InitDB()
    {
        Directory.CreateDirectory(@"I:\Twitch\Games\elementais");
        if (!File.Exists(caminhoBD)) SQLiteConnection.CreateFile(caminhoBD);
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS utilizadores (user_id TEXT PRIMARY KEY, username TEXT)", con)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS capturas (user_id TEXT, elemental_id TEXT, quantidade INT)", con)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS propostas_troca (proposer_id TEXT, proposer_name TEXT, target_id TEXT, target_name TEXT, elem_proposer TEXT, elem_target TEXT, reward_id TEXT, redemption_id TEXT, created_at TEXT)", con)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS historico_trocas (user_id TEXT, username TEXT, parceiro_id TEXT, parceiro_name TEXT, elem_dado TEXT, elem_recebido TEXT, data_troca TEXT, recuperacao_anunciada INT DEFAULT 0)", con)) cmd.ExecuteNonQuery();
        }
    }

    private void LimparTrocasExpiradas()
    {
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            List<Tuple<string, string, string, string, string>> expiradas = new List<Tuple<string, string, string, string, string>>();
            using (var cmd = new SQLiteCommand("SELECT proposer_name, target_name, reward_id, redemption_id, created_at FROM propostas_troca", con))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string propName = reader["proposer_name"].ToString();
                        string targetName = reader["target_name"].ToString();
                        string rewardId = reader["reward_id"].ToString();
                        string redemptionId = reader["redemption_id"].ToString();
                        string createdAtStr = reader["created_at"].ToString();
                        
                        if (DateTime.TryParse(createdAtStr, out DateTime createdAt))
                        {
                            if ((DateTime.UtcNow - createdAt).TotalMinutes >= 20.0)
                            {
                                expiradas.Add(Tuple.Create(propName, targetName, rewardId, redemptionId, createdAtStr));
                            }
                        }
                    }
                }
            }

            if (expiradas.Count > 0)
            {
                using (var trans = con.BeginTransaction())
                {
                    foreach (var exp in expiradas)
                    {
                        using (var deleteCmd = new SQLiteCommand("DELETE FROM propostas_troca WHERE redemption_id=@redId", con, trans))
                        {
                            deleteCmd.Parameters.AddWithValue("@redId", exp.Item4);
                            deleteCmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                }

                foreach (var exp in expiradas)
                {
                    if (!string.IsNullOrEmpty(exp.Item3) && !string.IsNullOrEmpty(exp.Item4))
                    {
                        CPH.TwitchRedemptionCancel(exp.Item3, exp.Item4);
                    }
                    CPH.SendMessage($"⏳ [TROCA] A proposta de troca de @{exp.Item1} para @{exp.Item2} expirou (limite de 20 minutos). Pontos devolvidos.");
                }
                
                CPH.RunAction("Elementais - Exportar Site", true);
            }
        }
    }

    public bool ExecutarSpawn()
    {
        CPH.LogInfo("[Elementais] ExecutarSpawn INICIADO...");
        InitDB();
        LimparTrocasExpiradas();

        long ultimoSpawnTicks = CPH.GetGlobalVar<long>("ultimoSpawnTicks");
        if (ultimoSpawnTicks == 0)
        {
            CPH.SetGlobalVar("cacaAtiva", false);
            CPH.SetGlobalVar("ultimoSpawnTicks", DateTime.Now.Ticks);
        }
        else
        {
            TimeSpan tempoDesdeSpawn = new TimeSpan(DateTime.Now.Ticks - ultimoSpawnTicks);
            if (tempoDesdeSpawn.TotalMinutes > 4.0)
            {
                CPH.SetGlobalVar("cacaAtiva", false);
            }
        }

        if (CPH.GetGlobalVar<bool>("cacaAtiva"))
        {
            CPH.LogInfo("[Elementais] Spawn CANCELADO: já existe caça ativa!");
            return false;
        }

        // =========================================================================
        // CONTROLADOR DE TIMING INTERNO: PROTEÇÃO CONTRA SPAWN EM CIMA DE SPRIT
        // =========================================================================
        long ultimoSpritTempo = CPH.GetGlobalVar<long>("ultimoSpritTempo");
        if (ultimoSpritTempo > 0)
        {
            TimeSpan tempoPassado = new TimeSpan(DateTime.Now.Ticks - ultimoSpritTempo);
            // Se o último Sprit foi usado há menos de 10 segundos, aplica o delay restante
            if (tempoPassado.TotalSeconds < 10.0)
            {
                double msFaltam = (10.0 - tempoPassado.TotalSeconds) * 1000.0;
                if (msFaltam > 0)
                {
                    CPH.LogInfo($"[Elementais] Spawn adiado por {(int)msFaltam}ms devido à animação de um Sprit ativo.");
                    Thread.Sleep((int)msFaltam);
                }
            }
        }
        // =========================================================================


        // Ler pasta Users dinamicamente
        string pastaUsers = @"I:\Twitch\Games\elementais\Sprites\Users";
        List<string> userFiles = new List<string>();
        if (Directory.Exists(pastaUsers))
        {
            var files = Directory.GetFiles(pastaUsers, "*.png");
            foreach (var f in files)
            {
                userFiles.Add(Path.GetFileName(f));
            }
        }

        // Escrever users_list.js dinamicamente
        try
        {
            List<string> jsLines = new List<string>();
            jsLines.Add("const userElementais = [");
            for (int i = 0; i < userFiles.Count; i++)
            {
                string filename = userFiles[i];
                string nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
                string comma = (i == userFiles.Count - 1) ? "" : ",";
                jsLines.Add($"  {{ id: \"u_{nameWithoutExt}\", file: \"Users/{filename}\", name: \"{nameWithoutExt}\" }}{comma}");
            }
            jsLines.Add("];");
            File.WriteAllLines(@"I:\Twitch\Games\elementais\users_list.js", jsLines);
        }
        catch (Exception ex)
        {
            CPH.LogWarn("Erro a escrever users_list.js: " + ex.Message);
        }

        bool spritTerraAtivo = CPH.GetGlobalVar<bool>("spritTerraAtivo");
        bool spritTerraSuper = CPH.GetGlobalVar<bool>("spritTerraSuper");
        bool spritAguaAtivo = CPH.GetGlobalVar<bool>("spritAguaAtivo");
        bool spritFogoAtivo = CPH.GetGlobalVar<bool>("spritFogoAtivo");
        bool spritPatoAtivo = CPH.GetGlobalVar<bool>("spritPatoAtivo");
        bool spritPatoSuper = CPH.GetGlobalVar<bool>("spritPatoSuper");
        bool spritGhostAtivo = CPH.GetGlobalVar<bool>("spritGhostAtivo");
        bool spritSleepyAtivo = CPH.GetGlobalVar<bool>("spritSleepyAtivo");
        bool spritDemonAtivo = CPH.GetGlobalVar<bool>("spritDemonAtivo");
        bool spritDemonSuper = CPH.GetGlobalVar<bool>("spritDemonSuper");
        bool spritPunkAtivo = CPH.GetGlobalVar<bool>("spritPunkAtivo");
        bool spritPunkSuper = CPH.GetGlobalVar<bool>("spritPunkSuper");
        bool spritKingAtivo = CPH.GetGlobalVar<bool>("spritKingAtivo");
        bool spritBossAtivo = CPH.GetGlobalVar<bool>("spritBossAtivo");
        bool spritPeixeAtivo = CPH.GetGlobalVar<bool>("spritPeixeAtivo");
        bool spritPeixeSuper = CPH.GetGlobalVar<bool>("spritPeixeSuper");
        bool spritAtacanteAtivo = CPH.GetGlobalVar<bool>("spritAtacanteAtivo");
        bool spritSevenAtivo = CPH.GetGlobalVar<bool>("spritSevenAtivo");

        int faseAtiva = CPH.GetGlobalVar<int>("faseAtiva");
        if (faseAtiva < 1 || faseAtiva > 4) faseAtiva = 1;

        Random rnd = new Random();
        string selectedFileName = "";
        string selectedDisplayName = "";
        string selectedDbId = "";

        List<int> validIndices = new List<int>();
        for (int i = 0; i < speciesList.Count; i++)
        {
            if (spritTerraAtivo)
            {
                if (spritTerraSuper)
                {
                    // Se for Terra SUPER, ignoramos Raros e Épicos (apenas Boss=13 e Grim=14 sobram)
                    if (i != 13 && i != 14)
                    {
                        continue;
                    }
                }
                else
                {
                    // Se for Terra normal, ignoramos Raros (Water=0, Earth=1, Fire=2, Fishy=10, Ar=15)
                    if (i == 0 || i == 1 || i == 2 || i == 10 || i == 15)
                    {
                        continue;
                    }
                }
            }
            validIndices.Add(i);
        }

        int spawnRoll = rnd.Next(1, 101);
        int limiteEspecial = spritTerraAtivo ? 10 : 7;

        // SISTEMA DE PITY (100% de chance para foco em cartas/variantes/comunidade em falta dos participantes dos últimos 5 lançamentos)
        MissingCard pityCard = null;
        if (!spritSevenAtivo)
        {
            using (var conPity = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                conPity.Open();
                pityCard = ObterCardEmFaltaJogadoresRecentes(conPity, validIndices, faseAtiva, spritGhostAtivo, userFiles, rnd);
            }
        }

        if (pityCard != null && pityCard.IsSpecial)
        {
            selectedFileName = pityCard.FileName;
            selectedDisplayName = pityCard.DisplayName;
            selectedDbId = pityCard.DbId;
            CPH.LogInfo($"[Spawn Pity] Pity (100%) ativado! Especial/Comunidade em falta escolhido: {selectedDisplayName} [ID: {selectedDbId}]");
        }
        else if (pityCard == null && spawnRoll <= limiteEspecial)
        {
            // Grupo de especiais / comunidade (7% base, 10% com Terra) com foco nos que estão em falta
            List<MissingCard> todosEspeciais = ObterTodosEspeciais(userFiles);
            List<MissingCard> especiaisEmFalta = new List<MissingCard>();
            HashSet<string> addedEspeciais = new HashSet<string>();

            using (var conEsp = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                conEsp.Open();
                List<string> recentUsers = new List<string>();
                using (var cmd = new SQLiteCommand("SELECT DISTINCT user_id FROM (SELECT user_id FROM lancamentos WHERE user_id IS NOT NULL AND user_id != '' ORDER BY id DESC LIMIT 5)", conEsp))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string uid = reader[0].ToString();
                            if (!string.IsNullOrEmpty(uid) && !recentUsers.Contains(uid)) recentUsers.Add(uid);
                        }
                    }
                }

                foreach (string uid in recentUsers)
                {
                    HashSet<string> owned = new HashSet<string>();
                    using (var cmd = new SQLiteCommand("SELECT elemental_id FROM capturas WHERE user_id=@uid AND quantidade > 0", conEsp))
                    {
                        cmd.Parameters.AddWithValue("@uid", uid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) owned.Add(reader["elemental_id"].ToString());
                        }
                    }
                    foreach (var esp in todosEspeciais)
                    {
                        if (!owned.Contains(esp.DbId) && !addedEspeciais.Contains(esp.DbId))
                        {
                            addedEspeciais.Add(esp.DbId);
                            especiaisEmFalta.Add(esp);
                        }
                    }
                }
            }

            MissingCard chosenEsp = (especiaisEmFalta.Count > 0) ? especiaisEmFalta[rnd.Next(0, especiaisEmFalta.Count)] : todosEspeciais[rnd.Next(0, todosEspeciais.Count)];
            selectedFileName = chosenEsp.FileName;
            selectedDisplayName = chosenEsp.DisplayName;
            selectedDbId = chosenEsp.DbId;
        }
        else
        {
            Species chosenSpecies = null;
            int chosenIndex = 0;
            string variantName = "normal";
            int variantIndex = 1;

            if (pityCard != null && !pityCard.IsSpecial && pityCard.ListIndex >= 0 && pityCard.ListIndex < speciesList.Count)
            {
                chosenSpecies = speciesList[pityCard.ListIndex];
                chosenIndex = pityCard.SpeciesId;
                variantName = pityCard.VariantName;
                variantIndex = pityCard.VariantIndex;
                CPH.LogInfo($"[Spawn Pity] Pity (100%) ativado! Card em falta escolhido: {chosenSpecies.Name} ({variantName}) [ID: {chosenIndex}_{variantIndex}]");
            }
            else
            {
                int totalWeight = 0;
                foreach (int idx in validIndices) totalWeight += speciesList[idx].Weight;
                int roll = rnd.Next(0, totalWeight);
                int weightSum = 0;

                foreach (int idx in validIndices)
                {
                    weightSum += speciesList[idx].Weight;
                    if (roll < weightSum)
                    {
                        chosenSpecies = speciesList[idx];
                        chosenIndex = GetSpeciesIdFromListIndex(idx);
                        break;
                    }
                }

                int variantRoll = rnd.Next(1, 101);

                if (spritGhostAtivo)
                {
                    // Bónus de Drop Oculto: 20% Normal (antes 30%), 80% Gold+ (antes 70%)
                    int ghostRoll = rnd.Next(1, 101);
                    if (ghostRoll <= 20)
                    {
                        variantName = "normal";
                        variantIndex = 1;
                    }
                    else
                    {
                        bool ehLendario = (chosenIndex == 6 || chosenIndex == 8 || chosenIndex == 10 || chosenIndex == 15 || chosenIndex == 16 || chosenIndex == 18 || chosenIndex == 19 || chosenIndex == 22 || chosenIndex == 23);
                        if (ehLendario)
                        {
                            if (ghostRoll <= 62) { variantName = "gold"; variantIndex = 2; }       // 42% Gold
                            else if (ghostRoll <= 80) { variantName = "gummy"; variantIndex = 3; }  // 18% Gummy (antes 15%)
                            else if (ghostRoll <= 94) { variantName = "galaxy"; variantIndex = 4; }  // 14% Galaxy (antes 10%)
                            else { variantName = "holofoil"; variantIndex = 5; }                    // 6% Holofoil (antes 3%)
                        }
                        else
                        {
                            if (ghostRoll <= 75) { variantName = "gold"; variantIndex = 2; }       // 55% Gold (antes 52%)
                            else if (ghostRoll <= 91) { variantName = "gummy"; variantIndex = 3; }  // 16% Gummy (antes 13%)
                            else if (ghostRoll <= 97) { variantName = "galaxy"; variantIndex = 4; }  // 6% Galaxy (antes 4%)
                            else { variantName = "holofoil"; variantIndex = 5; }                    // 3% Holofoil (antes 1%)
                        }
                    }
                }
                else if (faseAtiva == 1) 
                { 
                    variantName = "normal"; 
                    variantIndex = 1; 
                }
                else if (faseAtiva == 2) 
                { 
                    if (variantRoll <= 80) { variantName = "gold"; variantIndex = 2; }              // 80% Gold
                    else { variantName = "normal"; variantIndex = 1; }                              // 20% Normal
                }
                else if (faseAtiva == 3) 
                { 
                    if (variantRoll <= 30) { variantName = "gummy"; variantIndex = 3; }             // 30% Gummy
                    else if (variantRoll <= 80) { variantName = "gold"; variantIndex = 2; }          // 50% Gold
                    else { variantName = "normal"; variantIndex = 1; }                              // 20% Normal
                }
                else if (faseAtiva == 4) 
                { 
                    bool ehLendario = (chosenIndex == 6 || chosenIndex == 8 || chosenIndex == 10 || chosenIndex == 15 || chosenIndex == 16 || chosenIndex == 18 || chosenIndex == 19 || chosenIndex == 22 || chosenIndex == 23);
                    bool supportsGemFase4 = (chosenIndex == 1 || chosenIndex == 2 || chosenIndex == 4 || chosenIndex == 7 || chosenIndex == 10 || chosenIndex == 14 || chosenIndex == 16 || chosenIndex == 22);

                    if (supportsGemFase4)
                    {
                        if (variantRoll <= 10) { variantName = "gem"; variantIndex = 7; }              // 10% Gem
                        else if (variantRoll <= 22) { variantName = "cube"; variantIndex = 6; }        // 12% Cube
                        else if (variantRoll <= 35) { variantName = "holofoil"; variantIndex = 5; }    // 13% Holofoil
                        else if (variantRoll <= 50) { variantName = "galaxy"; variantIndex = 4; }      // 15% Galaxy
                        else if (variantRoll <= 65) { variantName = "gummy"; variantIndex = 3; }       // 15% Gummy
                        else if (variantRoll <= 80) { variantName = "gold"; variantIndex = 2; }        // 15% Gold
                        else { variantName = "normal"; variantIndex = 1; }                           // 20% Normal
                    }
                    else if (ehLendario)
                    {
                        if (variantRoll <= 12) { variantName = "cube"; variantIndex = 6; }            // 12% Cube
                        else if (variantRoll <= 26) { variantName = "holofoil"; variantIndex = 5; }    // 14% Holofoil
                        else if (variantRoll <= 44) { variantName = "galaxy"; variantIndex = 4; }      // 18% Galaxy
                        else if (variantRoll <= 62) { variantName = "gummy"; variantIndex = 3; }       // 18% Gummy
                        else if (variantRoll <= 80) { variantName = "gold"; variantIndex = 2; }        // 18% Gold
                        else { variantName = "normal"; variantIndex = 1; }                           // 20% Normal
                    }
                    else
                    {
                        if (variantRoll <= 10) { variantName = "cube"; variantIndex = 6; }            // 10% Cube
                        else if (variantRoll <= 22) { variantName = "holofoil"; variantIndex = 5; }    // 12% Holofoil
                        else if (variantRoll <= 40) { variantName = "galaxy"; variantIndex = 4; }      // 18% Galaxy
                        else if (variantRoll <= 60) { variantName = "gummy"; variantIndex = 3; }       // 20% Gummy
                        else if (variantRoll <= 80) { variantName = "gold"; variantIndex = 2; }        // 20% Gold
                        else { variantName = "normal"; variantIndex = 1; }                           // 20% Normal
                    }
                }
            }

            if (spritPatoAtivo)
            {
                if (spritPatoSuper)
                {
                    if (variantIndex < 3)
                    {
                        variantName = "gummy";
                        variantIndex = 3;
                    }
                }
                else
                {
                    if (variantIndex == 1)
                    {
                        variantName = "gold";
                        variantIndex = 2;
                    }
                }
            }

            if (spritBossAtivo)
            {
                bool bossSuper = CPH.GetGlobalVar<bool>("spritBossSuper");
                int limiteMinimo = bossSuper ? 4 : 3; // SUPER=Galaxy(4), Normal=Gummy(3)

                if (variantIndex < limiteMinimo)
                {
                    if (bossSuper)
                    {
                        // SUPER: garante Galaxy ou superior
                        int bossRoll = rnd.Next(1, 101);
                        if (bossRoll <= 20)
                        {
                            variantName = "holofoil";
                            variantIndex = 5;
                        }
                        else
                        {
                            variantName = "galaxy";
                            variantIndex = 4;
                        }
                    }
                    else
                    {
                        // Normal: garante Gummy ou superior
                        int bossRoll = rnd.Next(1, 101);
                        if (bossRoll <= 5)
                        {
                            variantName = "holofoil";
                            variantIndex = 5;
                        }
                        else if (bossRoll <= 30)
                        {
                            variantName = "galaxy";
                            variantIndex = 4;
                        }
                        else
                        {
                            variantName = "gummy";
                            variantIndex = 3;
                        }
                    }
                }
            }

            spritSevenAtivo = CPH.GetGlobalVar<bool>("spritSevenAtivo");
            if (spritSevenAtivo)
            {
                int sSevenSpecies = CPH.GetGlobalVar<int>("spritSevenTargetSpecies");
                int sSevenVariantIdx = CPH.GetGlobalVar<int>("spritSevenTargetVariantIndex");
                string sSevenVariantCode = CPH.GetGlobalVar<string>("spritSevenTargetVariantName");

                int listIdx = GetListIndexFromSpeciesId(sSevenSpecies);
                if (listIdx >= 0 && listIdx < speciesList.Count)
                {
                    chosenIndex = sSevenSpecies;
                    chosenSpecies = speciesList[listIdx];
                    variantIndex = sSevenVariantIdx;
                    variantName = string.IsNullOrEmpty(sSevenVariantCode) ? "gold" : sSevenVariantCode;
                }
            }

            // Salvaguarda final para as variantes Gem (7), Cube (6) e Holofoil (5) se a espécie não as suportar
            bool supportsCube = (chosenIndex == 2 || chosenIndex == 3 || chosenIndex == 6 || chosenIndex == 8 || chosenIndex == 10 || chosenIndex == 12 || chosenIndex == 15 || chosenIndex == 16 || chosenIndex == 19);
            bool supportsHolofoil = (chosenIndex == 1 || chosenIndex == 3 || chosenIndex == 5 || chosenIndex == 9 || chosenIndex == 10 || chosenIndex == 13 || chosenIndex == 16 || chosenIndex == 17 || chosenIndex == 18 || chosenIndex == 19 || chosenIndex == 23);
            bool supportsGem = (chosenIndex == 1 || chosenIndex == 2 || chosenIndex == 4 || chosenIndex == 7 || chosenIndex == 10 || chosenIndex == 14 || chosenIndex == 16 || chosenIndex == 22);

            if (variantIndex == 7 && !supportsGem)
            {
                if (supportsHolofoil)
                {
                    variantIndex = 5;
                    variantName = "holofoil";
                }
                else
                {
                    variantIndex = 4;
                    variantName = "galaxy";
                }
            }

            if (variantIndex == 6 && !supportsCube)
            {
                if (supportsHolofoil)
                {
                    variantIndex = 5;
                    variantName = "holofoil";
                }
                else
                {
                    variantIndex = 4;
                    variantName = "galaxy";
                }
            }

            if (variantIndex == 5 && !supportsHolofoil)
            {
                variantIndex = 4;
                variantName = "galaxy";
            }

            selectedFileName = chosenSpecies.Files.ContainsKey(variantName) ? chosenSpecies.Files[variantName] : chosenSpecies.Files["normal"];
            selectedDisplayName = chosenSpecies.Name + " (" + char.ToUpper(variantName[0]) + variantName.Substring(1) + ")";
            selectedDbId = $"{chosenIndex}_{variantIndex}";
        }

        int cacaID = CPH.GetGlobalVar<int>("cacaID") + 1;
        CPH.SetGlobalVar("cacaID", cacaID);

        CPH.SetGlobalVar("elementalAtivoId", selectedDbId);
        CPH.SetGlobalVar("elementalAtivoNome", selectedDisplayName);
        CPH.SetGlobalVar("elementalAtivoFicheiro", selectedFileName); 
        CPH.SetGlobalVar("cacaAtiva", true);         
        CPH.SetGlobalVar("tentativasGlobais", 0);    

        CPH.SetGlobalVar("spritsUsados", 0);

        bool terraAtivoNoSpawn = spritTerraAtivo;
        bool aguaAtivoNoSpawn = spritAguaAtivo;
        bool fogoAtivoNoSpawn = spritFogoAtivo;
        bool patoAtivoNoSpawn = spritPatoAtivo;
        bool ghostAtivoNoSpawn = spritGhostAtivo;
        bool sleepyAtivoNoSpawn = spritSleepyAtivo;
        bool demonAtivoNoSpawn = spritDemonAtivo;
        bool punkAtivoNoSpawn = spritPunkAtivo;
        bool kingAtivoNoSpawn = spritKingAtivo;
        bool auraAtivoNoSpawn = CPH.GetGlobalVar<bool>("spritAuraAtivo");
        bool bossAtivoNoSpawn = spritBossAtivo;
        bool peixeAtivoNoSpawn = spritPeixeAtivo;
        bool atacanteAtivoNoSpawn = spritAtacanteAtivo;
        if (fogoAtivoNoSpawn)
        {
            long ultimoFogoTempo = CPH.GetGlobalVar<long>("ultimoFogoTempo");
            if (ultimoFogoTempo > 0)
            {
                TimeSpan tempoPassado = new TimeSpan(DateTime.Now.Ticks - ultimoFogoTempo);
                if (tempoPassado.TotalMinutes >= 60.0) fogoAtivoNoSpawn = false;
            }
        }

        CPH.SetGlobalVar("cacaSpritGhostAtiva", ghostAtivoNoSpawn);
        CPH.SetGlobalVar("spritGhostAtivo", false);
        CPH.SetGlobalVar("cacaSpritAuraAtiva", auraAtivoNoSpawn);
        if (auraAtivoNoSpawn)
        {
            string spritAuraUser = CPH.GetGlobalVar<string>("spritAuraUser") ?? "";
            string spritAuraUserId = CPH.GetGlobalVar<string>("spritAuraUserId") ?? "";
            CPH.SetGlobalVar("cacaSpritAuraUser", spritAuraUser);
            CPH.SetGlobalVar("cacaSpritAuraUserId", spritAuraUserId);
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritAuraUser", "");
            CPH.SetGlobalVar("cacaSpritAuraUserId", "");
        }
        CPH.SetGlobalVar("spritAuraAtivo", false);
        CPH.SetGlobalVar("spritAuraUser", "");
        CPH.SetGlobalVar("spritAuraUserId", "");
        CPH.SetGlobalVar("cacaSpritSleepyAtiva", sleepyAtivoNoSpawn);
        if (sleepyAtivoNoSpawn)
        {
            int spritSleepyCount = CPH.GetGlobalVar<int>("spritSleepyCount");
            string spritSleepyUsers = CPH.GetGlobalVar<string>("spritSleepyUsers") ?? "";
            
            CPH.SetGlobalVar("cacaSpritSleepyCount", spritSleepyCount);
            CPH.SetGlobalVar("cacaSpritSleepyUsers", spritSleepyUsers);
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritSleepyCount", 0);
            CPH.SetGlobalVar("cacaSpritSleepyUsers", "");
        }
        CPH.SetGlobalVar("spritSleepyAtivo", false);
        CPH.SetGlobalVar("spritSleepyCount", 0);
        CPH.SetGlobalVar("spritSleepyUsers", "");

        CPH.SetGlobalVar("cacaSpritDemonAtiva", demonAtivoNoSpawn);
        if (demonAtivoNoSpawn)
        {
            string spritDemonUser = CPH.GetGlobalVar<string>("spritDemonUser") ?? "";
            CPH.SetGlobalVar("cacaSpritDemonUser", spritDemonUser);

            if (spritDemonSuper)
            {
                CPH.SetGlobalVar("cacaSpritDemonSuper", true);
                CPH.SetGlobalVar("spritDemonSuper", false);
            }
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritDemonUser", "");
            CPH.SetGlobalVar("cacaSpritDemonSuper", false);
        }
        CPH.SetGlobalVar("spritDemonAtivo", false);
        CPH.SetGlobalVar("spritDemonUser", "");

        CPH.SetGlobalVar("cacaSpritPunkAtiva", punkAtivoNoSpawn);
        if (punkAtivoNoSpawn)
        {
            string spritPunkUser = CPH.GetGlobalVar<string>("spritPunkUser") ?? "";
            string spritPunkUserId = CPH.GetGlobalVar<string>("spritPunkUserId") ?? "";
            CPH.SetGlobalVar("cacaSpritPunkUser", spritPunkUser);
            CPH.SetGlobalVar("cacaSpritPunkUserId", spritPunkUserId);
            CPH.SetGlobalVar("cacaSpritPunkCandidatos", "");

            if (spritPunkSuper)
            {
                CPH.SetGlobalVar("cacaSpritPunkSuper", true);
                CPH.SetGlobalVar("spritPunkSuper", false);
            }
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritPunkUser", "");
            CPH.SetGlobalVar("cacaSpritPunkUserId", "");
            CPH.SetGlobalVar("cacaSpritPunkCandidatos", "");
            CPH.SetGlobalVar("cacaSpritPunkSuper", false);
        }
        CPH.SetGlobalVar("spritPunkAtivo", false);
        CPH.SetGlobalVar("spritPunkUser", "");
        CPH.SetGlobalVar("spritPunkUserId", "");

        CPH.SetGlobalVar("cacaSpritKingAtiva", kingAtivoNoSpawn);
        if (kingAtivoNoSpawn)
        {
            string spritKingUser = CPH.GetGlobalVar<string>("spritKingUser") ?? "";
            string spritKingUserId = CPH.GetGlobalVar<string>("spritKingUserId") ?? "";
            CPH.SetGlobalVar("cacaSpritKingUser", spritKingUser);
            CPH.SetGlobalVar("cacaSpritKingUserId", spritKingUserId);
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritKingUser", "");
            CPH.SetGlobalVar("cacaSpritKingUserId", "");
        }
        CPH.SetGlobalVar("spritKingAtivo", false);
        CPH.SetGlobalVar("spritKingUser", "");
        CPH.SetGlobalVar("spritKingUserId", "");

        CPH.SetGlobalVar("cacaSpritBossAtiva", bossAtivoNoSpawn);
        if (bossAtivoNoSpawn)
        {
            string spritBossUser = CPH.GetGlobalVar<string>("spritBossUser") ?? "";
            string spritBossUserId = CPH.GetGlobalVar<string>("spritBossUserId") ?? "";
            CPH.SetGlobalVar("cacaSpritBossUser", spritBossUser);
            CPH.SetGlobalVar("cacaSpritBossUserId", spritBossUserId);

            bool spritBossSuper = CPH.GetGlobalVar<bool>("spritBossSuper");
            if (spritBossSuper)
            {
                CPH.SetGlobalVar("cacaSpritBossSuper", true);
                CPH.SetGlobalVar("spritBossSuper", false);
            }
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritBossUser", "");
            CPH.SetGlobalVar("cacaSpritBossUserId", "");
            CPH.SetGlobalVar("cacaSpritBossSuper", false);
        }
        CPH.SetGlobalVar("spritBossAtivo", false);
        CPH.SetGlobalVar("spritBossUser", "");
        CPH.SetGlobalVar("spritBossUserId", "");

        CPH.SetGlobalVar("cacaSpritPeixeAtiva", peixeAtivoNoSpawn);
        if (peixeAtivoNoSpawn)
        {
            string spritPeixeUser = CPH.GetGlobalVar<string>("spritPeixeUser") ?? "";
            string spritPeixeUserId = CPH.GetGlobalVar<string>("spritPeixeUserId") ?? "";
            CPH.SetGlobalVar("cacaSpritPeixeUser", spritPeixeUser);
            CPH.SetGlobalVar("cacaSpritPeixeUserId", spritPeixeUserId);

            if (spritPeixeSuper)
            {
                CPH.SetGlobalVar("cacaSpritPeixeSuper", true);
                CPH.SetGlobalVar("spritPeixeSuper", false);
            }
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritPeixeUser", "");
            CPH.SetGlobalVar("cacaSpritPeixeUserId", "");
            CPH.SetGlobalVar("cacaSpritPeixeSuper", false);
        }
        CPH.SetGlobalVar("spritPeixeAtivo", false);
        CPH.SetGlobalVar("spritPeixeUser", "");
        CPH.SetGlobalVar("spritPeixeUserId", "");

        CPH.SetGlobalVar("cacaSpritAtacanteAtiva", atacanteAtivoNoSpawn);
        if (atacanteAtivoNoSpawn)
        {
            string spritAtacanteUser = CPH.GetGlobalVar<string>("spritAtacanteUser") ?? "";
            string spritAtacanteUserId = CPH.GetGlobalVar<string>("spritAtacanteUserId") ?? "";
            string spritAtacanteSuper = CPH.GetGlobalVar<string>("spritAtacanteSuper") ?? "";
            CPH.SetGlobalVar("cacaSpritAtacanteUser", spritAtacanteUser);
            CPH.SetGlobalVar("cacaSpritAtacanteUserId", spritAtacanteUserId);
            CPH.SetGlobalVar("cacaSpritAtacanteSuper", spritAtacanteSuper);
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritAtacanteUser", "");
            CPH.SetGlobalVar("cacaSpritAtacanteUserId", "");
            CPH.SetGlobalVar("cacaSpritAtacanteSuper", "");
        }
        CPH.SetGlobalVar("spritAtacanteAtivo", false);
        CPH.SetGlobalVar("spritAtacanteUser", "");
        CPH.SetGlobalVar("spritAtacanteUserId", "");
        CPH.SetGlobalVar("spritAtacanteSuper", "");

        string displayNomeNoChat = ghostAtivoNoSpawn ? "um elemental mistério" : selectedDisplayName;

        EscreverEstado(string.Format("SPAWN;{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}", selectedFileName, selectedDisplayName, aguaAtivoNoSpawn, terraAtivoNoSpawn, fogoAtivoNoSpawn, patoAtivoNoSpawn, ghostAtivoNoSpawn, sleepyAtivoNoSpawn, demonAtivoNoSpawn, punkAtivoNoSpawn, kingAtivoNoSpawn));

        // =========================================================================
        // PROCESSAMENTO DE EFEITOS DE ELEMENTAIS ATIVOS
        // =========================================================================
        if (spritTerraAtivo)
        {
            if (spritTerraSuper)
            {
                CPH.SendMessage(string.Format("🌍 Terra [SUPER]: Apenas Lendários ou Míticos! Surgiu: {0}", displayNomeNoChat));
                CPH.SetGlobalVar("spritTerraSuper", false);
            }
            else
            {
                CPH.SendMessage(string.Format("🌍 Terra: Sem comuns e Especiais/Users a 10%! Surgiu: {0}", displayNomeNoChat));
            }
            CPH.SetGlobalVar("spritTerraAtivo", false);
        }

        if (spritAguaAtivo)
        {
            string spritAguaUser = CPH.GetGlobalVar<string>("spritAguaUser") ?? "";
            CPH.SetGlobalVar("cacaSpritAguaAtiva", true);
            CPH.SetGlobalVar("cacaSpritAguaUser", spritAguaUser);
            CPH.SetGlobalVar("spritAguaAtivo", false);
            CPH.SetGlobalVar("spritAguaUser", "");

            bool spritAguaSuper = CPH.GetGlobalVar<bool>("spritAguaSuper");
            if (spritAguaSuper)
            {
                CPH.SetGlobalVar("cacaSpritAguaSuper", true);
                CPH.SetGlobalVar("spritAguaSuper", false);
                CPH.SendMessage(string.Format("💧 Água [SUPER]: Captura -60% para todos exceto @{0}! Surgiu: {1}", spritAguaUser.Replace(",", ", @"), displayNomeNoChat));
            }
            else
            {
                CPH.SendMessage(string.Format("💧 Água: Captura -40% para todos exceto @{0}! Surgiu: {1}", spritAguaUser.Replace(",", ", @"), displayNomeNoChat));
            }
        }

        bool ventoAtivoNoSpawn = CPH.GetGlobalVar<bool>("spritVentoAtivo");
        CPH.SetGlobalVar("cacaSpritVentoAtiva", ventoAtivoNoSpawn);
        if (ventoAtivoNoSpawn)
        {
            string spritVentoUser = CPH.GetGlobalVar<string>("spritVentoUser") ?? "";
            string spritVentoUserId = CPH.GetGlobalVar<string>("spritVentoUserId") ?? "";
            string spritVentoSuper = CPH.GetGlobalVar<string>("spritVentoSuper") ?? "";

            CPH.SetGlobalVar("cacaSpritVentoUser", spritVentoUser);
            CPH.SetGlobalVar("cacaSpritVentoUserId", spritVentoUserId);
            CPH.SetGlobalVar("cacaSpritVentoSuper", spritVentoSuper);

            bool isAnySuper = !string.IsNullOrEmpty(spritVentoSuper);
            if (isAnySuper)
            {
                CPH.SendMessage(string.Format("🌪️ Vento [SUPER]: @{0} invocou o vento! Se não ficar em 1º lugar na fila, o vento baralha novamente a fila (até 2x se tiver SUPER)!", spritVentoUser.Replace(",", ", @")));
            }
            else
            {
                CPH.SendMessage(string.Format("🌪️ Vento: @{0} invocou o vento! Se não ficar em 1º lugar na fila, o vento baralha novamente a fila!", spritVentoUser.Replace(",", ", @")));
            }
        }
        else
        {
            CPH.SetGlobalVar("cacaSpritVentoUser", "");
            CPH.SetGlobalVar("cacaSpritVentoUserId", "");
            CPH.SetGlobalVar("cacaSpritVentoSuper", "");
        }
        CPH.SetGlobalVar("spritVentoAtivo", false);
        CPH.SetGlobalVar("spritVentoUser", "");
        CPH.SetGlobalVar("spritVentoUserId", "");
        CPH.SetGlobalVar("spritVentoSuper", "");

        bool peelyAtivoNoSpawn = CPH.GetGlobalVar<bool>("spritPeelyAtivo");
        if (spritSevenAtivo)
        {
            string sSevenUser = CPH.GetGlobalVar<string>("spritSevenUser") ?? "";
            bool sSevenSuper = CPH.GetGlobalVar<bool>("spritSevenSuper");
            int sSevenSpecies = CPH.GetGlobalVar<int>("spritSevenTargetSpecies");
            int sSevenVariantIdx = CPH.GetGlobalVar<int>("spritSevenTargetVariantIndex");

            string vFormatted = GetVariantNameFormatted(sSevenVariantIdx);
            int sIdx = GetListIndexFromSpeciesId(sSevenSpecies);
            string speciesName = (sIdx >= 0 && sIdx < speciesList.Count) ? speciesList[sIdx].Name : "Elemental";

            if (sSevenSuper)
            {
                CPH.SendMessage(string.Format("⚡ Seven [SUPER]: O número 7 trouxe de volta o [{0}] com a variante UPGRADE DUPLO [{1}] para @{2}!", speciesName, vFormatted, sSevenUser));
            }
            else
            {
                CPH.SendMessage(string.Format("⚡ Seven: O número 7 trouxe de volta o [{0}] com a variante [{1}] para @{2}!", speciesName, vFormatted, sSevenUser));
            }

            CPH.SetGlobalVar("spritSevenAtivo", false);
            CPH.SetGlobalVar("spritSevenSuper", false);
        }
        int spritLlamaSegredo = CPH.GetGlobalVar<int>("spritLlamaSegredo");
        if (spritLlamaSegredo > 0)
        {
            string llamaUser = CPH.GetGlobalVar<string>("spritLlamaUser") ?? "um utilizador";
            CPH.SendMessage(string.Format("🦙 MISTÉRIO REVELADO! A Llama de @{0} invocou o poder do [{1}]! ✨", llamaUser, GetNomeElemento(spritLlamaSegredo)));
            CPH.SetGlobalVar("spritLlamaSegredo", 0);
        }

        if (peelyAtivoNoSpawn)
        {
            string spritPeelyUser = CPH.GetGlobalVar<string>("spritPeelyUser") ?? "";
            string spritPeelyUserId = CPH.GetGlobalVar<string>("spritPeelyUserId") ?? "";
            bool spritPeelySuper = CPH.GetGlobalVar<bool>("spritPeelySuper");

            CPH.SetGlobalVar("cacaSpritPeelyAtiva", true);
            CPH.SetGlobalVar("cacaSpritPeelyUser", spritPeelyUser);
            CPH.SetGlobalVar("cacaSpritPeelyUserId", spritPeelyUserId);
            CPH.SetGlobalVar("cacaSpritPeelySuper", spritPeelySuper);

            CPH.SetGlobalVar("spritPeelyAtivo", false);
            CPH.SetGlobalVar("spritPeelyUser", "");
            CPH.SetGlobalVar("spritPeelyUserId", "");
            CPH.SetGlobalVar("spritPeelySuper", false);

            if (spritPeelySuper)
            {
                CPH.SendMessage(string.Format("🍌 Peely [SUPER]: @{0} colocou cascas de banana gigantes! As 2 primeiras pessoas da fila vão escorregar para o último lugar!", spritPeelyUser.Replace(",", ", @")));
            }
            else
            {
                CPH.SendMessage(string.Format("🍌 Peely: @{0} colocou uma casca de banana! A primeira pessoa da fila vai escorregar para o último lugar!", spritPeelyUser.Replace(",", ", @")));
            }
        }

        if (spritFogoAtivo)
        {
            long ultimoFogoTempo = CPH.GetGlobalVar<long>("ultimoFogoTempo");
            if (ultimoFogoTempo > 0)
            {
                TimeSpan tempoPassado = new TimeSpan(DateTime.Now.Ticks - ultimoFogoTempo);
                if (tempoPassado.TotalMinutes >= 60.0)
                {
                    CPH.SetGlobalVar("spritFogoAtivo", false);
                    CPH.SendMessage("🔥 Fogo expirou! Ciclo de spawns voltou ao normal.");
                }
                else
                {
                    int minutosRestantes = (int)Math.Ceiling(60.0 - tempoPassado.TotalMinutes);
                    CPH.SendMessage($"🔥 Fogo: Surgiu {displayNomeNoChat}! (Restam {minutosRestantes}m rápidos)");
                }
            }
        }

        if (spritPatoAtivo)
        {
            if (spritPatoSuper)
            {
                CPH.SendMessage(string.Format("🦆 Pato [SUPER]: {0} promovido para Gummy ou superior!", displayNomeNoChat));
                CPH.SetGlobalVar("spritPatoSuper", false);
            }
            else
            {
                CPH.SendMessage(string.Format("🦆 Pato: {0} promovido para Ouro ou superior!", displayNomeNoChat));
            }
            CPH.SetGlobalVar("spritPatoAtivo", false);
        }



        if (sleepyAtivoNoSpawn && !spritTerraAtivo && !spritAguaAtivo && !spritFogoAtivo && !spritPatoAtivo && !ghostAtivoNoSpawn)
        {
            CPH.SendMessage("💤 Sonolento: Alguém no lobby vai adormecer!");
        }

        if (demonAtivoNoSpawn)
        {
            string cacaSpritDemonUser = CPH.GetGlobalVar<string>("cacaSpritDemonUser") ?? "";
            bool cacaSpritDemonSuper = CPH.GetGlobalVar<bool>("cacaSpritDemonSuper");
            if (cacaSpritDemonSuper)
            {
                CPH.SendMessage(string.Format("😈 Demónio [SUPER]: Apenas @{0} pode usar Master Ball e Ultra Ball neste {1}!", cacaSpritDemonUser.Replace(",", ", @"), displayNomeNoChat));
            }
            else
            {
                CPH.SendMessage(string.Format("😈 Demónio: Apenas @{0} pode usar Master Ball neste {1}!", cacaSpritDemonUser.Replace(",", ", @"), displayNomeNoChat));
            }
        }

        if (punkAtivoNoSpawn)
        {
            string cacaSpritPunkUser = CPH.GetGlobalVar<string>("cacaSpritPunkUser") ?? "";
            bool cacaSpritPunkSuper = CPH.GetGlobalVar<bool>("cacaSpritPunkSuper");
            if (cacaSpritPunkSuper)
            {
                CPH.SendMessage(string.Format("🎸 Punk [SUPER]: Quem atirar neste spawn arrisca-se a perder um elemental (até 2 pessoas serão roubadas) para: @{0}!", cacaSpritPunkUser.Replace(",", ", @")));
            }
            else
            {
                CPH.SendMessage(string.Format("🎸 Punk: Quem atirar neste spawn arrisca-se a perder um elemental para: @{0}!", cacaSpritPunkUser.Replace(",", ", @")));
            }
        }

        if (kingAtivoNoSpawn)
        {
            string cacaSpritKingUser = CPH.GetGlobalVar<string>("cacaSpritKingUser") ?? "";
            CPH.SendMessage(string.Format("👑 Rei: Apenas @{0} pode tentar capturar este {1} (até 5 tentativas)!", cacaSpritKingUser.Replace(",", ", @"), displayNomeNoChat));
        }

        if (bossAtivoNoSpawn)
        {
            string cacaSpritBossUser = CPH.GetGlobalVar<string>("cacaSpritBossUser") ?? "";
            bool cacaSpritBossSuper = CPH.GetGlobalVar<bool>("cacaSpritBossSuper");
            if (cacaSpritBossSuper)
            {
                CPH.SendMessage(string.Format("👑 Boss [SUPER]: @{0} invocou o Boss! Este elemental é garantidamente Galaxy ou superior, mas todas as taxas de captura caíram em 60%!", cacaSpritBossUser.Replace(",", ", @")));
            }
            else
            {
                CPH.SendMessage(string.Format("👑 Boss: @{0} invocou o Boss! Este elemental é garantidamente Gummy ou superior, mas todas as taxas de captura caíram em 60%!", cacaSpritBossUser.Replace(",", ", @")));
            }
        }

        if (peixeAtivoNoSpawn)
        {
            string cacaSpritPeixeUser = CPH.GetGlobalVar<string>("cacaSpritPeixeUser") ?? "";
            bool cacaSpritPeixeSuper = CPH.GetGlobalVar<bool>("cacaSpritPeixeSuper");
            if (cacaSpritPeixeSuper)
            {
                CPH.SendMessage(string.Format("🎣 Peixe [SUPER]: Se @{0} capturar este {1}, pescará 2 elementais extras! 🐟🐟", cacaSpritPeixeUser.Replace(",", ", @"), displayNomeNoChat));
            }
            else
            {
                CPH.SendMessage(string.Format("🎣 Peixe: Se @{0} capturar este {1}, pescará um elemental extra! 🐟", cacaSpritPeixeUser.Replace(",", ", @"), displayNomeNoChat));
            }
        }

        if (atacanteAtivoNoSpawn)
        {
            string cacaSpritAtacanteUser = CPH.GetGlobalVar<string>("cacaSpritAtacanteUser") ?? "";
            CPH.SendMessage(string.Format("⚽ Atacante: @{0} tem direito a um ressalto extra gratuito se o seu remate falhar! 🥅", cacaSpritAtacanteUser.Replace(",", ", @")));
        }

        // =========================================================================
        // INICIALIZAÇÃO DO LOBBY DE CAPTURA DE 30 SEGUNDOS
        // =========================================================================
        if (kingAtivoNoSpawn)
        {
            CPH.SetGlobalVar("lobbyAtivo", false);
            CPH.SetGlobalVar("lobbyCount", 0);
            CPH.SetGlobalVar("lobbyResolvido", false);
            CPH.SetGlobalVar("lobbyFilaIndex", 0);
            CPH.SetGlobalVar("lobbyFilaTotal", 0);
        }
        else
        {
            CPH.SetGlobalVar("lobbyAtivo", true);
            CPH.SetGlobalVar("lobbyCount", 0);
            CPH.SetGlobalVar("lobbyResolvido", false);
            CPH.SetGlobalVar("lobbyFilaIndex", 0);
            CPH.SetGlobalVar("lobbyFilaTotal", 0);

            int currentCacaID = cacaID;
            var cph = CPH;
            new System.Threading.Thread(() => {
                System.Threading.Thread.Sleep(50000); // 20s de aviso de spawn + 30s de lobby ativo
                cph.SetArgument("lobbyTimerCacaID", currentCacaID);
                cph.RunAction("Elementais - Resolver Lobby");
            }).Start();
        }

        CPH.LogInfo($"[Elementais] Spawn gerado com balanceamento e trancas de Sprits: {selectedDisplayName}");
        CPH.SetGlobalVar("ultimoSpawnTicks", DateTime.Now.Ticks);
        CPH.RunAction("Elementais - Ativar Caça");

        return true;
    }

    private void EscreverEstado(string cmd)
    {
        for (int attempt = 0; attempt < 3; attempt++)
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

                bool atacante = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAtacanteAtiva") : CPH.GetGlobalVar<bool>("spritAtacanteAtivo");
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
                    aguaVal, terraVal, fogo, patoVal, ghost, sleepyVal, demonVal, punkVal, king, aura, bossVal, peixeVal, atacante, ventoVal, peelyVal, sevenVal);
                File.WriteAllText(caminhoEstado, cmd + suffix);
                CPH.LogInfo($"[Elementais] EscreverEstado Sucesso: {cmd}");
                break;
            }
            catch (Exception ex)
            {
                CPH.LogWarn($"[Elementais] Tentativa {attempt + 1} de escrever estado falhou: {ex.Message}");
                System.Threading.Thread.Sleep(50);
            }
        }
    }

    private string GetNomeElemento(int num)
    {
        switch (num)
        {
            case 1: return "Elemental de Água";
            case 2: return "Elemental de Terra";
            case 3: return "Elemental de Fogo";
            case 4: return "Elemental de Pato";
            case 5: return "Elemental de Fantasma";
            case 6: return "Elemental dos Sonhos";
            case 7: return "Elemental de Demónio";
            case 8: return "Elemental de Punk";
            case 18: return "Elemental Seven";
            case 9: return "Elemental de Rei";
            case 10: return "Elemental de Ponto Zero";
            case 12: return "Elemental de Peixoto";
            case 13: return "Elemental Atacante";
            case 14: return "Elemental de Aura";
            case 15: return "Elemental de Boss";
            case 16: return "Elemental Grim";
            case 17: return "Elemental de Ar";
            case 22: return "Elemental Llama";
            case 23: return "Elemental Peely";
            default: return "Elemental Desconhecido";
        }
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
    private int GetSpeciesIdFromListIndex(int idx)
    {
        if (idx < 10) return idx + 1;
        if (idx < 18) return idx + 2;
        return idx + 4;
    }

    private int GetListIndexFromSpeciesId(int sId)
    {
        if (sId >= 1 && sId <= 10) return sId - 1;
        if (sId >= 12 && sId <= 19) return sId - 2;
        if (sId >= 22 && sId <= 23) return sId - 4;
        return -1;
    }

    private List<MissingCard> ObterTodosEspeciais(List<string> userFiles)
    {
        var list = new List<MissingCard>();
        list.Add(new MissingCard { IsSpecial = true, DbId = "11_1", FileName = "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp", DisplayName = "BurntPeanut" });
        list.Add(new MissingCard { IsSpecial = true, DbId = "20_1", FileName = "T_Icon_BR_CokeParmesan_Default_L.webp", DisplayName = "Vini JR" });
        list.Add(new MissingCard { IsSpecial = true, DbId = "21_1", FileName = "T_Icon_BR_CompanyStargazer_Default_L.webp", DisplayName = "Pollo" });
        list.Add(new MissingCard { IsSpecial = true, DbId = "24_1", FileName = "T_Icon_Reload_FillerGrunt_icon_L.webp", DisplayName = "John Wick" });
        list.Add(new MissingCard { IsSpecial = true, DbId = "25_1", FileName = "T_Icon_BR_PedicureAntacid_L.webp", DisplayName = "Ironmouse" });

        if (userFiles != null)
        {
            foreach (var filename in userFiles)
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
                list.Add(new MissingCard
                {
                    IsSpecial = true,
                    DbId = $"u_{nameWithoutExt}",
                    FileName = $"Users/{filename}",
                    DisplayName = nameWithoutExt
                });
            }
        }
        return list;
    }

    private MissingCard ObterCardEmFaltaJogadoresRecentes(SQLiteConnection con, List<int> validIndices, int faseAtiva, bool ghostAtivo, List<string> userFiles, Random rnd)
    {
        try
        {
            // 1. Obter os user_ids ÚNICOS dos últimos 5 lançamentos (acertos ou falhas)
            List<string> recentUsers = new List<string>();
            using (var cmd = new SQLiteCommand("SELECT DISTINCT user_id FROM (SELECT user_id FROM lancamentos WHERE user_id IS NOT NULL AND user_id != '' ORDER BY id DESC LIMIT 5)", con))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string uid = reader[0].ToString();
                        if (!string.IsNullOrEmpty(uid) && !recentUsers.Contains(uid))
                        {
                            recentUsers.Add(uid);
                        }
                    }
                }
            }

            if (recentUsers.Count == 0) return null;

            List<MissingCard> todosEspeciais = ObterTodosEspeciais(userFiles);

            // Baralhar os utilizadores recentes para sortear um jogador aleatório entre os que estão a participar
            List<string> shuffledUsers = new List<string>(recentUsers);
            for (int i = shuffledUsers.Count - 1; i > 0; i--)
            {
                int k = rnd.Next(0, i + 1);
                string temp = shuffledUsers[i];
                shuffledUsers[i] = shuffledUsers[k];
                shuffledUsers[k] = temp;
            }

            // 2. Para o jogador sorteado, obter as cartas em falta desse utilizador
            foreach (string uid in shuffledUsers)
            {
                HashSet<string> ownedCards = new HashSet<string>();
                using (var cmd = new SQLiteCommand("SELECT elemental_id FROM capturas WHERE user_id=@uid AND quantidade > 0", con))
                {
                    cmd.Parameters.AddWithValue("@uid", uid);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ownedCards.Add(reader["elemental_id"].ToString());
                        }
                    }
                }

                List<MissingCard> userMissingCards = new List<MissingCard>();

                // 2.1 Verificar cartas regulares em falta para este jogador específico
                foreach (int idx in validIndices)
                {
                    var s = speciesList[idx];
                    int sId = GetSpeciesIdFromListIndex(idx);

                    foreach (var kv in s.Files)
                    {
                        string vName = kv.Key;
                        int vIdx = ObterIndiceVariantePorNome(vName);
                        if (vIdx <= 0) continue;

                        // Validar se esta variante pode sair na Fase ativa ou se Ghost está ativo
                        if (!ghostAtivo)
                        {
                            if (faseAtiva == 1 && vIdx > 1) continue;
                            if (faseAtiva == 2 && vIdx > 2) continue;
                            if (faseAtiva == 3 && vIdx > 3) continue;
                        }

                        string cardKey = $"{sId}_{vIdx}";
                        if (!ownedCards.Contains(cardKey))
                        {
                            userMissingCards.Add(new MissingCard
                            {
                                IsSpecial = false,
                                ListIndex = idx,
                                SpeciesId = sId,
                                VariantName = vName,
                                VariantIndex = vIdx
                            });
                        }
                    }
                }

                // 2.2 Verificar cartas especiais e de comunidade em falta para este jogador específico
                foreach (var esp in todosEspeciais)
                {
                    if (!ownedCards.Contains(esp.DbId))
                    {
                        userMissingCards.Add(esp);
                    }
                }

                // Se este jogador tiver cartas em falta, sorteamos uma carta para ele com os pesos de raridade
                if (userMissingCards.Count > 0)
                {
                    int totalWeight = 0;
                    foreach (var card in userMissingCards)
                    {
                        totalWeight += ObterPesoCardPity(card);
                    }

                    if (totalWeight <= 0) return userMissingCards[rnd.Next(0, userMissingCards.Count)];

                    int roll = rnd.Next(0, totalWeight);
                    int currentSum = 0;
                    foreach (var card in userMissingCards)
                    {
                        currentSum += ObterPesoCardPity(card);
                        if (roll < currentSum)
                        {
                            return card;
                        }
                    }

                    return userMissingCards[userMissingCards.Count - 1];
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            CPH.LogWarn("[Spawn Pity] Erro ao calcular cartas em falta: " + ex.Message);
            return null;
        }
    }

    private int ObterPesoCardPity(MissingCard card)
    {
        if (card == null) return 10;
        if (card.IsSpecial) return 30; // 3x para Especiais e Comunidade

        switch (card.VariantIndex)
        {
            case 1: return 10; // Normal: 1.0x
            case 2: return 12; // Gold: 1.2x
            case 3: return 15; // Gummy: 1.5x
            case 4: return 20; // Galaxy: 2.0x
            case 5: return 30; // Holofoil: 3.0x
            case 6: return 30; // Cube: 3.0x
            case 7: return 30; // Gem: 3.0x
            default: return 10;
        }
    }

    private int ObterIndiceVariantePorNome(string name)
    {
        switch (name.ToLower())
        {
            case "normal": return 1;
            case "gold": return 2;
            case "gummy": case "candy": return 3;
            case "galaxy": return 4;
            case "holofoil": case "holo": return 5;
            case "cube": return 6;
            case "gem": return 7;
            case "quack": return 8;
            default: return 1;
        }
    }

    public bool Execute() { return ExecutarSpawn(); }
}