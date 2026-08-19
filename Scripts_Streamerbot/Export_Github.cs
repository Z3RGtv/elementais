using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Text;

public class CPHInline
{
    // =========================================================================
    // CONFIGURAÇÃO DO TEU GITHUB - PREENCHE AQUI COM OS TEUS DADOS
    // =========================================================================
    private string githubUser = "Z3RGtv";
    private string githubRepo = "elementais";
    
    private string ObterGithubToken()
    {
        try
        {
            string tokenPath = @"I:\Twitch\Games\elementais\github_token.txt";
            if (File.Exists(tokenPath))
            {
                return File.ReadAllText(tokenPath).Trim();
            }
        }
        catch {}
        return CPH.GetGlobalVar<string>("github_token") ?? "";
    }
    // =========================================================================

    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";

    public bool Execute()
    {
        try
        {
            ExportarParaGitHub();
        }
        catch (Exception ex)
        {
            CPH.LogError("[Site Export] Erro crítico na execução: " + ex.Message);
        }
        return true;
    }

    private void ExportarParaGitHub()
    {
        try
        {
            Dictionary<string, string> nomes = new Dictionary<string, string>();
            Dictionary<string, int> pontos = new Dictionary<string, int>();
            Dictionary<string, List<string>> inventarios = new Dictionary<string, List<string>>();

            var stats = new Dictionary<string, (int total, int sucesso)>();
            stats["normal"] = (0, 0);
            stats["super"] = (0, 0);
            stats["ultra"] = (0, 0);
            stats["master"] = (0, 0);

            List<string> recentList = new List<string>();

            Dictionary<string, int> completedMap = new Dictionary<string, int>();
            Dictionary<string, string> oldestTradeMap = new Dictionary<string, string>();
            Dictionary<string, int> pendingMap = new Dictionary<string, int>();

            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                VerificarPerdaQuacks(con);

                // Garante que a tabela de lancamentos existe
                using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS lancamentos (id INTEGER PRIMARY KEY AUTOINCREMENT, user_id TEXT, username TEXT, elemental_id TEXT, tipo_bola TEXT, sucesso INT, agua_ativa INT, created_at TEXT DEFAULT CURRENT_TIMESTAMP)", con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Query estatísticas
                using (var cmd = new SQLiteCommand("SELECT tipo_bola, COUNT(*), SUM(sucesso) FROM lancamentos GROUP BY tipo_bola", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bola = reader[0].ToString().ToLower();
                            int total = Convert.ToInt32(reader[1]);
                            int sucesso = reader[2] != DBNull.Value ? Convert.ToInt32(reader[2]) : 0;
                            if (stats.ContainsKey(bola))
                            {
                                stats[bola] = (total, sucesso);
                            }
                        }
                    }
                }

                // Query recentes
                using (var cmd = new SQLiteCommand("SELECT username, elemental_id, tipo_bola, sucesso, datetime(created_at, 'localtime') FROM lancamentos ORDER BY id DESC LIMIT 30", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string user = reader[0].ToString();
                            string elem = reader[1].ToString();
                            string bType = reader[2].ToString();
                            int succ = Convert.ToInt32(reader[3]);
                            string dateStr = reader[4].ToString();

                            recentList.Add(string.Format("    {{ \"username\": \"{0}\", \"elementalId\": \"{1}\", \"bola\": \"{2}\", \"sucesso\": {3}, \"date\": \"{4}\" }}",
                                user, elem, bType, succ, dateStr));
                        }
                    }
                }
                
                // Puxa as capturas agrupadas para evitar duplicados na pontuação
                string query = @"
                    SELECT c.user_id, u.username, c.elemental_id, SUM(c.quantidade) as qtd,
                           CASE 
                               WHEN c.elemental_id = '1_8' THEN 300
                               WHEN c.elemental_id = '2_8' THEN 450
                               WHEN c.elemental_id = '3_8' THEN 600
                               WHEN c.elemental_id = '10_8' THEN 900
                               WHEN c.elemental_id LIKE 'u_%' THEN 100
                               ELSE COALESCE(p.pontos, e.pontos_custom, 0)
                           END as pts_unitario
                    FROM capturas c
                    INNER JOIN utilizadores u ON c.user_id = u.user_id
                    LEFT JOIN cfg_especies e ON e.id = CAST(SUBSTR(c.elemental_id, 1, INSTR(c.elemental_id, '_') - 1) AS INTEGER)
                    LEFT JOIN cfg_pontos p ON e.raridade = p.raridade AND p.variante_id = CAST(SUBSTR(c.elemental_id, INSTR(c.elemental_id, '_') + 1) AS INTEGER)
                    WHERE c.quantidade > 0
                    GROUP BY c.user_id, c.elemental_id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string uid = reader["user_id"].ToString();
                            string uname = reader["username"].ToString();
                            string elemId = reader["elemental_id"].ToString();
                            int qtd = Convert.ToInt32(reader["qtd"]);
                            int ptsBicho = Convert.ToInt32(reader["pts_unitario"]);

