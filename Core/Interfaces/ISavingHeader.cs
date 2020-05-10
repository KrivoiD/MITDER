using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Interfaces
{
	/// <summary>
	/// Наследники интерфеса указывают, что необходимо при сохранении в файл записать строку с названиями колонок.
	/// </summary>
	public interface ISavingHeader
	{
		/// <summary>
		/// Возращает строку с названиями колонок.
		/// </summary>
		/// <returns></returns>
		string GetHeader();
	}
}
