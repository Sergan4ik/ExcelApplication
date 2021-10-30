using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace ExcelApplication
{
    class ExcelApplicationVisitor : ExcelApplicationBaseVisitor<double>
    {
        Dictionary<string, double> tableIdentifier = new Dictionary<string, double>();
        public ExcelApplicationVisitor(Dictionary<string , double> table)
        {
            tableIdentifier = table;
        }
        public override double VisitCompileUnit(ExcelApplicationParser.CompileUnitContext context)
        {
            string expression = context.GetText();
            var indexOf = expression.IndexOf('<');
            if (indexOf > 0)
            {
                expression = expression.Substring(0, indexOf);
            }
            var lParen = expression.Count(x => x == '(');
            var rParen = expression.Count(x => x == ')');
            if (lParen != rParen)
            {
                throw new Exception("Invalid expression");
            }
            return Visit(context.expression());
        }
        public override double VisitNumberExpr(ExcelApplicationParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);
            return result;
        }
        //IdentifierExpr
        public override double VisitIdentifierExpr(ExcelApplicationParser.IdentifierExprContext context)
        {
            var result = context.GetText();
            Debug.WriteLine(result);
            
            double value;
            //видобути значення змінної з таблиці
            
            if (tableIdentifier.TryGetValue(result.ToString(), out value))
            {
                return value;
            }
            else
            {
                return 0.0;
            }
        }
        public override double VisitParenthesizedExpr(ExcelApplicationParser.ParenthesizedExprContext context)
        {
            string expression = context.GetText();
            if (expression[0] == '-')
            {
                return -Visit(context.expression());
            }
            return Visit(context.expression());
        }
        public override double VisitExponentialExpr(ExcelApplicationParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            Debug.WriteLine("{0} ^ {1}", left, right);
            return System.Math.Pow(left, right);
        }
        public override double VisitAdditiveExpr(ExcelApplicationParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == ExcelApplicationLexer.ADD)
            {
                Debug.WriteLine("{0} + {1}", left, right);
                return left + right;
            }
            else //ExcelApplicationLexer.SUBTRACT
            {
                Debug.WriteLine("{0} - {1}", left, right);
                return left - right;
            }
        }
        public override double VisitMultiplicativeExpr(ExcelApplicationParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == ExcelApplicationLexer.MULTIPLY)
            {
                Debug.WriteLine("{0} * {1}", left, right);
                return left * right;
            }
            else //ExcelApplicationLexer.DIVIDE
            {
                if (right != 0)
                {
                    Debug.WriteLine("{0} / {1}", left, right);
                    return left / right;
                }
                else
                {
                    throw new DivideByZeroException();
                }
            }
        }

        private List<string> DivideIntoParts(string exp)
        {
            exp = exp.Substring(exp.IndexOf('(') + 1, exp.LastIndexOf(')') - exp.IndexOf('(') - 1);
            string str = exp.Substring(0, exp.Length);
            var indexesOfDelimiter = new List<int>();

            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == ',')
                {
                    indexesOfDelimiter.Add(i);
                }
            }

            var parts = new List<string>();

            for (int i = 0; i <= indexesOfDelimiter.Count; i++)
            {
                int indexOfDelimiter = indexesOfDelimiter[i == indexesOfDelimiter.Count ? 0 : i];
                string part;
                if (i == indexesOfDelimiter.Count)
                {
                    part = exp.Substring(0, exp.Length);
                }
                else
                {
                    part = exp.Substring(0, indexOfDelimiter);
                }
                if (part.Count(x => x == '(') == part.Count(x => x == ')'))
                {
                    parts.Add(part);

                    if (i != indexesOfDelimiter.Count)
                    {
                        int lastLength = exp.Length;
                        exp = exp.Substring(indexOfDelimiter + 1, exp.Length - 1 - indexOfDelimiter);
                        for (int j = 0; j < indexesOfDelimiter.Count; j++)
                        {
                            indexesOfDelimiter[j] -= (lastLength - exp.Length);
                        }
                    }
                }
            }
            return parts;
        }

        public override double VisitMminMmaxExpr(ExcelApplicationParser.MminMmaxExprContext context)
        {
            var expression = context.GetText();
            List<string> parts = DivideIntoParts(expression);
            List<double> numbers = new List<double>();
            foreach(var part in parts)
            {
                double number = Calculator.Evaluate(part);
                numbers.Add(number);
            }
            if (context.operatorToken.Type == ExcelApplicationLexer.MMIN)
            {
                Debug.WriteLine(expression);
                return numbers.Min();
            }
            else //ExcelApplicationLexer.MMAX
            {
                Debug.WriteLine(expression);
                return numbers.Max();
            }
        }
        public override double VisitModDivExpr([NotNull] ExcelApplicationParser.ModDivExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == ExcelApplicationLexer.MOD)
            {
                Debug.WriteLine("{0} MOD {1}", left, right);
                return left % right;
            }
            else //ExcelApplicationLexer.DIV
            {
                Debug.WriteLine("{0} DIV {1}", left, right);
                return (int)left / (int)right;
            }
        }

        public override double VisitIncDecExpr([NotNull] ExcelApplicationParser.IncDecExprContext context)
        {
            double right = WalkLeft(context);
            if (context.operatorToken.Type == ExcelApplicationLexer.INC)
            {
                Debug.WriteLine("INC {0}", right);
                return right + 1;
            }
            else
            {
                Debug.WriteLine("DEC {0}", right);
                return right - 1;
            }
        }

        private double WalkLeft(ExcelApplicationParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<ExcelApplicationParser.ExpressionContext>(0));
        }
        private double WalkRight(ExcelApplicationParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<ExcelApplicationParser.ExpressionContext>(1));
        }

    }

}
