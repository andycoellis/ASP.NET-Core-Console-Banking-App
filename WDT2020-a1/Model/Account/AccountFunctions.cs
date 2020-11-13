using System;
using System.Collections.Generic;
using WDT2020_a1.Model;

namespace WDT2020_a1.Controller
{
    public class AccountFunctions
    {
        private ControllerFacade _controller;

        public AccountFunctions(ControllerFacade controller)
        {
            _controller = controller;
        }

        ///<summary>Open a new account for a Customer, with validation</summary>
        public bool OpenAccount(int custID, char accountType, double openingBalance)
        {
            // check if openingBalance is large enough to open account
            if (accountType == 'S' && openingBalance < 100)
                throw new CustomException("You need $100 or more to open a savings account");
            if (accountType == 'C' && openingBalance < 500)
                throw new CustomException("You must deposit $500 or more to open a checkings account");

            // check if customer already has an account of type
            var accounts = _controller.GetAccounts(custID);
            
          foreach (Account a in accounts)
            {
                if (a.AccountType == accountType)
                    // throw exception
                    throw new CustomException(
                        $"It looks like you already have the account: {(accountType == 'C' ? "Checking" : "Savings")}");
            }

            int number;

            // generate unique account number
            do { number = new Random().Next(1000, 9999); }
            // exits if number generated is not a used account number
            while (_controller.DoesAccountExist(number));

            // update database with new account
            var account = new Account(number, accountType, custID, openingBalance);
            _controller.InsertAccountDetails(account);

            // insert initial deposit transaction into database
            var transaction = new Transaction('D', number, openingBalance, DateTime.UtcNow);
            _controller.InsertTransaction(transaction);

            return true;
        }

        ///<summary>Display summary of all customer accounts</summary>
        public string DisplaySummary(int customerID)
        {
            // get all accounts of customer
            var accounts = _controller.GetAccounts(customerID);

            string summary = "Account Summary of " +
                    $"Customer ID {customerID}\n";

            double total = 0;

            foreach (Account a in accounts)
            {
                total += a.Balance;

                // set string based on Savings/Checking
                string type = a.AccountType == 'S' ? "Savings" : "Checking";

                // append summary string of account
                summary = summary + $"{type} Account Balance: {a.Balance:C2}\n";
            }

            // append total balance
            summary += $"Total Balance: {total:C2}";

            return summary;
        }

        ///<summary>Display summary of a single account</summary>
        public string ShowBalance(int accountNo)
        {
            // get specific account from database
            var acc = _controller.GetAccount(accountNo);

            // generate summary string
            string balance = $"Balance of Account {accountNo}:\n" +
                    $"{acc.Balance:C2}";

            return balance;
        }

        ///<summary>Return a pagenated list of list of 4 transactions per page</summary>
        public List<List<Transaction>> GetTransactionHistory(int accountNo)
        {
            // retrieve all transactions from database of an account
            var transactionHistory = _controller.GetTransactions(accountNo);

            // order by reversed datetime
            // https://stackoverflow.com/questions/1618863/how-to-sort-a-collection-by-datetime-in-c-sharp

            transactionHistory.Sort((y, x) => DateTime.Compare(x.TransactionTimeUtc, y.TransactionTimeUtc));

            // initialize empty lists for lists
            List<List<Transaction>> pagenatedList = new List<List<Transaction>>();
            // initialize empty list for transactions
            pagenatedList.Add(new List<Transaction>());

            int pageIndex = 0;
            int noOfTransactionsPerPage = 0;

            foreach (Transaction t in transactionHistory)
            {
                // add transaction to page
                pagenatedList[pageIndex].Add(t);
                noOfTransactionsPerPage++;

                // when 4 transactions added
                if (noOfTransactionsPerPage == 4)
                {
                    // move to next page
                    pageIndex++;
                    // initialize new empty list
                    pagenatedList.Add(new List<Transaction>());
                    // reset number of transactions on page
                    noOfTransactionsPerPage = 0;
                }
            }
            return pagenatedList;
        }
    }
}
