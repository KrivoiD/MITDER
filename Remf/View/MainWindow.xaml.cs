using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Remf.ViewModel;
using Remf.ViewModelClasses;

namespace Remf.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            ((MainWindowViewModel)DataContext).Dispose();
        }
    }
}
