using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

public class CPHInline
{
    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";

    private static readonly Dictionary<string, string> especiesNomeParaId = new Dictionary<string, string>
    {
        { "water", "1" }, { "earth", "2" }, { "fire", "3" }, { "duck", "4" }, { "ghost", "5" },
        { "sleepy", "6" }, { "demon", "7" }, { "punk", "8" }, { "king", "9" }, { "zeropoint", "10" },
        { "burntpeanut", "11" }, { "fishy", "12" }, { "soccer", "13" }, { "drifter", "14" }, { "aura", "14" },
        { "boss", "15" }, { "grim", "16" }, { "air", "17" }, { "ar", "17" }, { "seven", "18" }, { "batman", "19" },
        { "vinijr", "20" }, { "pollo", "21" },
        // Aliases em português e novas espécies
        { "agua", "1" }, { "terra", "2" }, { "fogo", "3" }, { "pato", "4" }, { "fantasma", "5" },
        { "sonhos", "6" }, { "dossonhos", "6" }, { "demonio", "7" }, { "rei", "9" }, { "pontozero", "10" },
        { "peixoto", "12" }, { "atacante", "13" }, { "llama", "22" }, { "lhama", "22" },
        { "peely", "23" }, { "banana", "23" }, { "johnwick", "24" }, { "john", "24" }, { "wick", "24" },
        { "ironmouse", "25" }, { "iron", "25" }, { "mouse", "25" }
    };

