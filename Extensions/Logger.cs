using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Extensions
{
	public static class Logger
	{
		private static string _now => DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.ffff");

		public static void Initialize(string filePath)
		{
			var listener = new TextWriterTraceListener(filePath);
			Trace.Listeners.Add(listener);
			Trace.AutoFlush = true;
		}

		public static void Fatal(Exception ex)
		{
			WriteLine(InfoType.Fatal, ex.ToString());
		}

		public static void Fatal(string message, Exception ex)
		{
			WriteLine(InfoType.Fatal, message + "\n\t" + ex.ToString());
		}

		public static void Info(string message)
		{
			WriteLine(InfoType.Info, message);
		}

		public static void Warn(string message)
		{
			WriteLine(InfoType.Warn, message);
		}

		public static void Debug(string message)
		{
			WriteLine(InfoType.Debug, message);
		}

		public static void Error(string message)
		{
			WriteLine(InfoType.Error, message);
		}

		public static void Close()
		{
			Trace.Flush();
			Trace.Close();
		}

		private static void WriteLine(InfoType type, string message)
		{
			Trace.WriteLine($"{_now} | {type} | {message}");
			Trace.Flush();
		}

		public static void WriteLine(string customMessage)
		{
			Trace.WriteLine(customMessage);
			Trace.Flush();
		}

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
