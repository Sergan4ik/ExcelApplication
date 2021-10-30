using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelApplication
{
    public partial class ExcelPage : Form
    {
        private DataGrid dataGrid = new DataGrid();
        private DataGridEditor dataGridEditor = new DataGridEditor();
        private bool isSaved = false;
        public ExcelPage()
        {
            InitializeComponent();
            CreateDataGridView(dataGrid.RowCount, dataGrid.ColCount);
        }

        private void CreateDataGridView(int rows, int columns)
        {
            ExcelWorksheet.ColumnCount = columns;
            for (int i = 0; i < columns; i++)
            {
                string colName = NumberConverter.ConvertTo26System(i);
                ExcelWorksheet.Columns[i].Name = colName;
                ExcelWorksheet.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            ExcelWorksheet.RowCount = rows;
            for (int j = 0; j < rows; j++)
            {
                ExcelWorksheet.Rows[j].HeaderCell.Value = (j).ToString();
            }
            dataGrid.SetGrid(rows, columns);

        }

        public bool IsEmpty()
        {
            for (int r = 0; r < ExcelWorksheet.RowCount; r++)
            {
                for (int c = 0; c < ExcelWorksheet.ColumnCount; c++)
                {
                    if (dataGrid.Grid[r][c].Value != "0" && dataGrid.Grid[r][c].Value != ""
                        && dataGrid.Grid[r][c].Value != " ")
                        return false;
                }
            }
            return true;
        }

        private void ExcelWorksheet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int column = ExcelWorksheet.SelectedCells[0].ColumnIndex;
            int row = ExcelWorksheet.SelectedCells[0].RowIndex;

            string expression = dataGrid.Grid[row][column].Expression;
            string value = dataGrid.Grid[row][column].Value;

            calcBox.Text = expression;
            calcBox.Focus();
        }


        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (isSaved == false)
            {
                if (!IsEmpty())
                {
                    var dr = MessageBox.Show("Are you sure you want to open an other file? Your data will be lost, save this firstly.",
                                              "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.No)
                    {
                        return;
                    }
                }
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ExcelApplicattion|*.txt";
            openFileDialog.Title = "Open ExcelApp file";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            StreamReader sReader = new StreamReader(openFileDialog.FileName);
            dataGrid.Clear();
            ExcelWorksheet.Rows.Clear();
            ExcelWorksheet.Columns.Clear();
            int row;
            int column;
            Int32.TryParse(sReader.ReadLine(), out row);
            Int32.TryParse(sReader.ReadLine(), out column);
            CreateDataGridView(row, column);
            dataGridEditor.Open(row, column, sReader, ExcelWorksheet, dataGrid);
            sReader.Close();
        }

        private void CalcBtn_Click(object sender, EventArgs e)
        {
            int column = ExcelWorksheet.SelectedCells[0].ColumnIndex;
            int row = ExcelWorksheet.SelectedCells[0].RowIndex;
            string expression = calcBox.Text;
            if (expression == "")
            {
                return;
            }
            dataGrid.UpdateCell(row, column, expression, ExcelWorksheet);
            ExcelWorksheet[column, row].Value = dataGrid.Grid[row][column].Value;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ExcelApp|*.txt";
            sfd.Title = "Save ExcelApp file";
            sfd.ShowDialog();

            if (sfd.FileName != "")
            {
                FileStream fs = (FileStream)sfd.OpenFile();
                StreamWriter sw = new StreamWriter(fs);
                dataGridEditor.Save(sw, dataGrid);
                isSaved = true;
                sw.Close();
                fs.Close();
            }
        }

        private void AddRowBtn_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = new DataGridViewRow();
            if (ExcelWorksheet.Columns.Count == 0)
            {
                MessageBox.Show("Error! No columns");
                return;
            }
            ExcelWorksheet.Rows.Add(row);
            ExcelWorksheet.Rows[dataGrid.RowCount].HeaderCell.Value = (dataGrid.RowCount).ToString();
            dataGridEditor.AddRow(ExcelWorksheet, dataGrid);
        }

        private void DeleteRowBtn_Click(object sender, EventArgs e)
        {
            if (!dataGridEditor.DeleteRow(ExcelWorksheet, dataGrid))
            {
                return;
            }
            ExcelWorksheet.Rows.RemoveAt(dataGrid.RowCount);
        }

        private void AddColBtn_Click(object sender, EventArgs e)
        {
            if (ExcelWorksheet.Rows.Count == 0)
            {
                MessageBox.Show("Error! No rows");
                return;
            }
            string colName = NumberConverter.ConvertTo26System(dataGrid.ColCount);
            ExcelWorksheet.Columns.Add(colName, colName);
            dataGridEditor.AddColumn(ExcelWorksheet, dataGrid);
        }

        private void DelColBtn_Click(object sender, EventArgs e)
        {
            if (!dataGridEditor.DeleteColumn(ExcelWorksheet, dataGrid))
            {
                return;
            }
            ExcelWorksheet.Columns.RemoveAt(dataGrid.ColCount);
        }

        private void ExcelPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsEmpty() || isSaved)
            {
                e.Cancel = false;
            }
            else
            {
                var dr = MessageBox.Show("Are you sure you want to exit the program without saving data? It will be lost.",
                "Warning", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            
        }

        private void ExcelWorksheet_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == -1) return;

            //dataGrid.UpdateCell(e.RowIndex, e.ColumnIndex, ExcelWorksheet.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), ExcelWorksheet);
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string debug = "";
            foreach (var a in dataGrid.Grid)
                foreach (var b in a)
                    debug += b.Name + " " + b.Value + " " + b.Expression + "\n";
            string helpMsg = "This is the custom version of Excel application." +
                " It suuports basic arithmetic operations: +, -, *, /, MOD, DIV, MMIN(x1,...xN), MMAX(x1,...xN)." +
                " Program also allows you to add/delete columns and rows, and saving/opening your file as well." +
                " Select the cell, enter the expression and click on Calculate button, the result will appear immediately." +
                " Using is quite simple, so I'm sure you will cope with this program. Good Luck!";

            MessageBox.Show(debug, "Help", MessageBoxButtons.OK);
        }

    }
} 
