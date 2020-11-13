using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.Data.SqlClient;
using WDT2020_a1.Model;

namespace WDT2020_a1.Controller
{
    public class CustomerController : IController<Customer>
    {
        private DataTable _inMemoryAuthentication;


        public CustomerController(DataTable inMemoryAuthentication)
        {
            _inMemoryAuthentication = inMemoryAuthentication;
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(AppEngine.ConnectionString);
        }

        public void Insert(Customer customer)
        {
            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "INSERT into Customer values (@customerID, @name, @address, @city, @postCode)";
            command.Parameters.AddWithValue("customerID", Convert.ToInt32(customer.CustomerID));
            command.Parameters.AddWithValue("name", customer.Name);
            command.Parameters.AddWithValue("address", string.IsNullOrEmpty(customer.Address) ? "null" : customer.Address);
            command.Parameters.AddWithValue("city", string.IsNullOrEmpty(customer.City) ? "null" : customer.City);
            command.Parameters.AddWithValue("postCode", string.IsNullOrEmpty(customer.PostCode) ? "null" : customer.PostCode);

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

        public List<Customer> GetAll()
        {
            List<Customer> customers = new List<Customer>();

            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = "select * from Customer";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            try
            {
                foreach (var entry in _inMemoryAuthentication.Select())
                {
                    var customer = new Customer(Convert.ToInt32(entry["CustomerID"]), entry["Name"].ToString());

                    customer.Address = entry["Address"].ToString();
                    customer.City = entry["City"].ToString();
                    if (!entry["PostCode"].ToString().Equals(""))
                        customer.PostCode = entry["PostCode"].ToString();

                    customers.Add(customer);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }

            return customers;
        }

        public Customer Get(int custID)
        {
            string name = "";
            string address = "";
            string city = "";
            string postCode = "";

            using var connection = CreateConnection();
            connection.Open();

            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from Customer WHERE CustomerID = @customerID";
            command.Parameters.AddWithValue("customerID", custID);

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            try
            {
                foreach (var entry in _inMemoryAuthentication.Select())
                {
                    if (Convert.ToInt32(entry["CustomerID"]) == custID)
                    {
                        name = entry["Name"].ToString();
                        address = entry["Address"].ToString();
                        city = entry["City"].ToString();
                        if (!entry["PostCode"].ToString().Equals(""))
                            postCode = entry["PostCode"].ToString();

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }

            var customer = new Customer(custID, name);
            customer.Address = address;
            customer.City = city;
            customer.PostCode = postCode;

            return customer;
        }

        public void Update(Customer item)
        {
            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "UPDATE Customer set Address = @address, City = @city, PostCode = @postCode where CUSTOMERID = @customerID";
            command.Parameters.AddWithValue("address", item.Address);
            command.Parameters.AddWithValue("customerID", Convert.ToInt32(item.CustomerID));
            command.Parameters.AddWithValue("city", item.City);
            command.Parameters.AddWithValue("postCode", item.PostCode);

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

        public bool DoesExist(int custID)
        {
            var response = false;

            using var connection = CreateConnection();
            connection.Open();

            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from Customer";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);


            foreach (var entry in _inMemoryAuthentication.Select())
            {
                if (Convert.ToInt32(entry["CustomerID"]) == custID)
                {
                    response = true;
                }
            }
            return response;
        }

        ///<summary>This method does not exist for the Customer implementation</summary>
        public List<Customer> GetAll(int id)
        {
            throw new CustomException("Does not exist for Customer");
        }
    }
}
