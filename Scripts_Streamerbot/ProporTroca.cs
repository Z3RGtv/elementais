using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

public class CPHInline
{
    // Estrutura para definir cada espécie
    public class Species
    {
        public string Name { get; set; }
        public int Weight { get; set; }
        public Dictionary<string, string> Files { get; set; }
    }

    // Proposta de troca
    public class TradeProposal
    {
        public string ProposerId { get; set; }
        public string ProposerName { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public string ElemProposer { get; set; }
        public string RewardId { get; set; }
        public string RedemptionId { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    // Variáveis estáticas persistem entre execuções durante a execução do Streamer.bot
    private static bool cacaAtiva = false;
    private static string elementalAtivoId = "";
    private static string elementalAtivoNome = "";
    private static List<string> filaUtilizadores = new List<string>();
    private static readonly object lockFila = new object();

    // Gestão de Trocas
    private static List<TradeProposal> pendingTrades = new List<TradeProposal>();
    private static readonly object lockTrocas = new object();
    private static DateTime lastSpawnTime = DateTime.MinValue;

    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";

    // Mapeamento estático de todas as espécies, pesos e caminhos dos ficheiros webp
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

    private int GetListIndexFromSpeciesId(int sId)
    {
        if (sId >= 1 && sId <= 10) return sId - 1;
        if (sId >= 12 && sId <= 19) return sId - 2;
        if (sId >= 22 && sId <= 23) return sId - 4;
        return -1;
    }

    // Auxiliar: Resolve ID de base de dados para o nome do ficheiro webp
    private string ObterFicheiroPorId(string dbId)
    {
        if (dbId.StartsWith("u_"))
        {
            return $"Users/{dbId.Substring(2)}.png";
        }
        if (dbId == "11_1") return "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp";
        if (dbId == "20_1") return "T_Icon_BR_CokeParmesan_Default_L.webp";
        if (dbId == "21_1") return "T_Icon_BR_CompanyStargazer_Default_L.webp";
        if (dbId == "24_1") return "T_Icon_Reload_FillerGrunt_icon_L.webp";
        if (dbId == "25_1") return "T_Icon_BR_PedicureAntacid_L.webp";

        var partes = dbId.Split('_');
        if (partes.Length == 2)
        {
            int speciesId = int.Parse(partes[0]);
            int variantIndex = int.Parse(partes[1]);

            int sIdx = GetListIndexFromSpeciesId(speciesId);
            if (sIdx >= 0 && sIdx < speciesList.Count)
            {
                var s = speciesList[sIdx];
                string variant = ObterNomeVariantePorIndex(variantIndex);
                if (s.Files.ContainsKey(variant))
                    return s.Files[variant];
            }
        }
        return "";
    }

    private string ObterNomeVariantePorIndex(int index)
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

    // Auxiliar: Resolve ID para nome amigável para exibição no chat
    private string ObterNomeElementalPorId(string dbId)
    {
        if (dbId.StartsWith("u_"))
        {
            return dbId.Substring(2); // Retorna apenas o nome do user
        }
        if (dbId == "11_1") return "BurntPeanut";
        if (dbId == "20_1") return "Vini JR";
        if (dbId == "21_1") return "Pollo";
        if (dbId == "24_1") return "John Wick";
        if (dbId == "25_1") return "Ironmouse";

        var partes = dbId.Split('_');
        if (partes.Length == 2)
        {
            try
            {
                int speciesId = int.Parse(partes[0]);
                int variantIndex = int.Parse(partes[1]);

                int sIdx = GetListIndexFromSpeciesId(speciesId);
                if (sIdx >= 0 && sIdx < speciesList.Count)
                {
                    var s = speciesList[sIdx];
                    string variant = ObterNomeVariantePorIndex(variantIndex);
                    return $"{s.Name} ({char.ToUpper(variant[0]) + variant.Substring(1)})";
                }
            }
            catch {}
        }
        return dbId;
    }

    // Auxiliar: Retorna um nome amigável para exibição
    private string ObterNomeExibicao(string species, string variant)
    {
        if (species.Equals("burntpeanut", StringComparison.OrdinalIgnoreCase))
            return "BurntPeanut";
            
        string varText = char.ToUpper(variant[0]) + variant.Substring(1);
        return $"{species} ({varText})";
    }

    // Helper: Obtém a quantidade atual de um elemental pertencente a um utilizador
    private int ObterQuantidadeElemental(string userId, string elemId)
    {
        int qtd = 0;
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@userId AND elemental_id=@elemId", con))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@elemId", elemId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        qtd = reader.GetInt32(0);
                    }
                }
            }
        }
        return qtd;
    }

    // Helper: Resolve targetId através do nome de utilizador na tabela
    private string ObterUserIdPorUsername(string username)
    {
        string userId = "";
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            using (var cmd = new SQLiteCommand("SELECT user_id FROM utilizadores WHERE username=@username COLLATE NOCASE", con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userId = reader.GetString(0);
                    }
                }
            }
        }
        return userId;
    }

    // Helper: Transação para alterar a quantidade de elementais (+1 ou -1)
    private void AlterarQuantidade(SQLiteConnection con, SQLiteTransaction trans, string userId, string elemId, int delta)
    {
        int qtdAtual = 0;
        using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@userId AND elemental_id=@elemId", con, trans))
        {
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@elemId", elemId);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    qtdAtual = reader.GetInt32(0);
                }
            }
        }

        int novaQtd = qtdAtual + delta;
        if (novaQtd <= 0)
        {
            using (var cmd = new SQLiteCommand("DELETE FROM capturas WHERE user_id=@userId AND elemental_id=@elemId", con, trans))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@elemId", elemId);
                cmd.ExecuteNonQuery();
            }
        }
        else if (qtdAtual > 0)
        {
            using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade=@novaQtd WHERE user_id=@userId AND elemental_id=@elemId", con, trans))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@elemId", elemId);
                cmd.Parameters.AddWithValue("@novaQtd", novaQtd);
                cmd.ExecuteNonQuery();
            }
        }
        else
        {
            using (var cmd = new SQLiteCommand("INSERT INTO capturas (user_id, elemental_id, quantidade) VALUES (@userId, @elemId, @novaQtd)", con, trans))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@elemId", elemId);
                cmd.Parameters.AddWithValue("@novaQtd", novaQtd);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // 1. Inicializa ou verifica a base de dados
    public void InitDB()
    {
        Directory.CreateDirectory(@"I:\Twitch\Games\elementais");

        if (!File.Exists(caminhoBD))
        {
            SQLiteConnection.CreateFile(caminhoBD);
            CPH.LogInfo("Base de dados criada.");
        }

        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS utilizadores (user_id TEXT PRIMARY KEY, username TEXT)", con))
                cmd.ExecuteNonQuery();

            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS capturas (user_id TEXT, elemental_id TEXT, quantidade INT)", con))
                cmd.ExecuteNonQuery();
        }
    }

    private void EscreverEstado(string estado)
    {
        try
        {
            File.WriteAllText(caminhoEstado, estado);
        }
        catch (Exception ex)
        {
            CPH.LogWarn("Erro ao escrever estado: " + ex.Message);
        }
    }

    // 2. Ação de Spawn (para o Temporizador)
    public bool ExecutarSpawn()
    {
        InitDB(); // Garante que a BD existe
        LimparTrocasExpiradas();

        lock (lockFila)
        {
            if (cacaAtiva) return false;

            cacaAtiva = true;
            filaUtilizadores.Clear();
            lastSpawnTime = DateTime.Now;

            // Obter faseAtiva dos Globais do Streamer.bot. Se não definido, default para 1.
            int faseAtiva = CPH.GetGlobalVar<int>("faseAtiva");
            if (faseAtiva < 1 || faseAtiva > 4)
            {
                faseAtiva = 1;
            }

            Random rnd = new Random();
            string selectedFileName = "";
            string selectedDisplayName = "";
            string selectedDbId = "";

            // 1. Rolar 3% de chance de spawn do especial BurntPeanut
            if (rnd.Next(1, 101) <= 3)
            {
                selectedFileName = "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp";
                selectedDisplayName = "BurntPeanut";
                selectedDbId = "11_1";
            }
            else
            {
                // 2. Rolar espécies normais com base nos seus pesos (gacha)
                int totalWeight = 0;
                foreach (var s in speciesList) totalWeight += s.Weight;

                int roll = rnd.Next(0, totalWeight);
                Species chosenSpecies = null;
                int chosenIndex = 0;
                int weightSum = 0;
                for (int i = 0; i < speciesList.Count; i++)
                {
                    weightSum += speciesList[i].Weight;
                    if (roll < weightSum)
                    {
                        chosenSpecies = speciesList[i];
                        chosenIndex = i + 1; // 1-indexed
                        break;
                    }
                }

                // 3. Rolar a variante dependendo da Fase ativa
                string variantName = "normal";
                int variantIndex = 1;
                int variantRoll = rnd.Next(1, 101);

                if (faseAtiva == 1)
                {
                    variantName = "normal";
                    variantIndex = 1;
                }
                else if (faseAtiva == 2)
                {
                    // 80% Normal, 20% Gold
                    if (variantRoll <= 20) { variantName = "gold"; variantIndex = 2; }
                    else { variantName = "normal"; variantIndex = 1; }
                }
                else if (faseAtiva == 3)
                {
                    // 75% Normal, 20% Gold, 5% Candy
                    if (variantRoll <= 5) { variantName = "gummy"; variantIndex = 3; }
                    else if (variantRoll <= 25) { variantName = "gold"; variantIndex = 2; }
                    else { variantName = "normal"; variantIndex = 1; }
                }
                else if (faseAtiva == 4)
                {
                    // 73% Normal, 20% Gold, 5% Candy, 2% Galaxy
                    if (variantRoll <= 2) { variantName = "galaxy"; variantIndex = 4; }
                    else if (variantRoll <= 7) { variantName = "gummy"; variantIndex = 3; }
                    else if (variantRoll <= 27) { variantName = "gold"; variantIndex = 2; }
                    else { variantName = "normal"; variantIndex = 1; }
                }

                selectedFileName = chosenSpecies.Files[variantName];
                selectedDisplayName = ObterNomeExibicao(chosenSpecies.Name, variantName);
                selectedDbId = $"{chosenIndex}_{variantIndex}";
            }

            elementalAtivoId = selectedDbId;
            elementalAtivoNome = selectedDisplayName;

            // Escreve o ficheiro webp e o nome para a Browser Source do OBS ler
            EscreverEstado($"SPAWN;{selectedFileName};{selectedDisplayName}");
            CPH.LogInfo($"Spawn gerado: {selectedDisplayName} (Fase: {faseAtiva}, Ficheiro: {selectedFileName})");
        }
        return true;
    }

    // 3. Ação de Resgate ("Atirar Bola")
    public bool ExecutarResgate()
    {
        if (!args.ContainsKey("userId") || !args.ContainsKey("userName")) return false;

        string userId = args["userId"].ToString();
        string userName = args["userName"].ToString();
        string rewardId = args.ContainsKey("rewardId") ? args["rewardId"].ToString() : "";
        string redemptionId = args.ContainsKey("redemptionId") ? args["redemptionId"].ToString() : "";
        string rawInput = args.ContainsKey("rawInput") ? args["rawInput"].ToString().ToLower() : "";

        string bola = "normal";
        if (args.ContainsKey("tipoBola"))
        {
            bola = args["tipoBola"].ToString().ToLower();
        }
        else
        {
            if (rawInput.Contains("super")) bola = "super";
            else if (rawInput.Contains("ultra")) bola = "ultra";
            else if (rawInput.Contains("master")) bola = "master";
        }

        // Validação 1: Caça não está ativa
        if (!cacaAtiva)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            return false;
        }

        // Validações Anti-spam
        lock (lockFila)
        {
            // Validação 2: Utilizador já está na fila?
            if (filaUtilizadores.Contains(userId))
            {
                CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                return false;
            }

            // Validação 3: Utilizador já tem o limite de 2 deste elemental?
            int qtdAtual = ObterQuantidadeElemental(userId, elementalAtivoId);
            if (qtdAtual >= 2)
            {
                CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                return false;
            }

            // Aprovado para tentar! Entra na fila
            filaUtilizadores.Add(userId);
            EscreverEstado($"ATIRAR;{userName};{bola}");
        }

        // Aguardar 5 segundos pela animação
        Thread.Sleep(5000);

        // Validação 4: A caça terminou enquanto este utilizador esperava?
        lock (lockFila)
        {
            if (!cacaAtiva)
            {
                // Devolve os pontos
                CPH.TwitchRedemptionCancel(rewardId, redemptionId);
                filaUtilizadores.Remove(userId);
                return false;
            }

            // Calcular o sucesso da captura
            int probabilidade = 20;
            if (bola == "super") probabilidade = 40;
            else if (bola == "ultra") probabilidade = 60;
            else if (bola == "master") probabilidade = 90;

            Random rnd = new Random();
            bool sucesso = rnd.Next(1, 101) <= probabilidade;

            if (sucesso)
            {
                cacaAtiva = false; // Fecha a caça para todos os próximos da fila
                EscreverEstado($"SUCESSO;{userName}");

                // Guardar na base de dados
                using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO utilizadores (user_id, username) VALUES (@userId, @userName)", con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@userName", userName);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand("UPDATE utilizadores SET username=@userName WHERE user_id=@userId", con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@userName", userName);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Incrementa a quantidade (limite já validado anteriormente)
                using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    using (var trans = con.BeginTransaction())
                    {
                        AlterarQuantidade(con, trans, userId, elementalAtivoId, 1);
                        trans.Commit();
                    }
                }

                // Aprova o resgate
                CPH.TwitchRedemptionFulfill(rewardId, redemptionId);
                CPH.SendMessage($"O @{userName} conseguiu capturar o {elementalAtivoNome} com uma bola {bola}!");
            }
            else
            {
                // Falhou
                EscreverEstado($"FALHA;{userName}");
                filaUtilizadores.Remove(userId); // Sai da fila para poder tentar de novo
                
                // Aprova o resgate (o utilizador gasta os pontos)
                CPH.TwitchRedemptionFulfill(rewardId, redemptionId);
                CPH.SendMessage($"A bola {bola} falhou para o @{userName}!");
            }
        }

        return true;
    }

    // 4. Comando !colecao
    public bool ExecutarColecao()
    {
        if (cacaAtiva) return false;
        LimparTrocasExpiradas();

        string userId = args.ContainsKey("userId") ? args["userId"].ToString() : args["user"].ToString();
        string userName = args["userName"].ToString();

        // Bloqueio de tempo: Faltar menos de 1 minuto para o próximo spawn esperado
        int intervalMinutes = CPH.GetGlobalVar<int>("spawnIntervalMinutes");
        if (intervalMinutes <= 0) intervalMinutes = 10;

        if (lastSpawnTime != DateTime.MinValue)
        {
            var timeSinceLast = DateTime.Now - lastSpawnTime;
            double secondsLimit = (intervalMinutes * 60) - 60; // 9 minutos se interval = 10
            if (timeSinceLast.TotalSeconds > secondsLimit && timeSinceLast.TotalSeconds < (intervalMinutes * 60))
            {
                CPH.SendMessage($"@{userName}, a caça está quase a começar! O comando está desativado.");
                return false;
            }
        }

        List<string> colecaoData = new List<string>();

        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            using (var cmd = new SQLiteCommand("SELECT elemental_id, quantidade FROM capturas WHERE user_id=@userId", con))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        int qtd = reader.GetInt32(1);
                        colecaoData.Add($"{id}:{qtd}");
                    }
                }
            }
        }

        if (colecaoData.Count > 0)
        {
            string listaFormatada = string.Join(",", colecaoData);
            EscreverEstado($"COLECAO;{userName};{listaFormatada}");
        }
        else
        {
            CPH.SendMessage($"@{userName}, a tua coleção ainda está vazia!");
        }

        return true;
    }

    public bool ExecutarProporTroca()
    {
        InitDB();
        LimparTrocasExpiradas();

        return true;
    }

    // 4. Comando !colecao
    public bool ExecutarColecao()
    {
        if (cacaAtiva) return false;
        LimparTrocasExpiradas();

        string userId = args.ContainsKey("userId") ? args["userId"].ToString() : args["user"].ToString();
        string userName = args["userName"].ToString();

        // Bloqueio de tempo: Faltar menos de 1 minuto para o próximo spawn esperado
        int intervalMinutes = CPH.GetGlobalVar<int>("spawnIntervalMinutes");
        if (intervalMinutes <= 0) intervalMinutes = 10;

        if (lastSpawnTime != DateTime.MinValue)
        {
            var timeSinceLast = DateTime.Now - lastSpawnTime;
            double secondsLimit = (intervalMinutes * 60) - 60; // 9 minutos se interval = 10
            if (timeSinceLast.TotalSeconds > secondsLimit && timeSinceLast.TotalSeconds < (intervalMinutes * 60))
            {
                CPH.SendMessage($"@{userName}, a caça está quase a começar! O comando está desativado.");
                return false;
            }
        }

        List<string> colecaoData = new List<string>();

        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            using (var cmd = new SQLiteCommand("SELECT elemental_id, quantidade FROM capturas WHERE user_id=@userId", con))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        int qtd = reader.GetInt32(1);
                        colecaoData.Add($"{id}:{qtd}");
                    }
                }
            }
        }

        if (colecaoData.Count > 0)
        {
            string listaFormatada = string.Join(",", colecaoData);
            EscreverEstado($"COLECAO;{userName};{listaFormatada}");
        }
        else
        {
            CPH.SendMessage($"@{userName}, a tua coleção ainda está vazia!");
        }

        return true;
    }

    public bool ExecutarProporTroca()
    {
        InitDB();
        LimparTrocasExpiradas();

        string rewardId = args.ContainsKey("rewardId") ? args["rewardId"].ToString() : "";
        string redemptionId = args.ContainsKey("redemptionId") ? args["redemptionId"].ToString() : "";

        if (cacaAtiva)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage("Não podes propor trocas durante uma caça ativa!");
            return false;
        }

        if (!args.ContainsKey("userId") || !args.ContainsKey("userName")) return false;

        string proposerId = args["userId"].ToString();
        string proposerName = args["userName"].ToString();
        string rawInput = args.ContainsKey("rawInput") ? args["rawInput"].ToString().Trim() : "";

        // Esperado: "@Pedro 1_2" ou "@Pedro 11"
        string[] parts = rawInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{proposerName}, formato inválido! Usa: @username [ID_do_teu_elemental] (Ex: @Pedro 1_2)");
            return false;
        }

        string targetName = parts[0].Replace("@", "").Trim();
        string elemProposer = parts[1].Trim().ToLower();

        // Se introduzirem apenas "11", padroniza para "11_1" (BurntPeanut)
        if (elemProposer == "11") elemProposer = "11_1";

        // 1. Validar se o proponente tem o elemental oferecido
        int proposerQty = ObterQuantidadeElemental(proposerId, elemProposer);
        if (proposerQty < 1)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{proposerName}, não tens nenhum elemental com ID '{elemProposer}' para oferecer!");
            return false;
        }

        // 2. Resolver o targetId do alvo
        string targetId = ObterUserIdPorUsername(targetName);
        if (string.IsNullOrEmpty(targetId))
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{proposerName}, @{targetName} não consta na base de dados (precisa de ter jogado pelo menos uma vez!).");
            return false;
        }

        if (targetId == proposerId)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{proposerName}, não podes fazer trocas contigo próprio!");
            return false;
        }

        // 3. Adicionar proposta pendente
        lock (lockTrocas)
        {
            // Remover propostas anteriores idênticas
            pendingTrades.RemoveAll(t => t.ProposerId == proposerId && t.TargetId == targetId);

            var proposal = new TradeProposal
            {
                ProposerId = proposerId,
                ProposerName = proposerName,
                TargetId = targetId,
                TargetName = targetName,
                ElemProposer = elemProposer,
                RewardId = rewardId,
                RedemptionId = redemptionId,
                ExpiryTime = DateTime.Now.AddSeconds(120) // 120s limit
            };
            pendingTrades.Add(proposal);
        }

        CPH.SendMessage($"PROPOSTA: @{proposerName} propõe dar o elemental [{ObterNomeElementalPorId(elemProposer)}] a @{targetName}. @{targetName}, usa o resgate 'Aceitar Troca' escrevendo '@{proposerName} [teu_elemental]' (Limite de 120s)!");
        return true;
    }

    public bool ExecutarAceitarTroca()
    {
        InitDB();
        LimparTrocasExpiradas();

        string rewardId = args.ContainsKey("rewardId") ? args["rewardId"].ToString() : "";
        string redemptionId = args.ContainsKey("redemptionId") ? args["redemptionId"].ToString() : "";

        if (cacaAtiva)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage("Não podes aceitar trocas durante uma caça ativa!");
            return false;
        }

        if (!args.ContainsKey("userId") || !args.ContainsKey("userName")) return false;

        string targetId = args["userId"].ToString();
        string targetName = args["userName"].ToString();
        string rawInput = args.ContainsKey("rawInput") ? args["rawInput"].ToString().Trim() : "";

        // Esperado: "@João 2_3" ou "@João 11"
        string[] parts = rawInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{targetName}, formato inválido! Usa: @proponente [ID_do_teu_elemental] (Ex: @João 2_3)");
            return false;
        }

        string proposerName = parts[0].Replace("@", "").Trim();
        string elemTarget = parts[1].Trim().ToLower();

        if (elemTarget == "11") elemTarget = "11_1";

        // Procura a proposta ativa correspondente
        TradeProposal proposal = null;
        lock (lockTrocas)
        {
            proposal = pendingTrades.Find(t => t.TargetId == targetId && t.ProposerName.Equals(proposerName, StringComparison.OrdinalIgnoreCase) && DateTime.Now <= t.ExpiryTime);
        }

        if (proposal == null)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{targetName}, não tens nenhuma proposta de troca pendente de @{proposerName}!");
            return false;
        }

        string proposerId = proposal.ProposerId;
        string elemProposer = proposal.ElemProposer;

        // 1. Validar se o alvo tem o elemental que quer oferecer
        int targetQty = ObterQuantidadeElemental(targetId, elemTarget);
        if (targetQty < 1)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{targetName}, não tens nenhuma unidade do elemental [{elemTarget}] para oferecer!");
            return false;
        }

        // 2. Validar limite de 2 para o proponente ao receber elemTarget
        int proposerTargetQty = ObterQuantidadeElemental(proposerId, elemTarget);
        if (proposerTargetQty >= 2)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{targetName}, a troca falhou! @{proposerName} já tem o limite de 2 do elemental [{elemTarget}].");
            return false;
        }

        // 3. Validar limite de 2 para o alvo ao receber elemProposer
        int targetProposerQty = ObterQuantidadeElemental(targetId, elemProposer);
        if (targetProposerQty >= 2)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage($"@{targetName}, a troca falhou! Tu já tens o limite de 2 do elemental [{elemProposer}].");
            return false;
        }

        // 4. Executar transação de troca
        try
        {
            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                using (var trans = con.BeginTransaction())
                {
                    // Proponente: dá elemProposer (-1), ganha elemTarget (+1)
                    AlterarQuantidade(con, trans, proposerId, elemProposer, -1);
                    AlterarQuantidade(con, trans, proposerId, elemTarget, 1);

                    // Alvo: dá elemTarget (-1), ganha elemProposer (+1)
                    AlterarQuantidade(con, trans, targetId, elemTarget, -1);
                    AlterarQuantidade(con, trans, targetId, elemProposer, 1);

                    trans.Commit();
                }
            }

            // Aprovar ambos os resgates na Twitch
            CPH.TwitchRedemptionFulfill(proposal.RewardId, proposal.RedemptionId);
            CPH.TwitchRedemptionFulfill(rewardId, redemptionId);

            // Remover da lista de trocas
            lock (lockTrocas)
            {
                pendingTrades.Remove(proposal);
            }

            // Obter nomes dos ficheiros e escrever estado de TROCA para o OBS
            string file1 = ObterFicheiroPorId(elemProposer);
            string file2 = ObterFicheiroPorId(elemTarget);
            EscreverEstado($"TROCA;{proposal.ProposerName};{targetName};{file1};{file2}");

            CPH.SendMessage($"TROCA CONCLUÍDA! @{proposal.ProposerName} trocou o seu [{ObterNomeElementalPorId(elemProposer)}] pelo [{ObterNomeElementalPorId(elemTarget)}] de @{targetName}!");
        }
        catch (Exception ex)
        {
            CPH.LogWarn("Erro a transacionar troca na BD: " + ex.Message);
            CPH.TwitchRedemptionCancel(proposal.RewardId, proposal.RedemptionId);
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
            CPH.SendMessage("Erro técnico na transação da base de dados. Ambas as propostas foram rejeitadas.");
        }

        return true;
    }

    // Limpa propostas que passaram da data de expiração
    private void LimparTrocasExpiradas()
    {
        lock (lockTrocas)
        {
            var agora = DateTime.Now;
            for (int i = pendingTrades.Count - 1; i >= 0; i--)
            {
                var trade = pendingTrades[i];
                if (agora > trade.ExpiryTime)
                {
                    try
                    {
                        CPH.TwitchRedemptionCancel(trade.RewardId, trade.RedemptionId);
                        CPH.SendMessage($"A proposta de troca de @{trade.ProposerName} para @{trade.TargetName} expirou e os pontos foram devolvidos.");
                    }
                    catch (Exception ex)
                    {
                        CPH.LogWarn("Erro ao rejeitar proposta expirada: " + ex.Message);
                    }
                    pendingTrades.RemoveAt(i);
                }
            }
        }
    }

    public bool Execute()
    {
        return ExecutarProporTroca();
    }
}
