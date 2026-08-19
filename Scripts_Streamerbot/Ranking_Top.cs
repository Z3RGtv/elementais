using System;
using System.Collections.Generic;
using System.Data.SQLite;

public class CPHInline
{
    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";

    public bool Execute()
    {
        string userId = args.ContainsKey("userId") ? args["userId"].ToString() : "";
        string userName = args.ContainsKey("userName") ? args["userName"].ToString() : "";

        if (string.IsNullOrEmpty(userId)) return true;

        List<KeyValuePair<string, int>> rankingOrdenado = new List<KeyValuePair<string, int>>();
        Dictionary<string, string> nomesPorUser = new Dictionary<string, string>();
        Dictionary<string, int> vitoriasPorUser = new Dictionary<string, int>();

        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            
            string query = @"
                SELECT c.user_id, u.username, u.posicao_vitoria,
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
                GROUP BY c.user_id, u.username, u.posicao_vitoria
                ORDER BY 
                    CASE WHEN u.posicao_vitoria IS NOT NULL AND u.posicao_vitoria > 0 THEN 0 ELSE 1 END ASC,
                    u.posicao_vitoria ASC,
                    total_pontos DESC";

            using (var cmd = new SQLiteCommand(query, con))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string uid = reader["user_id"].ToString();
                        string uname = reader["username"].ToString();
                        int pts = Convert.ToInt32(reader["total_pontos"]);
                        int posVit = (reader["posicao_vitoria"] != DBNull.Value) ? Convert.ToInt32(reader["posicao_vitoria"]) : 0;

                        nomesPorUser[uid] = uname;
                        vitoriasPorUser[uid] = posVit;
                        rankingOrdenado.Add(new KeyValuePair<string, int>(uid, pts));
                    }
                }
            }
        }

        if (rankingOrdenado.Count == 0)
        {
            CPH.SendMessage("🏆 [ELEMENTAIS] O mercado está vazio! Ainda ninguém tem pontos. 🌐 Álbum: https://z3rgtv.github.io/elementais/");
            return true;
        }

        // 1. Constroi o Top 3 Geral
        List<string> linhasTop3 = new List<string>();
        int limiteTop = Math.Min(3, rankingOrdenado.Count);
        
        for (int i = 0; i < limiteTop; i++)
        {
            string uId = rankingOrdenado[i].Key;
            string uName = nomesPorUser[uId];
            int pts = rankingOrdenado[i].Value;
            int vit = vitoriasPorUser.ContainsKey(uId) ? vitoriasPorUser[uId] : 0;

            if (vit > 0)
            {
                linhasTop3.Add(string.Format("{0}º 👑 @{1} (ÁLBUM COMPLETO #{2})", i + 1, uName, vit));
            }
            else
            {
                linhasTop3.Add(string.Format("{0}º @{1} ({2} pts)", i + 1, uName, pts));
            }
        }
        string stringTop3 = string.Join(" | ", linhasTop3);

        // 2. Localiza a posição e os pontos do utilizador que executou o comando
        int posicaoDoViewer = 0;
        int pontosDoViewer = 0;
        int vitoriaDoViewer = 0;

        for (int i = 0; i < rankingOrdenado.Count; i++)
        {
            if (rankingOrdenado[i].Key == userId)
            {
                posicaoDoViewer = i + 1;
                pontosDoViewer = rankingOrdenado[i].Value;
                vitoriaDoViewer = vitoriasPorUser.ContainsKey(userId) ? vitoriasPorUser[userId] : 0;
                break;
            }
        }

        // 3. Monta a mensagem final com o link do site acoplado no fim
        string msgRanking = string.Format("🏆 [ELEMENTAIS] {0}", stringTop3);
        
        if (posicaoDoViewer > 0)
        {
            if (vitoriaDoViewer > 0)
            {
                msgRanking += string.Format(" || 👑 Estás no lugar {0} (Campeão #{1} com Álbum Completo), @{2}!", posicaoDoViewer, vitoriaDoViewer, userName);
            }
            else
            {
                msgRanking += string.Format(" || 🎯 Estás no lugar {0} com {1} pts, @{2}!", posicaoDoViewer, pontosDoViewer, userName);
            }
        }
        else
        {
            msgRanking += string.Format(" || 🎯 Estás no lugar 0 com 0 pts, @{0}!", userName);
        }

        msgRanking += " || 🌐 Álbum Completo: https://z3rgtv.github.io/elementais/";

        CPH.SendMessage(msgRanking);
        return true;
    }
}