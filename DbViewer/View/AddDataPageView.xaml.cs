using DbViewer.Model;
using System;
using System.Collections.Generic;
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
using Xceed.Wpf.Toolkit;

namespace DbViewer.View
{
    /// <summary>
    /// Логика взаимодействия для AddDataPageView.xaml
    /// </summary>
    public partial class AddDataPageView : UserControl
    {
        public AddDataPageView()
        {
            InitializeComponent();
            Loaded += AddDataPageView_Loaded;
        }

        private void AddDataPageView_Loaded(object sender, RoutedEventArgs e)
        {
            tables.ItemsSource = Db.GetTables();
            tables.SelectionChanged += Tables_SelectionChanged;
        }

        private void Tables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            while (stackPanel.Children.Count > 3)
            {
                stackPanel.Children.RemoveAt(stackPanel.Children.Count - 2);
            }
            List<ForeignKey> foreignKeys = Db.RetrieveForeignKeyInfo(tables.SelectedValue.ToString());
            List<string> fk = new List<string>();
            foreach (ForeignKey foreignKey in foreignKeys)
            {
                foreach (ForeignKeyColumn column in foreignKey.Columns)
                {
                    fk.Add(column.DetailColumnName);
                }
            }

            var columns = Db.GetColumns(tables.SelectedValue.ToString());
            foreach (var column in columns)
            {
                stackPanel.Children.Insert(stackPanel.Children.Count - 1, new TextBlock()
                {
                    Text = $"{column.Key}",
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Style = FindResource("mainText") as Style
                });
                if (fk.Contains(column.Key))
                {
                    string fkTable = FindMasterTableName(foreignKeys, column.Key);
                    string columnName = FindMasterColumnName(foreignKeys, column.Key);
                    List<string> columnData = Db.GetValuesFromColumn(fkTable, columnName);
                    ComboBox comboBox = new ComboBox
                    {
                        Width = 150,
                        Margin = new Thickness(5, 0, 0, 5),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        ItemsSource = columnData,
                        Style = FindResource("mainComboBox") as Style
                    };
                    stackPanel.Children.Insert(stackPanel.Children.Count - 1, comboBox);
                }
                else if (column.Value.Name == "DateTime")
                {
                    if (column.Key.Contains("Время"))
                    {
                        stackPanel.Children.Insert(stackPanel.Children.Count - 1, new TimePicker()
                        {
                            Width = 150,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(5, 0, 0, 5),
                            FontFamily = new FontFamily("Rounded Mplus"),
                            FontSize = 14
                        });
                    }
                    else
                    {
                        stackPanel.Children.Insert(stackPanel.Children.Count - 1, new DatePicker()
                        {
                            Width = 150,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(5, 0, 0, 5),
                            SelectedDateFormat = DatePickerFormat.Long,
                            Style = FindResource("mainDatePicker") as Style
                        });
                    }
                }
                else
                {
                    stackPanel.Children.Insert(stackPanel.Children.Count - 1, new TextBox()
                    {
                        Width = 150,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5, 0, 0, 5),
                        Style = FindResource("mainTextBox") as Style
                    });
                }
            }


        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string tableName = tables.SelectedValue.ToString();
            List<KeyValuePair<string, Type>> columns = Db.GetColumns(tableName);
            List<string> values = new List<string>();

            for (int i = 3; i < stackPanel.Children.Count - 1; i += 2)
            {
                if (stackPanel.Children[i].GetType() == typeof(TextBox))
                {
                    values.Add((stackPanel.Children[i] as TextBox).Text);
                }
                else if (stackPanel.Children[i].GetType() == typeof(ComboBox))
                {
                    values.Add((stackPanel.Children[i] as ComboBox).SelectedValue.ToString());
                }
                else if (stackPanel.Children[i].GetType() == typeof(DatePicker))
                {
                    values.Add((stackPanel.Children[i] as DatePicker).SelectedDate.ToString());
                }
                else if (stackPanel.Children[i].GetType() == typeof(TimePicker))
                {
                    DateTime dt = (DateTime)(stackPanel.Children[i] as TimePicker).Value;
                    values.Add(dt.ToString("h:mm tt"));
                }
            }

            string result = Db.AddValue(tableName, values);
            if (result == "201")
            {
                System.Windows.MessageBox.Show("Значение успешно добавлено");
            }
            else
            {
                System.Windows.MessageBox.Show("Ошибка добавления\n" + result);
            }
        }

        private string FindMasterTableName(List<ForeignKey> foreignKeys, string fkColumnName)
        {
            foreach (ForeignKey fk in foreignKeys)
            {
                foreach (ForeignKeyColumn column in fk.Columns)
                {
                    if (column.DetailColumnName == fkColumnName)
                    {
                        return fk.MasterTableName;
                    }
                }
            }
            return string.Empty;
        }

        private string FindMasterColumnName(List<ForeignKey> foreignKeys, string fkColumnName)
        {
            foreach (ForeignKey fk in foreignKeys)
            {
                foreach (ForeignKeyColumn column in fk.Columns)
                {
                    if (column.DetailColumnName == fkColumnName)
                    {
                        return column.MasterColumnName;
                    }
                }
            }
            return string.Empty;
        }
    }
}
