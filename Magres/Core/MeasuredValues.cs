using Interfaces;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Magres.Core
{
	/// <summary>
	/// Класс, хранящий значения напряжений или сопротивления одного измерения.
	/// </summary>
	public class MeasuredValues : ISavingHeader
	{
		/// <summary>
		/// Беспараметрический конструктор
		/// </summary>
		public MeasuredValues()
		{

		}
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="date">Дата и время измерения значений</param>
		public MeasuredValues(DateTime date)
		{
			Date = date;
		}

		/// <summary>
		/// Значение напряжения в мВ на термопаре.
		/// </summary>
		public double Temperature { get; set; }
		/// <summary>
		/// Значение напряжения в В на образце.
		/// </summary>
		public double Voltage { get; set; }
		/// <summary>
		/// Значение сопротивления в А
		/// </summary>
		public double Currency { get; set; }
		/// <summary>
		/// Значение сопротивления в Ом
		/// </summary>
		public double Resistance { get; set; }
		/// <summary>
		/// Дата и время измерения
		/// </summary>
		public DateTime Date { get; private set; }

		/// <summary>
		/// Возращает строку с названиями колонок.
		/// </summary>
		/// <returns></returns>
		public string GetHeader()
		{
			return string.Format(CultureInfo.InvariantCulture, "Date\tTemperature\tCurrency\tVoltage\tResistance");
		}

		/// <summary>
		/// Возращает строку со значениями данных.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:HH:mm:ss}\t{1:N4}\t{2}\t{3}\t{4}", new object[] { Date, Temperature, Currency, Voltage, Resistance });
		}
	}
}