                            nomes[uid] = uname;

                            if (!inventarios.ContainsKey(uid)) inventarios[uid] = new List<string>();
                            
                            // Adiciona ao formato "ID:QUANTIDADE" para o site ler igual ao teu OBS
                            inventarios[uid].Add(string.Format("\"{0}\":{1}", elemId, qtd));

                            // Contabiliza os pontos (apenas de bichos únicos)
                            if (pontos.ContainsKey(uid)) pontos[uid] += ptsBicho;
                            else pontos[uid] = ptsBicho;
                        }
                    }
                }
                // 1. Obter contagem e data mais antiga de trocas completas nas últimas 2 horas
                using (var cmd = new SQLiteCommand("SELECT user_id, COUNT(*), MIN(data_troca) FROM historico_trocas WHERE data_troca > datetime('now', '-2 hours') GROUP BY user_id", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string uid = reader[0].ToString();
                            int count = Convert.ToInt32(reader[1]);
                            string minDate = reader[2].ToString();
                            completedMap[uid] = count;
                            oldestTradeMap[uid] = minDate;
                        }
                    }
                }

                // 2. Obter contagem de propostas pendentes enviadas por cada utilizador
                using (var cmd = new SQLiteCommand("SELECT proposer_id, COUNT(*) FROM propostas_troca GROUP BY proposer_id", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string uid = reader[0].ToString();
                            int count = Convert.ToInt32(reader[1]);
                            pendingMap[uid] = count;
                        }
                    }
                }

                // 3. Obter dados de vitória (Hall da Fama dos 100%)
                Dictionary<string, int> vitoriaPosMap = new Dictionary<string, int>();
                Dictionary<string, string> vitoriaDataMap = new Dictionary<string, string>();
                using (var cmd = new SQLiteCommand("SELECT user_id, posicao_vitoria, data_vitoria FROM utilizadores WHERE posicao_vitoria IS NOT NULL AND posicao_vitoria > 0", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string uid = reader[0].ToString();
                            int pos = Convert.ToInt32(reader[1]);
                            string dt = reader[2] != DBNull.Value ? reader[2].ToString() : "";
                            vitoriaPosMap[uid] = pos;
                            vitoriaDataMap[uid] = dt;
                        }
                    }
                }
            }

            // Ler pasta Users dinamicamente para exportar todos os utilizadores disponiveis
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

            // Ordena os utilizadores: Primeiro quem completou 100% por ordem de chegada (1º, 2º, 3º...), depois por pontos
            var listaOrdenada = new List<KeyValuePair<string, int>>(pontos);
            listaOrdenada.Sort((x, y) => {
                int pos_x = vitoriaPosMap.ContainsKey(x.Key) ? vitoriaPosMap[x.Key] : 999999;
                int pos_y = vitoriaPosMap.ContainsKey(y.Key) ? vitoriaPosMap[y.Key] : 999999;
                if (pos_x != pos_y) return pos_x.CompareTo(pos_y);
                return y.Value.CompareTo(x.Value);
            });

            // Constroi o texto JSON manualmente de forma limpa e leve
            StringBuilder json = new StringBuilder();
            json.Append("{\n");
            json.Append($"  \"updatedAt\": \"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\",\n");

            // Injetar estatísticas_bolas
            json.Append("  \"estatisticas_bolas\": {\n");
            int statCount = 0;
            foreach (var kv in stats)
            {
                string bola = kv.Key;
                int total = kv.Value.total;
                int sucesso = kv.Value.sucesso;
                double rate = total > 0 ? Math.Round((double)sucesso / total * 100.0, 1) : 0.0;
                string comma = (statCount == stats.Count - 1) ? "" : ",";
                json.Append(string.Format("    \"{0}\": {{ \"total\": {1}, \"sucesso\": {2}, \"rate\": {3} }}{4}\n",
                    bola, total, sucesso, rate.ToString("F1", System.Globalization.CultureInfo.InvariantCulture), comma));
                statCount++;
            }
            json.Append("  },\n");

            // Injetar recentes_lancamentos
            json.Append("  \"recentes_lancamentos\": [\n");
            json.Append(string.Join(",\n", recentList));
            json.Append("\n  ],\n");
            
            // Adicionar availableUsers
            json.Append("  \"availableUsers\": [\n");
            for (int i = 0; i < userFiles.Count; i++)
            {
                string filename = userFiles[i];
                string nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
                string comma = (i == userFiles.Count - 1) ? "" : ",";
                json.Append($"    {{ \"id\": \"u_{nameWithoutExt}\", \"file\": \"Users/{filename}\", \"name\": \"{nameWithoutExt}\" }}{comma}\n");
            }
            json.Append("  ],\n");

            // Query active proposals from DB
            List<string> propostasList = new List<string>();
            using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand("SELECT proposer_name, target_name, elem_proposer, elem_target, created_at FROM propostas_troca", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string propName = reader["proposer_name"].ToString();
                            string targName = reader["target_name"].ToString();
                            string elemProp = reader["elem_proposer"].ToString();
                            string elemTarg = reader["elem_target"].ToString();
                            string createdAt = reader["created_at"].ToString();

                            propostasList.Add(string.Format("    {{ \"proposerName\": \"{0}\", \"targetName\": \"{1}\", \"elemProposer\": \"{2}\", \"elemTarget\": \"{3}\", \"createdAt\": \"{4}\" }}", 
                                propName, targName, elemProp, elemTarg, createdAt));
                        }
                    }
                }
            }

            json.Append("  \"propostas\": [\n");
            json.Append(string.Join(",\n", propostasList));
            json.Append("\n  ],\n");

            json.Append("  \"ranking\": [\n");

            for (int i = 0; i < listaOrdenada.Count; i++)
            {
                string uid = listaOrdenada[i].Key;
                string uname = nomes[uid];
                int pts = listaOrdenada[i].Value;
                string invStr = string.Join(",", inventarios[uid]);

                int completed = completedMap.ContainsKey(uid) ? completedMap[uid] : 0;
                int pending = pendingMap.ContainsKey(uid) ? pendingMap[uid] : 0;
                int available = Math.Max(0, 5 - completed - pending);
                
                string nextRecovery = "null";
                if (completed > 0 && oldestTradeMap.ContainsKey(uid))
                {
                    try
                    {
                        DateTime minDate = DateTime.Parse(oldestTradeMap[uid]);
                        nextRecovery = $"\"{minDate.AddHours(2):yyyy-MM-ddTHH:mm:ssZ}\"";
                    }
                    catch {}
                }

                int vPos = vitoriaPosMap.ContainsKey(uid) ? vitoriaPosMap[uid] : 0;
                string vDate = vitoriaDataMap.ContainsKey(uid) && !string.IsNullOrEmpty(vitoriaDataMap[uid]) ? $"\"{vitoriaDataMap[uid]}\"" : "null";

                json.Append("    {\n");
                json.Append($"      \"username\": \"{uname}\",\n");
                json.Append($"      \"pontos\": {pts},\n");
                json.Append($"      \"posicaoVitoria\": {vPos},\n");
                json.Append($"      \"dataVitoria\": {vDate},\n");
                json.Append($"      \"trocasDisponiveis\": {available},\n");
                json.Append($"      \"proximaRecuperacao\": {nextRecovery},\n");
                json.Append($"      \"inventario\": {{{invStr}}}\n");
                json.Append("    }");
                if (i < listaOrdenada.Count - 1) json.Append(",");
                json.Append("\n");
            }

            json.Append("  ]\n");
            json.Append("}");

            // Guarda uma cópia local para consistência
            try
            {
                File.WriteAllText(@"I:\Twitch\Games\elementais\inventario.json", json.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[Site Export] Não foi possível salvar cópia local: " + ex.Message);
            }

            // Envia para a API do GitHub
            EnviarFicheiroGitHub("inventario.json", json.ToString());

            // Garante que o Portal do Site (site_index.html) é sempre publicado como index.html no GitHub Pages
            try
            {
                if (File.Exists(@"I:\Twitch\Games\elementais\site_index.html"))
                {
                    string siteHtml = File.ReadAllText(@"I:\Twitch\Games\elementais\site_index.html", Encoding.UTF8);
                    EnviarFicheiroGitHub("index.html", siteHtml);
                    EnviarFicheiroGitHub("site_index.html", siteHtml);
                }
                if (File.Exists(@"I:\Twitch\Games\elementais\site.js"))
                {
                    string siteJs = File.ReadAllText(@"I:\Twitch\Games\elementais\site.js", Encoding.UTF8);
                    EnviarFicheiroGitHub("site.js", siteJs);
                }
                if (File.Exists(@"I:\Twitch\Games\elementais\script.js"))
                {
                    string scriptJs = File.ReadAllText(@"I:\Twitch\Games\elementais\script.js", Encoding.UTF8);
                    EnviarFicheiroGitHub("script.js", scriptJs);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[Site Export] Não foi possível sincronizar ficheiros do site: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            CPH.LogError("[Site Export] Erro na exportação: " + ex.Message);
        }
    }

    private void EnviarFicheiroGitHub(string path, string conteudo)
    {
        using (var client = new WebClient())
        {
            client.Headers.Add("User-Agent", "StreamerBot-Exporter");
            client.Headers.Add("Authorization", "token " + githubToken);
            client.Headers.Add("Content-Type", "application/json");
            client.Encoding = Encoding.UTF8;

            string url = $"https://api.github.com/repos/{githubUser}/{githubRepo}/contents/{path}";

            // Passo 1: O GitHub exige saber o "sha" do ficheiro se ele já existir
            string sha = null;
            try
            {
                string resBody = client.DownloadString(url);
                int shaIndex = resBody.IndexOf("\"sha\":\"");
                if (shaIndex != -1)
                {
                    sha = resBody.Substring(shaIndex + 7, 40);
                }
            }
            catch (Exception ex)
            {
                CPH.LogWarn("[Site Export] Ficheiro não existe no GitHub ou erro ao ler SHA: " + ex.Message);
            }

            // Passo 2: Prepara o corpo do upload com o conteúdo em Base64
            string base64Conteudo = Convert.ToBase64String(Encoding.UTF8.GetBytes(conteudo));
            
            // Construir o JSON de envio de forma simples e segura
            string body = "{\n" +
                          $"  \"message\": \"Atualização automática do inventário da Stream\",\n" +
                          $"  \"content\": \"{base64Conteudo}\"";
            if (sha != null) body += $",\n  \"sha\": \"{sha}\"";
            body += "\n}";

            try
            {
                // Re-adiciona os headers porque o WebClient os limpa/reseta após a primeira requisição (DownloadString)
                client.Headers.Clear();
                client.Headers.Add("User-Agent", "StreamerBot-Exporter");
                client.Headers.Add("Authorization", "token " + ObterGithubToken());
                client.Headers.Add("Content-Type", "application/json");

                string response = client.UploadString(url, "PUT", body);
                CPH.LogInfo("[Site Export] Inventário sincronizado com o GitHub com sucesso!");
            }
            catch (Exception ex)
            {
                CPH.LogError("[Site Export] Falha ao enviar para o GitHub: " + ex.Message);
            }
        }
    }

    private void VerificarPerdaQuacks(SQLiteConnection con)
    {
        try
        {
            var milestones = new (int req, string elemId, string nome)[]
            {
                (100, "10_8", "Ponto Zero Quack"),
                (75, "3_8", "Fogo Quack"),
                (40, "2_8", "Terra Quack"),
                (20, "1_8", "Água Quack")
            };

            List<string> userIds = new List<string>();
            using (var cmd = new SQLiteCommand("SELECT DISTINCT user_id FROM capturas", con))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) userIds.Add(reader[0].ToString());
                }
            }

            foreach (string uid in userIds)
            {
                // Conta elementais únicos EXCLUINDO os Quacks
                int uniqueCount = 0;
                using (var cmd = new SQLiteCommand("SELECT COUNT(DISTINCT elemental_id) FROM capturas WHERE user_id=@uid AND quantidade > 0 AND elemental_id NOT LIKE '%_8'", con))
                {
                    cmd.Parameters.AddWithValue("@uid", uid);
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) uniqueCount = Convert.ToInt32(res);
                }

                // Obtém o username do utilizador
                string username = uid;
                using (var cmd = new SQLiteCommand("SELECT username FROM utilizadores WHERE user_id=@uid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", uid);
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) username = res.ToString();
                }

                foreach (var m in milestones)
                {
                    // Verifica se o jogador tem o Quack
                    int qtyQuack = 0;
                    using (var cmd = new SQLiteCommand("SELECT quantidade FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
                    {
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.Parameters.AddWithValue("@eid", m.elemId);
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value) qtyQuack = Convert.ToInt32(res);
                    }

                    if (qtyQuack > 0 && uniqueCount < m.req)
                    {
                        // Remove o Quack (reduz quantidade a 0 ou deleta)
                        using (var cmd = new SQLiteCommand("DELETE FROM capturas WHERE user_id=@uid AND elemental_id=@eid", con))
                        {
                            cmd.Parameters.AddWithValue("@uid", uid);
                            cmd.Parameters.AddWithValue("@eid", m.elemId);
                            cmd.ExecuteNonQuery();
                        }

                        CPH.SendMessage(string.Format("🦆💔 @{0} perdeu um elemental e já não tem o requisito de {1} únicos. O {2} foi removido! 😢", 
                            username, m.req, m.nome));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogError("[Export - PerdaQuacks] Erro ao verificar perda: " + ex.Message);
        }
    }
}