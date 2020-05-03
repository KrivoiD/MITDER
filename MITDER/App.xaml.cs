using MITDER.View;
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
            WindowService.AddMapping<StepSettingsViewModel, StepSettingsView>();
            WindowService.ShowMainWindow<MainWindowViewModel>();
            //base.OnStartup(e);
        }
    }
}
