using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WDT2020_a1.Model;

namespace WDT2020_a1.View
{
    public class Callback
    {
        private AppEngine _engine;


        /// <summary>Return the option number the user has chosen from the menu</summary>
        private int GetInput(List<string> menu)
        {
            int option = -1;

            while (option == -1)
            {
                foreach (var item in menu)
                {
                    Console.WriteLine($"{menu.IndexOf(item) + 1}.{item}");
                }
                try
                {
                    Console.Write("\n> ");
                    var key = Console.ReadLine();

                    if (Validation.ValidateIsUserExiting(key))
                        ExitMenu();

                    Validation.ValidateMenuChoice(key, menu.Count);

                    option = Convert.ToInt32(key);
                }
                catch (CustomException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return option;
        }

        ///<summary>Header to display at top of user prompts, input a system script
        ///bool represents if a carrot '> ' is necessary</summary>
        private static void DisplayPrompt(string script, bool carrot)
        {
            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine(script);
            if (carrot)
                Console.Write("\n> ");
            else
                Console.WriteLine();
        }


        public Callback(AppEngine engine)
        {
            _engine = engine;
        }


        public void Run()
        {
            LoggedOutMenu();
        }

        private void LoggedOutMenu()
        {
            bool run = true;

            while (run)
            {
                Console.Clear();
                Console.WriteLine(new string('=', 50));
                Console.WriteLine("Welcome to The National Wealth Bank of Australasia\n");

                var menu = new List<string> { "Login", "Sign Up", "Exit" };

                var choice = GetInput(menu);

                if (choice == 1)
                    EnterLoginID();

                if (choice == 2)
                    SignUp();

                if (choice == 3)
                    break;
            }
            ExitMenu();
        }

        private void EnterLoginID()
        {
            Console.Clear();
            DisplayPrompt("Please enter your login ID", true);

            try
            {
                var loginID = Console.ReadLine();

                Validation.ValidateID(loginID, "Login");

                if (!_engine.DoesLoginIdExist(loginID))
                    throw new CustomException($"Sorry the login {loginID} does not exist");
                else
                    AuthenticatePassword(loginID);

            }
            catch (CustomException e)
            {
                Console.WriteLine(e.Message);
                Thread.Sleep(2000);
            }
        }

        private void AuthenticatePassword(string loginID)
        {
            bool canLogin = true;

            if (_engine.GetPasswordAttempts(loginID) > 2)
            {
                Console.WriteLine("\nYou have reached you maximum failed login attempts\n" +
                    "you must now reset your password");

                if (ConfirmDetailsForReset(loginID))
                {
                    ResetPassword(_engine.GetLogin(loginID));
                }

                else
                {
                    Console.WriteLine("\nPasswrod given was incorrect\n");
                    canLogin = false;
                }
            }

            if (canLogin)
            {
                DisplayPrompt("Please enter your password", true);

                try
                {
                    var password = CreatePassword(_engine.GetLogin(loginID));

                    if (_engine.AuthenticatePassword(password, loginID))
                    {
                        MenuLoggedIn(loginID);
                    }
                    else
                    {
                        Console.WriteLine("\nPassword's did not match");
                        Console.WriteLine("\nplease press any key to continue...");
                        Console.ReadKey(true);
                    }

                }
                catch (CustomException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("please press any key to continue...");
                    Console.ReadKey(true);
                }
            }

        }

        private void MenuLoggedIn(string loginID)
        {
            var login = _engine.GetLogin(loginID);
            var customer = _engine.GetCustomer(login.CustomerID);

            while (true)
            {
                Console.Clear();
                Console.Write(new string('=', 20));
                Console.Write("ATM BRANCH");
                Console.WriteLine(new string('=', 20));
                Console.WriteLine($"\nWelcome back {customer.Name.ToUpper()} to your HOME page");
                Console.WriteLine($"\nIt is Currently {_engine.GetCurrentTime()}\n");
                Console.WriteLine($"Total Balance: ${_engine.GetCustomerTotalBalance(customer.CustomerID)}");

                var menu = new List<string> { "Accounts", "Open an Account", "Display Details", "Logout" };

                DisplayPrompt("Please select an option", false);

                var option = GetInput(menu);

                if (option == 1)
                    DisplayAccounts(customer);

                if (option == 2)
                    OpenNewAccount(customer);

                if (option == 3)
                    DisplayCustomerDetails(customer);

                if (option == 4)
                    return;
            }
        }

        private void OpenNewAccount(Customer customer)
        {
            Console.Clear();
            Console.Write(new string('=', 16));
            Console.Write("OPEN A NEW ACCOUNT");
            Console.WriteLine(new string('=', 16));

            var accounts = _engine.GetAccounts(customer.CustomerID);
            var numberOfAccounts = accounts.Count;

            Console.WriteLine($"Current number of accounts: {numberOfAccounts}");

            bool notConfirmed = true;

            if(numberOfAccounts >= 2)
            {
                Console.WriteLine("Looks like you already have the maximum amount of accounts");
                Thread.Sleep(2000);
                notConfirmed = false;
            }
            DisplayPrompt("Which type of account would you like to open?", false);
            var menu = new List<string> { "Savings", "Checking" };


            while (notConfirmed)
            {
                try
                {
                    var option = GetInput(menu);

                    Validation.ValidateMenuChoice(option.ToString(), menu.Count);

                    DisplayPrompt("How much money would you like to deposit", true);

                    var amount = Console.ReadLine();

                    Validation.Money(amount);

                    if (_engine.OpenNewAccount(customer.CustomerID, option == 1 ? 'S' : 'C', double.Parse(amount)))
                    {
                        notConfirmed = false;
                        Console.WriteLine("Congratulations you've opened an account");
                        Thread.Sleep(2000);
                    }

                }

                catch (CustomException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void DisplayCustomerDetails(Customer customer)
        {
            var login = _engine.GetLoginFromCustID(customer.CustomerID);

            Console.Clear();
            Console.Write(new string('=', 17));
            Console.Write("CUSTOMER DETAILS");
            Console.WriteLine(new string('=', 17));

            Console.WriteLine
                (
                $"{customer.Name}\n" +
                $"{customer.Address}\n" +
                $"{customer.City}\n" +
                $"{customer.PostCode}"
                );

            var menu = new List<string> { "Change Address", "Reset Password", "[Previous Page]" };

            DisplayPrompt("Please select an option", false);

            var option = GetInput(menu);

            if (option == 1)
                CreateAddress(customer.CustomerID);

            if (option == 2)
                ResetPassword(login);

            if (option == 3)
            {
                Console.Clear();
                return;
            }
        }

        private void DisplayTransactions(Account account)
        {
            // retrieve pagenated list of list of transactions
            var pagedTransactions = _engine.TransactionHistory(account.AccountNumber);
            var maxPages = pagedTransactions.Count;
            var page = 0;

            var menu = new List<string> { "Previous Page", "Next Page", "[Return Home]" };

            DisplayPrompt("Please select an option", false);

            while(true)
            {
                Console.Clear();
                Console.Write(
                    $"{new string('=', 19)}" +
                    $"MY STATEMENT" +
                    $"{new string('=', 19)}\n"
                    );

                foreach (Transaction transaction in pagedTransactions[page])
                {
                    Console.WriteLine(transaction.ToString());
                    Console.WriteLine(new string('-', 50));
                }

                Console.WriteLine($"{new string('=', 22)}PAGE {page + 1}{new string('=', 22)}");
                var option = GetInput(menu);

                if (option == 1)
                    if (page > 0)
                        page--;

                if (option == 2)
                    if (page < maxPages - 1)
                        page++;

                if (option == 3)
                    return;
            }
        }

        private void DisplayAccounts(Customer customer)
        {
            Console.Clear();
            Console.Write(
                $"{new string('=', 17)}" +
                $"CURRENT ACCOUNTS" +
                $"{new string('=', 17)}\n"
                );

            //Get All available accounts
            var accounts = _engine.GetAccounts(customer.CustomerID);
            var list = new List<string>();

            foreach(var account in accounts)
            {
                if (account.AccountType == 'C')
                    list.Add($"Checking Account:\tAccount Number: {account.AccountNumber}");
                else
                    list.Add($"Savings Account:\tAccount Number: {account.AccountNumber}");
            }

            list.Add("[return to previous page]");

            DisplayPrompt("Please select an account", false);

            var option = GetInput(list);

            if (option == list.Count)
                return;

            DisplayAccountMenu(accounts[option - 1]);
            
        }

        private void DisplayAccountMenu(Account account)
        {
            Console.Clear();
            Console.Write(new string('=', 17));
            Console.Write("ACCOUNT DETAILS");
            Console.WriteLine(new string('=', 17));

            Console.WriteLine
                (
                $"Account Number: {account.AccountNumber}\n" +
                $"Account Type: {(account.AccountType == 'C' ? "Checking" : "Savings")}\n\n" +
                $"Balance: ${account.Balance}\n\n" +
                $"Amount of Transactions: {_engine.TransactionsCount(account.AccountNumber)}"
                );
            Console.WriteLine(new string('-', 49));

            DisplayPrompt("Please select an option", false);


            var menu = new List<string> {"Deposit into this account", "Withdraw from this account",
                                                "Transfer money to another account", "View Transactions", "[previous page]"};

            var option = GetInput(menu);

            if (option == 1)
                DepositMoney(account);

            if (option == 2)
                Withdraw(account);

            if (option == 3)
                TransferMoney(account);

            if (option == 4)
                DisplayTransactions(account) ;

            if (option == 5)
                return;

        }

        private void DepositMoney(Account account)
        {
            Console.Clear();
            Console.Write(new string('=', 21));
            Console.Write("DEPOSIT");
            Console.WriteLine(new string('=', 21));

            Console.WriteLine($"Account No: {account.AccountNumber}");
            Console.WriteLine($"Current Balance: ${account.Balance}");

            bool notConfirmed = true;

            while(notConfirmed)
            {
                DisplayPrompt("How Much Would You Like to Deposit", true);
                try
                {
                    var input = Console.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        notConfirmed = false;
                        throw new CustomException("Cancelling Transaction");
                    }

                    Validation.Money(input);

                    _engine.Deposit(account.AccountNumber, double.Parse(input));

                    Console.WriteLine($"{new string('=', 15)}SUCESSFUL TRANSACTION{new string('=', 15)}");
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine(_engine.GetLatestTransaction(account.AccountNumber));
                    Console.WriteLine(new string('-', 50));

                    Console.WriteLine("\npress any key to continue...");
                    Console.ReadKey(true);

                    notConfirmed = false;
                }

                catch(CustomException e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(2000);
                }
            }
        }

        private void TransferMoney(Account account)
        {
            Console.Clear();
            Console.Write(new string('=', 21));
            Console.Write("TRANSFERS");
            Console.WriteLine(new string('=', 21));

            Console.WriteLine($"Account No: {account.AccountNumber}");
            Console.WriteLine($"Current Balance: ${account.Balance}");

            bool notConfirmed = true;

            while (notConfirmed)
            {
                if ((account.AccountType == 'C' && account.Balance <= 200) || (account.AccountType == 'S' && account.Balance <= 0))
                {
                    Console.WriteLine("\nSorry you do not have enough funds for this transaction\nplease top up your account");
                    Console.WriteLine("\nplease press any key to continue...");
                    Console.ReadKey(true);
                    notConfirmed = false;
                }
                else
                {
                    DisplayPrompt("How Much Would You Like to Transfer", true);
                    try
                    {
                        var input = Console.ReadLine();

                        if(double.Parse(input) == 0 || string.IsNullOrEmpty(input))
                        {
                            Console.WriteLine("Okay cancelling order..\n\npress any key to continue");
                            Console.ReadKey(true);
                            return;
                        }


                        Validation.Money(input);

                        DisplayPrompt("Which account number would you like to transfer to?", true);

                        var destinationAccount = Console.ReadLine();

                        Validation.AccountNumber(destinationAccount);

                        if (_engine.Transfer(account.AccountNumber, Convert.ToInt32(destinationAccount), double.Parse(input)))
                        {
                            Console.Clear();
                            Console.WriteLine($"{new string('=', 15)}SUCESSFUL TRANSACTION{new string('=', 15)}");
                            Console.WriteLine(new string('-', 50));
                            Console.WriteLine(_engine.GetLatestTransaction(account.AccountNumber));
                            Console.WriteLine(new string('-', 50));

                            Console.WriteLine("\npress any key to continue...");
                            Console.ReadKey(true);

                            notConfirmed = false;
                        }
                    }

                    catch (CustomException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private void Withdraw(Account account)
        {
            Console.Clear();
            Console.Write(new string('=', 21));
            Console.Write("WITHDRAW");
            Console.WriteLine(new string('=', 21));

            Console.WriteLine($"Account No: {account.AccountNumber}");
            Console.WriteLine($"Current Balance: {account.Balance}");

            bool notConfirmed = true;

            while (notConfirmed)
            {
                if ((account.AccountType == 'C' && account.Balance <= 200) || (account.AccountType == 'S' && account.Balance <= 0))
                {
                    Console.WriteLine("\nSorry you do not have enough funds for this transaction\nplease top up your account");
                    Console.WriteLine("\nplease press any key to continue...");
                    Console.ReadKey(true);
                    notConfirmed = false;
                }
                else
                {
                    DisplayPrompt("How Much Would You Like to Withdraw", true);
                    try
                    {
                        var input = Console.ReadLine();

                        if (string.IsNullOrEmpty(input))
                        {
                            Console.WriteLine("...cancelling transaction");
                            Thread.Sleep(2000);
                            return;
                        }

                        Validation.Money(input);

                        if (_engine.Withdraw(account.AccountNumber, double.Parse(input)))
                        {
                            Console.WriteLine($"{new string('=', 15)}SUCESSFUL TRANSACTION{new string('=', 15)}");
                            Console.WriteLine(new string('-', 50));
                            Console.WriteLine(_engine.GetLatestTransaction(account.AccountNumber));
                            Console.WriteLine(new string('-', 50));

                            Console.WriteLine("\npress any key to continue...");
                            Console.ReadKey(true);

                            notConfirmed = false;
                        }
                    }

                    catch (CustomException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }


        private bool ConfirmDetailsForReset(string loginID)
        {
            var response = false;

            var login = _engine.GetLogin(loginID);
            var custID = login.CustomerID;

            DisplayPrompt("Please enter your Post Code", true);

            var postCode = Console.ReadLine();

            if (postCode.Equals(_engine.GetCustomer(custID).PostCode))
                response = true;

            return response;
        }

        private void SignUp()
        {
            Console.Clear();
            DisplayPrompt("Please fill out the following information when prompted", false);

            int custID = -1;

            try
            {
                custID = CreateCustomer();

                if (custID != -1)
                {
                    CreateLogin(custID);

                    CreateAddress(custID);
                }
            }
            catch (CustomException e)
            {
                Console.WriteLine(e.Message);
            }

            if (custID == -1)
                Console.WriteLine("Something went wrong, please try again");
            else
            {
                var login = _engine.GetLoginFromCustID(custID);

                Console.WriteLine(
                    $"{new string('*', 40)}" +
                    $"\nPLEASE NOTE YOUR DETAILS\n" +
                    $"CUSTOMER ID: {login.CustomerID}\n" +
                    $"LOGIN ID: {login.LoginID}\n" +
                    $"{new string('*', 40)}\n\n" +
                    $"press any key to continue...\n"
                    );
                Console.ReadKey(true);
            }
        }

        /// <summary>Creates a customer and returns the unique ID number</summary>
        private int CreateCustomer()
        {
            var custID = -1;
            try
            {
                Console.Write("First Name:\n> ");

                var firstName = Console.ReadLine();

                Console.Write("\nLast Name:\n> ");

                var lastName = Console.ReadLine();

                //Adding the customer to the database
                custID = _engine.CreateNewCustomer(firstName, lastName);

                if (custID == -1)
                    Console.WriteLine("Customer account could not be created");
            }
            catch (CustomException e)
            {
                Console.WriteLine(e.Message);
            }
            return custID;
        }

        public void CreateLogin(int custID)
        {
            var run = true;

            do
            {
                try
                {
                    DisplayPrompt("Please enter a password for your account", true);
                    var password = CreatePassword();

                    DisplayPrompt("Please renter the password", true);

                    var password2 = CreatePassword();

                    if (password.Equals(password2))
                    {
                        Validation.ValidatePassword(password);

                        //This will add the new user into the login database
                        _engine.CreateNewLoginDetails(_engine.GetCustomer(custID), password);
                        run = false;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("The passwords did not match");
                    }
                }
                catch (CustomException e)
                {
                    Console.WriteLine(e.Message);
                }

            } while (run);
        }

        private void CreateAddress(int custID)
        {
            Console.Clear();
            DisplayPrompt("Please fill in the following", false);

            bool run = true;

            while (run)
            {
                try
                {
                    Console.Write("Address:\n> ");
                    var address = Console.ReadLine();

                    Console.Write("\nCity:\n> ");
                    var city = Console.ReadLine();

                    Console.Write("\nPost Code:\n> ");
                    var postCode = Console.ReadLine();

                    Console.Clear();
                    Console.WriteLine($"\n{address}\n{city}\n{postCode}");

                    DisplayPrompt("Are these details are correct [Y/N]", false);

                    var response = Console.ReadLine();

                    if (response.ToUpper().Contains("N"))
                    {
                        Console.Clear();
                        DisplayPrompt("Please enter your address", false);
                    }

                    if (response.ToUpper().Contains("Y"))
                    {
                        var customer = _engine.GetCustomer(custID);
                        customer.Address = address;
                        customer.City = city;
                        customer.PostCode = postCode;

                        _engine.UpdateAddress(customer);

                        Console.WriteLine("\nAddress has been updated, press any key to continue....");
                        Console.ReadKey(true);
                        Console.Clear();

                        run = false;
                    }
                }
                catch (CustomException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>Returns a user inputed password</summary>
        private string CreatePassword()
        {
            bool run = true;

            string password = "";

            while (run)
            {
                var input = Console.ReadKey(true);

                if (input.Key != ConsoleKey.Backspace && input.Key != ConsoleKey.Enter)
                {
                    password += input.KeyChar;
                    Console.Write('*');
                }
                if (input.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }
                if (input.Key == ConsoleKey.Enter)
                    run = false;
            }

            return password;
        }

        //This code was learnt from https://stackoverflow.com/questions/3404421/password-masking-console-application
        /// <summary>Returns a user inputed password with further options for the user</summary>
        private string CreatePassword(Login login)
        {
            bool run = true;

            string password = "";

            while (run)
            {
                var input = Console.ReadKey(true);

                if (input.Key != ConsoleKey.Backspace && input.Key != ConsoleKey.Enter)
                {
                    password += input.KeyChar;
                    Console.Write('*');
                }

                if (input.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }

                if (input.Key == ConsoleKey.Enter && password.Length == 0)
                {
                    Console.WriteLine("Would you like to go back to MENU [Y/N]");
                    Console.Write("\n> ");
                    var exit = Console.ReadKey(true);

                    if (exit.Key == ConsoleKey.Y)
                        LoggedOutMenu();
                    else
                        AuthenticatePassword(login.LoginID);
                }

                if (input.Key == ConsoleKey.Enter)
                    run = false;
            }

            return password;
        }

        private void ResetPassword(Login login)
        {
            Console.Clear();
            Console.Write(new string('-', 18));
            Console.Write("RESET PASSWORD");
            Console.WriteLine(new string('-', 18));

            bool run = true;

            while (run)
            {
                try
                {
                    DisplayPrompt("Please enter a new password", true);

                    var password = CreatePassword(login);

                    Console.Clear();
                    DisplayPrompt("\nPlease enter the password again", true);

                    var password2 = CreatePassword(login);
                    Console.Clear();

                    if (password.Equals(password2))
                    {
                        _engine.UpdatePassword(login.LoginID, password);
                        Console.WriteLine("\nPassword has been reset\n\nPlease type your new password to login");
                        run = false;
                    }
                    else
                        Console.WriteLine("\nThe passwords do not match, please try again");
                }

                catch (CustomException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void ExitMenu()
        {
            Console.Clear();
            Console.Write(new string('=', 18));
            Console.Write("SESSION EXITED");
            Console.WriteLine(new string('=', 18));
            Console.Write($"{"\nHave a lovely day":-33}");
            _engine = default;
            return;
        }
    }
}
