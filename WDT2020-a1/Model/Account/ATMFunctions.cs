using System;
using WDT2020_a1.Model;

namespace WDT2020_a1.Controller
{
    public class ATMFunctions
    {

        private ControllerFacade _controller;


        public ATMFunctions(ControllerFacade controller)
        {
            _controller = controller;
        }

        ///<summary>Deposit an amount into an account</summary>
        public void Deposit(int accountNo, double amount)
        {
            // get account from database
            var account = _controller.GetAccount(accountNo);

            account.Balance += amount;

            // update account in database
            _controller.UpdateAccount(account);

            // Insert deposit into database
            var transaction = new Transaction('D', accountNo, amount, DateTime.UtcNow);
            _controller.InsertTransaction(transaction);
        }

        ///<summary>Withdraw an amount from an account</summary>
        public bool Withdraw(int accountNo, double amount)
        {
            // initialize amount to charge, and transaction to free
            double charge = amount;
            bool free = true;

            // get account from database
            var account = _controller.GetAccount(accountNo);

            // check if not free, update total charge for validation
            if (!CheckIfFreeTransaction(accountNo))
            {
                free = false;
                charge = amount + (amount/10);
            }

            // validate amount, if too low, throw exception
            if (account.AccountType == 'S')
                { if (account.Balance < charge) throw new CustomException("Your savings account can not go below $0.00)"); }

            if (account.AccountType == 'C')
                // -200 as checking accounts cannot go below $200 balance
                { if (account.Balance - 200 < charge) throw new CustomException("Your checking account can not go below $200.00"); }

            account.Balance -= charge;

            // update account in database
            _controller.UpdateAccount(account);

            // Service Charge
            if (!free)
            {
                // create Service charge as 0.1 of amount of withdrawal as per business rules
                var serviceCharge = new Transaction('S', accountNo, (amount / 10), DateTime.UtcNow);
                _controller.InsertTransaction(serviceCharge);
            }

            // Withdrawal
            // create withdrawal, using amount entered, insert into database
            var withdrawal = new Transaction('W', accountNo, amount, DateTime.UtcNow);
            _controller.InsertTransaction(withdrawal);

            return true;
        }

        ///<summary>Transfer an amount between two accounts</summary>
        public bool Transfer(int accountNo, int destinationAccNo, double amount)
        {
            // initialize amount to charge, and transaction to free
            double charge = amount;
            bool free = true;

            // retrieve account from database
            var account = _controller.GetAccount(accountNo);

            // check if not free, update total charge if required
            if (!CheckIfFreeTransaction(accountNo))
            {
                free = false;
                charge = amount + (amount/5);
            }

            // validate if account will allow total charge
            if (account.AccountType == 'S')
                { if (account.Balance < charge) throw new CustomException("You do not have enough money in you account for the transfer"); }

            if (account.AccountType == 'C')
                // -200 as checking accounts cannot go below $200 balance
                { if (account.Balance - 200 < charge) throw new CustomException("Your checking account can not go below $200.00"); }

            account.Balance -= charge;

            // update account in database
            _controller.UpdateAccount(account);


            // Service Charge transaction
            if (!free)
            {
                // create Service charge as 0.2 of amount of transfer as per business rules
                var serviceCharge = new Transaction('S', accountNo, (amount / 5), DateTime.UtcNow);
                _controller.InsertTransaction(serviceCharge);
            }

            // add transfer to database with base amount transferred
            var transfer = new Transaction('T', accountNo, destinationAccNo, amount, DateTime.UtcNow);
            _controller.InsertTransaction(transfer);

            // Updating Destination Account
            // get destination account from Database...
            var destAccount = _controller.GetAccount(destinationAccNo);

            // update balance...
            destAccount.Balance += amount;
            _controller.UpdateAccount(destAccount);

            // insert Deposit transaction to reflect transfer
            var deposit = new Transaction('D', destinationAccNo, amount, DateTime.UtcNow);
            _controller.InsertTransaction(deposit);

            return true;
        }

        ///<summary>Checks if an account has any free transactions remaining</summary>
        private bool CheckIfFreeTransaction(int accountNo)
        {
            // get all transactions of the account
            var transactions = _controller.GetTransactions(accountNo);

            int count = 0;
            foreach (Transaction t in transactions)
            {
                // count how many withdrawals or transfers
                if (t.TransactionType == 'W' || t.TransactionType == 'T')
                {
                    count++;
                }
            }

            // If more than 4 W or T, all free transactions used
            return count < 4 ? true : false;
        }
    }
}