    private static readonly Dictionary<string, string[]> arquivosVariantes = new Dictionary<string, string[]>
    {
        { "1", new string[] { "T_Icon_BR_Creature_Sprite_Water_Unvault_Ch7S3_ui_L.webp", "T_Icon_BR_Creature_Sprite_Water_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Water_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Water_Galaxy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Water_Holofoil_ui_L.webp", "", "T_Icon_BR_Creature_Sprite_Water_Gem_ui_L.webp", "T_Icon_BR_Creature_Sprite_Water_Quack_ui_L.webp" } },
        { "2", new string[] { "T_Icon_BR_Creature_Sprite_Earth_Ch7S3_UI_L.webp", "T_Icon_BR_Creature_Sprite_Earth_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Earth_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Earth_Galaxy_ui_L.webp", "", "T_Icon_BR_Creature_Sprite_Earth_Cube_ui_L.webp", "T_Icon_BR_Creature_Sprite_Earth_Gem_ui_L.webp", "T_Icon_BR_Creature_Sprite_Earth_Quack_ui_L.webp" } },
        { "3", new string[] { "T_Icon_BR_Creature_Sprite_Fire_Unvault_Ch7S3_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fire_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fire_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fire_Galaxy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fire_Holofoil_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fire_Cube_ui_L.webp", "", "T_Icon_BR_Creature_Sprite_Fire_Quack_ui_L.webp" } },
        { "4", new string[] { "T_Icon_BR_Duck_Default_L.webp", "T_Icon_BR_Duck_Gold_L.webp", "T_Icon_BR_Duck_Candy_L.webp", "T_Icon_BR_Duck_Galaxy_L.webp", "", "", "T_Icon_BR_Duck_Gem_L.webp" } },
        { "5", new string[] { "T_Icon_BR_Creature_Sprite_Ghost_Unvault_L.webp", "T_Icon_BR_Creature_Sprite_Ghost_Gold_L.webp", "T_Icon_BR_Creature_Sprite_Ghost_Candy_L.webp", "T_Icon_BR_Creature_Sprite_Ghost_Galaxy_L.webp", "T_Icon_BR_Creature_Sprite_Ghost_Holo_L.webp", "" } },
        { "6", new string[] { "T_Icon_BR_Creature_Sprite_Sleepy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Sleepy_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Sleepy_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Sleepy_Galaxy_ui_L.webp", "", "T_Icon_BR_Creature_Sprite_Sleepy_Cube_ui_L.webp" } },
        { "7", new string[] { "T_Icon_BR_RedDemon_Default_L.webp", "T_Icon_BR_RedDemon_Gold_L.webp", "T_Icon_BR_RedDemon_Candy_L.webp", "T_Icon_BR_RedDemon_Galaxy_L.webp", "", "", "T_Icon_BR_RedDemon_Gem_L.webp" } },
        { "8", new string[] { "T_Icon_BR_Creature_Sprite_Punk_ui_L.webp", "T_Icon_BR_Creature_Sprite_Punk_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Punk_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Punk_Galaxy_ui_L.webp", "", "T_Icon_BR_Creature_Sprite_Punk_Cube_ui_L.webp" } },
        { "9", new string[] { "T_Icon_BR_Creature_Sprite_King_ui_L.webp", "T_Icon_BR_Creature_Sprite_King_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_King_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_King_Galaxy_ui_L.webp", "T_Icon_BR_Creature_Sprite_King_Holofoil_ui_L.webp", "" } },
        { "10", new string[] { "T_Icon_BR_Creature_Sprite_ZeroPoint_ui_L.webp", "T_Icon_BR_Creature_Sprite_ZeroPoint_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_ZeroPoint_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_ZeroPoint_Galaxy_ui_L.webp", "T_Icon_BR_Creature_Sprite_ZeroPoint_Holofoil_ui_L.webp", "T_Icon_BR_Creature_Sprite_ZeroPoint_Cube_ui_L.webp", "T_Icon_BR_Creature_Sprite_ZeroPoint_Gem_ui_L.webp", "T_Icon_BR_Creature_Sprite_ZeroPoint_Quack_ui_L.webp" } },
        { "11", new string[] { "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp", "", "", "", "", "" } },
        { "12", new string[] { "T_Icon_BR_Creature_Sprite_Fishy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fishy_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fishy_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Fishy_Galaxy_ui_L.webp", "", "T_Icon_BR_Creature_Sprite_Fishy_Cube_L.webp" } },
        { "13", new string[] { "T_Icon_BR_Creature_Sprite_Soccer_ui_L.webp", "T_Icon_BR_Creature_Sprite_Soccer_Gold_L.webp", "T_Icon_BR_Creature_Sprite_Soccer_Candy_L.webp", "T_Icon_BR_Creature_Sprite_Soccer_Galaxy_L.webp", "T_Icon_BR_Creature_Sprite_Soccer_Holofoil_L.webp", "" } },
        { "14", new string[] { "T_Icon_BR_Creature_Sprite_Drifter_ui_L.webp", "T_Icon_BR_Creature_Sprite_Drifter_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Drifter_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Drifter_Galaxy_ui_L.webp", "", "", "T_Icon_BR_Creature_Sprite_Drifter_Gem_ui_L.webp" } },
        { "15", new string[] { "T_Icon_BR_Creature_Sprite_Boss_ui_L.webp", "T_Icon_BR_Creature_Sprite_Boss_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Boss_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Boss_Galaxy_ui_L.webp", "", "T_Icon_BR_Creature_Sprite_Boss_Cube_ui_L.webp" } },
        { "16", new string[] { "T_Icon_BR_GrimReaper_Default_L.webp", "T_Icon_BR_GrimReaper_Gold_L.webp", "T_Icon_BR_GrimReaper_Candy_L.webp", "T_Icon_BR_GrimReaper_Galaxy_L.webp", "T_Icon_BR_GrimReaper_Holofoil_L.webp", "T_Icon_BR_GrimReaper_Cube_L.webp", "T_Icon_BR_GrimReaper_Gem_L.webp" } },
        { "17", new string[] { "T_Icon_BR_Air_Default_L.webp", "T_Icon_BR_Air_Gold_L.webp", "T_Icon_BR_Air_Candy_L.webp", "T_Icon_BR_Air_Galaxy_L.webp", "T_Icon_BR_Air_Holo_L.webp", "" } },
        { "18", new string[] { "T_Icon_BR_Creature_Sprite_Seven_ui_L.webp", "T_Icon_BR_Creature_Sprite_Seven_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Seven_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Seven_Galaxy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Seven_Holofoil_ui_L.webp", "" } },
        { "19", new string[] { "T_Icon_BR_FossilMeal_Default_L.webp", "T_Icon_BR_FossilMeal_Gold_L.webp", "T_Icon_BR_FossilMeal_Candy_L.webp", "T_Icon_BR_FossilMeal_Galaxy_L.webp", "T_Icon_BR_FossilMeal_Holofoil_L.webp", "T_Icon_BR_FossilMeal_Cube_L.webp" } },
        { "20", new string[] { "T_Icon_BR_CokeParmesan_Default_L.webp", "", "", "", "", "" } },
        { "21", new string[] { "T_Icon_BR_CompanyStargazer_Default_L.webp", "", "", "", "", "" } },
        { "22", new string[] { "T_Icon_BR_Creature_Sprite_Llama_ui_L.webp", "T_Icon_BR_Creature_Sprite_Llama_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Llama_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Llama_Galaxy_ui_L.webp", "", "", "T_Icon_BR_Creature_Sprite_Llama_Gem_ui_L.webp" } },
        { "23", new string[] { "T_Icon_BR_Creature_Sprite_Peely_ui_L.webp", "T_Icon_BR_Creature_Sprite_Peely_Gold_ui_L.webp", "T_Icon_BR_Creature_Sprite_Peely_Candy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Peely_Galaxy_ui_L.webp", "T_Icon_BR_Creature_Sprite_Peely_Holofoil_ui_L.webp", "" } },
        { "24", new string[] { "T_Icon_Reload_FillerGrunt_icon_L.webp", "", "", "", "", "" } },
        { "25", new string[] { "T_Icon_BR_PedicureAntacid_L.webp", "", "", "", "", "" } }
    };

