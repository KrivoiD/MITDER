using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Services
{
	/// <summary>
	/// Логгер, выводящий трассировку в текстовый файл
	/// </summary>
	public static class Logger
	{
		private static string _now = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.ffff");

		/// <summary>
		/// Инициализирует файловый приемник трассировки для коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// </summary>
		/// <param name="filePath">Путь к файлу вывода трассировки</param>
		public static void Initialize(string filePath)
		{
			var listener = new TextWriterTraceListener(filePath);
			Trace.Listeners.Add(listener);
			Trace.AutoFlush = true;
		}

		/// <summary>
		/// Записывает сообщение в приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// со статусом <see cref="InfoType.Info"/>
		/// </summary>
		/// <param name="ex">Исключение для вывода в лог</param>
		public static void Fatal(Exception ex)
		{
			WriteLine(InfoType.Fatal, ex.ToString());
		}

		/// <summary>
		/// Записывает сообщение в приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// со статусом <see cref="InfoType.Info"/>
		/// </summary>
		/// <param name="message">Произвольное сообщение</param>
		/// <param name="ex">Исключение для вывода</param>
		public static void Fatal(string message, Exception ex)
		{
			WriteLine(InfoType.Fatal, message + "\n\t" + ex.ToString());
		}

		/// <summary>
		/// Записывает сообщение в приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// со статусом <see cref="InfoType.Info"/>
		/// </summary>
		/// <param name="message">Cообщение для записи</param>
		public static void Info(string message)
		{
			WriteLine(InfoType.Info, message);
		}

		/// <summary>
		/// Записывает сообщение в приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// со статусом <see cref="InfoType.Warn"/>
		/// </summary>
		/// <param name="message">Cообщение для записи</param>
		public static void Warn(string message)
		{
			WriteLine(InfoType.Warn, message);
		}

		/// <summary>
		/// Записывает сообщение в приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// со статусом <see cref="InfoType.Debug"/>
		/// </summary>
		/// <param name="message">Cообщение для записи</param>
		public static void Debug(string message)
		{
			WriteLine(InfoType.Debug, message);
		}

		/// <summary>
		/// Записывает сообщение в приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// со статусом <see cref="InfoType.Error"/>
		/// </summary>
		/// <param name="message">Cообщение для записи</param>
		public static void Error(string message)
		{
			WriteLine(InfoType.Error, message);
		}

		/// <summary>
		/// Закрывает приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// </summary>
		public static void Close()
		{
			Trace.Flush();
			Trace.Close();
		}

		private static void WriteLine(InfoType type, string message)
		{
			Trace.WriteLine(string.Format("{0} | {1} | {2}", _now, type, message));
			Trace.Flush();
		}

		/// <summary>
		/// Записывает указанное сообщение в приемники трассировки в коллекции <see cref="System.Diagnostics.Trace.Listeners"/>
		/// без статуса <see cref="InfoType"/> и без текущей даты и времени.
		/// </summary>
		/// <param name="customMessage">Произвольное сообщение</param>
		public static void WriteLine(string customMessage = "")
		{
			Trace.WriteLine(customMessage);
			Trace.Flush();
		}

		/// <summary>
		/// Статус сообщений
		/// </summary>
		private enum InfoType
		{
			Debug,
			Info,
			Warn,
			Error,
			Fatal
		}
	}
}
