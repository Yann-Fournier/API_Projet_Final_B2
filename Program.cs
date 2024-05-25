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
        dynamic data = new int[0]; // Variable qui recupérera les json des requetes sql.  
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

        // Console.WriteLine(parameters);

        // Recupération du token d'authentification -----------------------------------------------------------------------------------------------------------
        NameValueCollection auth = context.Request.Headers;
        string token = "";
        dynamic truc = 10;
        int User_Id = -1;
        bool Is_Admin = false;
        try
        {
            token = auth["Authorization"].Replace("Bearer ", "");
            truc = SQLRequest.ExecuteSelectQuery(connection, "SELECT Users.Id, Users.Is_Admin FROM Users JOIN Auth ON Users.Id = Auth.Id WHERE Auth.Token = '" + token + "';");
            User_Id = truc[0].Id;
            Is_Admin = truc[0].Is_Admin;
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
                data = "They are too many parameters.";
            }
            else
            {
                data = "Hello! Welcome to the home page of this API. This is a project for our school. You can find the documentation at : https://github.com/Yann-Fournier/Projet_Final_B2.";
            }
        }
        else if (context.Request.HttpMethod == "GET")
        {
            switch (path)
            {
                case "/get_token": // param : , user_id
                    if (parameters.Count == 1)
                    {
                        data = SQLRequest.ExecuteSelectQuery(connection, "SELECT Token FROM Auth WHERE Id = " + parameters["user_id"] + ";");
                        pasOk = false;
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/auteur": // param : , auteur_name, auteur_id
                    if (parameters.Count == 1)
                    {
                        string[] keys = parameters.AllKeys;
                        if (keys[0] == "auteur_id")
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Auteurs WHERE Id = " + parameters["auteur_id"] + ";");
                            pasOk = false;
                        }
                        else if (keys[0] == "auteur_name")
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Auteurs WHERE Nom = '" + parameters["auteur_name"] + "';");
                            pasOk = false;
                        }
                        else
                        {
                            pasOk = true;
                        }
                    }
                    else if (parameters.Count == 0)
                    {
                        data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Auteurs;");
                        pasOk = false;
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/categorie": // param : , categorie_name, categorie_id
                    if (Is_Admin)
                    {
                        if (parameters.Count == 1)
                        {
                            string[] keys = parameters.AllKeys;
                            if (keys[0] == "categorie_id")
                            {
                                data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Categories WHERE Id = " + parameters["categorie_id"] + ";");
                            }
                            else if (keys[0] == "categorie_name")
                            {
                                data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Categories WHERE Nom = '" + parameters["categorie_name"] + "';");
                            }
                            else
                            {
                                pasOk = true;
                            }
                        }
                        else if (parameters.Count == 0)
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Categories;");
                        }
                        else
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user": // param : , user_name, user_id
                    if (parameters.Count == 1)
                    {
                        string[] keys = parameters.AllKeys;
                        if (keys[0] == "user_id")
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Users WHERE Id = " + parameters["user_id"] + ";");
                            pasOk = false;
                        }
                        else if (keys[0] == "user_name")
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Users WHERE Nom = '" + parameters["user_name"] + "';");
                            pasOk = false;
                        }
                        else
                        {
                            pasOk = true;
                        }
                    }
                    else if (parameters.Count == 0)
                    {
                        data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Users;");
                        pasOk = false;
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection": // param : , collection_id
                    if (Is_Admin)
                    {
                        if (parameters.Count == 1)
                        {
                            string[] keys = parameters.AllKeys;
                            if (keys[0] == "collection_id")
                            {
                                data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Collections WHERE Id = " + parameters["collection_id"] + ";");
                            }
                            else
                            {
                                pasOk = true;
                            }
                        }
                        else if (parameters.Count == 0)
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Collections;");
                        }
                        else
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/commentaire": // param : , commentaire_id
                    if (Is_Admin)
                    {
                        if (parameters.Count == 1)
                        {
                            string[] keys = parameters.AllKeys;
                            if (keys[0] == "com_id")
                            {
                                data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Com WHERE Id = " + parameters["com_id"] + ";");
                            }
                            else
                            {
                                pasOk = true;
                            }
                        }
                        else if (parameters.Count == 0)
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Com;");
                        }
                        else
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/livre": // param : , livre_name, livre_id
                    if (parameters.Count == 1)
                        {
                            string[] keys = parameters.AllKeys;
                            if (keys[0] == "livre_id")
                            {
                                data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Livres WHERE Id = " + parameters["livre_id"] + ";");
                                pasOk = false;
                            }
                            else if (keys[0] == "livre_name")
                            {
                                data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Livres WHERE Nom = '" + parameters["livre_name"] + "';");
                                pasOk = false;
                            }
                            else
                            {
                                pasOk = true;
                            }
                        }
                        else if (parameters.Count == 0)
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Livres;");
                            pasOk = false;
                        }
                        else
                        {
                            pasOk = true;
                        }
                    break;
                default:
                    pasOk = true;
                    break;
            }
        }
        else if (context.Request.HttpMethod == "POST")
        {
            switch (path)
            {
                case "/auteur/create": // param : nom, desc, photo
                    if (Is_Admin)
                    {
                        data = "Vous avez créer un auteur";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/auteur/delete": // param : auteur_id
                    if (Is_Admin)
                    {
                        data = "Vous avez supprimer un auteur";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/categorie/create": // param : nom
                    if (Is_Admin)
                    {
                        data = "Vous avez créer une categorie";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/categorie/delete": // param : categorie_id
                    if (Is_Admin)
                    {
                        data = "Vous avez supprimer une categorie";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/create": // param : email, mdp, photo, nom, is_admin
                    data = "Vous avez creer un utilisateur";
                    break;
                case "/user/delete": // param : user_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de supprimer un utilisateur";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Votre compte à bien été supprimer";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/change_info": // param : email, mdp, photo, nom, is_admin (optio)
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de changer les info d'un utilisateur";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de changer les vos infos";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/follow_user": // param : follow_user_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de faire suivre un autre utilisateur à ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de suivre un utilisateur";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/unfollow_user": // param : unfollow_user_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de faire ne plus suivre un autre utilisateur à ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de ne plus suivre un utilisateur";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/follow_auteur": // param : follow_auteur_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de faire suivre un auteur à ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de suivre un auteur";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/unfollow_auteur": // param : follow_auteur_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de faire suivre un auteur à ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de ne plus suivre un auteur";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/create": // param : nom, is_private
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de créer une collection pour ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de vous créer une collection ";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/delete": // parma : collection_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de supprimer une collection pour ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de supprimer une de vos collection";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/add_livre": // param : collection_id, livre_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez d'ajouter un livre à une collection pour ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez d'ajouter un livre à une de vos collection";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/delete_livre": // param : collection_id, livre_id
                    if (Int32.Parse(parameters[0]) != User_Id && Is_Admin)
                    {
                        data = "Vous venez de supprimer un livre d'une collection de ....";
                    }
                    else if (Int32.Parse(parameters[0]) == User_Id)
                    {
                        data = "Vous venez de supprimer un livre d'une de vos collection";
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/commentaire/add": // param : livre_id, text_commentaire
                    data = "Vous avez ajouter un commentaire";
                    break;
                case "/commentaire/delete": // param : commentaire_id
                    if (Is_Admin)
                    {
                        data = "Vous venez de supprimer un commentaire";
                    }
                    break;
                case "/livre/add": // param : nom, description, photo, isbn, editeur, prix, auteur_id, categorie_id
                    if (Is_Admin)
                    {
                        data = "Vous venez d'ajouter un livre";
                    }
                    break;
                case "/livre/delete": // param : livre_id
                    if (Is_Admin)
                    {
                        data = "Vous venez de supprimer un livre";
                    }
                    break;
                case "/livre/change_info": // param : nom, description, photo, isbn, editeur, prix, auteur_id, categorie_id (opti)
                    if (Is_Admin)
                    {
                        data = "Vous venez de changer les infos d'un livre";
                    }
                    break;
                default:
                    pasOk = true;
                    break;
            }
        }
        else
        {
            pasOk = true;
        }

        if (pasOk)
        {
            data = "404 - Not Found:\n\n   - Verify the request method\n   - Verify the url\n   - Verify the parameters\n   - Verify your token";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        else
        {
            data = JsonConvert.SerializeObject(data);
        }

        // Envoie de la réponse.
        byte[] buffer = Encoding.UTF8.GetBytes(data);
        context.Response.ContentLength64 = buffer.Length;
        Stream output = context.Response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }
}