    public bool Execute()
    {
        string userId = args.ContainsKey("userId") ? args["userId"].ToString() : "";
        string userName = args.ContainsKey("userName") ? args["userName"].ToString() : "";
        string rewardId = args.ContainsKey("rewardId") ? args["rewardId"].ToString() : "";
        string redemptionId = args.ContainsKey("redemptionId") ? args["redemptionId"].ToString() : "";
        string rawInput = args.ContainsKey("rawInput") ? args["rawInput"].ToString().Trim() : "";

        if (string.IsNullOrEmpty(rawInput))
        {
            DevolverPontos(rewardId, redemptionId, userName, "Uso incorreto! Para propor: [NomeDoJogador] [TeuElemental] [ElementalPedido]. Para responder: [sim|nao] [NomeDoProponente].");
            return true;
        }

        string[] parts = rawInput.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        // Garante que a BD e a tabela existam
        InitDB();

        // Limpar propostas expiradas (mais de 20 minutos)
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            LimparPropostasExpiradas(con);
        }

        if (parts.Length == 2)
        {
            // MODO DE RESPOSTA: sim/nao [proponente]
            string acao = parts[0].ToLower().Replace("ã", "a");
            string proposerName = parts[1].Replace("@", "").Trim().ToLower();

            if (acao != "sim" && acao != "nao" && acao != "não")
            {
                DevolverPontos(rewardId, redemptionId, userName, "Ação de resposta inválida! Usa 'sim' ou 'nao'.");
                return true;
            }

            ProcessarResposta(acao, proposerName, userId, userName, rewardId, redemptionId);
        }
        else if (parts.Length == 3)
        {
            // MODO DE PROPOSTA: [jogadorAlvo] [meuElemental] [elementalDele]
            string targetName = parts[0].Replace("@", "").Trim().ToLower();
            string elemOferecidoFriendly = parts[1].Trim();
            string elemPedidoFriendly = parts[2].Trim();

            string elemOferecido = ResolverElementalId(elemOferecidoFriendly);
            string elemPedido = ResolverElementalId(elemPedidoFriendly);

            ProcessarProposta(targetName, elemOferecido, elemPedido, userId, userName, rewardId, redemptionId);
        }
        else
        {
            DevolverPontos(rewardId, redemptionId, userName, "Formato inválido! Proposta: [Jogador] [TeuElemental] [ElementalPedido]. Resposta: [sim|nao] [Proponente].");
        }

