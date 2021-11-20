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
        public SqlRequestPageView()
        {
            InitializeComponent();
            Loaded += SqlRequestPageView_Loaded;
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
            KeyValuePair<string, KeyValuePair<string, string>> request = new KeyValuePair<string, KeyValuePair<string, string>>();
            if (Requests.SelectedValue != null)
            {
                request = (KeyValuePair<string, KeyValuePair<string, string>>)Requests.SelectedValue;
            }

            string type = request.Value.Value;
            if ( !string.IsNullOrEmpty(type) )
            {
                if(type == "VIEW")
                {
                    List<string> columns = Db.GetColumnNameFromView(request.Key);
                    DataTable dt = new DataTable();

                    DataColumn dataColumn;
                    foreach(string column in columns)
                    {
                        dataColumn = new DataColumn();
                        dataColumn.DataType = Type.GetType("System.String");
                        dataColumn.ColumnName = column;
                        dt.Columns.Add(dataColumn);
                    }

                    List<List<string>> values = Db.ExecuteView(request.Key);
                    foreach (List<string> value in values)
                    {
                        DataRow row = dt.NewRow();
                        for (int i = 0; i < columns.Count; i++)
                        {
                            row[columns[i]] = value[i];

                        }
                        dt.Rows.Add(row);
                    }

                    DataView view = new DataView(dt);
                    dataGrid.ItemsSource = view;
                }
                else if(type == "FUNCTION")
                {
                    List<string> valuesName = new List<string>();
                    string requestString = request.Value.Key;
                    requestString = requestString.ToUpper();
                    string condition = requestString.Split(new string[] { "WHERE" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] conditions = condition.Split(new string[] { "AND", "OR" }, 20, StringSplitOptions.RemoveEmptyEntries);
                    for(int i = 0; i < conditions.Length; i++)
                    {
                        conditions[i] = conditions[i].Replace("(", "");
                        conditions[i] = conditions[i].Replace(")", "");
                        conditions[i] = conditions[i].Replace("[", "");
                        conditions[i] = conditions[i].Replace("]", "");
                        conditions[i] = conditions[i].Replace(";", "");
                        conditions[i] = conditions[i].Replace("\r\n", "");
                        string[] temp = conditions[i].Split(new string[] { ">", "<", "=", ">=", "<=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                        temp[1] = temp[1].ToLower();
                        temp[1] = FirstCharToUpper(temp[1]);
                        valuesName.Add(temp[1]);
                    }
                }
            }
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

    }
}
