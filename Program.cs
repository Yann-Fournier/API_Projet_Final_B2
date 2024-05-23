using System;
using System.Net;
using System.Text;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.IO;
using System.Data.SQLite;
using Newtonsoft.Json;
using System.Text.Json;
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
        // Initialisation de la réponse
        dynamic data = 10; // Variable qui recupérera les json des requetes sql.  
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
        bool Is_Admin = false
        try
        {
            token = auth["Authorization"].Replace("Bearer ", "");

            data = SQLRequest.ExecuteQuery(connection, "SELECT Users.Id, Users.Is_Admin FROM Users JOIN Auth ON Users.Id = Auth.Id WHERE Auth.Token = '" + token + "';");
            User_Id = data[0].Id;
            Is_Admin = data[0].Is_Admin;
        }
        catch (Exception e)
        {
            pasOk = true;
        }


        // Exécution de la requête
        if (path == "") // Page d'acceuil, méthode http pas importante.
        {
            if (parameters.Count != 0)
            {
                responseString = "They are too many parameters.";
            }
            else
            {
                responseString = "Hello! Welcome to the home page of this API. This is a project for our school. You can find the documentation at : https://github.com/Yann-Fournier/Projet_Final_B2.";
            }
        }
        else if (context.Request.HttpMethod == "GET")
        {
            switch ((path, Is_Admin))
            {

            } 
        }
        else if (context.Request.HttpMethod == "POST")
        {
            switch ((path, Is_Admin))
            {

            } 
        }
        else
        {
            responseString = "404 - Not Found:\n\n   - Verify the request method\n   - Verify the url\n   - Verify the parameters\n   - Verify your token";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        if (pasOk)
        {
            data = "404 - Not Found:\n\n   - Verify the request method\n   - Verify the url\n   - Verify the parameters\n   - Verify your token";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        } else {
            string jsonString = JsonConvert.SerializeObject(data);
        }

        // Envoie de la réponse.
        byte[] buffer = Encoding.UTF8.GetBytes(jsonString);
        context.Response.ContentLength64 = buffer.Length;
        Stream output = context.Response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }
}