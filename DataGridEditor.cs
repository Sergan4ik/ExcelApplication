using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelApplication
{
    class DataGridEditor
    {
        public void AddRow(DataGridView dataGrid, DataGrid grid)
        {
            List<Cell> newRow = new List<Cell>();
            for (int j = 0; j < grid.ColCount; j++)
            {

                Cell cell = new Cell(grid.RowCount, j);
                newRow.Add(cell);
                if(!grid.Dictionary.ContainsKey(newRow.Last().Name))
                    grid.Dictionary.Add(newRow.Last().Name, "");
            }
            grid.Grid.Add(newRow);
            grid.UpdateReferences();
            foreach (var list in grid.Grid)
            {
                foreach (var cell in list)
                {
                    if (cell.ThisCellDependsOn != null)
                    {
                        foreach (var cellDepOn in cell.ThisCellDependsOn)
                        {
                            if (cellDepOn.Row == grid.RowCount)
                            {
                                if (!cellDepOn.CellsDependentOnThis.Contains(cell))
                                {
                                    cellDepOn.CellsDependentOnThis.Add(cell);
                                }
                            }
                        }
                    }
                }
            }
            for (int j = 0; j < grid.ColCount; j++)
            {
                grid.UpdateCell(grid.RowCount, j, "", dataGrid);
            }
            grid.RowCount++;
        }

        public bool DeleteRow(DataGridView dataGrid, DataGrid grid)
        {
            if (grid.RowCount == 0)
            {
                return false;
            }
            List<Cell> cellsDependentOnLastRow = new List<Cell>();
            List<string> notEmptyCells = new List<string>();
            int current = grid.RowCount - 1;
            for (int i = 0; i < grid.ColCount; i++)
            {
                var cell = new Cell(current, i);
                string cellName = cell.Name;
                if (grid.Dictionary.ContainsKey(cell.Name))
                {
                    if (grid.Dictionary[cellName] != "" && grid.Dictionary[cellName] != " ")
                    {
                        if(grid.Dictionary[cellName] != "0")
                            notEmptyCells.Add(cellName);
                        if (grid.Grid[current][i].CellsDependentOnThis != null)
                        {
                            cellsDependentOnLastRow.AddRange(grid.Grid[current][i].CellsDependentOnThis);
                        }
                    }
                }
            }
                if (cellsDependentOnLastRow.Count != 0 || notEmptyCells.Count != 0)
                {
                    string errorMsg = "";
                    if (cellsDependentOnLastRow.Count != 0)
                    {
                        errorMsg += "There are cells that point to cells from last row: ";
                        foreach (var curCell in cellsDependentOnLastRow)
                        {
                            errorMsg += string.Join(";", curCell.Name);
                            errorMsg += " ";
                        }
                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK);
                    return false;
                    }
                    if (notEmptyCells.Count != 0)
                    {
                        errorMsg += "There are not empty cells :";
                        errorMsg += string.Join(";", notEmptyCells.ToArray());
                        errorMsg += " ";
                    }
                    errorMsg += " Are you sure to delete this row";
                    DialogResult dr = MessageBox.Show(errorMsg, "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.No)
                    {
                        return false;
                    }
                }
                for (int i = 0; i < grid.ColCount; i++)
                {
                    var cell = new Cell(current, i);
                    grid.Dictionary.Remove(cell.Name);
                }
                grid.Grid.RemoveAt(current);
                grid.RowCount--;
                return true;
        }

        public void AddColumn(DataGridView dataGridView, DataGrid grid)
        {
            List<Cell> newCol = new List<Cell>();
            for (int j = 0; j < grid.RowCount; j++)
            {
                Cell cell = new Cell(j, grid.ColCount);
                grid.Grid[j].Add(cell);
                newCol.Add(cell);
                if (!grid.Dictionary.ContainsKey(newCol.Last().Name))
                    grid.Dictionary.Add(newCol.Last().Name, "");
            }
            grid.UpdateReferences();
            foreach (var list in grid.Grid)
            {
                foreach (var cell in list)
                {
                    if (cell.ThisCellDependsOn != null)
                    {
                        foreach (var cellDepOn in cell.ThisCellDependsOn)
                        {
                            if (cellDepOn.Column == grid.ColCount)
                            {
                                if (!cellDepOn.CellsDependentOnThis.Contains(cell))
                                {
                                    cellDepOn.CellsDependentOnThis.Add(cell);
                                }
                            }
                        }
                    }
                }
            }
            for (int j = 0; j < grid.RowCount; j++)
            {
                grid.UpdateCell(j, grid.ColCount, "", dataGridView);
            }
            grid.ColCount++;
        }

        public bool DeleteColumn(DataGridView dataGrid, DataGrid grid)
        {
            if (grid.ColCount == 0)
            {
                return false;
            }
            List<Cell> cellsOnLastCol = new List<Cell>();
            List<string> notEmptyCells = new List<string>();
            int current = grid.ColCount - 1;
            for (int i = 0; i < grid.RowCount; i++)
            {
                var cell = new Cell(i, current);
                string cellName = cell.Name;
                if (grid.Dictionary.ContainsKey(cell.Name))
                {
                    if (grid.Dictionary[cellName] != "" && grid.Dictionary[cellName] != " ")
                    {
                        if (grid.Dictionary[cellName] != "0")
                            notEmptyCells.Add(cellName);
                        if (grid.Grid[i][current].CellsDependentOnThis != null)
                        {
                            cellsOnLastCol.AddRange(grid.Grid[i][current].CellsDependentOnThis);
                        }
                    }
                }
            }
                if (cellsOnLastCol.Count != 0 || notEmptyCells.Count != 0)
                {
                    string errorMsg = "";
                    if (cellsOnLastCol.Count != 0)
                    {
                        errorMsg += "There are cells that point to cells from last column: ";
                        foreach (var curCell in cellsOnLastCol)
                        {
                            errorMsg += string.Join(";", curCell.Name);
                            errorMsg += " ";
                        }
                        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK);
                        return false;
                    }
                    if (notEmptyCells.Count != 0)
                    {
                        errorMsg += "There are not empty cells :";
                        errorMsg += string.Join(";", notEmptyCells.ToArray());
                        errorMsg += " ";
                    }
                    errorMsg += " Are you sure to delete this column";
                    DialogResult dr = MessageBox.Show(errorMsg, "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.No)
                    {
                        return false;
                    }
                }
            for (int i = 0; i < grid.RowCount; i++)
            {
                var cell = new Cell(i, current);
                grid.Dictionary.Remove(cell.Name);
                grid.Grid[i].RemoveAt(current);
            }
            grid.ColCount--;
            return true;
        }

        public void Open(int rows, int columns, StreamReader sReader, DataGridView dataGridView, DataGrid grid)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    string index = sReader.ReadLine();
                    string expression = sReader.ReadLine();
                    string value = sReader.ReadLine();

                    if (expression != "")
                    {
                        grid.Dictionary[index] = value;
                    }
                    else
                    {
                        grid.Dictionary[index] = expression;
                    }
                    int refCount = Convert.ToInt32(sReader.ReadLine());
                    List<Cell> newReferences = new List<Cell>();

                    string reference;

                    for (int k = 0; k < refCount; k++)
                    {
                        reference = sReader.ReadLine();
                        int row = NumberConverter.ConvertFrom26System(reference).row;
                        int col = NumberConverter.ConvertFrom26System(reference).column;
                        newReferences.Add(grid.Grid[row][col]);
                    }

                    int pointerCount = Convert.ToInt32(sReader.ReadLine());
                    List<Cell> newPointers = new List<Cell>();

                    string pointer;

                    for (int k = 0; k < pointerCount; k++)
                    {
                        pointer = sReader.ReadLine();
                        int row = NumberConverter.ConvertFrom26System(pointer).row;
                        int col = NumberConverter.ConvertFrom26System(pointer).column;
                        newPointers.Add(grid.Grid[row][col]);
                    }

                    grid.Grid[i][j].SetCell(value, expression, newReferences, newPointers);

                    int currentRow = grid.Grid[i][j].Row;
                    int currentCol = grid.Grid[i][j].Column;
                    dataGridView[currentCol, currentRow].Value = grid.Dictionary[index];
                }
            }
        }

        public void Save(StreamWriter streamWriter, DataGrid grid)
        {
            streamWriter.WriteLine(grid.RowCount);
            streamWriter.WriteLine(grid.ColCount);
            foreach (var list in grid.Grid)
            {
                foreach (var cell in list)
                {
                    streamWriter.WriteLine(cell.Name);
                    streamWriter.WriteLine(cell.Expression);
                    streamWriter.WriteLine(cell.Value);

                    if (cell.ThisCellDependsOn == null)
                    {
                        streamWriter.WriteLine("0");
                    }
                    else
                    {
                        streamWriter.WriteLine(cell.ThisCellDependsOn.Count);
                        foreach (var cellDepOn in cell.ThisCellDependsOn)
                        {
                            streamWriter.WriteLine(cellDepOn.Name);
                        }
                    }
                    if (cell.CellsDependentOnThis == null)
                    {
                        streamWriter.WriteLine("0");
                    }
                    else
                    {
                        streamWriter.WriteLine(cell.CellsDependentOnThis.Count);
                        foreach (var depOnCell in cell.CellsDependentOnThis)
                        {
                            streamWriter.WriteLine(depOnCell.Name);
                        }
                    }
                }
            }
        }
    }
}
