using System;
using System.IO;
using System.Windows;

using MITDER.View;
using MITDER.ViewModel;
using MITDER.Properties;

using NLog;
using System.Diagnostics;

namespace MITDER
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public Logger Log { get; private set; } = LogManager.GetCurrentClassLogger();

		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			System.Diagnostics.Trace.Listeners.Add(new NLogTraceListener());
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Fatal(e.ExceptionObject as Exception, "Необработанное исключение.");
			Log.Fatal("Приложение аварийно завершается.");
			LogManager.Flush();
			Settings.Default["IsLastCrashed"] = true;
			Settings.Default.Save();
			this.Shutdown(-1);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			Log.Info("Приложение MITDER запустилось.");

			var isLastCrashed = Settings.Default.IsLastCrashed;
			var savedFilePath = Settings.Default.SavedFilePath;
			bool? result = true;

			if (isLastCrashed && !string.IsNullOrWhiteSpace(savedFilePath))
			{
				savedFilePath = AddSuffix(savedFilePath, "_afterCrashed");
			}
			else
			{
				result = WindowService.ShowSaveDialog(out savedFilePath);
			}

			if (result ?? false)
			{
				Settings.Default["SavedFilePath"] = savedFilePath;
				Settings.Default.Save();
			}
			else
			{
				Log.Info("Пользователь не указал файл сохранения. Приложение закрывается.");
				LogManager.Flush();
				this.Shutdown(0);
				return;
			}

			//try
			//{
				WindowService.AddMapping<MainWindowViewModel, MainWindow>();
				WindowService.AddMapping<StepSettingsViewModel, StepSettingsView>();
				WindowService.ShowMainWindow<MainWindowViewModel>();
				//base.OnStartup(e);
			//}
			//catch (Exception ex)
			//{
			//	this.OnUnhandledException(this, new UnhandledExceptionEventArgs(ex, true));
			//}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Settings.Default["IsLastCrashed"] = false;
			Settings.Default["SavedFilePath"] = string.Empty;
			Settings.Default.Save();
			base.OnExit(e);
		}

		private string AddSuffix(string filename, string suffix)
		{
			string fDir = Path.GetDirectoryName(filename);
			string fName = Path.GetFileNameWithoutExtension(filename);
			string fExt = Path.GetExtension(filename);
			return Path.Combine(fDir, String.Concat(fName, suffix, fExt));
		}
	}
}
