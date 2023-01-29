using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SYSTEM_ZARZADZANIA_SKLEPEM_BUDOWLANYM
{
    class LoggedUser
    {
        //A class representing ongoing logged user session
        //loggedLogin being equal to "" means user is not logged in
        private MySqlConnection databaseConnection;
        private int loggedID;
        private string loggedLogin;
        private int accessLevel;
        private string firstName;
        private string lastName;



        public LoggedUser(MySqlConnection connection)
        {
            //Default values after creating object instance
            databaseConnection = connection;
            loggedLogin = "";
            accessLevel = 0;
            firstName = "";
            lastName = "";
    }




        public string GetLogin()
        {
            return this.loggedLogin;
        }

        public int GetAccessLevel()
        {
            return this.accessLevel;
        }

        public string GetFirstName()
        {
            return this.firstName;
        }

        public string GetLastName()
        {
            return this.lastName;
        }

        public int GetID()
        {
            return this.loggedID;
        }




        public int Login(string login, string password)
        {
            

            if (login != "" && password != "")
            {
                string loginSanitized = SanitizeString(login);
                string passwordHash = GetStringSha256Hash(password);
                try
                {
                    databaseConnection.Open();
                    string sql = $"SELECT COUNT(*) FROM employees WHERE login = '{loginSanitized}' AND password = '{passwordHash}';";
                    MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                    int result = (int)(long)command.ExecuteScalar();
                    if (result > 0)
                    {
                        //success
                        //przypianie loginu
                        loggedLogin = loginSanitized;

                        //przypianie uprawnien
                        sql = $"SELECT accessLevel FROM employees WHERE login = '{loginSanitized}';";
                        command = new MySqlCommand(sql, databaseConnection);
                        accessLevel = (int)command.ExecuteScalar();

                        //imie
                        sql = $"SELECT firstName FROM employees WHERE login = '{loginSanitized}';";
                        command = new MySqlCommand(sql, databaseConnection);
                        firstName = (string)command.ExecuteScalar();

                        //nazwisko
                        sql = $"SELECT lastName FROM employees WHERE login = '{loginSanitized}';";
                        command = new MySqlCommand(sql, databaseConnection);
                        lastName = (string)command.ExecuteScalar();
                        
                        //id
                        sql = $"SELECT employeeID FROM employees WHERE login = '{loginSanitized}';";
                        command = new MySqlCommand(sql, databaseConnection);
                        loggedID = (int)command.ExecuteScalar();

                        databaseConnection.Close();
                        return 0;
                    }
                    else
                    {
                        //wrong login/password
                        CreateLogMessage("Błędny login lub hasło przy próbie logowania ", false);
                        databaseConnection.Close();
                        return 2;
                    }
                }
                catch (MySqlException e)
                {
                    //sql/other error
                    CreateLogMessage($"Błąd połączenia przy próbie zalogowania ({e.Message})", false);
                    databaseConnection.Close();
                    return 4;
                }

            }
            else
            {
                CreateLogMessage("Podano błędne (np puste) parametry przy próbie logowania", false);
                return 5;
            }

            //end login method
        }

        public int Logout()
        {
            if (loggedLogin == "")
            {
                return 1;
            }
            else
            {
                loggedLogin = "";
                accessLevel = 0;
                firstName = "";
                lastName = "";
                loggedID = -1;
                return 0;
            }
        }

        /*
            * - FUNCTION RETURN VALUES -
            *<0 - returned total price
            * 0 - successfull
            * 1 - not logged in
            * 2 - wrong login/password
            * 3 - no permissions
            * 4 - sql/other error
            * 5 - invalid/empty parameters
            * 6 - insufficient stock
            * 7 - product does not exist
        */


        public int RegisterUser(string newLogin, string newName, string newSurname, string newJobTitle, string newPassword, int newAccessLevel)
        {

            if (loggedLogin != "")
            {
                if (!(newAccessLevel <= 3 && newAccessLevel >= 0))
                {
                    //wrong parameters
                    return 5;
                }
                else if (accessLevel >= 2)
                {
                    
                    string newNameSanitized = SanitizeString(newName);
                    string newSurnameSanitized = SanitizeString(newSurname);
                    string newLoginSanitized = SanitizeString(newLogin);
                    string newPasswordHash = GetStringSha256Hash(newPassword);
                    string newJobTitleSanitized = SanitizeString(newJobTitle);

                    try
                    {
                        databaseConnection.Open();

                        string sql = $"SELECT COUNT(*) FROM employees WHERE login = '{newLoginSanitized}'";
                        MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                        int result = (int)(long)command.ExecuteScalar();
                        databaseConnection.Close();

                        if (result > 0)
                        {
                            //no permissions
                            return 3;
                        }
                        else
                        {
                            databaseConnection.Open();
                            sql = $"INSERT INTO `employees` (`employeeID`, `login`, `firstName`, `lastName`, `jobTitle`, `password`, `accessLevel`) VALUES ('', '{newLoginSanitized}', '{newNameSanitized}', '{newSurnameSanitized}', '{newJobTitleSanitized}', '{newPasswordHash}', '{newAccessLevel}')";
                            command = new MySqlCommand(sql, databaseConnection);
                            int registerResult = (int)(long)command.ExecuteNonQuery();
                            databaseConnection.Close();
                            if (registerResult == 1)
                            {
                                //success
                                return 0;
                            }
                            else
                            {
                                //sql/other error
                                CreateLogMessage("Błąd rejestracji użytkownika ", false);
                                return 4;
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        //sql/other error
                        CreateLogMessage("Błąd rejestracji użytkownika " + e.Message, false);
                        return 4;
                    }

                }
                else
                {
                    //no permissions
                    CreateLogMessage($"Próba rejestracji użytkownika bez uprawnień, użytkownik: {loggedLogin}", false);
                    return 3;
                }
            }
            else
            {
                //not logged in
                return 1;
            }


            //end registeruser method
        }

        public int DeleteUser(string loginToDelete)
        {
            if (loginToDelete == "")
            {
                //invalid parameter
                CreateLogMessage("Podano błędny lub pusty parametr lub parametry przy próbie usunięcia konta użytkownika", false);
                return 5;
            }
            else if (accessLevel < 3)
            {
                //no permissions
                CreateLogMessage($"Użytkownik {loggedLogin} próbował usunąć bez uprawnień konto użytkownika {loginToDelete}", false);
                return 3;
            }
            else
            {
                try
                {
                    string loginToDeleteSanitized = SanitizeString(loginToDelete);
                    string sql = $"DELETE FROM employees WHERE `employees`.`login` = '{loginToDeleteSanitized}'";
                    databaseConnection.Open();
                    MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                    int userDeleteResult = (int)(long)command.ExecuteNonQuery();
                    if (userDeleteResult == 1)
                    {
                        //wszytsko ok
                        databaseConnection.Close();
                        CreateLogMessage($"Użytkownik {loggedLogin} usunął konto użytkownika {loginToDeleteSanitized}. ", false);
                        return 0;
                    }
                    else
                    {
                        //Błąd inny
                        databaseConnection.Close();
                        CreateLogMessage($"Nie udało się użytkownikowi {loggedLogin} usunąć konta użytkownika {loginToDeleteSanitized}. ", false);
                        return 4;
                    }
                }
                catch (Exception e)
                {
                    //other/sql error
                    CreateLogMessage($"Nie udało się użytkownikowi {loggedLogin} usunąć konta użytkownika {loginToDelete}. ({e.Message})", false);
                    return 4;
                }

            }
        }

        public int RegisterClient(string newName, string newSurname, string newPhoneNumber, string newCity, string newEmail)
        {
            if (newName == "" || newSurname == "" || newPhoneNumber == "" || newCity == "" || newEmail == "")
            {
                //Empty parameter
                CreateLogMessage("Podano puste parametry przy próbie rejestracji klienta. ", false);
                return 5;
            }
            else
            {
                string newNameSanitized = SanitizeString(newName);
                string newSurnameSanitized = SanitizeString(newSurname);
                string newPhoneNumberSanitized = SanitizeString(newPhoneNumber);
                string newCitySanitized = SanitizeString(newCity);
                string newEmailSanitized = SanitizeString(newEmail);


                try
                {

                    string sql = $"INSERT INTO `customers` (`customerID`, `firstName`, `lastName`, `phoneNr`, `city`, `email`) VALUES ('', '{newNameSanitized}', '{newSurnameSanitized}', '{newPhoneNumberSanitized}', '{newCitySanitized}', '{newEmailSanitized}')";
                    databaseConnection.Open();
                    MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                    int registerResult = (int)(long)command.ExecuteNonQuery();
                    databaseConnection.Close();
                    if (registerResult == 1)
                    {
                        //success
                        return 0;
                    }
                    else
                    {
                        //sql/other error
                        CreateLogMessage("Błąd rejestracji klienta ", false);
                        return 4;
                    }
                }
                catch (Exception e)
                {
                    //sql/other error
                    CreateLogMessage($"Błąd rejestracji klienta ({e.Message})", false);
                    return 4;
                }
            }
            //end registeruser method
        }


        public int CreateTransaction(int[] productID, int[] productAmounts, int customerID)
        {
            //Validation
            if (productID.Length != productAmounts.Length)
            {
                //Bad parameters
                CreateLogMessage("Długości tablic w parametrach są różne! ", false);
                return 5;
            }
            var set = new HashSet<int>();
            foreach (var num in productID)
            {
                if (!set.Add(num))
                {
                    //repeats in prduct id table
                    CreateLogMessage("W tablicy z numerami produktów znajdują powtórzenia. ", false);
                    return 5;
                }
            }
            //Validating customer id
            string customerIDCheckSQL = $"SELECT COUNT(*) FROM `customers` WHERE `customerID` = {customerID};";
            MySqlCommand idCheckCommand = new MySqlCommand(customerIDCheckSQL, databaseConnection);
            int customerIDCheckResult;
            try
            {
                databaseConnection.Open();
                customerIDCheckResult = int.Parse(idCheckCommand.ExecuteScalar().ToString());
                databaseConnection.Close();
            }
            catch (Exception e)
            {
                //error 4
                CreateLogMessage("Wystąpił problem przy walidacji id klienta podczas dodawania nowej transkacji. ({e.Message})", false);
                return 4;
            }
            if (customerIDCheckResult != 1)
            {
                //erro jakis
                CreateLogMessage("Nie ma takiego klietna przy próbie dodania transakcji. ", false);
                return 5;
            }



            //Generating transaction ID
            string transactionIDSQL = "SELECT MAX(`transactionID`) FROM `purchases`";
            int newTransactionID;
            try
            {
                databaseConnection.Open();
                MySqlCommand command = new MySqlCommand(transactionIDSQL, databaseConnection);
                newTransactionID = int.Parse(command.ExecuteScalar().ToString());
                newTransactionID++;
                databaseConnection.Close();
            }
            catch (Exception e)
            {
                //sql/other error
                CreateLogMessage($"Inny/sql błąd przy generowaniu nowego id zamówienia. ({e.Message})", false);
                return 4;
            }

            int productStockLevel= 0;
            int productExist;
            int transactionInsertResult;
            int stockSubstractingResult;
            string productStockSQL;
            string productExistSQL;
            string stockSubstractingSQL;
            string priceGettingSQL;


            int totalPrice = 0;

            // --- Main loop throudhthgg every ordered item --- //
            for (int i = 0; i < productID.Length; i++)
            {

                //Sprawdzenie czy produkt istnieje
                productExistSQL = $"SELECT COUNT(*) FROM `products` WHERE `productID` = {productID[i]}";
                productExist = int.Parse(ExecuteScalarCommand(productExistSQL));
                if (productExist != 1)
                {
                    //produkt nie istnieje
                    CreateLogMessage($"Próba zakupu nieistniejącego produktu o id {productID[i]}", false);
                    return 7;
                }


                //Sprawdzenie stanów magazynowych
                productStockSQL = $"SELECT `quantityInStock` FROM `products` WHERE `productID` = {productID[i]}";
                productStockLevel = int.Parse(ExecuteScalarCommand(productStockSQL));

                if (productStockLevel < productAmounts[i])
                {
                    //Niewystarczające stany magazynowe
                    CreateLogMessage($"Niewystarczajace stany magazynowe na sprzedaż produktu: {productID[i]} w ilości: {productAmounts[i]}. ", false);
                    return 6;
                }
               
                string insertPurchaseSQL = $"INSERT INTO `purchases` (`id`, `customerID`, `employeeID`, `productID`, `amount`, `transactionID`) VALUES ('', '{customerID}', '{loggedID}', '{productID[i]}', '{productAmounts[i]}', '{newTransactionID}');";
                try
                {
                    databaseConnection.Open();
                    MySqlCommand command = new MySqlCommand(insertPurchaseSQL, databaseConnection);
                    transactionInsertResult = command.ExecuteNonQuery();
                    databaseConnection.Close();
                    if (transactionInsertResult != 1)
                    {
                        //error
                        CreateLogMessage("Błąd przy dodawaniu nowej transakcji", false);
                        return 4;
                    }
                }
                catch (Exception e)
                {
                    //error 4
                    CreateLogMessage($"Błąd przy dodawaniu nowej transakcji. ({e.Message})", false);
                    return 4;
                }


                //Odjęcie ze stanu magazynowego
                stockSubstractingSQL = $"UPDATE `products` SET `quantityInStock` = '{productStockLevel - productAmounts[i]}' WHERE `productID` = {productID[i]};";
                stockSubstractingResult = int.Parse(ExecuteNonQueryCommand(stockSubstractingSQL));
                if (stockSubstractingResult != 1)
                {
                    CreateLogMessage($"Błąd przy odejmowaniu stanów magazynowych", false);
                    return 4;
                }

                //Dodanie do ceny
                priceGettingSQL = $"SELECT sellingPrice FROM `products` WHERE `productID` = {productID[i]};";
                totalPrice += int.Parse(ExecuteScalarCommand(priceGettingSQL)) * productAmounts[i];

            }

            //sukces
            return totalPrice * (-1);
        }

        public int RegisterProduct(string newName, string newVendor, string newDescription, int newPrice)
        {
            if (newName == "" || newVendor  == "" || newDescription  == "" || newPrice < 0)
            {
                //bad parameters
                CreateLogMessage("Błędne lub puste parametry przy dodawaniu nowego produktu", false);
                return 5;
            }
            else if (accessLevel < 2)
            {
                //no permissions
                CreateLogMessage($"Brak uprawnień do dodania nowego produktu, użytkownik ({loggedLogin})", false);
                return 3;

            }
            string newNameSanitized = SanitizeString(newName);
            string newVendorSanitized = SanitizeString(newVendor);
            string newDescriptionSanitized = SanitizeString(newDescription);
            
            string registerSQL = $"INSERT INTO `products` (`productID`, `productName`, `productVendor`, `productDescription`, `quantityInStock`, `sellingPrice`) VALUES('', '{newNameSanitized}', '{newVendorSanitized}', '{newDescriptionSanitized}', 0, {newPrice});";
            int productRegistrationResult = int.Parse(ExecuteNonQueryCommand(registerSQL));
            if (productRegistrationResult != 1)
            {
                //error
                CreateLogMessage("Błąd przy dodawaniu produktu nowego. ", false);
                return 4;
            }
            else
            {
                //success
                return 0;
            }
            
        }

        public int ModifyStockLevel(int productID, int modAmount)
        {
            
            
            if (productID < 0 || productID == null)
            {
                //bad parameters
                CreateLogMessage("ProduktId nie moze byc mniejszy od zera ani pusty", false);
                return 5;
            }
            else if (int.Parse(ExecuteScalarCommand($"SELECT COUNT(*) FROM products WHERE productID = {productID};")) != 1)
            {
                //item doesnt exist
                CreateLogMessage("Próba dodania nieistniejacego produktu", false);
                return 7;
            }
            int currentStockLevel = int.Parse(ExecuteScalarCommand($"SELECT quantityInStock FROM products WHERE productID = {productID};"));
            if ((-1) * modAmount > currentStockLevel)
            {
                //nie ma tyle
                CreateLogMessage("Próba zmniejszenia produktu na stanie o więcej niz go jest", false);
                return 6;
            }
            else if (accessLevel < 1)
            {
                //brak uprawnien
                CreateLogMessage($"Próba dodania/zabrania stanu magazynowego bez uprawnien, uzytkownik {loggedLogin}", false);
                return 3;
            }

            int result = int.Parse(ExecuteNonQueryCommand($"UPDATE `products` SET `quantityInStock` = {modAmount + currentStockLevel} WHERE `products`.`productID` = {productID};"));
            if (result != 1)
            {
                //error 
                CreateLogMessage($"Błąd sql przy zmianie stanu magazuynowego. ", false);
                return 4;
            }
            else
            {
                //ok
                return 0;
            }

        }

        public int ChangePassword(string oldPassword, string newPassword)
        {
            if (loggedLogin == "")
            {
                //not logged in
                return 1;
            }
            else if (oldPassword == "" || newPassword == "")
            {
                return 5;
            }
            try
            {
                string oldPasswordHash = GetStringSha256Hash(oldPassword);
                string newPasswordHash = GetStringSha256Hash(newPassword);
                databaseConnection.Open();
                string sql = $"SELECT COUNT(*) FROM employees WHERE login = '{loggedLogin}' AND password = '{oldPasswordHash}';";
                MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                int result = (int)(long)command.ExecuteScalar();
                databaseConnection.Close();
                if (result == 1)
                {
                    databaseConnection.Open();
                    sql = $"UPDATE employees SET password = '{newPasswordHash}' WHERE login = '{loggedLogin}'";
                    command = new MySqlCommand(sql, databaseConnection);
                    
                    int passwordChangeResult = (int)(long)command.ExecuteNonQuery();
                    databaseConnection.Close();
                    if (passwordChangeResult == 1)
                    {
                        //success
                        
                        return 0;
                    }
                    else
                    {
                        //sql/other error
                        CreateLogMessage($"Błąd przy zmianie hasła, użytkownik {loggedLogin}", false);
                        return 4;
                    }
                }
                else
                {
                    //wrong password
                    CreateLogMessage($"Podano błędne hasło przy zmianie hasła, użytkownik: {loggedLogin}", false);
                    return 2;
                }
            }
            catch (Exception e)
            {
                //sql/other error
                CreateLogMessage("Błąd przy zmianie hasła " + e.Message, false);
                return 4;
            }
            
        }

        public int ResetUsersPassword(string loginOfReseted, string password)
        {

            if (loggedLogin != "")
            {
                if (loginOfReseted != "" && password != "")
                {
                    if (accessLevel >= 3)
                    {
                        string loginOfResetedSanitized = SanitizeString(loginOfReseted);
                        string passwordHash = GetStringSha256Hash(password);
                        string newPasswordHash = GetStringSha256Hash("1234");
                        try
                        {
                            databaseConnection.Open();
                            string sql = $"SELECT COUNT(*) FROM employees WHERE login = '{loggedLogin}' AND password = '{passwordHash}';";
                            MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                            int passwordCheckResult = (int)(long)command.ExecuteScalar();
                            databaseConnection.Close();
                            if (passwordCheckResult == 1)
                            {
                                databaseConnection.Open();
                                sql = $"UPDATE employees SET password = '{newPasswordHash}' WHERE login = '{loginOfResetedSanitized}';";
                                command = new MySqlCommand(sql, databaseConnection);
                                int passwordResetResult = (int)(long)command.ExecuteNonQuery();
                                if (passwordResetResult == 1)
                                {
                                    //success
                                    databaseConnection.Close();
                                    CreateLogMessage($"Użytkownik {loggedLogin} zresetował hasło użytkownika {loginOfReseted}", false);
                                    return 0;
                                }
                                else
                                {
                                    //sql/other error
                                    CreateLogMessage($"Błąd przy resetowaniu hasła, użytkownik: {loggedLogin}", false);
                                    databaseConnection.Close();
                                    return 4;
                                }
                            }
                            else
                            {
                                //wrong password
                                return 2;
                            }
                        }
                        catch (Exception e)
                        {
                            CreateLogMessage($"Błąd przy próbie resetowania hasła ({e.Message})", false);
                            return 4;
                        }


                    }
                    else
                    {
                        //no permissions
                        CreateLogMessage($"Próba resetowania hasła bez uprawnień, użytkownik: {loggedLogin}", false);
                        return 3;
                    }

                }
                else
                {
                    //empty login/passwd
                    return 5;
                }
            }
            else
            {
                //not logged in
                return 1;
            }

        }





        public string ExecuteScalarCommand(string sql)
        {
            string result;
            try
            {
                databaseConnection.Open();
                MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                result = command.ExecuteScalar().ToString();
                databaseConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                //error
                CreateLogMessage($"Błąd przy wykonywaniu komendy '{sql}', ({e.Message})", false);
                return "";
            }
        }

        public string ExecuteNonQueryCommand(string sql)
        {
            string result;
            try
            {
                databaseConnection.Open();
                MySqlCommand command = new MySqlCommand(sql, databaseConnection);
                result = command.ExecuteNonQuery().ToString();
                databaseConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                //error
                CreateLogMessage($"Błąd przy wykonywaniu komendy '{sql}', ({e.Message})", false);
                return "";
            }
        }
        public int DisplayTable(string sql, string[] customColumnNames)
        {
            if (sql == "")
            {
                //bad parameters
                CreateLogMessage("Podano puste polecenie przy próbie załadowania zawartości tablicy. ", false);
                return 5;
            }
            try
            {



                // Get the data from the table
                databaseConnection.Open();
                MySqlCommand selectCommand = new MySqlCommand(sql, databaseConnection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(selectCommand);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                databaseConnection.Close();

                // Create a 2-dimensional array to hold the data
                int rows = dataTable.Rows.Count;
                int cols = dataTable.Columns.Count;
                string[,] dataArray = new string[rows, cols];

                //Creating an array for max lengths of column element
                int[] columnMaxLength = new int[cols];
                int totalTableWidth = 0;
                if (customColumnNames != null)
                {
                    for (int lengthIndex = 0; lengthIndex < customColumnNames.Length; lengthIndex++)
                    {
                        columnMaxLength[lengthIndex] = customColumnNames[lengthIndex].Length;
                    }
                }
                else
                {
                    for(int lengthIndex = 0; lengthIndex < cols; lengthIndex++)
                    {
                        columnMaxLength[lengthIndex] = customColumnNames[lengthIndex].Length;
                    }
                }
                

                // Copy the data from the table to the array and calculating max kolumna element lengths
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        dataArray[i, j] = dataTable.Rows[i][j].ToString();
                        if (columnMaxLength[j] < dataArray[i, j].Length)
                        {
                            columnMaxLength[j] = dataArray[i, j].Length;
                        }
                    }
                }


                //displaying tags and separator on the top of the tabele
                if (customColumnNames != null && dataArray[0, 0] != "")
                {
                    //Calculating total table width
                    for (int i = 0; i < cols; i++)
                    {
                        totalTableWidth += columnMaxLength[i] + 1;
                    }
                    totalTableWidth--;

                    //Displaying kolumn names row
                    for (int i = 0; i < cols; i++)
                    {
                        Console.Write(customColumnNames[i]);
                        for (int k = 0; k <= (columnMaxLength[i] - customColumnNames[i].Length); k++)
                        {
                            Console.Write(" ");
                        }
                    }
                    Console.Write("\n");

                    //Displaying tags and content separator
                    string separator = "";
                    for (int i = 0; i < totalTableWidth; i++)
                    {
                        separator += "═";
                    }
                    Console.WriteLine(separator);
                }

                
                
                //Displaying the table
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Console.Write(dataArray[i, j]);
                        //Adding alignment spaces
                        for (int k = 0; k <= (columnMaxLength[j] - dataArray[i, j].Length); k++)
                        {
                            Console.Write(" ");
                        }
                        
                    }
                    Console.Write("\n");
                }

                return 0;
            }
            catch (Exception e)
            {
                //error
                CreateLogMessage($"Wystąpił błąd przy próbie pobierania danych ({e.Message})", false);
                return 4;
            }
            

            
        }


        public int[] GetTransactionIDS()
        {
            HashSet<int> values = new HashSet<int>(); // use a hashset to store unique values

            try
            {
                databaseConnection.Open();

                string query = "SELECT DISTINCT transactionID FROM purchases;";

                using (MySqlCommand command = new MySqlCommand(query, databaseConnection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            values.Add(int.Parse(reader["transactionID"].ToString()));
                        }
                    }
                }
                databaseConnection.Close();
                return new List<int>(values).ToArray();
            }
            catch (Exception e)
            {
                //error
                CreateLogMessage("Bład przy ładowaniu id transkacji " + e.Message, false);
                return null;
            }
            
        }

        public int[] GetUserIDS()
        {
            HashSet<int> values = new HashSet<int>(); // use a hashset to store unique values

            try
            {
                databaseConnection.Open();

                string query = "SELECT DISTINCT employeeID FROM employees;";

                using (MySqlCommand command = new MySqlCommand(query, databaseConnection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            values.Add(int.Parse(reader["employeeID"].ToString()));
                        }
                    }
                }
                databaseConnection.Close();
                return new List<int>(values).ToArray();
            }
            catch (Exception e)
            {
                //error
                CreateLogMessage("Bład przy ładowaniu id userów " + e.Message, false);
                return null;
            }

        }



        internal static string GetStringSha256Hash(string text)
        {
            //Function for generating SHA256 hashes
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public string SanitizeString(string input)
        {
            //Function for sanitizing strings, especially recieved directly from user. 
            Regex ruleRegex = new Regex(@"[^\w\.\s@-]");
            return Regex.Replace(input, $"{ruleRegex}", "");
        }


        public static void CreateLogMessage(string message, bool showInConsole)
        {
            //Metoda do tworzenia logów w pliku log.txt. W parametrze podajemy treść wiadomości, znacznik czasowy zostanie dodany automatycznie. 
            //Proszę o rozważne korzystanie z opcji tworzenia logów :)

            string path = "log.txt";
            DateTime timestamp = DateTime.Now;

            using (StreamWriter streamWriter = File.AppendText(path))
            {
                streamWriter.WriteLine(timestamp.ToString() + " - " + message);
            }

            if (showInConsole)
            {
                Console.WriteLine(message);
            }

        }







        //end loggeduser class
    }
}