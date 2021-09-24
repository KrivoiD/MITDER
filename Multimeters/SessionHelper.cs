using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ivi.Visa;
using Services;

namespace Multimeters
{
	static class SessionHelper
	{
		/// <summary>
		/// Возвращает список ошибок, полученных с прибора. В результате очищается буфер ошибок на приборе.
		/// </summary>
		/// <param name="formattedIO">Установленная сессия с прибором</param>
		/// <returns>Список ошибок</returns>
		public static List<string> GetErrors(IMessageBasedSession session)
		{
			if (session == null || session.FormattedIO == null)
				throw new ArgumentException();
			var errors = new List<string>(4);
			string error;
			try
			{
				session.FormattedIO.PrintfAndFlush("SYSTEM:ERROR?");
				error = session.FormattedIO.ReadUntilEnd();
				errors.Add(error);
				//do
				//{
				//} while (!(error.Contains("No error") || !error.Contains("Noerror")));
			}
			catch (Exception ex)
			{
				errors.Insert(0, "//n====>" + ex.ToString());
			}
			finally
			{
				session.FormattedIO.WriteLine("*CLS");
			}
			//удаляем последнию запись, которая указывает, что отсутствуют ошибки
			//if(errors.Count > 0)
			//	errors.RemoveAt(errors.Count - 1);
			return errors;
		}

		/// <summary>
		/// Возвращает список ошибок (<see cref="GetErrors(IMessageBasedSession)"/>) в виде одной строки.
		/// </summary>
		/// <param name="formattedIO">Установленная сессия с прибором</param>
		/// <returns>Единая строка из ошибок</returns>
		public static string GetErrorsResult(IMessageBasedSession session)
		{
			var errorResult = string.Empty;
			foreach (var error in GetErrors(session))
				errorResult += error;
			return errorResult;
		}

		/// <summary>
		/// Возвращает модель устройства или VISA-адрес
		/// </summary>
		/// <param name="session">Установленная сессия прибора</param>
		/// <returns></returns>
		private static string GetDeviceName(IMessageBasedSession session)
		{
			if (session is IUsbSession usbSession)
				return usbSession.ModelName;
			return session.ResourceName;
		}
	}
}
