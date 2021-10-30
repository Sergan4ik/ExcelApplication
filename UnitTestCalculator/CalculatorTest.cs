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
        public void TestMminInsideMmin()
        {
            string expression = "MMIN(1, -3, MMIN(3, -10, 5))";
            double expected = -10;
            Assert.AreEqual(Calculator.Evaluate(expression), expected);
        }
    }
}
