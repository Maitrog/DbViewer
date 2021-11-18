using DbViewer.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DbViewer.View
{
    /// <summary>
    /// Логика взаимодействия для DeleteDataPageView.xaml
    /// </summary>
    public partial class DeleteDataPageView : UserControl
    {
        public DeleteDataPageView()
        {
            InitializeComponent();
            Loaded += DeleteDataPageView_Loaded;
        }

        private void DeleteDataPageView_Loaded(object sender, RoutedEventArgs e)
        {
            tables.ItemsSource = Db.GetTable();
            tables.SelectionChanged += Tables_SelectionChanged;
        }

        private void Tables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTableData();
        }

        private void UpdateTableData()
        {
            dataGrid.Columns.Clear();
            List<KeyValuePair<string, Type>> columns = Db.GetColumn(tables.SelectedItem.ToString());
            DataTable dt = new DataTable();

            DataColumn dataColumn;
            foreach (var column in columns)
            {
                switch (column.Value.Name)
                {
                    case "Boolean":
                        dataColumn = new DataColumn();
                        dataColumn.DataType = Type.GetType("System.Boolean");
                        dataColumn.ColumnName = column.Key;
                        dt.Columns.Add(dataColumn);
                        break;
                    case "Int32":
                        dataColumn = new DataColumn();
                        dataColumn.DataType = Type.GetType("System.Int32");
                        dataColumn.ColumnName = column.Key;
                        dt.Columns.Add(dataColumn);
                        break;
                    default:
                        dataColumn = new DataColumn();
                        dataColumn.DataType = Type.GetType("System.String");
                        dataColumn.ColumnName = column.Key;
                        dt.Columns.Add(dataColumn);
                        break;
                }
            }

            var res = Db.GetAllFromTable(tables.SelectedValue.ToString());
            foreach (List<string> data in res)
            {
                DataRow row = dt.NewRow();
                for (int i = 0; i < columns.Count; i++)
                {
                    row[columns[i].Key] = data[i];

                }
                dt.Rows.Add(row);
            }
            DataView view = new DataView(dt);
            dataGrid.ItemsSource = view;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            object[] selectedElement = null;
            string table = tables.SelectedValue.ToString();
            if (dataGrid.SelectedValue != null)
            {
                selectedElement = (dataGrid.SelectedValue as DataRowView).Row.ItemArray;
            }

            if(!string.IsNullOrEmpty(table) && selectedElement != null)
            {
                string result = Db.DeleteValue(table, selectedElement);
                if (result == "200")
                {
                    UpdateTableData();
                    MessageBox.Show("Значение успешно удалено");
                }
                else
                {
                    MessageBox.Show("Ошибка удаления\n" + result);
                }    
            }
        }
    }
}
