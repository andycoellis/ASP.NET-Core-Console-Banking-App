using System;
using System.Text.RegularExpressions;

namespace WDT2020_a1.Model
{
    public static class Validation
    {
        private const string LOGIN = "Login";
        private const string CUSTOMER = "Customer";

        public static string ValidateID(string input, string idType)
        {
            int length = idType.Equals(LOGIN) ? AppEngine.LOGIN_ID_LENGTH : AppEngine.CUST_ID_LENGTH;

            if (input.Contains(" "))
                throw new CustomException(idType, "must not contain white spaces between digits");

            if (string.IsNullOrEmpty(input))
                throw new CustomException(idType, "no ID was given");

            if (input.Length != length)
                throw new CustomException(idType, $"ID given did not contain {idType} digits");

            if (!int.TryParse(input, out _))
                throw new CustomException(idType, "ID should not contain any letters");

            return input;
        }

        public static string AccountNumber(string input)
        {
            if (input.Contains(" "))
                throw new CustomException("Must not contain white spaces between digits");

            if (string.IsNullOrEmpty(input))
                throw new CustomException("No Account Number was given");

            if (input.Length != 4)
                throw new CustomException("ID given did not contain 4 digits");

            if (!int.TryParse(input, out _))
                throw new CustomException("Account Number should not contain any letters");

            return input;
        }

        public static string ValidateName(string name)
        {
            if (name.Contains(" "))
                throw new CustomException("A name can not contain white spaces");

            if (string.IsNullOrEmpty(name))
                throw new CustomException("No name was given");

            if (!Regex.IsMatch(name, @"^[a-zA-Z]+$"))
                throw new CustomException("A name must only contain letters of the alphabet");

            return name;
        }

        public static string ValidatePassword(string password)
        {
            if (password.Contains(" "))
                throw new CustomException("A password can not contain white spaces");

            if (string.IsNullOrEmpty(password))
                throw new CustomException("No password was given");

            if (password.Length < AppEngine.PASSWORD_LENGTH_MIN)
                throw new CustomException("Password must be a minimum of 5 characters in length");

            return password;
        }

        public static bool ValidateDoesUserWantMenu(string input)
        {
            input = input.ToUpper();
            bool response = false;

            if (input.Equals("H"))
            {
                HelpMenu();
                response = true;
            }

            if (input.Equals("HELP"))
            {
                HelpMenu();
                response = true;
            }

            if (input.Equals("M"))
                response = true;

            if (input.Equals("MENU"))
                response = true;

            if (input.Equals("HOME"))
                response = true;

            if (input.Equals("HOBIT"))
                response = true;

            return response;
        }

        public static bool ValidateIsUserExiting(string input)
        {

            input = input.ToUpper();
            bool response = false;

            if (input.Equals("Q"))
                response = true;

            if (input.Equals("QUIT"))
                response = true;

            if (input.Equals("EXIT"))
                response = true;

            if (input.Equals("SHIT"))
                response = true;

            if (input.Equals("WDT2020"))
                response = true;

            return response;
        }

        public static void ValidateMenuChoice(string input, int size)
        {
            if (string.IsNullOrEmpty(input))
                throw new CustomException("...cancelling");

            var min = 1;
            var max = size;

            var number = Convert.ToInt32(input);

            if (!Regex.IsMatch(input, "^[0-9]+$"))
                throw new CustomException("Please enter only numbers listed");

            if (number < min || number > max)
                throw new CustomException("Please enter a number from whats listed");
        }

        private static void HelpMenu()
        {
            Console.WriteLine();
            Console.WriteLine
                (
                $"{new string('+', 20)}" +
                $"HELP MENU" +
                $"{new string('+', 20)}\n\n" +
                $"To EXIT the system type Q, Quit or Exit\n" +
                $"To return back to LOGIN type Menu or Home\n\n" +
                $"{new string('+', 49)}\n"
                );

            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        public static void Money(string amount)
        {
            if (string.IsNullOrEmpty(amount))
                throw new CustomException("No amount was entered");

            if (double.Parse(amount) <= 0)
                throw new CustomException("You must enter an amount greater than zero");

            if (!Regex.IsMatch(amount, @"^[0-9]\d*(\.\d{1,2})?$"))
                throw new CustomException("Please only enter only numbers\nand only to two decimal places");


        }
    }
}