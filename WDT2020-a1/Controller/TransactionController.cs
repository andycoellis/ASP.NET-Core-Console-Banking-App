using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WDT2020_a1.Model;

namespace WDT2020_a1.Controller
{
    public class TransactionController : IController<Transaction>
    {
        private DataTable _inMemoryAuthentication;


        public TransactionController(DataTable inMemoryAuthentication)
        {
            _inMemoryAuthentication = inMemoryAuthentication;
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(AppEngine.ConnectionString);
        }

        ///<summary>Updates details that were null allowed and insertion</summary>
        public void Insert(Transaction item)
        {
            var time = item.TransactionTimeUtc.ToString("yyyy-MM-dd HH:mm");

            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "INSERT into [Transaction] values ( @transactionType, @accountNumber, @destinationAccountNumber, @amount, @comment, @transactionTime)";
            command.Parameters.AddWithValue("transactionType", item.TransactionType);
            command.Parameters.AddWithValue("accountNumber", Convert.ToInt32(item.AccountNumber));
            command.Parameters.AddWithValue("destinationAccountNumber", Convert.ToInt32(item.DestinationAccountNumber));
            command.Parameters.AddWithValue("amount", Convert.ToDouble(item.Amount));
            command.Parameters.AddWithValue("comment", item.Amount);
            command.Parameters.AddWithValue("transactionTime", time);

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

        ///<summary>Find and return all Transactions associated with an account number</summary>
        public List<Transaction> GetAll(int accountNumber)
        {
            List<Transaction> transactions = new List<Transaction>();

            using var connection = CreateConnection();
            connection.Open();

            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from [Transaction] WHERE AccountNumber = @accountNumber";
            command.Parameters.AddWithValue("accountNumber", accountNumber);

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            foreach (var entry in _inMemoryAuthentication.Select())
            {
                var transactionID = Convert.ToInt32(entry["TransactionID"]);
                var transactionType = Convert.ToChar(entry["TransactionType"]);
                var accNo = Convert.ToInt32(entry["AccountNumber"]);
                int destinationAccount;
                if (string.IsNullOrWhiteSpace(entry["DestinationAccountNumber"].ToString()))
                {
                    destinationAccount = -1;
                }
                else
                    destinationAccount = Convert.ToInt32(entry["DestinationAccountNumber"]);
                var amount = Convert.ToDouble(entry["Amount"]);
                var comment = entry["Comment"].ToString();
                DateTime transactionTime = (DateTime)entry["TransactionTimeUtc"];


                transactions.Add
                        (new Transaction
                        (
                           transactionID,
                           transactionType,
                           accNo,
                           destinationAccount,
                           amount,
                           comment,
                           transactionTime
                        ));

            }
            return transactions;

        }

        /// <summary>Checks if any transactions exist given an account number</summary>
        public bool DoesExist(int accountNumber)
        {
            bool response = false;
            List<Transaction> transactions = new List<Transaction>();

            using var connection = CreateConnection();
            connection.Open();

            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from [Transaction] WHERE AccountNumber = @accountNumber";
            command.Parameters.AddWithValue("accountNumber", accountNumber);

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            if (_inMemoryAuthentication.Select().Length > 0)
                response = true;

            return response;
        }

        public List<Transaction> GetAll()
        {
            throw new CustomException("Not implemented for thie controller");
        }

        public Transaction Get(int id)
        {
            throw new CustomException("Not implemented for thie controller");
        }

        public void Update(Transaction item)
        {
            throw new CustomException("Not implemented for thie controller");
        }
    }
}