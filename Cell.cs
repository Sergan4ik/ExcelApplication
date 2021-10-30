using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelApplication
{
    public class Cell : DataGridViewTextBoxCell
    {
        public string Name { get; set; }
        public new string Value { get => _value ; set { _value = value; OnValueChanged?.Invoke(this, _value); } }
        public string Expression { get => _expression; set { _expression = value; OnValueChanged?.Invoke(this, _value);} }

        public int Row { get; set; }
        public int Column { get; set; }

        public List<Cell> CellsDependentOnThis = new List<Cell>();
        public List<Cell> ThisCellDependsOn = new List<Cell>();
        public List<Cell> NewThisCellDepOn = new List<Cell>();

        public event Action<Cell, string> OnValueChanged;

        private string _value;
        private string _expression;
        public Cell(int row, int column)
        {
            this.Row = row;
            this.Column = column;
            Value = "0";
            Expression = "";
            Name = NumberConverter.ConvertTo26System(column) + Convert.ToString(row);
        }

        public void SetCell(string value, string expression, List<Cell> depOnCell, List<Cell> cellDepOn)
        {
            this.Value = value;
            this.Expression = expression;
            this.ThisCellDependsOn.Clear();
            this.ThisCellDependsOn.AddRange(cellDepOn);

            this.CellsDependentOnThis.Clear();
            this.CellsDependentOnThis.AddRange(depOnCell);
        }

        public bool ContainsLoop(List<Cell> cells)
        {
            foreach(var cell in cells)
            {
                if(cell.Name == Name)
                {
                    return true;
                }
            }
            foreach(var depOnCell in CellsDependentOnThis)
            {
                foreach(var cell in cells)
                {
                    if(depOnCell.Name == cell.Name)
                    {
                        return true;
                    }
                }
                if(depOnCell.ContainsLoop(cells))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddCellDepOnAndDepOnCell()
        {
            foreach(Cell cellDepOn in NewThisCellDepOn)
            {
                cellDepOn.CellsDependentOnThis.Add(this);
                OnValueChanged += cellDepOn.UpdateCellValue;
            }
            ThisCellDependsOn = NewThisCellDepOn;
        }

        public void RemoveCellDepOnAndDepOnCell()
        {
            if(ThisCellDependsOn != null)
            {
                foreach(Cell cellDepOn in ThisCellDependsOn)
                {
                    OnValueChanged -= cellDepOn.UpdateCellValue;
                    cellDepOn.CellsDependentOnThis.Remove(this);
                }
                ThisCellDependsOn = null;
            }
            return;
        }


        public void UpdateCellValue(Cell from , string res)
        {
            if (Expression == "") return;

            Value = Calculator.Evaluate(Expression).ToString();
        }
       
    }
}
