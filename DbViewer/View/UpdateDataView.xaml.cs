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
    /// Логика взаимодействия для UpdateDataView.xaml
    /// </summary>
    public partial class UpdateDataView : UserControl
    {

        private string _tableName;
        private object[] _valuse;

        public UpdateDataView(string tableName, object[] valuse)
        {
            _tableName = tableName;
            _valuse = valuse;
            InitializeComponent();
            Loaded += UpdateDataView_Loaded;
        }

        private void UpdateDataView_Loaded(object sender, RoutedEventArgs e)
        {
            List<ForeignKey> foreignKeys = Db.RetrieveForeignKeyInfo(_tableName);
            List<string> fk = new List<string>();
            foreach (ForeignKey foreignKey in foreignKeys)
            {
                foreach (ForeignKeyColumn column in foreignKey.Columns)
                {
                    fk.Add(column.DetailColumnName);
                }
            }

            var columns = Db.GetColumns(_tableName);
            for (int i = 0; i < columns.Count; i++)
            {
                KeyValuePair<string, Type> column = columns[i];
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
                    comboBox.SelectedValue = comboBox.ItemsSource.Cast<string>().FirstOrDefault(x => x == _valuse[i].ToString());
                }
                else if (column.Value.Name == "DateTime")
                {
                    if (column.Key.Contains("Время"))
                    {
                        TimePicker timePicker = new TimePicker()
                        {
                            Width = 150,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(5, 0, 0, 5),
                            FontFamily = new FontFamily("Rounded Mplus"),
                            FontSize = 14,
                            Text = _valuse[i].ToString()
                    };
                        //timePicker.Text = _valuse[i].ToString(); 
                        stackPanel.Children.Insert(stackPanel.Children.Count - 1, timePicker);
                    }
                    else
                    {
                        DatePicker datePicker = new DatePicker()
                        {
                            Width = 150,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(5, 0, 0, 5),
                            SelectedDateFormat = DatePickerFormat.Long,
                            Style = FindResource("mainDatePicker") as Style,
                            Text = _valuse[i].ToString()
                        };
                        stackPanel.Children.Insert(stackPanel.Children.Count - 1, datePicker);
                    }
                }
                else
                {
                    stackPanel.Children.Insert(stackPanel.Children.Count - 1, new TextBox()
                    {
                        Width = 150,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5, 0, 0, 5),
                        Style = FindResource("mainTextBox") as Style,
                        Text = _valuse[i].ToString()
                    });
                }
            }
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

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<string, Type>> columns = Db.GetColumns(_tableName);
            List<string> columnsName = new List<string>();
            List<string> newValues = new List<string>();
            foreach (var column in columns)
            {
                columnsName.Add(column.Key);
            }

            for (int i = 2; i < stackPanel.Children.Count - 1; i += 2)
            {
                if (stackPanel.Children[i].GetType() == typeof(TextBox))
                {
                    newValues.Add((stackPanel.Children[i] as TextBox).Text);
                }
                else if (stackPanel.Children[i].GetType() == typeof(ComboBox))
                {
                    newValues.Add((stackPanel.Children[i] as ComboBox).SelectedValue.ToString());
                }
                else if (stackPanel.Children[i].GetType() == typeof(DatePicker))
                {
                    newValues.Add((stackPanel.Children[i] as DatePicker).SelectedDate.ToString());
                }
                else if (stackPanel.Children[i].GetType() == typeof(TimePicker))
                {
                    DateTime dt = (DateTime)(stackPanel.Children[i] as TimePicker).Value;
                    newValues.Add(dt.ToString("h:mm tt"));
                }
            }

            string result = Db.UpdateValue(_tableName, newValues, _valuse);
            if (result == "201")
            {
                System.Windows.MessageBox.Show("Значение успешно обновлено");
            }
            else
            {
                System.Windows.MessageBox.Show("Ошибка обновления данных\n" + result);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateDataPageView updateDataPageView = new UpdateDataPageView(_tableName);
            Grid.SetColumn(updateDataPageView, 2);
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).MainGrid.Children.RemoveAt(2);
                    (window as MainWindow).MainGrid.Children.Insert(2, updateDataPageView);
                }
            }
        }
    }
}
