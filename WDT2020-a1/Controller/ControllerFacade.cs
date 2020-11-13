using System;
using System.Collections.Generic;
using System.Data;

using WDT2020_a1.Model;

namespace WDT2020_a1.Controller
{
    public class ControllerFacade
    {
        private CustomerController _customerManager;
        private LoginController _loginManager;
        private AccountsController _accountsManager;
        private TransactionController _transactionManager;



        public ControllerFacade(DataTable _inMemoryAuthentication)
        {
            _customerManager = new CustomerController(_inMemoryAuthentication);
            _loginManager = new LoginController(_inMemoryAuthentication);
            _accountsManager = new AccountsController(_inMemoryAuthentication);
            _transactionManager = new TransactionController(_inMemoryAuthentication);
        }

        ///<summary>Empty Constructor for testing purposes</summary>
        public ControllerFacade() { }

        /*
         *    CUSTOMERS
         *    
         */

        ///<summary>Insert a new customer into the database, all properties full</summary>
        public void InsertNewCustomerComplete(Customer customer)
        {
            _customerManager.Insert(customer);
        }

        ///<summary>Insert a new customer into the database, only their name and Customer ID</summary>
        public void InsertName(Customer customer)
        {
            _customerManager.Insert(customer);
        }

        ///<summary>Update database with Customer details</summary>
        public void UpdateCustomer(Customer customer)
        {
            _customerManager.Update(customer);
        }

        ///<summary>Return a customer from the database, using a given Customer ID</summary>
        public Customer GetCustomer(int custId)
        {
            return _customerManager.Get(custId);
        }

        ///<summary>Get a List of Customers in the Database</summary>
        public List<Customer> GetCustomers()
        {
            return _customerManager.GetAll();
        }

        ///<summary>Check that the CustomerID exists in the database - customer table</summary>
        public bool DoesCustomerExist(int custID)
        {
            return _customerManager.DoesExist(custID);
        }


        /*
         *       LOGINS
         *       
         */

        ///<summary>Insert new login details to the database</summary>
        public void InsertLogin(Login confirmDetails)
        {
            _loginManager.Insert(confirmDetails);
        }

        ///<summary>Check that the LoginId exists in the database</summary>
        public bool DoesLoginExist(string loginID)
        {
            return _loginManager.DoesExist(Convert.ToInt32(loginID));
        }

        ///<summary>Return Login object given a Login ID string</summary>
        public Login GetLogin(string loginID)
        {
            return _loginManager.Get(Convert.ToInt32(loginID));
        }

        ///<summary>Get a List of Logins from the Database</summary>
        public List<Login> GetLogins()
        {
            return _loginManager.GetAll();
        }

        ///<summary>Return Login object given a Customer ID string</summary>
        public Login GetLoginUsingCust(int custID)
        {
            return _loginManager.GetLoginDetails(custID);
        }

        ///<summary>Return login PasswordHash, given a Login ID</summary>
        public string GetHashedPassword(string loginID)
        {
            return _loginManager.GetPasswordHash(Convert.ToInt32(loginID));
        }

        ///<summary>Update The Hashed Password Stored in Database</summary>
        public void UpdatePassword(Login login)
        {
            _loginManager.Update(login);
        }

        ///<summary>Retrieve how many password attempts are logged in the [Login] table</summary>
        public int PasswordAttempts(string loginID)
        {
            return _loginManager.GetPasswordAttempts(Convert.ToInt32(loginID));
        }

        ///<summary>Increment attempts on the password</summary>
        public void PasswordFailed(string loginID)
        {
            var login = _loginManager.Get(Convert.ToInt32(loginID));
            login.FailedPasswordAttempt();

            _loginManager.Update(login);
        }

        ///<summary>Reset attempts on the password</summary>
        public void PasswordAccepted(string loginID)
        {
            var login = _loginManager.Get(Convert.ToInt32(loginID));
            login.ResetPasswordAttempts();

            _loginManager.Update(login);
        }



        /*
         *       ACCOUNTS
         *       
         */


        ///<summary>Insert Account Details</summary>
        public void InsertAccountDetails(Account account)
        {
            _accountsManager.Insert(account);
        }

        ///<summary>Update the Accounts details</summary>
        public void UpdateAccount(Account account)
        {
            _accountsManager.Update(account);
        }

        ///<summary>Get a List of all associated Accounts</summary>
        public List<Account> GetAccounts(int custID)
        {
            return _accountsManager.GetAll(custID);
        }
        ///<summary>Get a specific account by account number</summary>
        public Account GetAccount(int accountNo)
        {
            return _accountsManager.Get(accountNo);
        }

        ///<summary>Update account balance</summary>
        public void Update(Account account)
        {
            _accountsManager.Update(account);
        }

        ///<summary>Checks if an accountID already exists</summary>
        public bool DoesExist(int accountID)
        {
            return _accountsManager.DoesExist(accountID);
        }

        ///<summary>Check if an account number exists</summary>
        public bool DoesAccountExist(int accountNo)
        {
            return _accountsManager.DoesExist(accountNo);
        }



        /*
         *       TRANSACTIONS
         *       
         */


        ///<summary>Insert Transaction Details</summary>
        public void InsertTransaction(Transaction transaction)
        {
            _transactionManager.Insert(transaction);
        }

        ///<summary>Get a List of all associated Transactions</summary>
        public List<Transaction> GetTransactions(int accountNo)
        {
            return _transactionManager.GetAll(accountNo);
        }
    }
}
