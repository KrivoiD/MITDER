using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

using Extensions;

using MITDER.Properties;
using MITDER.View;
using MITDER.ViewModel;

namespace MITDER
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			string logFilePath = GetLogFilePath();
			Logger.Initialize(logFilePath);
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Fatal("Приложение аварийно завершается.", e.ExceptionObject as Exception);
			Settings.Default["IsLastCrashed"] = true;
			Settings.Default.Save();
			this.Shutdown(-1);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			Logger.WriteLine("-------------------------------------------------------------------");
			Logger.Info("Приложение MITDER запустилось.");

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
				Logger.Warn("Пользователь не указал файл сохранения.");
				this.Shutdown(0);
				return;
			}

			WindowService.AddMapping<MainWindowViewModel, MainWindow>();
			WindowService.AddMapping<StepSettingsViewModel, StepSettingsView>();
			WindowService.ShowMainWindow<MainWindowViewModel>();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Settings.Default["IsLastCrashed"] = false;
			Settings.Default["SavedFilePath"] = string.Empty;
			Settings.Default.Save();
			Logger.Info("Приложение завершилось.");
			Logger.Close();
			base.OnExit(e);
		}

		private string AddSuffix(string filename, string suffix)
		{
			string fDir = Path.GetDirectoryName(filename);
			string fName = Path.GetFileNameWithoutExtension(filename);
			string fExt = Path.GetExtension(filename);
			return Path.Combine(fDir, String.Concat(fName, suffix, fExt));
		}

		private static string GetLogFilePath()
		{
			var appDirectory = Directory.GetCurrentDirectory();
			var logPath = Path.Combine(appDirectory, "logs");
			if (!Directory.Exists(logPath))
				Directory.CreateDirectory(logPath);
			var logFilePath = Path.Combine(logPath, DateTime.Now.ToString("yyyy-MM-dd-HH") + ".log");
			return logFilePath;
		}
	}
}
