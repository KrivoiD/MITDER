using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Core
{
	/// <summary>
	/// Класс, хранящий значения напряжений и сопротивления одного измерения.
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
		/// Значение напряжения в мВ на верхней термопаре.
		/// </summary>
		public double TopTemperature { get; set; }
		/// <summary>
		/// Значение напряжения в мВ на нижней термопаре.
		/// </summary>
		public double BottomTemperature { get; set; }
		/// <summary>
		/// Значение сопротивления в Ом
		/// </summary>
		public double Resistance { get; set; }
		/// <summary>
		/// Значение сопротивления в Ом при обратном токе
		/// </summary>
		public double ReverseResistance { get; set; }
		/// <summary>
		/// Значение термоЭДС в мВ
		/// </summary>
		public double ThermoEDF { get; set; }
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
			return string.Format(CultureInfo.InvariantCulture, "BottomTemperature\tTopTemperature\tResistance\tReverseResistance\tThermoEDF(mV)");
		}

		/// <summary>
		/// Возращает строку со значениями данных.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:N4}\t{1:N4}\t{2}\t{3}\t{4}", new object[] { BottomTemperature, TopTemperature, Resistance, ReverseResistance, ThermoEDF });
		}
	}
}
