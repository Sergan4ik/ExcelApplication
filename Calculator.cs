using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExcelApplication
{
    public static class Calculator
    {
        public static double Evaluate(string expression , DataGrid targetGrig = null)
        {
            if (targetGrig != null) return 0;

            ExcelApplicationLexer lexer = new ExcelApplicationLexer(new AntlrInputStream(expression));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowExceptionErrorListener());
            var tokens = new CommonTokenStream(lexer);
            var parser = new ExcelApplicationParser(tokens);
            ExcelApplicationParser.CompileUnitContext tree = parser.compileUnit();
            ExcelApplicationVisitor visitor = new ExcelApplicationVisitor(null);
            return visitor.Visit(tree);
        }

        private static string ReplaceCellsWithValues(string _expression , DataGrid targetGrid)
        {
            string res = _expression;
            string cellPattern = @"[A-Z]+[0-9]+";
            Regex regex = new Regex(cellPattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(res);
            Index number;
            foreach (Match match in matches)
            {
                if (targetGrid.Dictionary.ContainsKey(match.Value))
                {
                    number = NumberConverter.ConvertFrom26System(match.Value);
                }
            }
            MatchEvaluator mEvaluator = new MatchEvaluator(targetGrid.RefToValue);
            string newExpr = regex.Replace(res, mEvaluator);


            return res;
        }
    }
}