        return true;
    }

    private void ProcessarProposta(string targetName, string elemOferecido, string elemPedido, string userId, string userName, string rewardId, string redemptionId)
    {
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();

            // 1. Procurar o utilizador alvo na BD
            string targetId = null;
            string targetNameReal = null;
            using (var cmd = new SQLiteCommand("SELECT user_id, username FROM utilizadores WHERE LOWER(username) = @name", con))
            {
                cmd.Parameters.AddWithValue("@name", targetName);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        targetId = reader["user_id"].ToString();
                        targetNameReal = reader["username"].ToString();
                    }
                }
            }

            if (targetId == null)
            {
                DevolverPontos(rewardId, redemptionId, userName, $"@{targetName} não consta na base de dados (deve ter jogado pelo menos uma vez).");
                return;
            }

            if (targetId == userId)
            {
                DevolverPontos(rewardId, redemptionId, userName, "Não podes propor uma troca contigo mesmo!");
                return;
            }

            // 2. Verificar se já existe exatamente esta proposta pendente
            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM propostas_troca WHERE proposer_id=@pid AND target_id=@tid AND elem_proposer=@elemP AND elem_target=@elemT", con))
            {
                cmd.Parameters.AddWithValue("@pid", userId);
                cmd.Parameters.AddWithValue("@tid", targetId);
                cmd.Parameters.AddWithValue("@elemP", elemOferecido);
                cmd.Parameters.AddWithValue("@elemT", elemPedido);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    DevolverPontos(rewardId, redemptionId, userName, "Já tens uma proposta idêntica pendente com esta pessoa!");
                    return;
                }
            }

            // 2.5 Verificar limites de trocas (máximo 5, recuperação em 2h)
            int propTrocasDisp = ObterTrocasDisponiveis(con, userId);
            if (propTrocasDisp < 1)
            {
                string tempoEspera = ObterTempoProximaRecuperacao(con, userId);
                DevolverPontos(rewardId, redemptionId, userName, $"não tens trocas disponíveis de momento (máximo 5, cada troca recupera em 2h)! Próxima recuperação em: {tempoEspera}.");
                return;
            }

            int targetTrocasDisp = ObterTrocasDisponiveis(con, targetId);
            if (targetTrocasDisp < 1)
            {
                string tempoEspera = ObterTempoProximaRecuperacao(con, targetId);
                DevolverPontos(rewardId, redemptionId, userName, $"@{targetNameReal} não tem trocas disponíveis no momento! Próxima recuperação em: {tempoEspera}.");
                return;
            }

            // 3. Verificar se o proponente tem o elemental oferecido disponível
            int dispProposer = ObterQuantidadeDisponivel(con, userId, elemOferecido);
            if (dispProposer < 1)
            {
                DevolverPontos(rewardId, redemptionId, userName, $"Não tens o elemental {GetNomeLegivelBicho(elemOferecido)} disponível para propor (ou está bloqueado noutra troca).");
                return;
            }

            // 4. Verificar se o alvo tem o elemental pedido disponível
            int dispTarget = ObterQuantidadeDisponivel(con, targetId, elemPedido);
            if (dispTarget < 1)
            {
                DevolverPontos(rewardId, redemptionId, userName, $"@{targetNameReal} não tem o elemental {GetNomeLegivelBicho(elemPedido)} disponível para trocar.");
                return;
            }

            // 5. Verificar limites de inventário (Capacidade máxima de 2 cópias no total)
            int totalProposerRecebe = ObterQuantidadeTotal(con, userId, elemPedido);
            if (totalProposerRecebe >= 2)
            {
                DevolverPontos(rewardId, redemptionId, userName, $"Já atingiste o limite máximo de 2 cópias de {GetNomeLegivelBicho(elemPedido)} na tua coleção!");
                return;
            }

            int totalTargetRecebe = ObterQuantidadeTotal(con, targetId, elemOferecido);
            if (totalTargetRecebe >= 2)
            {
                DevolverPontos(rewardId, redemptionId, userName, $"@{targetNameReal} já atingiu o limite de 2 cópias de {GetNomeLegivelBicho(elemOferecido)}!");
                return;
            }

            // 5.5 Verificar requisitos de Quacks
            int reqProp = 0;
            if (!ValidarRequisitoQuack(con, userId, elemPedido, out reqProp))
            {
                DevolverPontos(rewardId, redemptionId, userName, $"Não cumpres o requisito de {reqProp} elementais únicos para receber o {GetNomeLegivelBicho(elemPedido)}!");
                return;
            }

            int reqTarg = 0;
            if (!ValidarRequisitoQuack(con, targetId, elemOferecido, out reqTarg))
            {
                DevolverPontos(rewardId, redemptionId, userName, $"@{targetNameReal} não cumpre o requisito de {reqTarg} elementais únicos para receber o {GetNomeLegivelBicho(elemOferecido)}!");
                return;
            }

            // 6. Inserir proposta na base de dados
            using (var cmd = new SQLiteCommand("INSERT INTO propostas_troca (proposer_id, proposer_name, target_id, target_name, elem_proposer, elem_target, reward_id, redemption_id, created_at) VALUES (@pid, @pname, @tid, @tname, @elemP, @elemT, @rId, @redId, @createdAt)", con))
            {
                cmd.Parameters.AddWithValue("@pid", userId);
                cmd.Parameters.AddWithValue("@pname", userName);
                cmd.Parameters.AddWithValue("@tid", targetId);
                cmd.Parameters.AddWithValue("@tname", targetNameReal);
                cmd.Parameters.AddWithValue("@elemP", elemOferecido);
                cmd.Parameters.AddWithValue("@elemT", elemPedido);
                cmd.Parameters.AddWithValue("@rId", rewardId);
                cmd.Parameters.AddWithValue("@redId", redemptionId);
                cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }

            CPH.SendMessage($"🔄 [TROCA] @{userName} propôs trocar {GetNomeLegivelBicho(elemOferecido)} pelo {GetNomeLegivelBicho(elemPedido)} de @{targetNameReal}! Para aceitar/recusar, resgate \"Trocar Elemental\" e escreva: sim @{userName} ou nao @{userName}. Limite: 20 minutos!");

            // Exporta para o site atualizar as propostas
            CPH.RunAction("Elementais - Exportar Site", true);
        }
    }

    private void ProcessarResposta(string acao, string proposerName, string targetId, string targetName, string rewardId, string redemptionId)
    {
        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();

            // 1. Procurar proposta pendente
            string proposerId = null;
            string proposerNameReal = null;
            string elemProposer = null;
            string elemTarget = null;
            string propRewardId = null;
            string propRedemptionId = null;

            using (var cmd = new SQLiteCommand("SELECT proposer_id, proposer_name, elem_proposer, elem_target, reward_id, redemption_id FROM propostas_troca WHERE target_id=@tid AND LOWER(proposer_name)=@pname", con))
            {
                cmd.Parameters.AddWithValue("@tid", targetId);
                cmd.Parameters.AddWithValue("@pname", proposerName);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        proposerId = reader["proposer_id"].ToString();
                        proposerNameReal = reader["proposer_name"].ToString();
                        elemProposer = reader["elem_proposer"].ToString();
                        elemTarget = reader["elem_target"].ToString();
                        propRewardId = reader["reward_id"].ToString();
                        propRedemptionId = reader["redemption_id"].ToString();
                    }
                }
            }

            if (proposerId == null)
            {
                DevolverPontos(rewardId, redemptionId, targetName, $"Não tens nenhuma proposta de troca ativa de @{proposerName}!");
                return;
            }

            if (acao == "nao" || acao == "não")
            {
                // RECUSAR: Deletar proposta, reembolsar ambos os resgates
                using (var cmd = new SQLiteCommand("DELETE FROM propostas_troca WHERE target_id=@tid AND LOWER(proposer_name)=@pname", con))
                {
                    cmd.Parameters.AddWithValue("@tid", targetId);
                    cmd.Parameters.AddWithValue("@pname", proposerName);
                    cmd.ExecuteNonQuery();
                }

                // Devolver pontos do Proponente (A)
                if (!string.IsNullOrEmpty(propRewardId) && !string.IsNullOrEmpty(propRedemptionId))
                    CPH.TwitchRedemptionCancel(propRewardId, propRedemptionId);

                // Devolver pontos do Alvo que recusou (B)
                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionCancel(rewardId, redemptionId);

                CPH.SendMessage($"❌ [TROCA] @{targetName} recusou a proposta de troca de @{proposerNameReal}. Pontos de ambos devolvidos.");
                CPH.RunAction("Elementais - Exportar Site", true);
                return;
            }

            // ACEITAR: Validar novamente se ambos continuam a ter posse e espaço na biblioteca
            int totalPropPossui = ObterQuantidadeTotal(con, proposerId, elemProposer);
            int totalTargPossui = ObterQuantidadeTotal(con, targetId, elemTarget);

            // Validar limites de trocas de ambos (máximo 5)
            int completedProp = ObterTrocasConcluidas(con, proposerId);
            int completedTarg = ObterTrocasConcluidas(con, targetId);

            if (completedProp >= 5 || completedTarg >= 5)
            {
                using (var cmd = new SQLiteCommand("DELETE FROM propostas_troca WHERE target_id=@tid AND LOWER(proposer_name)=@pname", con))
                {
                    cmd.Parameters.AddWithValue("@tid", targetId);
                    cmd.Parameters.AddWithValue("@pname", proposerName);
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(propRewardId) && !string.IsNullOrEmpty(propRedemptionId))
                    CPH.TwitchRedemptionCancel(propRewardId, propRedemptionId);
                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionCancel(rewardId, redemptionId);

                string erroMsg = "";
                if (completedProp >= 5 && completedTarg >= 5)
                    erroMsg = $"Ambos (@{proposerNameReal} e @{targetName}) já atingiram o limite de 5 trocas!";
                else if (completedProp >= 5)
                    erroMsg = $"@{proposerNameReal} já atingiu o limite de 5 trocas!";
                else
                    erroMsg = $"@{targetName}, já atingiste o teu limite de 5 trocas!";

                CPH.SendMessage($"⚠️ [TROCA CANCELADA] {erroMsg} Pontos de ambos devolvidos.");
                CPH.RunAction("Elementais - Exportar Site", true);
                return;
            }

            if (totalPropPossui < 1 || totalTargPossui < 1)
            {
                // Proposta inválida porque um deles já não tem o bicho. Cancela.
                using (var cmd = new SQLiteCommand("DELETE FROM propostas_troca WHERE target_id=@tid AND LOWER(proposer_name)=@pname", con))
                {
                    cmd.Parameters.AddWithValue("@tid", targetId);
                    cmd.Parameters.AddWithValue("@pname", proposerName);
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(propRewardId) && !string.IsNullOrEmpty(propRedemptionId))
                    CPH.TwitchRedemptionCancel(propRewardId, propRedemptionId);
                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionCancel(rewardId, redemptionId);

                CPH.SendMessage($"⚠️ [TROCA] Troca cancelada! Um dos utilizadores já não possui o elemental necessário no seu inventário.");
                CPH.RunAction("Elementais - Exportar Site", true);
                return;
            }

            // Validar limites de 2 cópias
            if (ObterQuantidadeTotal(con, proposerId, elemTarget) >= 2 || ObterQuantidadeTotal(con, targetId, elemProposer) >= 2)
            {
                using (var cmd = new SQLiteCommand("DELETE FROM propostas_troca WHERE target_id=@tid AND LOWER(proposer_name)=@pname", con))
                {
                    cmd.Parameters.AddWithValue("@tid", targetId);
                    cmd.Parameters.AddWithValue("@pname", proposerName);
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(propRewardId) && !string.IsNullOrEmpty(propRedemptionId))
                    CPH.TwitchRedemptionCancel(propRewardId, propRedemptionId);
                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionCancel(rewardId, redemptionId);

                CPH.SendMessage($"⚠️ [TROCA] Troca cancelada! Um dos utilizadores já tem o limite de 2 cópias do elemental que iria receber.");
                CPH.RunAction("Elementais - Exportar Site", true);
                return;
            }

            // Validar requisitos de Quacks
            int checkReqP = 0, checkReqT = 0;
            if (!ValidarRequisitoQuack(con, proposerId, elemTarget, out checkReqP) || !ValidarRequisitoQuack(con, targetId, elemProposer, out checkReqT))
            {
                using (var cmd = new SQLiteCommand("DELETE FROM propostas_troca WHERE target_id=@tid AND LOWER(proposer_name)=@pname", con))
                {
                    cmd.Parameters.AddWithValue("@tid", targetId);
                    cmd.Parameters.AddWithValue("@pname", proposerName);
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(propRewardId) && !string.IsNullOrEmpty(propRedemptionId))
                    CPH.TwitchRedemptionCancel(propRewardId, propRedemptionId);
                if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                    CPH.TwitchRedemptionCancel(rewardId, redemptionId);

                CPH.SendMessage($"⚠️ [TROCA] Troca cancelada! Um dos utilizadores não cumpre os requisitos de elementais únicos necessários para obter o Quack.");
                CPH.RunAction("Elementais - Exportar Site", true);
                return;
            }

            // EXECUTAR SWAP DE ELEMENTAIS (SQLite Transaction)
            using (var trans = con.BeginTransaction())
            {
                try
                {
                    // Proponente dá elemProposer e recebe elemTarget
                    AlterarQuantidade(con, trans, proposerId, elemProposer, -1);
                    AlterarQuantidade(con, trans, proposerId, elemTarget, 1);

                    // Alvo dá elemTarget e recebe elemProposer
                    AlterarQuantidade(con, trans, targetId, elemTarget, -1);
                    AlterarQuantidade(con, trans, targetId, elemProposer, 1);

                    // Registar histórico de troca para o Proponente
                    using (var cmd = new SQLiteCommand("INSERT INTO historico_trocas (user_id, username, parceiro_id, parceiro_name, elem_dado, elem_recebido, data_troca) VALUES (@uid, @uname, @pid, @pname, @dado, @recebido, @data)", con, trans))
                    {
                        cmd.Parameters.AddWithValue("@uid", proposerId);
                        cmd.Parameters.AddWithValue("@uname", proposerNameReal);
                        cmd.Parameters.AddWithValue("@pid", targetId);
                        cmd.Parameters.AddWithValue("@pname", targetName);
                        cmd.Parameters.AddWithValue("@dado", elemProposer);
                        cmd.Parameters.AddWithValue("@recebido", elemTarget);
                        cmd.Parameters.AddWithValue("@data", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }

                    // Registar histórico de troca para o Alvo
                    using (var cmd = new SQLiteCommand("INSERT INTO historico_trocas (user_id, username, parceiro_id, parceiro_name, elem_dado, elem_recebido, data_troca) VALUES (@uid, @uname, @pid, @pname, @dado, @recebido, @data)", con, trans))
                    {
                        cmd.Parameters.AddWithValue("@uid", targetId);
                        cmd.Parameters.AddWithValue("@uname", targetName);
                        cmd.Parameters.AddWithValue("@pid", proposerId);
                        cmd.Parameters.AddWithValue("@pname", proposerNameReal);
                        cmd.Parameters.AddWithValue("@dado", elemTarget);
                        cmd.Parameters.AddWithValue("@recebido", elemProposer);
                        cmd.Parameters.AddWithValue("@data", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }

                    // Deletar a proposta pendente
                    using (var cmd = new SQLiteCommand("DELETE FROM propostas_troca WHERE target_id=@tid AND LOWER(proposer_name)=@pname", con, trans))
                    {
                        cmd.Parameters.AddWithValue("@tid", targetId);
                        cmd.Parameters.AddWithValue("@pname", proposerName);
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    DevolverPontos(rewardId, redemptionId, targetName, "Erro técnico na Base de Dados ao concluir a troca.");
                    CPH.LogWarn("Erro na transação de troca: " + ex.Message);
                    return;
                }
            }

            // Fulfill redemptions
            if (!string.IsNullOrEmpty(propRewardId) && !string.IsNullOrEmpty(propRedemptionId))
                CPH.TwitchRedemptionFulfill(propRewardId, propRedemptionId);
            if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
                CPH.TwitchRedemptionFulfill(rewardId, redemptionId);

            // Escrever comando do overlay no OBS
            string fileProposer = ObterFicheiroWebp(elemProposer);
            string fileTarget = ObterFicheiroWebp(elemTarget);
            try { File.WriteAllText(caminhoEstado, $"TROCA;{proposerNameReal};{targetName};{fileProposer};{fileTarget}"); } catch {}

            int restProp = Math.Max(0, 5 - (completedProp + 1));
            int restTarg = Math.Max(0, 5 - (completedTarg + 1));
            CPH.SendMessage($"🤝 [TROCA CONCLUÍDA] @{proposerNameReal} trocou {GetNomeLegivelBicho(elemProposer)} com @{targetName} por um {GetNomeLegivelBicho(elemTarget)}! 🎉 (Trocas restantes: @{proposerNameReal}: {restProp}/5 | @{targetName}: {restTarg}/5)");

            // Atualizar coleções e site
            CPH.RunAction("Elementais - Exportar Site", true);
        }
    }

    private void AlterarQuantidade(SQLiteConnection con, SQLiteTransaction trans, string userId, string elementalId, int valor)
    {
        using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO capturas (user_id, elemental_id, quantidade) VALUES (@uid, @eid, 0)", con, trans))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elementalId);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new SQLiteCommand("UPDATE capturas SET quantidade = quantidade + @val WHERE user_id=@uid AND elemental_id=@eid", con, trans))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elementalId);
            cmd.Parameters.AddWithValue("@val", valor);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new SQLiteCommand("DELETE FROM capturas WHERE user_id=@uid AND elemental_id=@eid AND quantidade <= 0", con, trans))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elementalId);
            cmd.ExecuteNonQuery();
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

    private int ObterQuantidadeTotal(SQLiteConnection con, string userId, string elemId)
    {
        using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@eid", elemId);
            object res = cmd.ExecuteScalar();
            if (res != null && res != DBNull.Value) return Convert.ToInt32(res);
        }
        return 0;
    }

    private string ResolverElementalId(string input)
    {
        if (input.StartsWith("u_", StringComparison.OrdinalIgnoreCase))
        {
            string inputLower = input.Trim().ToLower();
            
            // 1. Tentar encontrar com a capitalização exata na base de dados
            try
            {
                using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand("SELECT DISTINCT elemental_id FROM capturas WHERE LOWER(elemental_id) = @eid", con))
                    {
                        cmd.Parameters.AddWithValue("@eid", inputLower);
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                        {
                            return res.ToString();
                        }
                    }
                }
            }
            catch {}

            // 2. Se não estiver na BD, procurar na pasta de Sprites dos utilizadores
            string pastaUsers = @"I:\Twitch\Games\elementais\Sprites\Users";
            if (Directory.Exists(pastaUsers))
            {
                string nomeAlvo = inputLower.Substring(2);
                foreach (string filepath in Directory.GetFiles(pastaUsers, "*.png"))
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(filepath);
                    if (nameWithoutExt.ToLower() == nomeAlvo)
                    {
                        return "u_" + nameWithoutExt;
                    }
                }
            }
            return input;
        }

        input = input.Trim().ToLower();

        if (input.Contains("_"))
        {
            return input;
        }

        if (input == "burntpeanut")
        {
            return "11_1";
        }

        int i = input.Length - 1;
        while (i >= 0 && char.IsDigit(input[i]))
        {
            i--;
        }

        string especieDigitada = "";
        int varianteDigitada = 1;

        if (i < input.Length - 1)
        {
            especieDigitada = input.Substring(0, i + 1);
            int.TryParse(input.Substring(i + 1), out varianteDigitada);
        }
        else
        {
            especieDigitada = input;
        }

        if (especiesNomeParaId.ContainsKey(especieDigitada))
        {
            string especieId = especiesNomeParaId[especieDigitada];
            return $"{especieId}_{varianteDigitada}";
        }

        return "u_" + input;
    }

    private string ObterFicheiroWebp(string id)
    {
        if (id.StartsWith("u_"))
        {
            string username = id.Substring(2).ToLower();
            string pastaUsers = @"I:\Twitch\Games\elementais\Sprites\Users";
            if (Directory.Exists(pastaUsers))
            {
                var files = Directory.GetFiles(pastaUsers, "*.png");
                foreach (var f in files)
                {
                    string filename = Path.GetFileName(f);
                    if (Path.GetFileNameWithoutExtension(filename).ToLower() == username)
                    {
                        return "Users/" + filename;
                    }
                }
            }
            return "Users/default.png";
        }

        if (id == "11_1") return "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp";
        if (id == "20_1") return "T_Icon_BR_CokeParmesan_Default_L.webp";
        if (id == "21_1") return "T_Icon_BR_CompanyStargazer_Default_L.webp";
        if (id == "24_1") return "T_Icon_Reload_FillerGrunt_icon_L.webp";
        if (id == "25_1") return "T_Icon_BR_PedicureAntacid_L.webp";

        try
        {
            string[] partes = id.Split('_');
            string especieId = partes[0];
            int varianteDigitada = int.Parse(partes[1]);
            if (arquivosVariantes.ContainsKey(especieId))
            {
                return arquivosVariantes[especieId][varianteDigitada - 1];
            }
        }
        catch {}

        return "";
    }

    private string GetNomeLegivelBicho(string id)
    {
        if (id.StartsWith("u_"))
        {
            string name = id.Substring(2);
            if (name.Length > 0)
                name = char.ToUpper(name[0]) + name.Substring(1);
            return name;
        }

        try
        {
            string[] partes = id.Split('_');
            int especieId = int.Parse(partes[0]);
            int varianteId = int.Parse(partes[1]);

            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(@"
                    SELECT e.nome, v.nome, e.raridade
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
                            string raridade = reader[2].ToString();
                            if (raridade == "custom")
                            {
                                return espNome;
                            }
                            string varNome = reader[1] != DBNull.Value ? reader[1].ToString() : "Normal";
                            return string.Format("{0} ({1})", espNome, varNome);
                        }
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

    private void DevolverPontos(string rewardId, string redemptionId, string userName, string mensagem)
    {
        if (!string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(redemptionId))
            CPH.TwitchRedemptionCancel(rewardId, redemptionId);
        CPH.SendMessage($"@{userName}, {mensagem} Pontos devolvidos.");
    }

    private int ObterTrocasDisponiveis(SQLiteConnection con, string userId)
    {
        int completedTrades = 0;
        using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM historico_trocas WHERE user_id = @uid AND data_troca > datetime('now', '-2 hours')", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            completedTrades = Convert.ToInt32(cmd.ExecuteScalar());
        }

        int pendingProposals = 0;
        using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM propostas_troca WHERE proposer_id = @uid", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            pendingProposals = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return Math.Max(0, 5 - completedTrades - pendingProposals);
    }

    private int ObterTrocasConcluidas(SQLiteConnection con, string userId)
    {
        using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM historico_trocas WHERE user_id = @uid AND data_troca > datetime('now', '-2 hours')", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    private string ObterTempoProximaRecuperacao(SQLiteConnection con, string userId)
    {
        using (var cmd = new SQLiteCommand("SELECT MIN(data_troca) FROM historico_trocas WHERE user_id = @uid AND data_troca > datetime('now', '-2 hours')", con))
        {
            cmd.Parameters.AddWithValue("@uid", userId);
            object minDateObj = cmd.ExecuteScalar();
            if (minDateObj != null && minDateObj != DBNull.Value)
            {
                try
                {
                    DateTime minDate = DateTime.Parse(minDateObj.ToString());
                    DateTime nextRecovery = minDate.AddHours(2);
                    TimeSpan timeLeft = nextRecovery - DateTime.UtcNow;
                    if (timeLeft.TotalSeconds > 0)
                    {
                        List<string> parts = new List<string>();
                        if (timeLeft.Days > 0) parts.Add($"{timeLeft.Days}d");
                        if (timeLeft.Hours > 0) parts.Add($"{timeLeft.Hours}h");
                        if (timeLeft.Minutes > 0) parts.Add($"{timeLeft.Minutes}m");
                        if (parts.Count == 0) return "menos de 1m";
                        return string.Join(" ", parts);
                    }
                }
                catch {}
            }
        }
        return "agora";
    }

    private bool ValidarRequisitoQuack(SQLiteConnection con, string userId, string elementalId, out int reqNecessario)
    {
        reqNecessario = 0;
        if (elementalId == "1_8") reqNecessario = 20;
        else if (elementalId == "2_8") reqNecessario = 40;
        else if (elementalId == "3_8") reqNecessario = 75;
        else if (elementalId == "10_8") reqNecessario = 100;

        if (reqNecessario > 0)
        {
            int uniqueCount = 0;
            using (var cmd = new SQLiteCommand("SELECT COUNT(DISTINCT elemental_id) FROM capturas WHERE user_id=@uid AND quantidade > 0 AND elemental_id NOT LIKE '%_8'", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) uniqueCount = Convert.ToInt32(res);
            }
            return uniqueCount >= reqNecessario;
        }
        return true;
    }

    private void LimparPropostasExpiradas(SQLiteConnection con)
    {
        List<(string proposerId, string proposerName, string targetId, string targetName, string rewardId, string redemptionId, string createdAt)> expired = new List<(string, string, string, string, string, string, string)>();

        using (var cmd = new SQLiteCommand("SELECT proposer_id, proposer_name, target_id, target_name, reward_id, redemption_id, created_at FROM propostas_troca", con))
        {
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string pId = reader["proposer_id"].ToString();
                    string pName = reader["proposer_name"].ToString();
                    string tId = reader["target_id"].ToString();
                    string tName = reader["target_name"].ToString();
                    string rId = reader["reward_id"].ToString();
                    string redId = reader["redemption_id"].ToString();
                    string createdAt = reader["created_at"].ToString();

                    try
                    {
                        DateTime created = DateTime.Parse(createdAt).ToUniversalTime();
                        if ((DateTime.UtcNow - created).TotalMinutes > 20.0)
                        {
                            expired.Add((pId, pName, tId, tName, rId, redId, createdAt));
                        }
                    }
                    catch {}
                }
            }
        }

        foreach (var ex in expired)
        {
            // Devolver pontos se houver resgate associado
            if (!string.IsNullOrEmpty(ex.rewardId) && !string.IsNullOrEmpty(ex.redemptionId))
            {
                try
                {
                    CPH.TwitchRedemptionCancel(ex.rewardId, ex.redemptionId);
                }
                catch {}
            }

            // Eliminar da BD
            using (var cmdDel = new SQLiteCommand("DELETE FROM propostas_troca WHERE proposer_id=@pid AND target_id=@tid AND created_at=@created", con))
            {
                cmdDel.Parameters.AddWithValue("@pid", ex.proposerId);
                cmdDel.Parameters.AddWithValue("@tid", ex.targetId);
                cmdDel.Parameters.AddWithValue("@created", ex.createdAt);
                cmdDel.ExecuteNonQuery();
            }

            CPH.SendMessage($"🔄 [TROCA EXPIRADA] A proposta de @{ex.proposerName} para @{ex.targetName} expirou (limite 20m)! Pontos devolvidos.");
        }
    }
}