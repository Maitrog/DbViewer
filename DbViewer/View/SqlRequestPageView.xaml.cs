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
    /// Логика взаимодействия для SqlRequestPageView.xaml
    /// </summary>
    public partial class SqlRequestPageView : UserControl
    {
        private KeyValuePair<string, KeyValuePair<string, string>> _request;
        private List<string> _valuesName;
        public SqlRequestPageView()
        {
            InitializeComponent();
            Loaded += SqlRequestPageView_Loaded;
            _request = new KeyValuePair<string, KeyValuePair<string, string>>();
            _valuesName = null;
        }

        private void SqlRequestPageView_Loaded(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<string, KeyValuePair<string, string>>> requests = Db.RetrieveProveduresInfo();
            requests.AddRange(Db.RetrieveViewsInfo());
            Requests.ItemsSource = requests;
            Requests.DisplayMemberPath = "Key";
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            string type = _request.Value.Value;
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "VIEW")
                {
                    List<string> columns = Db.GetColumnNameFromView(_request.Key);
                    DataTable dt = CreateDataTable(columns);

                    List<List<string>> result = Db.ExecuteView(_request.Key);
                    FillTable(columns, dt, result);

                    DataView view = new DataView(dt);
                    dataGrid.ItemsSource = view;
                }
                else if (type == "FUNCTION")
                {
                    List<string> values = new List<string>();
                    for (int i = 3; i < stackPanel.Children.Count; i+=2)
                    {
                        values.Add((stackPanel.Children[i] as TextBox).Text);
                    }

                    List<string> columns = Db.GetColumnNameFromFunction(_request.Key, _valuesName, values);
                    DataTable dt = CreateDataTable(columns);

                    List<List<string>> result = Db.ExecuteFunction(_request.Key, _valuesName, values);
                    FillTable(columns, dt, result);

                    DataView view = new DataView(dt);
                    dataGrid.ItemsSource = view;
                }
                else if (type == "PROCEDURE")
                {
                    List<string> values = new List<string>();
                    for (int i = 3; i < stackPanel.Children.Count; i+=2)
                    {
                        values.Add((stackPanel.Children[i] as TextBox).Text);
                    }

                    string result = Db.ExecuteProcedure(_request.Key, values);
                    List<string> columns = new List<string> { "Результат" };
                    DataTable dt = CreateDataTable(columns);
                    FillTable(columns, dt, new List<List<string>> { new List<string> { result } });

                    DataView view = new DataView(dt);
                    dataGrid.ItemsSource = view;
                }
            }
        }

        private void Requests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            while (stackPanel.Children.Count > 2)
            {
                stackPanel.Children.RemoveAt(stackPanel.Children.Count - 1);
            }
            if (Requests.SelectedValue != null)
            {
                _request = (KeyValuePair<string, KeyValuePair<string, string>>)Requests.SelectedValue;
            }

            string type = _request.Value.Value;
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "FUNCTION")
                {
                    List<string> valuesName = new List<string>();
                    string requestString = _request.Value.Key;
                    requestString = requestString.ToUpper();
                    string condition = requestString.Split(new string[] { "WHERE" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                    ParseWhere(valuesName, condition);

                    AddFields(valuesName.ToArray());
                }
                else if (type == "PROCEDURE")
                {
                    string requestString = _request.Value.Key;
                    requestString = requestString.ToUpper();
                    if (requestString.Contains("INSERT INTO"))
                    {
                        string valuesString = requestString.Split(new string[] { "VALUES" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                        valuesString = ReplaceSymbols(valuesString);
                        string[] values = valuesString.Split(new string[] { ", " }, 20, StringSplitOptions.RemoveEmptyEntries);
                        AddFields(values);
                    }
                    else if (requestString.Contains("DELETE"))
                    {
                        List<string> valuesName = new List<string>();
                        string condition = requestString.Split(new string[] { "WHERE" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                        ParseWhere(valuesName, condition);

                        AddFields(valuesName.ToArray());
                    }
                    else if (requestString.Contains("UPDATE"))
                    {
                        List<string> valuesName = new List<string>();
                        string[] res = requestString.Split(new string[] { "SET", "WHERE" }, 3, StringSplitOptions.None);
                        string valuesString = res[1];
                        string conditionsString = null;
                        if (res.Length == 3)
                        {
                            conditionsString = res[2];
                        }
                        string[] values = valuesString.Split(new string[] { "," }, 10, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < values.Length; i++)
                        {
                            string value = values[i].Split(new string[] { "=" }, 10, StringSplitOptions.RemoveEmptyEntries)[1];
                            value = Normolize(value);
                            valuesName.Add(value);
                        }
                        if (!string.IsNullOrEmpty(conditionsString))
                        {
                            ParseWhere(valuesName, conditionsString);
                        }

                        AddFields(valuesName.ToArray());
                    }
                }
            }
        }
        private static void FillTable(List<string> columns, DataTable dt, List<List<string>> result)
        {
            foreach (List<string> data in result)
            {
                DataRow row = dt.NewRow();
                for (int i = 0; i < columns.Count; i++)
                {
                    row[columns[i]] = data[i];

                }
                dt.Rows.Add(row);
            }
        }

        private static DataTable CreateDataTable(List<string> columns)
        {
            DataTable dt = new DataTable();

            DataColumn dataColumn;
            foreach (string column in columns)
            {
                dataColumn = new DataColumn();
                dataColumn.DataType = Type.GetType("System.String");
                dataColumn.ColumnName = column;
                dt.Columns.Add(dataColumn);
            }

            return dt;
        }

        private string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input[0].ToString().ToUpper() + input.Substring(1);
            }
        }


        private string Normolize(string value)
        {
            value = ReplaceSymbols(value);
            value = value.ToLower();
            value = FirstCharToUpper(value);
            return value;
        }

        private void ParseWhere(List<string> valuesName, string conditionsString)
        {
            string[] conditions = conditionsString.Split(new string[] { "AND", "OR" }, 20, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < conditions.Length; i++)
            {
                string temp = conditions[i].Split(new string[] { ">", "<", "=", ">=", "<=" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                temp = Normolize(temp);
                valuesName.Add(temp);
            }
        }

        private void AddFields(string[] values)
        {
            _valuesName = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                if (int.TryParse(values[i], out int res))
                {
                    continue;
                }
                else if (values[i].Contains('\''))
                {
                    continue;
                }
                else
                {
                    values[i] = values[i].ToLower();
                    values[i] = FirstCharToUpper(values[i]);
                    _valuesName.Add(values[i]);
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = values[i],
                        Margin = new Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Style = FindResource("mainText") as Style
                    });
                    stackPanel.Children.Add(new TextBox
                    {
                        Width = 150,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5, 0, 5, 5),
                        Style = FindResource("mainTextBox") as Style
                    });
                }
            }
        }

        private static string ReplaceSymbols(string str)
        {
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace("]", "");
            str = str.Replace("[", "");
            str = str.Replace(";", "");
            str = str.Replace("\r\n", "");

            if (str[0] == ' ')
            {
                str = str.Substring(1);
            }
            return str;
        }
    }
}
