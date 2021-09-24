using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPowerSupply
	{
		/// <summary>
		/// Указывает, инициализированно ли устройство
		/// </summary>
		bool IsInitialized { get; }
		/// <summary>
		/// Устанавливает заданное значение тока
		/// </summary>
		/// <param name="value">Устанавливаемое значение тока в Амперах</param>
		/// <returns>Возвращает установленную силу тока</returns>
		double? SetCurrent(double value);
		/// <summary>
		/// Устанавливает заданное значение напряжения
		/// </summary>
		/// <param name="value">Устанавливаемое значение напряжения в Вольтах</param>
		/// <returns>Возвращает установленное напряжение тока</returns>
		double? SetVoltage(double value);
		/// <summary>
		/// Устанавливает заданные значения тока и напряжения
		/// </summary>
		/// <param name="currency">Устанавливаемое значение тока в Амерах</param>
		/// <param name="voltage">Устанавливаемое значение напряжения в Вольтах</param>
		void SetPower(double currency, double voltage);
		/// <summary>
		/// Включает/выключает выходное питание источника питания
		/// </summary>
		/// <param name="isOn">Если true, то включает питание на подключенную нагрузку</param>
		/// <returns>Успешность включения/выключения питания</returns>
		bool TurnPower(bool isOn);
	}
}
