using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WDT2020_a1.Model
{
    public class Customer
    {
        //This is to conform 
        public int CustomerID { get; }
        public string Name { get; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }

        public List<Account> Accounts { get; set; }

        [JsonConstructor]
        public Customer(int CustomerID, string Name, string Address, string City, string PostCode)
        {
            this.CustomerID = CustomerID;
            this.Name = Name;
            this.Address = Address;
            this.City = City;
            this.PostCode = PostCode;
        }

        public Customer(int custID, string name)
        {
            CustomerID = custID;
            Name = name;
        }


        public void AddAccount(Account account)
        {
            Accounts.Add(account);
        }
    }
}
