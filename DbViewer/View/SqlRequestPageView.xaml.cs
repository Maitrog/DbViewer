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
            Procedures.ItemsSource = Db.RetrieveProveduresInfo();
            Db.RetrieveViewsInfo();
        }
    }
}
