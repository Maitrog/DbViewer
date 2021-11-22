using DbViewer.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DbViewer.View
{
    /// <summary>
    /// Логика взаимодействия для AllDataPageView.xaml
    /// </summary>
    public partial class AllDataPageView : UserControl
    {
        public AllDataPageView()
        {
            InitializeComponent();
            Loaded += AllDataPageView_Loaded;
        }

        private void AllDataPageView_Loaded(object sender, RoutedEventArgs e)
        {
            tables.ItemsSource = Db.GetTables();
            tables.SelectionChanged += Tables_SelectionChanged;
        }

        private void Tables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataGrid.Columns.Clear();
            List<KeyValuePair<string, Type>> columns = Db.GetColumns(tables.SelectedItem.ToString());
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

            var res = Db.GetValuseFromTable(tables.SelectedValue.ToString());
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
    }
}
