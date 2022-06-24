using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enums
{
	/// <summary>
	/// Перечисление единиц измерения температур для термопар
	/// </summary>
	public enum TemperatureUnit
	{
		/// <summary>
		/// Милливольт
		/// </summary>
		MilliVolt = 0,
		/// <summary>
		/// Градус Кельвин
		/// </summary>
		Kelvin,
		/// <summary>
		/// Градус Цельсий
		/// </summary>
		Celcius
	}
}
