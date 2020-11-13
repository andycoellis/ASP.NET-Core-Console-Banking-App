using System;
using System.Collections.Generic;

namespace WDT2020_a1.Model
{
    public class Account
    {

        public int AccountNumber { get; set; }
        public char AccountType { get; set; }
        public int CustomerID { get; set; }
        public double Balance { get; set; }

        public List<Transaction> Transactions { get; set; }

        public Account(int accountNumber, char accountType, int customerID, double balance)
        {
            AccountNumber = accountNumber;
            AccountType = accountType;
            CustomerID = customerID;
            Balance = balance;
        }
    }
}
