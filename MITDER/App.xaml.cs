using MITDER.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace MITDER
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            WindowService.AddMapping<MainWindowViewModel, MainWindow>();
            WindowService.ShowWindow<MainWindowViewModel>();
            //base.OnStartup(e);
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }
    }
}
