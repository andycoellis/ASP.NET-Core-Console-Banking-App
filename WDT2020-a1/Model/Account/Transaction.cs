using System;
using Newtonsoft.Json;

namespace WDT2020_a1.Model
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public char TransactionType { get; set; }
        public int AccountNumber { get; set; }
        public int DestinationAccountNumber { get; set; }
        public double Amount { get; set; }
        public string Comment { get; set; }
        public DateTime TransactionTimeUtc { get; set; }

        [JsonConstructor]
        public Transaction(string TransactionTimeUtc)
        {
            this.TransactionTimeUtc = DateTime.Parse(TransactionTimeUtc);
        }

        public Transaction(char transactionType, int accountNumber, double amount, DateTime date)
        {
            TransactionType = transactionType;
            AccountNumber = accountNumber;
            DestinationAccountNumber = accountNumber;
            Amount = amount;
            TransactionTimeUtc = date;
        }

        public Transaction(char transactionType, int accountNumber, int destinationAccountNumber, double amount, DateTime date)
        {
            TransactionType = transactionType;
            AccountNumber = accountNumber;
            DestinationAccountNumber = destinationAccountNumber;
            Amount = amount;
            TransactionTimeUtc = date;
        }

        public Transaction
            (
            int transactionID, char transactionType,
            int accountNumber, int destinationAccountNumber,
            double amount, string comment, DateTime transactionTimeUtc
            )
        {
            TransactionID = transactionID;
            TransactionType = transactionType;
            AccountNumber = accountNumber;
            DestinationAccountNumber = destinationAccountNumber;
            Amount = amount;
            Comment = comment;
            TransactionTimeUtc = transactionTimeUtc;
        }

        /// <summary>
        /// Call this to update Transaction after initialised with JSON
        /// This will fill 
        /// </summary>
        /// <param name="account"></param>
        public void UpdateTransactionDetails(Account account)
        {
            TransactionType = 'D';
            AccountNumber = account.AccountNumber;
            Amount = account.Balance;
            DestinationAccountNumber = account.AccountNumber;
        }

        public override string ToString()
        {
            return $"TransactionID: {TransactionID}\n" +
                   $"Transaction Type: {TransactionType}\n" +
                   $"Account Number: {AccountNumber}\n" +
                   $"Destination Account Number: {DestinationAccountNumber}\n" +
                   $"Amount: {Amount}\n" +
                   $"Comment: {Comment}\n" +
                   $"Transaction Date & Time: {TransactionTimeUtc.ToLocalTime()}\n";
        }
    }
}
