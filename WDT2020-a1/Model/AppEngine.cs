using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SimpleHashing;
using WDT2020_a1.Controller;

namespace WDT2020_a1.Model
{

    public class AppEngine
    {
        //Setup of configurations to get data from web server
        private static IConfigurationRoot Configuration { get; } =
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static string ConnectionString { get; } = Configuration["ConnectionString"];

        //Associated Controllers
        private ControllerFacade _controller;
        private DataTable _inMemoryAuthentication;
        private Login _confirmDetails;

        //Account Functions
        private AccountFunctions _accounts;
        private ATMFunctions _atmFunctions;

        //Id length constraints
        internal const int CUST_ID_LENGTH = 4;
        internal const int LOGIN_ID_LENGTH = 8;
        internal const int PASSWORD_LENGTH_MIN = 5;


        //Allows the AuthenticationController to propargate through the application
        public AppEngine()
        {

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            //Load all available loginID into memory
            var command = connection.CreateCommand();
            command.CommandText = "select * from Login";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            _controller = new ControllerFacade(_inMemoryAuthentication);
            _confirmDetails = new Login();

            try
            {
                //Generate Customer from REST webservice
                GetServerCustomers();

                //Generate Customer Logins from REST webservice
                GetServerLogins();
            }
            catch(AggregateException e)
            {
                Console.WriteLine($"SERVER ERROR: We are unable to locate the server at the moment\n\n{e.Message}\n\n" +
                    "SYSTEM WILL CONTINUE WITHOUT SERVER DATA\n");
                Console.WriteLine("...please press any key to continue");
                Console.ReadKey(true);
            }

            //Attach Account Functionality
            _accounts = new AccountFunctions(_controller);
            _atmFunctions = new ATMFunctions(_controller);
        }


       /*
        *           LOGIN FUNCTIONALITY
        *
        */

        ///<summary>Return the Login of a customer</summary>
        public Login GetLogin(string loginID)
        {
            Validation.ValidateID(loginID, "Login");

            if (!_controller.DoesLoginExist(loginID))
                throw new CustomException("This Login ID does not exist in the Database");

            return _controller.GetLogin(loginID);
        }

        ///<summary>Return the Login of a customer</summary>
        public Login GetLoginFromCustID(int custID)
        {
            Validation.ValidateID(custID.ToString(), "Customer");

            if (!_controller.DoesCustomerExist(custID))
                throw new CustomException("This Customer ID does not exist in the Database");

            return _controller.GetLoginUsingCust(custID);
        }


        ///<Summary>Check that the customer login ID exists in the database</Summary>
        public bool DoesLoginIdExist(string loginID)
        {
            Validation.ValidateID(loginID, "Login");

            return _controller.DoesLoginExist(loginID);
        }


        ///<summary>Creates an in memory object of Login</summary>
        public void CreateNewLoginDetails(Customer customer, string password)
        {
            if (!DoesCustIdExist(customer.CustomerID))
                throw new CustomException("The customer id does not exist in the database");

            //The struct CustID takes in an int => as does the Database
            _confirmDetails.CustomerID = Convert.ToInt32(customer.CustomerID);

            //Add login Id to the temp struct
            _confirmDetails.LoginID = GenerateLoginId();

            //Add hash to temp struct
            _confirmDetails.PasswordHash = GenerateHash(password);

            ConfirmLoginDetails();
        }


        ///<summary>Previews the in memory object of Login</summary>
        public string PreviewLoginDetails()
        {
            var callback =
                $"Login ID: {(_confirmDetails.LoginID != null ? _confirmDetails.LoginID : "Empty")}" +
                $"\nCust ID: {_confirmDetails.CustomerID}" +
                $"{(_confirmDetails.PasswordHash != null ? "\n\nPassword is Complete - READY TO SUBMIT" : "Password needs To Be Added")}";

            return callback;
        }

        ///<summary>Confirm details of the Login object before committing to the database</summary>
        public void ConfirmLoginDetails()
        {
            if (!IsLoginApplicationComplete())
            {
                throw new InvalidOperationException("There are null properties in your login application");
            }

            _controller.InsertLogin(_confirmDetails);

            //Clear the in memory as to not allow floating sensitive information
            _confirmDetails = default;
            _confirmDetails = new Login();
        }

        ///<summary>Checks that all Login object properties are filled out before commiting to database</summary>
        private bool IsLoginApplicationComplete()
        {
            var response = true;

            if (_confirmDetails.CustomerID.Equals(null) || _confirmDetails.PasswordHash == null || _confirmDetails.LoginID == null)
            {
                response = false;
            }

            return response;
        }

        /*
         *           CUSTOMER FUNCTIONALITY
         *
         */


