using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

public class CPHInline
{
    private string caminhoBD = @"I:\Twitch\Games\elementais\elementais.db";
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";

    public bool Execute()
    {
        string userId = args.ContainsKey("userId") ? args["userId"].ToString() : "";
        string userName = args.ContainsKey("userName") ? args["userName"].ToString() : "";

        if (string.IsNullOrEmpty(userId)) return true;

        List<string> itens = new List<string>();

        using (var con = new SQLiteConnection("Data Source=" + caminhoBD + ";Version=3;"))
        {
            con.Open();
            // CRUCIAL: Selecionamos o ID do elemental E a sua quantidade real guardada
            string query = "SELECT elemental_id, quantidade FROM capturas WHERE user_id = @uid AND quantidade > 0";
            using (var cmd = new SQLiteCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string elemId = reader["elemental_id"].ToString();
                        int qtd = Convert.ToInt32(reader["quantidade"]);
                        
                        // Formata exatamente como o JS espera -> ID:QUANTIDADE
                        itens.Add($"{elemId}:{qtd}");
                    }
                }
            }
        }

        string colecaoStr = string.Join(",", itens);

        // Escreve no ficheiro para o OBS atualizar visualmente
        try 
        { 
            File.WriteAllText(caminhoEstado, $"COLECAO;{userName};{colecaoStr}"); 
        } 
        catch {}

        return true;
    }
}