using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WDT2020_a1.Controller;
using WDT2020_a1.Model;
using Xunit;

namespace WDT2020_a1.Tests
{
    public class LoginTests
    {
        //Setup for tests
        AppEngine auth;
        ControllerFacade controller;
        public const string PASSWORD = "password";
        public string HashedPassword;


        //Setup constructor for test class
        public LoginTests()
        {
            auth = new AppEngine();
            controller = new ControllerFacade();
            HashedPassword = auth.GenerateHash(PASSWORD);

        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("this is a password fail")]
        [InlineData(" password")]
        [InlineData(" password ")]
        [InlineData("word")]
        [InlineData("        ")]
        [InlineData("ab")]
        [InlineData("1234")]
        public void AuthenticatePassword_ArgumentException(string password)
        {
            Assert.Throws<CustomException>(() => auth.AuthenticatePassword(password, new LoginTests().HashedPassword));
        }

        [Theory]
        [ClassData(typeof(LoginNullTestData))]
        public void AuthenticatePassword_ArgumentNullException(string password, string hash)
        {
            Assert.Throws<NullReferenceException>(() => auth.AuthenticatePassword(password, hash));
        }

        [Theory]
        [InlineData("password", 64, true)]
        [InlineData("thisIsALongerPassword", 64, true)]
        [InlineData("five5", 64, true)]
        [InlineData("password", 32, false)]
        public void GenertateHash_AppropriateSize(string password, int intSize, bool expected)
        {
            var hashSize = auth.GenerateHash(password).Length;

            var actual = hashSize == intSize;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("this is a password fail")]
        [InlineData(" password")]
        [InlineData(" password ")]
        [InlineData("word")]
        [InlineData("        ")]
        [InlineData("ab")]
        [InlineData("1234")]
        public void GenertateHash_ArugmentException(string password)
        {
            Assert.Throws<CustomException>(() => auth.GenerateHash(password));
        }

        [Theory]
        [InlineData(null)]
        public void GenertateHash_ArgumentNullException(string password)
        {
            Assert.Throws<NullReferenceException>(() => auth.GenerateHash(password));
        }

        [Theory]
        [InlineData(8)]
        public void GenerateLoginId_CorrectSize(int expected)
        {
            var actual = auth.GenerateLoginId().Length;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(4)]
        public void GenerateCustId_CorrectSize(int expected)
        {
            var actual = auth.GenerateCustId().ToString().Length;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        public void DoesLoginIdExist_ArgumentNullException(string id)
        {
            Assert.Throws<NullReferenceException>(() => auth.DoesLoginIdExist(id));
        }

        [Theory]
        [InlineData("123456788")]
        [InlineData("123")]
        [InlineData(" 1234567")]
        [InlineData("1234567 ")]
        [InlineData("123 5678")]
        [InlineData("123 56 8")]
        public void DoesLoginIdExist_ArgumentException(string id)
        {
            Assert.Throws<CustomException>(() => auth.DoesLoginIdExist(id));
        }

        [Theory]
        [InlineData(null)]
        public void DoesCustIdExist_ArgumentNullException(string id)
        {
            Assert.Throws<CustomException>(() => auth.DoesCustIdExist(Convert.ToInt32(id)));
        }

        [Theory]
        [InlineData("123")]
        [InlineData("12")]
        [InlineData("12345")]
        public void DoesCustIdExist_ArgumentException(string id)
        {
            Assert.Throws<CustomException>(() => auth.DoesCustIdExist(Convert.ToInt32(id)));
        }
    }

    //Test data to load into a collection of responses
    public class LoginNullTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new Object[] { null, new LoginTests().HashedPassword };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}