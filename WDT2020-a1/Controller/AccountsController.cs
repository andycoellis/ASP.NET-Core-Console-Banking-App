using System;
using System.Collections.Generic;
using System.Linq;
using WDT2020_a1.Model;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WDT2020_a1.Controller
{
    public class AccountsController : IController<Account>
    {
        private DataTable _inMemoryAuthentication;


        public AccountsController(DataTable inMemoryAuthentication)
        {
            _inMemoryAuthentication = inMemoryAuthentication;
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(AppEngine.ConnectionString);
        }

        public void Insert(Account item)
        {
            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "INSERT into Account values (@accountNumber, @accountType, @customerID, @balance)";
            command.Parameters.AddWithValue("accountNumber", item.AccountNumber);
            command.Parameters.AddWithValue("accountType", item.AccountType);
            command.Parameters.AddWithValue("customerID", item.CustomerID);
            command.Parameters.AddWithValue("balance", item.Balance);

            try
            {
                command.ExecuteNonQuery();

                _inMemoryAuthentication.AcceptChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }
        }

        public void Update(Account item)
        {
            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            "UPDATE Account set Balance = @balance where AccountNumber = @accountNumber";
            command.Parameters.AddWithValue("balance", item.Balance);
            command.Parameters.AddWithValue("accountNumber", item.AccountNumber);

            try
            {
                command.ExecuteNonQuery();

                _inMemoryAuthentication.AcceptChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }
        }

        public Account Get(int accountID)
        {
            int accountNum = 0;
            char accountType = ' ';
            int customerID = 0;
            double balance = 0;

            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from Account WHERE AccountNumber = @accountNumber";
            command.Parameters.AddWithValue("accountNumber", accountID);

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            try
            {
                foreach (var entry in _inMemoryAuthentication.Select())
                {
                    if (Convert.ToInt32(entry["AccountNumber"]) == accountID)
                    {
                        accountNum = Convert.ToInt32(entry["AccountNumber"]);
                        accountType = Convert.ToChar(entry["AccountType"]);
                        customerID = Convert.ToInt32(entry["CustomerID"]);
                        balance = Convert.ToDouble(entry["Balance"]);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }

            var account = new Account(accountNum, accountType, customerID, balance);

            return account;
        }


        public List<Account> GetAll(int customerID)
        {
            List<Account> accounts = new List<Account>();

            using var connection = CreateConnection();
            connection.Open();

            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from Account WHERE CustomerID = @customerID";
            command.Parameters.AddWithValue("customerID", customerID);

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);


            foreach (var entry in _inMemoryAuthentication.Select())
            {
                var accountNumber = Convert.ToInt32(entry["AccountNumber"]);
                var accountType = Convert.ToChar(entry["AccountType"]);
                var balance = Convert.ToDouble(entry["Balance"]);

                accounts.Add(new Account(accountNumber, accountType, customerID, balance));
            }
            return accounts;
        }


        public bool DoesExist(int accountNumber)
        {
            var response = false;

            using var connection = CreateConnection();
            connection.Open();

            //Load all available accountNumber into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from Account";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);


            foreach (var entry in _inMemoryAuthentication.Select())
            {
                if (Convert.ToInt32(entry["AccountNumber"]) == accountNumber)
                {
                    response = true;
                }
            }
            return response;
        }
    }
}
