using System;
using System.Net;
using System.Text;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.IO;
using System.Data.SQLite;
using app;

class Program
{
    static async Task Main(string[] args)
    { 
        // Connection à la base de données
        SQLiteConnection connection = SQLRequest.OpenBDDConnection();
        
        // Création de l'api en localhost sur le port 8080
        string url = "http://localhost:8080/";
        var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        Console.WriteLine($"Ecoute sur {url}");
        
        // Boucle permettant de récuperer les requêtes
        while (true)
        {
            try
            {
                Console.WriteLine($"En attente d'une requetes");
                HttpListenerContext context = await listener.GetContextAsync().ConfigureAwait(false);
                await ProcessRequest(context, connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
            }
        }
    }

    static async Task ProcessRequest(HttpListenerContext context, SQLiteConnection connection)
    {
        string responseString = ""; // Initialisation de la réponse
        bool pasOk = false;
        
        // Récupération du chemin et mise en forme
        string path = context.Request.Url.AbsolutePath.ToLower();
        if (path[path.Length - 1] == '/')
        {
            path = path.Substring(0, path.Length - 1);
        }

        // Parse les paramètres de la chaîne de requête
        string param = context.Request.Url.Query;
        NameValueCollection paramet = HttpUtility.ParseQueryString(param); 
        NameValueCollection parameters = new NameValueCollection();
        foreach (String key in paramet)
        {
            parameters.Add(key, paramet[key].Replace("+", " "));
        }
        
        // Recupération du token d'authentification -----------------------------------------------------------------------------------------------------------
        NameValueCollection auth = context.Request.Headers;
        string token = "";
        int User_Id = -1;
        try
        {
            token = auth["Authorization"].Replace("Bearer ", "");
            // Console.WriteLine(token + " " + token.Length);

            User_Id = int.Parse(SQLRequest.GetIdFromToken(connection,
                "SELECT Users.Id AS Id FROM Users JOIN Auth ON Users.Id = Auth.Id WHERE Auth.Token = '" + token + "';"));
        }
        catch (Exception e)
        {
            // pasOk = true;
        }

        Console.WriteLine(path);
    }
}