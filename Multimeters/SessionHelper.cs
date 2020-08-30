using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ivi.Visa;

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
			do
			{
				session.FormattedIO.PrintfAndFlush("SYSTEM:ERROR?");
				error = session.FormattedIO.ReadString();
				errors.Add(error);
			} while (!error.Contains("No error"));
			//удаляем последнию запись, которая указывает, что отсутствуют ошибки
			errors.RemoveAt(errors.Count - 1);
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
	}
}
