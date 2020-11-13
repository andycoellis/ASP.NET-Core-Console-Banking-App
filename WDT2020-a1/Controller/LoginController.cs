using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WDT2020_a1.Model;

namespace WDT2020_a1.Controller
{
    public class LoginController : IController<Login>
    {
        private DataTable _inMemoryAuthentication;


        public LoginController(DataTable inMemoryAuthentication)
        {
            _inMemoryAuthentication = inMemoryAuthentication;
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(AppEngine.ConnectionString);
        }

        public void Insert(Login item)
        {
            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "INSERT into LOGIN values (@loginID, @customerID, @passwordHash, @passwordAttempts)";
            command.Parameters.AddWithValue("loginID", item.LoginID);
            command.Parameters.AddWithValue("customerID", Convert.ToInt32(item.CustomerID));
            command.Parameters.AddWithValue("passwordHash", item.PasswordHash);
            command.Parameters.AddWithValue("passwordAttempts", Convert.ToInt32(item.PasswordAttempts));

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

        public List<Login> GetAll()
        {
            List<Login> logins = new List<Login>();

            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = "select * from Login";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            try
            {
                foreach (var entry in _inMemoryAuthentication.Select())
                {
                    var login = new Login();

                    login.LoginID = entry["LoginID"].ToString();
                    login.CustomerID = Convert.ToInt32(entry["CustomerID"]);
                    login.PasswordHash = entry["PasswordHash"].ToString();
                    login.PasswordAttempts = Convert.ToInt32(entry["PasswordAttempts"]);

                    logins.Add(login);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }

            return logins;
        }

        public List<Login> GetAll(int id)
        {
            throw new CustomException("Not implemented for this Controller");
        }

        public Login Get(int loginID)
        {
            int custID = 0;
            string hash = "";
            int passwordAttempts = 0;

            using var connection = CreateConnection();
            connection.Open();

            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from Login WHERE LoginID = @loginID";
            command.Parameters.AddWithValue("loginID", loginID.ToString());

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            try
            {
                foreach (var entry in _inMemoryAuthentication.Select())
                {
                    if (Convert.ToInt32(entry["LoginID"]) == loginID)
                    {
                        custID = Convert.ToInt32(entry["CustomerID"]);
                        hash = entry["PasswordHash"].ToString();
                        passwordAttempts = Convert.ToInt32(entry["PasswordAttempts"]);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }

            return new Login(loginID.ToString(), custID, hash, passwordAttempts);
        }

        /// <summary>Return Login details from a provided Customer ID</summary>
        internal Login GetLoginDetails(int customerID)
        {
            string loginID = "";
            string hash = "";
            int passwordAttempts = 0;

            using var connection = CreateConnection();
            connection.Open();

            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from Login WHERE CustomerID = @custID";
            command.Parameters.AddWithValue("custID", customerID);

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);

            try
            {
                foreach (var entry in _inMemoryAuthentication.Select())
                {
                    if (Convert.ToInt32(entry["CustomerID"]) == customerID)
                    {
                        loginID = entry["LoginID"].ToString();
                        hash = entry["PasswordHash"].ToString();
                        passwordAttempts = Convert.ToInt32(entry["PasswordAttempts"]);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e);
            }

            return new Login(loginID, customerID, hash);
        }

        public void Update(Login item)
        {
            using var connection = CreateConnection();
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "UPDATE LOGIN set PasswordHash = @passwordHash, PasswordAttempts = @passwordAttempts  where LoginID = @loginID";
            command.Parameters.AddWithValue("passwordHash", item.PasswordHash);
            command.Parameters.AddWithValue("passwordAttempts", item.PasswordAttempts);
            command.Parameters.AddWithValue("loginID", item.LoginID);

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

        public bool DoesExist(int id)
        {
            bool response = false;

            if (id.ToString().Length == AppEngine.CUST_ID_LENGTH)
                response = CheckCustID(id);

            if (id.ToString().Length == AppEngine.LOGIN_ID_LENGTH)
                response = CheckLoginID(id);

            return response;
        }

        ///<summary>Iterates over database-login searching for LoginID return true if found</summary>
        private bool CheckLoginID(int loginID)
        {
            var response = false;

            using var connection = CreateConnection();
            connection.Open();

            //Load all available loginID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from LOGIN";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);


            foreach (var entry in _inMemoryAuthentication.Select())
            {
                if (Convert.ToInt32(entry["LoginID"]) == loginID)
                {
                    response = true;
                }
            }

            return response;
        }

        ///<summary>Iterates over database-login seravhing for customerID return true if found</summary>
        private bool CheckCustID(int custID)
        {

            var response = false;

            using var connection = CreateConnection();
            connection.Open();


            //Load all available custID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from LOGIN";

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

        ///<summary>Retrieve stored hash in the database</summary>
        public string GetPasswordHash(int loginID)
        {
            var storedHash = "";

            using var connection = CreateConnection();
            connection.Open();

            //Load all available loginID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from LOGIN";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);


            foreach (var entry in _inMemoryAuthentication.Select())
            {
                if (Convert.ToInt32(entry["LoginID"]) == loginID)
                {
                    storedHash = entry["PasswordHash"].ToString();
                }
            }

            return storedHash;
        }

        ///<summary>Retrieve how many times the password login is failed</summary>
        public int GetPasswordAttempts(int loginID)
        {
            int passwordAttempts = 0;

            using var connection = CreateConnection();
            connection.Open();

            //Load all available loginID into memory
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * from LOGIN";

            _inMemoryAuthentication = new DataTable();
            new SqlDataAdapter(command).Fill(_inMemoryAuthentication);


            foreach (var entry in _inMemoryAuthentication.Select())
            {
                if (Convert.ToInt32(entry["LoginID"]) == loginID)
                {
                    passwordAttempts = Convert.ToInt32(entry["PasswordAttempts"]);
                }
            }
            return passwordAttempts;
        }
    }
}
