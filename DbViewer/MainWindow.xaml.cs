using DbViewer.View;
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

namespace DbViewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> columns;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AllDataPageView allDataPage = new AllDataPageView();
            Grid.SetColumn(allDataPage, 2);
            MainGrid.Children.Insert(2, allDataPage);
        }

        private void AllData_Click(object sender, RoutedEventArgs e)
        {
            if (MainGrid.Children[2].GetType() != Type.GetType("DbViewer.View.AllDataPageView"))
            {
                AllDataPageView allDataPage = new AllDataPageView();
                Grid.SetColumn(allDataPage, 2);
                MainGrid.Children.RemoveAt(2);
                MainGrid.Children.Insert(2, allDataPage);
                ChangeColor(0);
            }
        }

        private void AddData_Click(object sender, RoutedEventArgs e)
        {
            if (MainGrid.Children[2].GetType() != Type.GetType("DbViewer.View.AddDataPageView"))
            {
                AddDataPageView addDataPage = new AddDataPageView();
                Grid.SetColumn(addDataPage, 2);
                MainGrid.Children.RemoveAt(2);
                MainGrid.Children.Insert(2, addDataPage);
                ChangeColor(1);
            }
        }

        private void DeleteData_Click(object sender, RoutedEventArgs e)
        {
            if (MainGrid.Children[2].GetType() != Type.GetType("DbViewer.View.DeleteDataPageView"))
            {
                DeleteDataPageView deleteDataPage = new DeleteDataPageView();
                Grid.SetColumn(deleteDataPage, 2);
                MainGrid.Children.RemoveAt(2);
                MainGrid.Children.Insert(2, deleteDataPage);
                ChangeColor(2);
            }
        }

        private void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            if (MainGrid.Children[2].GetType() != Type.GetType("DbViewer.View.UpdateDataPageView"))
            {
                UpdateDataPageView updateDataPageView = new UpdateDataPageView();
                Grid.SetColumn(updateDataPageView, 2);
                MainGrid.Children.RemoveAt(2);
                MainGrid.Children.Insert(2, updateDataPageView);
                ChangeColor(3);
            }
        }

        private void SqlRequest_Click(object sender, RoutedEventArgs e)
        {
            if (MainGrid.Children[2].GetType() != Type.GetType("DbViewer.View.SqlRequestPageView"))
            {
                SqlRequestPageView sqlRequestPageView = new SqlRequestPageView();
                Grid.SetColumn(sqlRequestPageView, 2);
                MainGrid.Children.RemoveAt(2);
                MainGrid.Children.Insert(2, sqlRequestPageView);
                ChangeColor(4);
            }
        }

        private void ChangeColor(int index)
        {
            for (int i = 0; i < (MainGrid.Children[0] as Grid).Children.Count; i++)
            {
                if (i != index)
                {
                    ((MainGrid.Children[0] as Grid).Children[i] as Button).Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                else
                {
                    ((MainGrid.Children[0] as Grid).Children[i] as Button).Background = new SolidColorBrush(Color.FromRgb(197, 197, 197));
                }
            }
        }
    }
}
