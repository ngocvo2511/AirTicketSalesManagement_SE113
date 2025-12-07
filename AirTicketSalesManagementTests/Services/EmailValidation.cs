using AirTicketSalesManagement.Services.EmailValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests.Services
{
    [TestFixture]
    public class EmailValidationTests
    {
        private EmailValidation _validation;

        [SetUp]
        public void Setup()
        {
            _validation = new EmailValidation();
        }

        [TestCase("", false)]
        [TestCase("ngocvo2502", false)]
        [TestCase("ngocvo2502@", false)]
        [TestCase("ngocvo2502@gmail", false)]
        [TestCase("ngocvo2502@.com", false)]
        [TestCase("@gmail.com", false)]
        [TestCase("NGOCVO2502@ANONYVIET.COM.VN", true)]
        [TestCase("ngocvo2502@gmail.com", true)]
        public void IsValid_ShouldReturnExpectedResult(string email, bool expected)
        {
            bool result = _validation.IsValid(email);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void IsValid_ShouldReturnFalse_WhenEmailIsNull()
        {
            var validator = new EmailValidation();
            Assert.IsFalse(validator.IsValid(null));
        }
    }
}