        /// <summary> Creates a New Customer and adds them into the database.
        /// Returns the newly created Customer ID </summary> 
        public int CreateNewCustomer(string firstname, string lastname)
        {
            int custID = -1;

            try
            {
                custID = GenerateCustId();

                Validation.ValidateName(firstname);
                Validation.ValidateName(lastname);

                var name = $"{firstname} {lastname}";

                Customer customer = new Customer(custID, name);

                AddNewCustomer(customer);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                custID = -1;
            }

            return custID;
        }

        /// <summary> Validate customer to add to database </summary>  
        private void AddNewCustomer(Customer customer)
        {
            _controller.InsertName(customer);
        }

        public void UpdateAddress(Customer customer)
        {

            if (_controller.DoesCustomerExist(customer.CustomerID))
            {
                _controller.UpdateCustomer(customer);
            }
            else
                throw new CustomException("Customer does not exist in database");

        }


        ///<summary> Gets an existing customer from the database </summary>
        public Customer GetCustomer(int custID)
        {
            Validation.ValidateID(custID.ToString(), "Customer");

            if (!_controller.DoesCustomerExist(custID))
                throw new CustomException("This Customer ID does not exist in the Database");

            return _controller.GetCustomer(custID);
        }

        ///<summary>Return the amount of Transactions in an account</summary>
        public int TransactionsCount(int accountNumber)
        {
            return _controller.GetTransactions(accountNumber).Count;
        }


        ///<Summary>Checks that the customer ID exists in the data base</Summary>
        public bool DoesCustIdExist(int custID)
        {
            Validation.ValidateID(custID.ToString(), "Customer");

            return _controller.DoesCustomerExist(custID);
        }


       /*
        *            ACCOUNT FUNCTIONALITY
        *
        */

        ///<summary>Retrieve all available accounts</summary>
        public List<Account> GetAccounts(int customerID)
        {
            return _controller.GetAccounts(customerID);
        }

        ///<summary>Get a Total balance from accounts</summary>
        public string GetCustomerTotalBalance(int custID)
        {
            double amount = 0;

            var accounts = _controller.GetAccounts(custID);

            foreach (var account in accounts)
            {
                amount += account.Balance;
            }

            return amount.ToString();
        }

        ///<summary>Display ther current date and time to screen</summary>
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString("f", CultureInfo.CreateSpecificCulture("en-AU"));
        }

        ///<summary>Deposit Money into own account</summary>
        public void Deposit(int accountNumber, double amount)
        {
            _atmFunctions.Deposit(accountNumber, amount);
        }

        ///<summary>Withdraw from ones own account</summary>
        public bool Withdraw(int accountNumber, double amount)
        {
            return _atmFunctions.Withdraw(accountNumber, amount);
        }

        ///<summary>Transfer Money to another account</summary>
        public bool Transfer(int accountNumber, int destinationAccount, double amount)
        {
            return _atmFunctions.Transfer(accountNumber, destinationAccount, amount);
        }

        ///<summary>Get the Transaction History</summary>
        public List<List<Transaction>> TransactionHistory(int accountNumber)
        {
            return _accounts.GetTransactionHistory(accountNumber);
        }


        ///<summary>Get the last entered Transaction</summary>
        public string GetLatestTransaction(int accountNumber)
        {
            return _controller.GetTransactions(accountNumber).LastOrDefault().ToString();
        }

        ///<summary>Open a new account</summary>
        public bool OpenNewAccount(int customerID, char accountType, double amount)
        {
            return _accounts.OpenAccount(customerID, accountType, amount);
        }


        /*
         *           SECURITY / AUTHENTICATION
         *
         */

        ///<summary>Argument takes a check password and a stored hash from the database</summary>
        public bool AuthenticatePassword(string password, string loginID)
        {
            Validation.ValidatePassword(password);
            var storedHash = _controller.GetHashedPassword(loginID);

            bool response = false;

            if (PBKDF2.Verify(storedHash, password))
            {
                //Reset password attempts on successful login
                _controller.PasswordAccepted(loginID);

                response = true;
            }
            else
            {
                //Password Attempts is incremented
                _controller.PasswordFailed(loginID);
            }

            return response;
        }

        ///<summary>Reset Password</summary>
        public bool AuthenticateUser(string loginID, int custID)
        {
            bool response = false;

            if (!DoesLoginIdExist(loginID))
                throw new CustomException($"The Login: {loginID} does not exist");

            if (!DoesCustIdExist(custID))
                throw new CustomException($"The Customer: {custID} does not exist");

            var loginDetails = _controller.GetLogin(loginID);

            if (loginDetails.CustomerID == Convert.ToInt32(custID))
            {
                loginDetails.ResetPasswordAttempts();
                response = true;
            }

            return response;
        }


