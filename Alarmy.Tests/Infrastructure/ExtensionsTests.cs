using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Tests.Infrastructure
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void RoundToMinute_ReturnsCurrentMinute_WhenSecondIsNull()
        {
            var value = new DateTime(2014, 11, 10, 23, 15, 0);
            var expected = new DateTime(2014, 11, 10, 23, 15, 0);
            Assert.AreEqual(expected, value.RoundToMinute());
        }

        [TestMethod]
        public void RoundToMinute_RoundsDown_WhenSecondIsLessThan30()
        {
            var value = new DateTime(2014, 11, 10, 23, 15, 1);
            var expected = new DateTime(2014, 11, 10, 23, 15, 0);
            Assert.AreEqual(expected, value.RoundToMinute());
        }

        [TestMethod]
        public void RoundToMinute_RoundsDown_WhenSecondIs30()
        {
            var value = new DateTime(2014, 11, 10, 23, 15, 30);
            var expected = new DateTime(2014, 11, 10, 23, 15, 0);
            Assert.AreEqual(expected, value.RoundToMinute());
        }

        [TestMethod]
        public void RoundToMinute_RoundsDown_WhenSecondIsMoreThan30()
        {
            var value = new DateTime(2014, 11, 10, 23, 15, 59);
            var expected = new DateTime(2014, 11, 10, 23, 15, 0);
            Assert.AreEqual(expected, value.RoundToMinute());
        }

        [TestMethod]
        public void Roundup_ReturnsCurrentQuarter_When1SecondLess()
        {
            var timespan = TimeSpan.FromMinutes(15);
            var value = new DateTime(2014, 11, 10, 22, 59, 59);
            var expected = new DateTime(2014, 11, 10, 23, 0, 0);
            Assert.AreEqual(expected, value.Roundup(timespan));
        }


        [TestMethod]
        public void Roundup_ReturnsNextQuarter_WhenAtEndOfQuarter()
        {
            var timespan = TimeSpan.FromMinutes(15);
            var value = new DateTime(2014, 11, 10, 23, 00, 00);
            var expected = new DateTime(2014, 11, 10, 23, 15, 0);
            Assert.AreEqual(expected, value.Roundup(timespan));
        }

        [TestMethod]
        public void Roundup_ReturnsCurrentQuarter_WhenInTheQuarter()
        {
            var timespan = TimeSpan.FromMinutes(15);
            var value = new DateTime(2014, 11, 10, 23, 00, 59);
            var expected = new DateTime(2014, 11, 10, 23, 15, 0);
            Assert.AreEqual(expected, value.Roundup(timespan));
        }
    }
}
