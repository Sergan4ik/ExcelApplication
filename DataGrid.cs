using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelApplication
{
    public class DataGrid
    {
        private const int DefaultColCount = 15;
        private const int DefaultRowCount = 15;
        public int ColCount;
        public int RowCount;

        public Dictionary<string, string> Dictionary = new Dictionary<string, string>();
        public List<List<Cell>> Grid = new List<List<Cell>>();

        public DataGrid()
        {
            SetGrid(DefaultRowCount, DefaultColCount);
        }

        public void SetGrid(int rows, int columns)
        {
            Clear();

            ColCount = columns;
            RowCount = rows;

            for (int i = 0; i < RowCount; i++)
            {
                List<Cell> newRow = new List<Cell>();
                for (int j = 0; j < ColCount; j++)
                {
                    newRow.Add(new Cell(i, j));
                    Dictionary.Add(newRow.Last().Name, "");
                }
                Grid.Add(newRow);
            }
        }

        public void Clear()
        {
            foreach(List<Cell> list in Grid)
            {
                list.Clear();
            }
            Grid.Clear();
            Dictionary.Clear();

            RowCount = 0;
            ColCount = 0;
        }

        public string Calculate(string expression)
        {
            string res = null;
            try
            {
                res = Convert.ToString(Calculator.Evaluate(expression));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return res;
        }

        public string RefToValue(Match match)
        {
            if(Dictionary.ContainsKey(match.Value))
            {
                if(Dictionary[match.Value] == "")
                {
                    return "0";
                }
                else
                {
                    return Dictionary[match.Value];
                }
            }
            return match.Value;
        }

        public string ConvertReferences(int row, int column, string expression)
        {
            string cellPattern = @"[A-Z]+[0-9]+";
            Regex regex = new Regex(cellPattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(expression);
            Index number;
            foreach(Match match in matches)
            {
                if(Dictionary.ContainsKey(match.Value))
                {
                    number = NumberConverter.ConvertFrom26System(match.Value);
                    Grid[row][column].NewThisCellDepOn.Add(Grid[number.row][number.column]);
                }
            }
            MatchEvaluator mEvaluator = new MatchEvaluator(RefToValue);
            string newExpr = regex.Replace(expression, mEvaluator);
            return newExpr;
        }

        public void UpdateReferences()
        {
            foreach(var list in Grid)
            {
                foreach(var cell in list)
                {
                    if(cell.ThisCellDependsOn != null)
                    {
                        cell.ThisCellDependsOn.Clear();
                    }
                    if (cell.NewThisCellDepOn != null)
                    {
                        cell.NewThisCellDepOn.Clear();
                    }
                    if(cell.Expression == "")
                    {
                        continue;
                    }
                    string newExpr = cell.Expression;
                    if(newExpr[0] == '=')
                    {
                        newExpr = ConvertReferences(cell.Row, cell.Column, cell.Expression);
                        cell.ThisCellDependsOn.AddRange(cell.NewThisCellDepOn);
                    }
                }
            }
        }

        public bool UpdateCellAndPointers(Cell cell, DataGridView dataGridView)
        {
            cell.NewThisCellDepOn.Clear();
            string newExpr = ConvertReferences(cell.Row, cell.Column, cell.Expression);
            if (newExpr != "")
            {
                if (newExpr[0] == '=')
                    newExpr = newExpr.Substring(1);
            }
            string value;
            try
            {
                value = Calculate(newExpr);
                Grid[cell.Row][cell.Column].Value = value;
                Dictionary[cell.Name] = value;
                dataGridView[cell.Column, cell.Row].Value = value;
                foreach (var depOnCell in cell.CellsDependentOnThis)
                {
                    if (!UpdateCellAndPointers(depOnCell, dataGridView))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch(Exception)
            {
                //WriteErrorMessage(cell, "Error expression in cell" + cell.Name, dataGridView);
                return false;
            }
        }

        public void WriteErrorMessage(Cell cell, string errorMessage, DataGridView dataGridView)
        {
            MessageBox.Show(errorMessage, "Error");
            //Grid[cell.Row][cell.Column].Expression = "";
            //Grid[cell.Row][cell.Column].Value = "0";
            //dataGridView[cell.Column, cell.Row].Value = "0";
            return;
        }

        public void UpdateCellValueOnly(Cell cell , string _value, DataGridView dataGridView)
        {
            dataGridView[cell.Row, cell.Column].Value = _value;
        }

        public void UpdateCell(int row, int col, string expression, DataGridView dataGridView)
        {
            //UpdateCellAndPointers(Grid[row][col], dataGridView);
            //return;
            Grid[row][col].RemoveCellDepOnAndDepOnCell();
            Grid[row][col].Expression = expression;
            Grid[row][col].NewThisCellDepOn.Clear();
            Cell cell = new Cell(row, col);
            if (expression != "")
                if (expression[0] == '=')
                {
                    expression = expression.Substring(1);
                }
            string convertedExpr = ConvertReferences(row, col, expression);
            if (Grid[row][col].ContainsLoop(Grid[row][col].NewThisCellDepOn))
            {
                WriteErrorMessage(cell, "Error! There is a loop", dataGridView);
                return;
            }
            try
            {
                Grid[row][col].AddCellDepOnAndDepOnCell();
                string value;
                value = Calculate(convertedExpr);
                Grid[row][col].Value = value;
                Dictionary[cell.Name] = value;

                foreach (var depOnCell in Grid[row][col].CellsDependentOnThis)
                {
                    UpdateCellAndPointers(depOnCell, dataGridView);
                }
            }
            catch (Exception)
            {
                //WriteErrorMessage(cell, "Error expression in cell" + cell.Name, dataGridView);
            }
        }

        

        

        

        
    }
}