        ///<summary>Generates a hash</summary>
        public string GenerateHash(string password)
        {
            Validation.ValidatePassword(password);

            return PBKDF2.Hash(password);
        }

        ///<summary>Update Password, calls GenerateHash() and updates to Database</summary>
        public void UpdatePassword(string loginID, string newPassword)
        {
            var login = _controller.GetLogin(loginID);

            login.PasswordHash = GenerateHash(newPassword);

            _controller.UpdatePassword(login);
        }

        ///<summary>Retrieve the amount of password attempts</summary>
        public int GetPasswordAttempts(string loginID)
        {
            return _controller.PasswordAttempts(loginID);
        }

        ///<summary>Generates a random number with a given length, intended use for CustomerID and LoginID</summary>
        private string GenerateId(int length)
        {
            string result = "";
            int number;

            //Create a boundary where there will always be a min where 1 is the lead digit and max where 9 is the lead
            var min = Convert.ToInt32(Math.Pow(10, length) - (.9 * Math.Pow(10, length)));
            var max = Convert.ToInt32(Math.Pow(10, length) - 1);

            //Random number created with checks that it does not already exist in the database
            do { number = new Random().Next(min, max); }
            //Checking condition where the loop will continue if a customer or login id already exists in the database
            while (length == LOGIN_ID_LENGTH ? _controller.DoesLoginExist(number.ToString()) : _controller.DoesCustomerExist(number));

            result = number.ToString();

            if (result == null || result.Length != length)
            {
                throw new ArgumentOutOfRangeException("The ID was generated incorrectly");
            }

            return result;
        }


        ///<summary>Generates a new unique customer login ID [8 digits long]</summary>
        public string GenerateLoginId()
        {
            return GenerateId(LOGIN_ID_LENGTH);
        }

        ///<summary>Generates a new unique customer ID [4 digits long]</summary>
        public int GenerateCustId()
        {
            return Convert.ToInt32(GenerateId(CUST_ID_LENGTH));
        }



        /*
         *          LOAD FROM SERVER
         *
         * 
         */


        /// <summary>Attempts to retrieve all Customers from the database, if there are none then load from server</summary>
        private void GetServerCustomers()
        {

            //Nested loops search for all associated customers, accounts and transactions
            var customerManager = _controller.GetCustomers();
            if (customerManager.Any())
            {
                foreach (var customer in customerManager)
                {
                    customer.Accounts = _controller.GetAccounts(customer.CustomerID);

                    foreach (var account in customer.Accounts)
                    {
                        account.Transactions = _controller.GetTransactions(account.AccountNumber);
                    }
                }
                return;
            }

            /*
             *      PLEASE NOTE THE CODE BELOW TO CONTACT A REST SERVER WAS ORIGINALLY
             *      DESIGNED BY MATTHEW BOLGER FROM DAY THREE <RMIT> TUTORIAL EXAMPLE
             *
             *      [WebServiceAndDatabaseExample] <PersonWebService.cs>
             * 
             */

            using var client = new HttpClient();
            var json =
            client.GetStringAsync("https://coreteaching01.csit.rmit.edu.au/~e87149/wdt/services/customers/").Result;

            var customers = JsonConvert.DeserializeObject<List<Customer>>(json);

            foreach (var customer in customers)
            {
                _controller.InsertNewCustomerComplete(customer);

                foreach (var account in customer.Accounts)
                {
                    _controller.InsertAccountDetails(account);

                    foreach (var transaction in account.Transactions)
                    {
                        transaction.UpdateTransactionDetails(account);
                        _controller.InsertTransaction(transaction);
                    }
                }
            }
        }

           /*
            *      PLEASE NOTE THE CODE BELOW TO CONTACT A REST SERVER WAS ORIGINALLY
            *      DESIGNED BY MATTHEW BOLGER FROM DAY THREE <RMIT> TUTORIAL EXAMPLE
            *
            *      [WebServiceAndDatabaseExample] <PersonWebService.cs>
            * 
            */


        /// <summary>Attempts to retrieve all Customers from the database, if there are none then load from server</summary>
        private void GetServerLogins()
        {
            var loginManager = _controller.GetLogins();
            if (loginManager.Any())
                return;

            using var client = new HttpClient();
            var json =
            client.GetStringAsync("https://coreteaching01.csit.rmit.edu.au/~e87149/wdt/services/logins/").Result;
            var logins = JsonConvert.DeserializeObject<List<Login>>(json);

            foreach (var log in logins)
            {
                _controller.InsertLogin(log);
            }
        }
    }
}
