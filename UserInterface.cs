using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SYSTEM_ZARZADZANIA_SKLEPEM_BUDOWLANYM
{
    //Class that contains methods and functions used in providing user interface.
    ///They should only be used for this purpose
    class UserInterface
    {

        //Default startup screen
        public static void StartupPanel (LoggedUser userSession)
        {
            while (true)
            {
                //default startup screen
                Console.Clear();
                Console.WriteLine("********************************************");
                Console.WriteLine("              Witamy w Systemie     ");
                Console.WriteLine("       Zarządzania Sklepem Budowlanym       ");
                Console.WriteLine("            Proszę wybrać opcję: ");
                Console.WriteLine("********************************************\n");

                Console.WriteLine("\n        ZALOGUJ SIĘ {1}\n");
                Console.WriteLine("        O PROGRAMIE {2}\n");
                Console.WriteLine("            WYJŚCIE {3}\n");

                Console.Write("  >> ");

                //input type check
                bool isNumber = int.TryParse(Console.ReadLine(), out int userAnswer);

                if (!isNumber)
                {
                    Console.Clear();
                    Console.WriteLine("Podaj numer!");
                    Thread.Sleep(1000);
                }
                else
                {
                    switch (userAnswer)
                    {
                        case 1:
                            //zaloguj sie metoda
                            LoginPage(userSession);
                            break;

                        case 2:
                            AboutPanel(userSession);
                            break;

                        case 3:
                            ExitConfirmation(userSession);
                            break;

                        default:
                            Console.Clear();
                            Console.WriteLine("Podaj liczbę. ");
                            Thread.Sleep(800);
                            break;
                    }
                }
            }

        //end of startup screen
        }

        public static void AboutPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("           O PROGRAMIE");
            Console.WriteLine("********************************\n");

            Console.WriteLine("System Zarządzania Sklepem Budownlnym");
            Console.WriteLine("Program został stworzony na potrzeby zaliczenia projektu podstaw programowania.");
            Console.WriteLine("Służy on do prezentacji modelu zarządzania sklepem budowlanym, w oparciu o ");
            Console.WriteLine("bazę danych napisaną w języku SQL i program kliencki napisany w C#.");
            Console.WriteLine("");
            Console.WriteLine("W tym programie możemy edytować liczbę pracowników, sprawdzać");
            Console.WriteLine("statystki i dane, stan magazynu, sprzedane przedmioty, listy klientów ");
            Console.WriteLine("i pracowników. Program oparty jest o interfejs tekstowy w konsoli.");
            Console.WriteLine("");
            Console.WriteLine("Autorzy projektu: ");
            Console.WriteLine(" - Patrycja Wierkin - projekt interfejsu użytkownika i poszczególnych paneli");
            Console.WriteLine(" - Amadeusz Reszke - połączenie efektów pracy zespołu w jedno");
            Console.WriteLine(" - Muhammad Zin Al-Din - stworzenie bazy danych i przygotowanie kwerend do komunikacji");
            Console.WriteLine(" - Tomasz Wysocki - testowanie i eliminacja błędów, stworzenie tego tekstu :)");

            Console.WriteLine("\nAby wyjść, naciśnij ESC... ");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            }
            while (key != ConsoleKey.Escape);
            StartupPanel(userSession);

        }

        public static void ExitConfirmation(LoggedUser userSession)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Czy jesteś pewien/na że chcesz wyjść z programu?\n");
                Console.WriteLine("              TAK {T}       NIE {N}");
                Console.Write("\n  >> ");

                char.TryParse(Console.ReadLine(), out char c);
                char endPick = char.ToUpper(Convert.ToChar(c));


                if (endPick == 'T')
                {
                    Environment.Exit(0);
                }
                else if (endPick == 'N')
                {
                    Console.Clear();

                    StartupPanel(userSession);
                    
                }
            }
        //end exit cnofirmation
        }

        public static void LoginPage(LoggedUser userSession) //Login method 
        {
            string? userInput;
            string? passInput;
            int loginResult;

            for (int tries = 0; tries < 3; tries++)
            {
                Console.Clear();
                Console.WriteLine("********************************");
                Console.WriteLine("           LOGOWANIE");
                Console.WriteLine("********************************\n");

                //collect user input
                Console.Write("Login: ");
                userInput = Console.ReadLine();

                Console.Write("Hasło: ");
                passInput = ReadPassword();

                loginResult = userSession.Login(userInput, passInput);
                //loginResult = userSession.Login("amares8", "Qwerty1@3");

                switch (loginResult)
                {
                    case 0:
                        //udane logowanie
                        UserPanel(userSession);
                        break;
                    case 2:
                        Console.WriteLine("\nPodany login lub hasło są błędne! ");
                        Thread.Sleep(1000);
                        break;
                    case 4:
                        //sql error
                        Console.WriteLine("\nWystąpił błąd z połaczeniem z bazą danych. ");
                        Thread.Sleep(1000);
                        break;
                    case 5:
                        //pusty login lub hasło
                        Console.WriteLine("\nLogin ani hasło nie mogą być puste! . ");
                        Thread.Sleep(1000);
                        break;
                    default:
                        //inny blad
                        Console.WriteLine("\nWystąpił niezidentyfikowany błąd. ");
                        Thread.Sleep(1000);
                        break;
                }
                

                
            }
            StartupPanel(userSession);
            //end LoginPage
        }

        public static void UserPanel(LoggedUser userSession)
        {

            bool isNumber;
            int chosenPanel;

            while (true)
            {
                

                Console.Clear();
                Console.WriteLine("********************************");
                Console.WriteLine(" Witaj " + userSession.GetFirstName() + " " + userSession.GetLastName());
                Console.WriteLine("********************************\n");

                Console.WriteLine("     WYBIERZ FUNKCJĘ:");
                
                if (userSession.GetAccessLevel() == 1)
                {
                    //ACCESS LEVEL 1
                    Console.WriteLine("      (0) Wyloguj");
                    Console.WriteLine("");
                    Console.WriteLine(" * OPERACJE *");
                    Console.WriteLine("      (1) Nowa transakcja");
                    Console.WriteLine("      (2) Zmiana stanów magazynowych");
                    Console.WriteLine("");
                    Console.WriteLine(" * STATYSTYKI *");
                    Console.WriteLine("      (3) Wykonane transkacje");
                    Console.WriteLine("");
                    Console.WriteLine(" * PRZEGLĄDANIE *");
                    Console.WriteLine("      (4) Produkty");
                    Console.WriteLine("      (5) Klienci");
                    Console.WriteLine("");
                    Console.WriteLine(" * BEZPIECZEŃSTWO *");
                    Console.WriteLine("      (6) Zmiana hasła");

                    bool isValidNumber;
                    do
                    {
                        Console.Write("\n >> ");
                        isValidNumber = int.TryParse(Console.ReadLine(), out chosenPanel);
                    }
                    while (chosenPanel == null || chosenPanel < 0 || chosenPanel > 6 || !isValidNumber);

                    switch (chosenPanel)
                    {
                        case 0:
                            LogoutConfirmationPanel(userSession);
                            break;
                        case 1:
                            TransactionPanel(userSession);
                            break;
                        case 2:
                            AddingToStockPanel(userSession);
                            break;
                        case 3:
                            BrowseTransactionsPanel(userSession);
                            break;
                        case 4:
                            BrowseProductsPanel(userSession);
                            break;
                        case 5:
                            BrowseClientsPanel(userSession);
                            break;
                        case 6:
                            ChangePasswordPanel(userSession);
                            break;
                    }
                    continue;

                    //END LEVEL 1
                }
                else if (userSession.GetAccessLevel() == 2)
                {
                    //ACCESS LEVEL 2
                    Console.WriteLine("      (0) Wyloguj");
                    Console.WriteLine("");
                    Console.WriteLine(" * OPERACJE *");
                    Console.WriteLine("      (1) Nowa transakcja");
                    Console.WriteLine("      (2) Zmiana stanów magazynowych");
                    Console.WriteLine("      (3) Dodawanie nowego produktu");
                    Console.WriteLine("");
                    Console.WriteLine(" * STATYSTYKI *");
                    Console.WriteLine("      (4) Wykonane transkacje");
                    Console.WriteLine("      (5) Statystyki pracowników");
                    Console.WriteLine("");
                    Console.WriteLine(" * PRZEGLĄDANIE *");
                    Console.WriteLine("      (6) Produkty");
                    Console.WriteLine("      (7) Klienci");
                    Console.WriteLine("      (8) Pracownicy");
                    Console.WriteLine("");
                    Console.WriteLine(" * BEZPIECZEŃSTWO *");
                    Console.WriteLine("      (9) Zmiana hasła");

                    bool isValidNumber;
                    do
                    {
                        Console.Write("\n >> ");
                        isValidNumber = int.TryParse(Console.ReadLine(), out chosenPanel);
                    }
                    while (chosenPanel == null || chosenPanel < 0 || chosenPanel > 9 || !isValidNumber);

                    switch (chosenPanel)
                    {
                        case 0:
                            LogoutConfirmationPanel(userSession);
                            break;
                        case 1:
                            TransactionPanel(userSession);
                            break;
                        case 2:
                            AddingToStockPanel(userSession);
                            break;
                        case 3:
                            AddingProductPanel(userSession);
                            break;
                        case 4:
                            BrowseTransactionsPanel(userSession);
                            break;
                        case 5:
                            BrowseUserStatisticsPanel(userSession);
                            break;
                        case 6:
                            BrowseProductsPanel(userSession);
                            break;
                        case 7:
                            BrowseClientsPanel(userSession);
                            break;
                        case 8:
                            BrowseUsersPanel(userSession);
                            break;
                        case 9:
                            ChangePasswordPanel(userSession);
                            break;
                    }
                    continue;


                    //END LEVEL 2
                }
                else if (userSession.GetAccessLevel() == 3)
                {
                    //ACCESS LEVEL 3
                    Console.WriteLine("      (0) Wyloguj");
                    Console.WriteLine("");
                    Console.WriteLine(" * OPERACJE *");
                    Console.WriteLine("      (1) Nowa transakcja");
                    Console.WriteLine("      (2) Zmiana stanów magazynowych");
                    Console.WriteLine("      (3) Dodawanie nowego produktu");
                    Console.WriteLine("");
                    Console.WriteLine(" * STATYSTYKI *");
                    Console.WriteLine("      (4) Wykonane transkacje");
                    Console.WriteLine("      (5) Statystyki pracowników");
                    Console.WriteLine("");
                    Console.WriteLine(" * PRZEGLĄDANIE *");
                    Console.WriteLine("      (6) Produkty");
                    Console.WriteLine("      (7) Klienci");
                    Console.WriteLine("      (8) Pracownicy");
                    Console.WriteLine("");
                    Console.WriteLine(" * BEZPIECZEŃSTWO *");
                    Console.WriteLine("      (9) Zmiana hasła");
                    Console.WriteLine("");
                    Console.WriteLine(" * ZARZĄDZANIE BAZĄ *");
                    Console.WriteLine("     (10) Dodawanie nowego pracownika");
                    Console.WriteLine("     (11) Usuwanie pracownika");
                    Console.WriteLine("     (12) Resetowanie hasła użytkownika\n");


                    bool isValidNumber;
                    do
                    {
                        Console.Write("\n >> ");
                        isValidNumber = int.TryParse(Console.ReadLine(), out chosenPanel);
                    }
                    while (chosenPanel == null || chosenPanel < 0 || chosenPanel > 12 || !isValidNumber);
                    
                    switch (chosenPanel)
                    {
                        case 0:
                            LogoutConfirmationPanel(userSession);
                            break;
                        case 1:
                            TransactionPanel(userSession);
                            break;
                        case 2:
                            AddingToStockPanel(userSession);
                            break;
                        case 3:
                            AddingProductPanel(userSession);
                            break;
                        case 4:
                            BrowseTransactionsPanel(userSession);
                            break;
                        case 5:
                            BrowseUserStatisticsPanel(userSession);
                            break;
                        case 6:
                            BrowseProductsPanel(userSession);
                            break;
                        case 7:
                            BrowseClientsPanel(userSession);
                            break;
                        case 8:
                            BrowseUsersPanel(userSession);
                            break;
                        case 9:
                            ChangePasswordPanel(userSession);
                            break;
                        case 10:
                            AddingUserPanel(userSession);
                            break;
                        case 11:
                            DeletingUserPanel(userSession);
                            break;
                        case 12:
                            ResetingPasswordPanel(userSession);
                            break;
                    }
                    continue;

                    //END LEVEL 3
                }


            }



        }

        public static void LogoutConfirmationPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("           WYLOGOWANIE");
            Console.WriteLine("********************************\n");

            Console.WriteLine("Czy na pewno chcesz się wylogować? (T/N): ");
            Console.Write("  >> ");
            string logoutconfirmation;
            do
            {
                logoutconfirmation = Console.ReadLine();
                logoutconfirmation = logoutconfirmation.ToUpper();
            }
            while (logoutconfirmation != "T" && logoutconfirmation != "N");
            if (logoutconfirmation == "T")
            {
                userSession.Logout();
                StartupPanel(userSession);
            }
            else
            {
                UserPanel(userSession);
            }
        }

        public static void TransactionPanel(LoggedUser userSession)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("********************************");
                Console.WriteLine("        NOWA TRANSAKCJA");
                Console.WriteLine("********************************\n");

                string[] clientColumnNames = { "ID", "Imię", "Nazwisko", "e-mail" };
                userSession.DisplayTable("SELECT `customerID`, `firstName`, `lastName`, `email` FROM `customers`;", clientColumnNames);

                Console.WriteLine("Czy klient znajduje się na liście? (T/N)");
                Console.Write("  >> ");
                string endPick;
                do
                {
                    endPick = Console.ReadLine();
                    endPick = endPick.ToUpper();
                }
                while (endPick != "T" && endPick != "N");

                

                //Dodawanie nowego klienta
                if (endPick == "N")
                {
                    string newFirstName;
                    string newLastName;
                    string newPhoneNumber;
                    string newCity;
                    string newEmail;


                    Console.Write("Imię: ");
                    newFirstName = Console.ReadLine();

                    Console.Write("Nazwisko: ");
                    newLastName = Console.ReadLine();

                    Console.Write("Numer telefonu: ");
                    newPhoneNumber = Console.ReadLine();

                    Console.Write("Miejscowość: ");
                    newCity = Console.ReadLine();

                    Console.Write("Adres e-mail: ");
                    newEmail = Console.ReadLine();

                    //Confiramation
                    Console.Write("Czy na pewno chcesz dodać nowego klienta? (T/N): ");
                    Console.Write("  >> ");
                    do
                    {
                        endPick = Console.ReadLine();
                        endPick = endPick.ToUpper();
                    }
                    while (endPick != "T" && endPick != "N");
                    if (endPick == "N")
                    {
                        UserPanel(userSession);
                    }

                    //Adding new client
                    int clientAddingResult = userSession.RegisterClient(newFirstName, newLastName, newPhoneNumber, newCity, newEmail);
                    switch (clientAddingResult)
                    {
                        case 0:
                            Console.WriteLine("Dodano klienta pomyślnie. ");
                            UserPanel(userSession);
                            break;
                        default:
                            Console.WriteLine("Nie udało się dodać klienta. ");
                            Thread.Sleep(2000);
                            UserPanel(userSession);
                            break;
                    }
                }

                //Wybór klienta
                int chosenClientID;
                bool isValidNumber;
                Console.Write("Wybierz ID klienta: ");
                do
                {
                    Console.Write("\n  >> ");
                    isValidNumber = int.TryParse(Console.ReadLine(), out chosenClientID);
                }
                while (!isValidNumber);

                //Wybór produktów
                Console.Clear();
                Console.WriteLine("********************************");
                Console.WriteLine("        NOWA TRANSAKCJA");
                Console.WriteLine("********************************\n");

                string[] productTableNames = { "ID", "Nazwa", "Producent", "Stan", "Cena" };
                userSession.DisplayTable("SELECT `productID`, `productName`, `productVendor`, `quantityInStock`, ROUND(`sellingPrice`/100, 2) FROM `products`", productTableNames);

                List<int> ids = new List<int>();
                List<int> amounts = new List<int>();
                bool isNumber;
                int chosenProduct;
                int chosenQuantity;
                while (true)
                {
                    Console.Write("\nPodaj ID produktu: ");
                    do
                    {
                        Console.Write("\n  >> ");
                        isNumber = int.TryParse(Console.ReadLine(), out chosenProduct);
                    }
                    while (!isNumber);
                    ids.Add(chosenProduct);

                    Console.Write("\nPodaj ilość: ");
                    do
                    {
                        Console.Write("\n  >> ");
                        isNumber = int.TryParse(Console.ReadLine(), out chosenQuantity);
                    }
                    while (!isNumber && chosenQuantity <= 0);
                    amounts.Add(chosenQuantity);

                    Console.WriteLine("\nCzy chcesz dodać kolejny produkt? (T/N)");
                    Console.Write("  >> ");
                    string anotherProductResponse;
                    do
                    {
                        anotherProductResponse = Console.ReadLine();
                        anotherProductResponse = anotherProductResponse.ToUpper();
                    }
                    while (anotherProductResponse != "T" && anotherProductResponse != "N");
                    if (anotherProductResponse == "N")
                    {
                        break;
                    }
                }

                //Transaction confirmation
                Console.WriteLine("\nCzy na pewno chcesz dokonać transkacji? (T/N)");
                Console.Write("  >> ");
                string transactionConfirmationResponse;
                do
                {
                    transactionConfirmationResponse = Console.ReadLine();
                    transactionConfirmationResponse = transactionConfirmationResponse.ToUpper();
                }
                while (transactionConfirmationResponse != "T" && transactionConfirmationResponse != "N");
                if (transactionConfirmationResponse == "N")
                {
                    UserPanel(userSession);
                }

                //Creating transaction
                int creatingTransactionResult = userSession.CreateTransaction(ids.ToArray(), amounts.ToArray(), chosenClientID);

                //Managing transaction result
                switch (creatingTransactionResult)
                {
                    case 4:
                        Console.WriteLine("Podczas dodawania zamówienia wystąpił błąd. ");
                        break;
                    case 5:
                        Console.WriteLine("Podane parametry są błędne. ");
                        break;
                    case 6:
                        Console.WriteLine("Niewystarczające stany magazynowe na zrealizowanie zamówienia. ");
                        break;
                    case 7:
                        Console.WriteLine("Wybrany produkt nie istnieje. ");
                        break;
                    default:
                        if (creatingTransactionResult <= 0)
                        {
                            creatingTransactionResult *= (-1);
                            Console.WriteLine($"Pomyślnie dodano zamówienie. Cena do zapłaty: {creatingTransactionResult / 100},{creatingTransactionResult % 100} zł. ");
                        }
                        else
                        {
                            Console.WriteLine("Podczas dodawania zamówienia wystąpił błąd. ");
                            
                        }
                        break;
                }
                Thread.Sleep(3000);
                UserPanel(userSession);




            }
            
        }

        public static void BrowseTransactionsPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("           TRANSAKCJE");
            Console.WriteLine("********************************\n");

            userSession.DisplayTable("SELECT login, firstName, lastName, jobTitle FROM employees", new string[] { "Login", "Imię", "Nazwisko", "Stanowisko"});

            Console.WriteLine("\nPodaj login użytkownika (dla wszystkich użytkowników pozostaw pusty): ");
            string chosenUserLogin;

            chosenUserLogin = userSession.SanitizeString(Console.ReadLine());
               

            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("           TRANSAKCJE");
            Console.WriteLine("********************************\n");


            string displayHeaderSQL;
            string displayContentSQL;


            //Wyswiatlenie tabeli
            int[] transationIDS = userSession.GetTransactionIDS();

            foreach (int transactionID in transationIDS)
            {
                if (chosenUserLogin == "")
                {
                    displayHeaderSQL = $"SELECT purchases.transactionID, DATE_FORMAT(purchases.date,'%d-%m-%y'), CONCAT(customers.firstName, ' ', customers.lastName), employees.login, ROUND(SUM(purchases.amount*products.sellingPrice)/100, 2) FROM customers, purchases, products, employees WHERE customers.customerID = purchases.customerID AND purchases.productID = products.productID AND purchases.employeeID = employees.employeeID AND purchases.transactionID = {transactionID} LIMIT 1;";
                    displayContentSQL = $"SELECT products.productName, products.productVendor, purchases.amount, ROUND(purchases.amount*products.sellingPrice/100, 2) FROM products, purchases, employees WHERE purchases.productID = products.productID AND employees.employeeID = purchases.employeeID AND purchases.transactionID = {transactionID};";
                }
                else
                {
                    displayHeaderSQL = $"SELECT purchases.transactionID, DATE_FORMAT(purchases.date,'%d-%m-%y'), CONCAT(customers.firstName, ' ', customers.lastName), employees.login, ROUND(SUM(purchases.amount*products.sellingPrice)/100, 2) FROM customers, purchases, products, employees WHERE employees.login='{chosenUserLogin}' AND customers.customerID=purchases.customerID AND purchases.productID=products.productID AND purchases.employeeID=employees.employeeID AND purchases.transactionID = {transactionID} LIMIT 1;";
                    displayContentSQL = $"SELECT products.productName, products.productVendor, purchases.amount, ROUND(purchases.amount*products.sellingPrice/100, 2) FROM products, purchases, employees WHERE purchases.productID = products.productID AND employees.employeeID = purchases.employeeID  AND employees.login = '{chosenUserLogin}' AND purchases.transactionID = {transactionID};";
                }

                userSession.DisplayTable(displayHeaderSQL, new string[] {"ID", "Data", "Klient", "Pracownik", "Łącznie"});
                Console.Write("\n");
                userSession.DisplayTable(displayContentSQL, new string[] {"Nazwa", "Producent", "Ilość", "Kwota" });

                Console.Write("\n\n\n\n");
            }
            




            Console.WriteLine("\nAby wyjść, naciśnij ESC... ");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            }
            while (key != ConsoleKey.Escape);
            UserPanel(userSession);

        }

        public static void BrowseUserStatisticsPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("     STATYSTYKI PRACOWNIKÓW");
            Console.WriteLine("********************************\n");

            int[] usersID = userSession.GetUserIDS();

            foreach (int userID in usersID)
            {
                if (int.Parse(userSession.ExecuteScalarCommand($"SELECT COUNT(DISTINCT purchases.transactionID) FROM purchases WHERE purchases.employeeID = {userID};")) > 0)
                { 
                    userSession.DisplayTable($"SELECT CONCAT(employees.firstName, ' ', employees.lastName), COUNT(DISTINCT purchases.transactionID), ROUND(SUM(purchases.amount*products.sellingPrice)/100, 2) FROM employees, purchases, products WHERE purchases.employeeID = {userID} AND products.productID = purchases.productID AND employees.employeeID = purchases.employeeID", new string[] { "Pracownik", "Transakcje", "Sprzedaż" });
                    Console.Write("\n\n");
                }
            }
            

            Console.WriteLine("\nAby wyjść, naciśnij ESC... ");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            }
            while (key != ConsoleKey.Escape);
            UserPanel(userSession);

        }

        public static void BrowseUsersPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("           PRACOWNICY");
            Console.WriteLine("********************************\n");

            string browseUsersSQL = $"SELECT employeeID, login, firstName, lastName, jobtitle, accessLevel FROM employees";
            userSession.DisplayTable(browseUsersSQL, new string[] { "ID", "Login", "Imię", "Nazwisko", "Stanowisko", "Uprawnienia" });

            Console.WriteLine("\nAby wyjść, naciśnij ESC... ");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            }
            while (key != ConsoleKey.Escape);
            UserPanel(userSession);
        }

        public static void BrowseClientsPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("            KLIENCI");
            Console.WriteLine("********************************\n");

            string browseClientsSQL = $"SELECT * FROM customers";
            userSession.DisplayTable(browseClientsSQL, new string[] { "ID", "Imię", "Nazwisko", "Nr telefonu", "Miejscowość", "Adres e-mail" });

            Console.WriteLine("\nAby wyjść, naciśnij ESC... ");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            }
            while (key != ConsoleKey.Escape);
            UserPanel(userSession);
        }

        public static void BrowseProductsPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("            PRODUKTY");
            Console.WriteLine("********************************\n");

            string browseProductsSQL = $"SELECT productID, productName, productVendor, quantityInStock, ROUND(sellingPrice/100, 2), productDescription FROM products";
            userSession.DisplayTable(browseProductsSQL, new string[] { "ID", "Nazwa", "Producent", "Stan", "Cena", "Opis" });

            Console.WriteLine("\nAby wyjść, naciśnij ESC... ");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            }
            while (key != ConsoleKey.Escape);
            UserPanel(userSession);
        }

        public static void AddingProductPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("          NOWY PRODUKT");
            Console.WriteLine("********************************\n");

            userSession.DisplayTable("SELECT productID, productName, productVendor, ROUND(sellingPrice/100, 2), quantityInStock, productDescription FROM products", new string[] { "ID", "Nazwa", "Producent", "Cena", "Stan", "Opis" });

            Console.Write("\nNazwa: ");
            string newName = Console.ReadLine();

            Console.Write("Producent: ");
            string newVendor = Console.ReadLine();

            Console.Write("Opis: ");
            string newDescription = Console.ReadLine();

            Console.Write("Cena (w groszach): ");
            int newPrice;
            bool isValidNumber;
            do
            {
                isValidNumber = int.TryParse(Console.ReadLine(), out newPrice);
            }
            while (!isValidNumber && newPrice <= 0);


            //Confirmation
            Console.WriteLine("Czy na pewno chcesz dodać nowy produkt? (T/N)");
            Console.Write("  >> ");
            string newProductConfirmation;
            do
            {
                newProductConfirmation = Console.ReadLine();
                newProductConfirmation = newProductConfirmation.ToUpper();
            }
            while (newProductConfirmation != "T" && newProductConfirmation != "N");
            if (newProductConfirmation == "N")
            {
                UserPanel(userSession);
            }

            //Dodawanie nowego produktu
            int productAddingResult = userSession.RegisterProduct(newName, newVendor, newDescription, newPrice);

            switch (productAddingResult)
            {
                case 3:
                    Console.WriteLine("Nie masz uprawnień by dodać nowy produkt. ");
                    break;
                case 4:
                    Console.WriteLine("Wystąpił błąd przy próbie dodania produktu. ");
                    break;
                case 5:
                    Console.WriteLine("Podano puste lub błędne parametry. ");
                    break;
                case 0:
                    Console.WriteLine("Dodano nowy produkt pomyślnie. ");
                    break;
            }
            Thread.Sleep(3000);
            UserPanel(userSession);
        }

        public static void AddingToStockPanel(LoggedUser userSession)
        {
            while (true)
            {


                Console.Clear();
                Console.WriteLine("********************************");
                Console.WriteLine("AKTUALIZACJA STANÓW MAGAZYNOWYCH");
                Console.WriteLine("********************************\n");

                userSession.DisplayTable("SELECT productID, productName, productVendor, ROUND(sellingPrice/100, 2), quantityInStock FROM products", new string[] { "ID", "Nazwa", "Producent", "Cena", "Stan" });

                int productID;
                int modificationAmount;
                bool isValidNumber;
                Console.Write("\nPodaj ID produktu, którego chcesz dodać/odjąć: ");
                do
                {
                    isValidNumber = int.TryParse(Console.ReadLine(), out productID);
                }
                while (!isValidNumber || productID == null);

                Console.Write("\nWskaż, o ile zmodyfikować wartość na stanie (aby odjąć, wpisz ze znakiem '-'): ");
                do
                {
                    isValidNumber = int.TryParse(Console.ReadLine(), out modificationAmount);
                }
                while (!isValidNumber || productID == null);



                //Confirmation
                Console.WriteLine($"Czy na pewno chcesz zmienić stan produktu {productID} o {modificationAmount}? (T/N) ");
                Console.Write("  >> ");
                string stockModificationConfirmation;
                do
                {
                    stockModificationConfirmation = Console.ReadLine();
                    stockModificationConfirmation = stockModificationConfirmation.ToUpper();
                }
                while (stockModificationConfirmation != "T" && stockModificationConfirmation != "N");
                if (stockModificationConfirmation == "N")
                {
                    UserPanel(userSession);
                }

                //Wykonanie
                int productStockModificationResult = userSession.ModifyStockLevel(productID, modificationAmount);
                switch (productStockModificationResult)
                {
                    case 0:
                        Console.WriteLine("Udało się zmienić stan produktu. ");
                        break;
                    case 3:
                        Console.WriteLine("Brak uprawnień. ");
                        break;
                    case 4:
                        Console.WriteLine("Wystapił błąd. ");
                        break;
                    case 5:
                        Console.WriteLine("Podano błędny parametr. ");
                        break;
                    case 6:
                        Console.WriteLine("Nie można zmniejszyć. Produktu nie ma tyle na stanie. ");
                        break;
                    case 7:
                        Console.WriteLine("Nie ma takiego produktu. ");
                        break;
                }
                Thread.Sleep(3000);
                Console.WriteLine("Czy chcesz dokonać kolejnej zmiany? (T/N)");

                string anotherModConfirmation;
                do
                {
                    anotherModConfirmation = Console.ReadLine();
                    anotherModConfirmation = anotherModConfirmation.ToUpper();
                }
                while (anotherModConfirmation != "T" && anotherModConfirmation != "N");
                if (anotherModConfirmation == "N")
                {
                    UserPanel(userSession);
                }
                

            }
        }

        public static void AddingUserPanel(LoggedUser userSession)
        {
            string newLogin;
            string newFirstName;
            string newLastName;
            string newJobTitle;
            string newPassword;
            int newAccessLevel;
            int registrationResult;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("********************************");
                Console.WriteLine("     REJESTRACJA UŻYTKOWNIKA");
                Console.WriteLine("********************************\n");


                string[] customColumnNames = { "Login", "Imię", "Nazwisko", "Stanowisko"};
                int tableDisplayResult = userSession.DisplayTable("SELECT `login`, `firstName`, `lastName`, `jobTitle` FROM `employees`", customColumnNames);

                switch (tableDisplayResult)
                {
                    case 0:
                        break;
                    default:
                        Console.WriteLine("Nie udało się załadować poprawnie listy pracowników. ");
                        break;
                }

                Console.WriteLine("\nDane nowego użytkownika: ");

                Console.Write("Login: ");
                newLogin = Console.ReadLine();

                Console.Write("Imię: ");
                newFirstName = Console.ReadLine();

                Console.Write("Nazwisko: ");
                newLastName = Console.ReadLine();

                Console.Write("Stanowisko: ");
                newJobTitle = Console.ReadLine();

                Console.Write("Hasło: ");
                newPassword = Console.ReadLine();

                Console.Write("Poziom uprawnień (0-3): ");
                bool isNumber;
                do
                {
                    isNumber = int.TryParse(Console.ReadLine(), out newAccessLevel);
                }
                while (!isNumber);
                

                
                Console.Write("Czy na pewno chcesz dodać użytkownika? (T/N): ");

                char.TryParse(Console.ReadLine(), out char c);
                char endPick = char.ToUpper(Convert.ToChar(c));


                if (endPick == 'N')
                {
                    UserPanel(userSession);
                }


                registrationResult = userSession.RegisterUser(newLogin, newFirstName, newLastName, newJobTitle, newPassword, newAccessLevel);

                switch (registrationResult)
                {
                    case 0:
                        Console.WriteLine("Dodano użytkownika " + newLogin + " pomyslnie. ");
                        break;
                    case 3:
                        Console.WriteLine("Brak uprawnień do utworzenia użytkownika. ");
                        break;
                    case 4:
                        Console.WriteLine("Wystapił problem z połaczeniem z bazą danych. ");
                        break;
                    case 5:
                        Console.WriteLine("Podano błędnie jeden lub więcej parametrów. ");
                        break;
                    default:
                        Console.WriteLine("Wystapił niezidentyfikowany problem. ");
                        break;
                }
                Thread.Sleep(3000);
                UserPanel(userSession);
                
            }
        }

        public static void ChangePasswordPanel(LoggedUser userSession)
        {
            int passwordChangeResult;
            


            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("          ZMIANA HASŁA");
            Console.WriteLine("********************************\n");

                

            Console.WriteLine("Podaj stare hasło: ");
            string oldPassword = ReadPassword();

            Console.WriteLine("\nPodaj nowe hasło: ");
            string newPassword1 = ReadPassword();
            Console.WriteLine("\nPowtórz nowe hasło: ");
            string newPassword2 = ReadPassword();
            Console.Write("\n");
            if (oldPassword == "" || newPassword1 == "" || newPassword2 == "")
            {
                Console.WriteLine("Hasło nie może być puste! ");
                Thread.Sleep(2000);
            }
            else if (newPassword1 != newPassword2)
            {
                Console.WriteLine("Hasła nie są zgodne! ");
                Thread.Sleep(2000);
            }
            else
            {
                passwordChangeResult = userSession.ChangePassword(oldPassword, newPassword1);

                switch (passwordChangeResult)
                {
                    case 0:
                        Console.WriteLine("Hasło zmienione pomyślnie. ");
                        break;
                    case 1:
                        Console.WriteLine("Błąd logowania. ");
                        break;
                    case 2:
                        Console.WriteLine("Podane hasło jest błędne. ");
                        break;
                    case 5:
                        Console.WriteLine("Błędne parametry. ");
                        break;
                    default:
                        Console.WriteLine("Błąd przy zmianie hasła. ");
                        break;
                }
            }
            Thread.Sleep(2000);
            UserPanel(userSession);

        }

        public static void DeletingUserPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("      KASOWANIE UŻYTKOWNIKA");
            Console.WriteLine("********************************\n");

            string[] customColumnNames = { "Login", "Imię", "Nazwisko", "Stanowisko" };
            int tableDisplayResult = userSession.DisplayTable("SELECT `login`, `firstName`, `lastName`, `jobTitle` FROM `employees`", customColumnNames);
            
            switch (tableDisplayResult)
            {
                case 0:
                    break;
                default:
                    Console.WriteLine("Nie udało się załadować poprawnie listy pracowników. ");
                    break;
            }

            Console.Write("\nPodaj nazwę użytkownika którego chcesz usunąć: ");
            string userToDelete = Console.ReadLine();

            Console.Write("\nCzy na pewno chcesz skasować użytkownika? Operacja jest nieodwracalna! (T/N): ");

            char.TryParse(Console.ReadLine(), out char c);
            char endPick = char.ToUpper(Convert.ToChar(c));

            if (endPick == 'N')
            {
                UserPanel(userSession);
            }

            int userDeletingResult = userSession.DeleteUser(userToDelete);
            switch (userDeletingResult)
            {
                case 0:
                    Console.WriteLine("Użytkownik usunięty pomyslnie. ");
                    break;
                case 3:
                    Console.WriteLine("Nie masz uprawnień do kasowania kont użytkowników. ");
                    break;
                case 4:
                    Console.WriteLine("Nie udało się usunąć konta użytkownika. ");
                    break;
                case 5:
                    Console.WriteLine("Podano pusty lub błędny parametr! ");
                    break;
            }
            Thread.Sleep(2000);

        }

        public static void ResetingPasswordPanel(LoggedUser userSession)
        {
            Console.Clear();
            Console.WriteLine("********************************");
            Console.WriteLine("       RESETOWANIE HASŁA");
            Console.WriteLine("********************************\n");

            userSession.DisplayTable("SELECT login, firstName, lastName, jobTitle FROM employees", new string[] { "Login", "Imię", "Nazwisko", "Stanowisko"});

            Console.Write("\nHasło po zresetowaniu zostanie ustawione na '1234'. ");
            Console.Write("\nWybierz login użytkownika, któremu chcesz zresetować hasło: ");

            //Getting login
            bool isValidNumber;
            string resetLogin;
            do
            {
                Console.Write("\n >> ");
                resetLogin = Console.ReadLine();
            }
            while (resetLogin == "");

            //Geting confirmation password
            Console.WriteLine("\nPotwierdź swoim hasłem: ");
            string password = ReadPassword();


            //Confirmation
            Console.WriteLine("\nCzy na pewno chcesz zresetować hasło? (T/N)");
            Console.Write("  >> ");
            string resetResponse;
            do
            {
                resetResponse = Console.ReadLine();
                resetResponse = resetResponse.ToUpper();
            }
            while (resetResponse != "T" && resetResponse != "N");
            if (resetResponse == "N")
            {
                UserPanel(userSession);
            }

            //Resetowanie
            int resetingResult = userSession.ResetUsersPassword(resetLogin, password);

            switch (resetingResult)
            {
                case 0:
                    //sukces
                    Console.WriteLine("\nPomyślnie zresetowano hasło. ");
                    break;
                case 1:
                    //not logged int, shoudnt ever appear :)
                    Console.WriteLine("\nNie zalogowano. ");
                    break;
                case 2:
                    //wrong password
                    Console.WriteLine("\nPodano błedne hasło. ");
                    break;
                case 3:
                    //no permissions
                    Console.WriteLine("\nBrak uprawnień do resetowania hasła. ");
                    break;
                case 4:
                    //sql/other error
                    Console.WriteLine("\nPodczas resetowania wystąpił błąd. ");
                    break;
                case 5:
                    //invalid parameters
                    Console.WriteLine("\nPodano niepoprawne dane. ");
                    break;
                default:
                    // shoundt ever appear
                    Console.WriteLine("\nPodczas resetowania wystąpił błąd. ");
                    break;
            }

            Thread.Sleep(2000);
            UserPanel(userSession);




                


        }

        static string ReadPassword()
        {
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            }
            while (key != ConsoleKey.Enter);
            return pass;
        }



    }




}
