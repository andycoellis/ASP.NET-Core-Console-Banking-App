using System;
using Newtonsoft.Json;

namespace WDT2020_a1.Model
{
    public class Login
    {
        public string LoginID { get; set; }
        public int CustomerID { get; set; }
        public string PasswordHash { get; set; }
        public int PasswordAttempts { get; set; }

        [JsonConstructor]
        public Login(string loginId, int custID, string hash)
        {
            LoginID = loginId;
            CustomerID = custID;    
            PasswordHash = hash;
            PasswordAttempts = 0;
        }

        public Login(string loginId, int custID, string hash, int passwordAttempts)
        {
            LoginID = loginId;
            CustomerID = custID;
            PasswordHash = hash;
            PasswordAttempts = passwordAttempts;
        }

        /// <summary>Empty constructor is for startup, empty template is avaliable for new customers</summary>
        public Login()
        {
            PasswordAttempts = 0;
        }

        public void ResetPasswordAttempts()
        {
            PasswordAttempts = 0;
        }

        public void FailedPasswordAttempt()
        {
            PasswordAttempts++;
        }
    }
}