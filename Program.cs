//https://zetcode.com/csharp/mysql/ -- MYSQL
//https://zetcode.com/csharp/json/  -- JSON

using System;
using System.IO;
using System.Security;
using MySql.Data.MySqlClient;

using System.Text.Json;
using System.Text.RegularExpressions;


namespace SYSTEM_ZARZADZANIA_SKLEPEM_BUDOWLANYM
{
    
    class Program
    {
        
        

        public static string LoadConnectionConfig(string configFile)
        {
            //Ładujemy konfigurację połączenia z bazą danych, w parametrze ściezka do pliku
            //Konfiguracja json "server" "user" "password" "database"
            try
            {
                string jsonString = File.ReadAllText(configFile);
                using JsonDocument document = JsonDocument.Parse(jsonString);
                JsonElement root = document.RootElement;

                string server = root.GetProperty("server").ToString();
                string user = root.GetProperty("user").ToString();
                string password = root.GetProperty("password").ToString();
                string database = root.GetProperty("database").ToString();

                string connectionString = $"server={server};userid={user};password={password};database={database}";
                if (connectionString == "")
                {
                    LoggedUser.CreateLogMessage($"Błąd ładowania konfiguracji DB z pliku {configFile}", false);
                    return "";
                }
                else
                {
                    return connectionString;
                }
            }
            catch (Exception e)
            {
                LoggedUser.CreateLogMessage($"Błąd ładowania konfiguracji DB z pliku {configFile} ({e.Message})", false);
                return "";
            }    
        }



        

        

        static void Main(string[] args)
        {
            Console.Title = "System Zarządzania Sklepem Budowlanym";


            string connectionString = LoadConnectionConfig("dbConfig.json");
            if (connectionString == "")
            {
                Console.WriteLine("Błąd ładowania konfiguracji bazy danych z pliku. ");
                Thread.Sleep(3000);
                return;
            }
            
                
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
      
            LoggedUser userSession = new LoggedUser(databaseConnection);

            //Start interfejstu
            UserInterface.StartupPanel(userSession);
            
            
            

            
            
        }
    }
}