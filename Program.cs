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
                                data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Commentaires WHERE Id = " + parameters["commentaire_id"] + ";");
                            }
                            else
                            {
                                pasOk = true;
                            }
                        }
                        else if (parameters.Count == 0)
                        {
                            data = SQLRequest.ExecuteSelectQuery(connection, "SELECT * FROM Commentaires;");
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
                        if (parameters.Count == 3)
                        {
                            try
                            {
                                string query = "INSERT INTO Auteurs (Nom, Description, Photo) VALUES ('" + parameters["nom"] + "', '" + parameters["description"] + "', '" + parameters["photo"] + "');";
                                SQLRequest.ExecuteOtherQuery(connection, query);
                                data = "Vous avez créer un auteur";
                            }
                            catch (Exception e)
                            {
                                pasOk = true;
                            }
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
                case "/auteur/delete": // param : auteur_id
                    if (Is_Admin)
                    {
                        if (parameters.Count == 1)
                        {
                            try
                            {
                                string query = "DELETE FROM Auteurs WHERE Id = " + parameters["auteur_id"] + ";";
                                SQLRequest.ExecuteOtherQuery(connection, query);
                                data = "Vous avez supprimer un auteur";
                            }
                            catch (Exception e)
                            {
                                pasOk = true;
                            }
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
                case "/categorie/create": // param : nom
                    if (Is_Admin)
                    {
                        if (parameters.Count == 1)
                        {
                            try
                            {
                                string query = "INSERT INTO Categories (Nom) VALUES ('" + parameters["nom"] + "');";
                                SQLRequest.ExecuteOtherQuery(connection, query);
                                data = "Vous avez créer une categorie";
                            }
                            catch (Exception e)
                            {
                                pasOk = true;
                            }
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
                case "/categorie/delete": // param : categorie_id
                    if (Is_Admin)
                    {
                        if (parameters.Count == 1)
                        {
                            try
                            {
                                string query = "DELETE FROM Categories WHERE Id = " + parameters["categorie_id"] + ";";
                                SQLRequest.ExecuteOtherQuery(connection, query);
                                data = "Vous avez supprimer une categorie";
                            }
                            catch (Exception e)
                            {
                                pasOk = true;
                            }
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
                case "/user/create": // param : email, mdp, photo, nom, is_admin
                    if (parameters.Count == 5)
                    {
                        try
                        {
                            string query = "INSERT INTO Users (Email, Mdp, Photo, Nom, Is_Admin) VALUES ('" + parameters["email"] + "', '" + parameters["mdp"] + "', '" + parameters["photo"] + "', '" + parameters["nom"] + "', " + parameters["is_admin"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous avez ajouter un auteur";
                            pasOk = false;
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/delete": // param : user_id
                    if (parameters.Count == 1)
                    {
                        try
                        {
                            if (Int32.Parse(parameters["user_id"]) != User_Id && Is_Admin)
                            {
                                string query = "DELETE FROM Users WHERE Id = " + parameters["user_id"] + ";";
                                SQLRequest.ExecuteOtherQuery(connection, query);
                                data = "Vous avez supprimer un auteur";
                            }
                            else if (Int32.Parse(parameters["user_id"]) == User_Id)
                            {
                                string query = "DELETE FROM Users WHERE Id = " + parameters["user_id"] + ";";
                                SQLRequest.ExecuteOtherQuery(connection, query);
                                data = "Votre compte à bien été supprimer";
                            }
                            else
                            {
                                pasOk = true;
                            }
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }

                    break;
                case "/user/change_info": // param : email, mdp, photo, nom, is_admin (admin), user_id (admin) (optio)
                    if (Is_Admin)
                    {
                        if (parameters.Count > 0 && parameters.Count < 7)
                        {
                            string[] keys = parameters.AllKeys;
                            foreach (string key in keys)
                            {
                                if (key == "email")
                                {
                                    string query = "UPDATE Users SET Email = '" + parameters["email"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre email à bien été modifié.";
                                }
                                else if (key == "mdp")
                                {
                                    string query = "UPDATE Users SET Mdp = '" + parameters["mdp"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre mot de passe à bien été modifié.";
                                }
                                else if (key == "photo")
                                {
                                    string query = "UPDATE Users SET Photo = '" + parameters["photo"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre photo à bien été modifiée.";
                                }
                                else if (key == "nom")
                                {
                                    string query = "UPDATE Users SET Nom = '" + parameters["nom"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre nom à bien été modifié.";
                                }
                                else if (key == "is_admin")
                                {
                                    try
                                    {
                                        string query = "UPDATE Users SET Is_Admin = '" + parameters["is_admin"] + "' WHERE Id = " + parameters["user_id"] + ";";
                                        SQLRequest.ExecuteOtherQuery(connection, query);
                                        data = data + " Vos les permissions ont bien été modifiées.";
                                    }
                                    catch (Exception e)
                                    {
                                        pasOk = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            pasOk = true;
                        }
                    }
                    else if (User_Id != -1)
                    {
                        if (parameters.Count > 0 && parameters.Count < 6)
                        {
                            string[] keys = parameters.AllKeys;
                            foreach (string key in keys)
                            {
                                if (key == "email")
                                {
                                    string query = "UPDATE Users SET Email = '" + parameters["email"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre email à bien été modifié.";
                                }
                                else if (key == "mdp")
                                {
                                    string query = "UPDATE Users SET Mdp = '" + parameters["mdp"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre mot de passe à bien été modifié.";
                                }
                                else if (key == "photo")
                                {
                                    string query = "UPDATE Users SET Photo = '" + parameters["photo"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre photo à bien été modifiée.";
                                }
                                else if (key == "nom")
                                {
                                    string query = "UPDATE Users SET Nom = '" + parameters["nom"] + "' WHERE Id = " + User_Id + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " Votre nom à bien été modifié.";
                                }
                            }
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
                case "/user/follow_user": // param : follow_user_id, user_id (admin)
                    if (Is_Admin && parameters.Count == 2)
                    {
                        try
                        {
                            string query = "INSERT INTO Users_Suivi (Id_User, Id_User_Suivi) VALUES (" + parameters["user_id"] + ", " + parameters["follow_user_id"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de faire suivre un autre utilisateur à qq'un.";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }

                    }
                    else if (User_Id != -1 && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "INSERT INTO Users_Suivi (Id_User, Id_User_Suivi) VALUES (" + User_Id + ", " + parameters["follow_user_id"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de suivre un autre utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/unfollow_user": // param : unfollow_user_id, user_id (admin)
                    if (Is_Admin && parameters.Count == 2)
                    {
                        try
                        {
                            string query = "DELETE FROM Users_Suivi WHERE Id_User = " + parameters["user_id"] + " AND Id_User_Suivi = " + parameters["unfollow_user_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de faire ne plus suivre un autre utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else if (User_Id != -1 && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "DELETE FROM Users_Suivi WHERE Id_User = " + User_Id + " AND Id_User_Suivi = " + parameters["unfollow_user_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de ne plus suivre un utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/follow_auteur": // param : follow_auteur_id, user_id (admin)
                    if (Is_Admin && parameters.Count == 2)
                    {
                        try
                        {
                            string query = "INSERT INTO Auteurs_Suivi (Id_User, Id_Auteur) VALUES (" + parameters["user_id"] + ", " + parameters["follow_auteur_id"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de faire suivre un auteur à un utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else if (User_Id != -1 && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "INSERT INTO Auteurs_Suivi (Id_User, Id_Auteur) VALUES (" + User_Id + ", " + parameters["follow_auteur_id"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de suivre un auteur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/user/unfollow_auteur": // param : unfollow_auteur_id, user_id (admin)
                    if (Is_Admin && parameters.Count == 2)
                    {
                        try
                        {
                            string query = "DELETE FROM Auteurs_Suivi WHERE Id_User = " + parameters["user_id"] + " AND Id_Auteur = " + parameters["unfollow_auteur_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de faire ne plus suivre un auteur à un utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else if (User_Id != -1 && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "DELETE FROM Users_Suivi WHERE Id_User = " + User_Id + " AND Id_Auteur = " + parameters["unfollow_auteur_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de ne plus suivre un auteur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/create": // param : nom, is_private, user_id (admin)
                    if (Is_Admin && parameters.Count == 3)
                    {
                        try
                        {
                            string query = "INSERT INTO Collections (Id_User, Nom, Is_Private) VALUES (" + parameters["user_id"] + ", '" + parameters["nom"] + "', " + parameters["is_private"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de créer une collection pour un utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }

                    }
                    else if (User_Id != -1 && parameters.Count == 2)
                    {
                        try
                        {
                            string query = "INSERT INTO Collections (Id_User, Nom, Is_Private) VALUES (" + User_Id + ", '" + parameters["nom"] + "', " + parameters["is_private"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de vous créer une collection ";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/delete": // parma : collection_id
                    if (Is_Admin && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "DELETE FROM Collec WHERE Id_Collection = " + parameters["collection_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            query = "DELETE FROM Collections WHERE Id = " + parameters["collection_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de supprimer une collection pour un utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else if (User_Id != -1 && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "DELETE FROM Collec INNER JOIN Collections ON Collec.Id_Collection = Collections.Id WHERE Collections.Id_User = " + User_Id + "AND Collec.Id_Collection = " + parameters["collection_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            query = "DELETE FROM Collections WHERE Id = " + parameters["collection_id"] + " AND Id_User = " + User_Id + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de supprimer une de vos collection";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/add_livre": // param : collection_id, livre_id
                    if (Is_Admin && parameters.Count == 2)
                    {
                        try
                        {
                            string query = "INSERT INTO Collec (Id_Livre, Id_Collection) VALUES (" + parameters["livre_id"] + ", " + parameters["collection_id"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez d'ajouter un livre à une collection pour un utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else if (User_Id != -1 && parameters.Count == 2)
                    {
                        try
                        {
                            string checkQuery = "SELECT COUNT(*) FROM Collections WHERE Id = " + parameters["collection_id"] + " AND Id_User = " + User_Id + ";";
                            dynamic count = SQLRequest.ExecuteSelectQuery(connection, checkQuery);
                            if (count[0]["COUNT(*)"] > 0)
                            {
                                // Condition remplie, insérez les données
                                string insertQuery = "INSERT INTO Collec (Id_Livre, Id_Collection) VALUES (" + parameters["livre_id"] + ", " + parameters["collection_id"] + ");";
                                using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                                {
                                    int rowsAffected = insertCommand.ExecuteNonQuery();
                                    data = "Vous venez d'ajouter un livre à une de vos collection";
                                }
                            }
                            else
                            {
                                pasOk = true;
                            }

                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/collection/delete_livre": // param : collection_id, livre_id
                    if (Is_Admin && parameters.Count == 2)
                    {
                        try
                        {
                            string query = "DELETE FROM Collec WHERE Id_Livre = " + parameters["livre_id"] + " AND Id_Collection = " + parameters["collection_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de supprimer un livre d'une collection d'un utilisateur";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }

                    }
                    else if (User_Id != -1 && parameters.Count == 2)
                    {
                        try
                        {
                            string checkQuery = "SELECT COUNT(*) FROM Collections WHERE Id = " + parameters["collection_id"] + " AND Id_User = " + User_Id + ";";
                            dynamic count = SQLRequest.ExecuteSelectQuery(connection, checkQuery);
                            if (count[0]["COUNT(*)"] > 0)
                            {
                                // Condition remplie, suppression des données
                                string insertQuery = "DELETE FROM Collec WHERE Id_Livre = " + parameters["livre_id"] + " AND Id_Collection = " + parameters["collection_id"] + ";";
                                using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                                {
                                    int rowsAffected = insertCommand.ExecuteNonQuery();
                                    data = "Vous venez de supprimer un livre d'une de vos collection";
                                }
                            }
                            else
                            {
                                pasOk = true;
                            }

                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/commentaire/add": // param : livre_id, text_commentaire
                    if (parameters.Count == 2)
                    {
                        try
                        {
                            string query = "INSERT INTO Commentaires (Id_User, Id_Livre, Com) VALUES (" + User_Id + ", " + parameters["livre_id"] + ", '" + parameters["text_commentaire"] + "');";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous avez ajouter un commentaire";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/commentaire/delete": // param : commentaire_id
                    if (Is_Admin && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "DELETE FROM Commentaires WHERE Id = " + parameters["commentaire_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de supprimer un commentaire";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    else
                    {
                        pasOk = true;
                    }
                    break;
                case "/livre/add": // param : nom, description, photo, isbn, editeur, prix, auteur_id, categorie_id
                    if (Is_Admin && parameters.Count == 8)
                    {
                        try
                        {
                            string query = "INSERT INTO Livres (Id_Auteur, Id_Categorie, Nom, Description, Photo, ISBN, Editeur, Prix) VALUES (" + parameters["auteur_id"] + ", " + parameters["categorie_id"] + ", '" + parameters["nom"] + "', '" + parameters["description"] + "', '" + parameters["photo"] + "', '" + parameters["isbn"] + "', '" + parameters["editeur"] + "', " + parameters["prix"] + ");";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez d'ajouter un livre";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    break;
                case "/livre/delete": // param : livre_id
                    if (Is_Admin && parameters.Count == 1)
                    {
                        try
                        {
                            string query = "DELETE FROM Livres WHERE Id = " + parameters["livre_id"] + ";";
                            SQLRequest.ExecuteOtherQuery(connection, query);
                            data = "Vous venez de supprimer un livre";
                        }
                        catch (Exception e)
                        {
                            pasOk = true;
                        }
                    }
                    break;
                case "/livre/change_info": // param : livre_id (ob), nom, description, photo, isbn, editeur, prix, auteur_id, categorie_id, (opti)
                    if (Is_Admin)
                    {
                        if (parameters.Count > 0 && parameters.Count < 7)
                        {
                            string[] keys = parameters.AllKeys;
                            foreach (string key in keys)
                            {
                                if (key == "id_auteur")
                                {
                                    string query = "UPDATE Livres SET Id_Auteur = '" + parameters["id_auteur"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";
                                }
                                else if (key == "id_categorie")
                                {
                                    string query = "UPDATE Livres SET Id_Categorie = '" + parameters["id_categorie"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";
                                }
                                else if (key == "nom")
                                {
                                    string query = "UPDATE Livres SET Nom = '" + parameters["nom"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";
                                }
                                else if (key == "description")
                                {
                                    string query = "UPDATE Livres SET Description = '" + parameters["description"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";
                                }
                                else if (key == "photo")
                                {

                                    string query = "UPDATE Livres SET Photo = '" + parameters["photo"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";

                                }
                                else if (key == "isbn")
                                {
                                    string query = "UPDATE Livres SET ISBN = '" + parameters["isbn"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";
                                }
                                else if (key == "editeur")
                                {
                                    string query = "UPDATE Livres SET Editeur = '" + parameters["editeur"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";
                                }
                                else if (key == "prix")
                                {
                                    string query = "UPDATE Livres SET Prix = '" + parameters["prix"] + "' WHERE Id = " + parameters["livre_id"] + ";";
                                    SQLRequest.ExecuteOtherQuery(connection, query);
                                    data = data + " L'auteur de ce livre à bien été modifier.";
                                }
                            }
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