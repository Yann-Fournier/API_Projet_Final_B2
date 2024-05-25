using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace app;

public class SQLRequest
{
    // Connection à la base de données ------------------------------------------------------------------------------------------------------
    public static SQLiteConnection OpenBDDConnection()
    {
        // string connectionString = "Data Source=chemin_de_la_database";
        string connectionString = @"Data Source=../BDD_Projet_Final_B2/Database_Biblio.db"; // Chemin relatif
        // string connectionString = $"serve=localhost;port=3306;User ID=root;Password=root;";

        // Création de la connection
        SQLiteConnection connection = new SQLiteConnection(connectionString);
        try
        {
            // Ouverture de la connection avec la base de données
            connection.Open();
            Console.WriteLine("Connexion réussie à la base de données MySql!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur de connexion: " + ex.Message);
        }
        return connection;
    }

    public static dynamic ExecuteSelectQuery(SQLiteConnection connection, string query)
    {
        DataTable dataTable = new DataTable();
        // Console.WriteLine(query);
        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
        {
            adapter.Fill(dataTable);
        }

        string jsonResult = ConvertDataTableToJson(dataTable);
        return JsonConvert.DeserializeObject(jsonResult);
    }

    static string ConvertDataTableToJson(DataTable dataTable)
    {
        return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
    }
}