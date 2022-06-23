using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Helpers
{
	/// <summary>
	/// Класс, следящий за скоростью изменения температуры и указывающий на направление изменения питания.
	/// Для добавления нового значение использовать <see cref="PowerHelper.AddCurrentTemperature(double)"/>.
	/// Для получения коэффициента изменения использовать <see cref="PowerHelper.GetPowerChangingDirection"/>
	/// </summary>
	public class PowerHelper
	{
		/// <summary>
		/// Величина скорости изменения температуры по умолчанию в мВ/с (примерно 360 град/час).
		/// </summary>
		public const double DEFAULT_TEMPERATURE_RATE = 0.004;
		/// <summary>
		/// Величина диапазона стабильности скорости по умолчанию в мВ/С
		/// </summary>
		public const double DEFAULT_RATE_STABILITY_RANGE = 0.0005;

		private double _interval;
		private MovableAverage _rateList;
		private double _rate;

		private double _lastTemperature;
		private double _temperatureRate;
		private double _rateStabilityRange;

		private int _measurementQty = 0;
		private int _measurementCounter = 0;

		/// <summary>
		/// Необходимая скорость изменения температуры в мВ/с.
		/// <br /> По умолчанию установлено значение <see cref="DEFAULT_TEMPERATURE_RATE"/>.
		/// </summary>
		public double TemperatureRate
		{
			get { return _temperatureRate; }
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Скорость изменения температуры не может быть меньше либо равным нулю.");
				_temperatureRate = value;
			}
		}
		/// <summary>
		/// Диапазон стабильности в мВ/с для <see cref="TemperatureRate"/>. 
		/// <br /> При попадании текущей скорости изменения температуры в диапазон (<see cref="TemperatureRate"/> +/- указанное значение) не приводит к изменению питания источником.
		/// <br /> По умолчанию установлена 0,0005 мВ/с.
		/// </summary>
		public double RateStabilityRange
		{
			get { return _rateStabilityRange; }
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Диапазон стабильности не может быть меньше либо равным нулю.");
				_rateStabilityRange = value;
			}
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="lastValuesAmount">Количество последних показаний, используемых для расчета средней скорости изменения температуры</param>
		/// <param name="interval">Интервал измерений показаний температур, в секундах</param>
		public PowerHelper(double interval)
		{
			TemperatureRate = DEFAULT_TEMPERATURE_RATE;
			RateStabilityRange = DEFAULT_RATE_STABILITY_RANGE;
			_interval = interval;

			//TODO: переделать КОСТЫЛЬ
			//выжидаем 5 секунд, прежде чем даем реальную команду на изменение питания
			//т.е. каждый 4-секундный запрос будет вычислять возвращаемый коэффициент, в остальных случая всегда ноль. 
			_measurementQty = (int)(4 / interval);

			_rateList = new MovableAverage(_measurementQty);
		}

		/// <summary>
		/// Добавляет новое значение температуры для вычисления скорости изменения
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Возвращает коэффициент, указывающий на изменение питания. 
		/// Смотри <seealso cref="GetPowerChangingDirection()"/></returns>
		public int AddCurrentTemperature(double value)
		{
			_rate = _rateList.AddValue(Math.Abs(value - _lastTemperature) / _interval);
			_lastTemperature = value;
			return GetPowerChangingDirection();
		}

		/// <summary>
		/// Возвращает коэффициент, указывающий на изменение питания.
		/// </summary>
		/// <returns>
		///  1 - необходимо увеличить питание<br /> 
		///  0 - оставить без изменений<br />
		/// -1 - необходимо уменьшить питание</returns>
		public int GetPowerChangingDirection()
		{
			if (++_measurementCounter < _measurementQty)
			{
				return 0;
			}
			else
			{
				_measurementCounter = 0;
			}
			if (_rate < TemperatureRate - RateStabilityRange)
				return 1;
			if (_rate > TemperatureRate + RateStabilityRange)
				return -1;
			return 0;
		}
	}
}
