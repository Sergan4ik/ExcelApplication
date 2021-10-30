using System;
using ExcelApplication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestCalculator
{
    [TestClass]
    public class CalculatorTest
    {
        [TestMethod]
        public void TestDivision()
        {
            string expression = "12/0";
            Assert.ThrowsException<DivideByZeroException>(() => Calculator.Evaluate(expression));
        }

        [TestMethod]
        public void TestParentheses()
        {
            string expression = "((12+1)";
            Assert.ThrowsException<Exception>(() => Calculator.Evaluate(expression));
        }

        [TestMethod]
        public void TestUnaryOperator()
        {
            string expression = "-(1234 + 1)";
            double expected = -1235;
            Assert.AreEqual(Calculator.Evaluate(expression), expected);
        }

        [TestMethod]
        public void TestDec()
        {
            string expression = "dec 0";
            double expected = -1;
            Assert.AreEqual(Calculator.Evaluate(expression), expected);
        }
    }
}
