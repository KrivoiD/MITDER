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
		private int _interval;
		private MovableAverage _rateList;
		private double _rate;

		private double _lastTemperature;
		private double _temperatureRate;
		private double _rateRangeStability;

		/// <summary>
		/// Необходимая скорость изменения температуры в мВ/с.
		/// По умолчанию установлено 0,002 мВ/с, что примерно 180 град/час
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
		public double RateRangeStability
		{
			get { return _rateRangeStability; }
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Диапазон стабильности не может быть меньше либо равным нулю.");
				_rateRangeStability = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lastValuesAmount">Количество последних показаний, используемых для расчета средней скорости изменения температуры</param>
		/// <param name="interval">Интервал измерений показаний температур, в секундах</param>
		public PowerHelper(int lastValuesAmount, int interval)
		{
			TemperatureRate = 0.002;
			RateRangeStability = 0.0005;
			_interval = interval;
			_rateList = new MovableAverage(lastValuesAmount);
		}

		public void AddCurrentTemperature(double bottom)
		{
			_rate = _rateList.AddValue((bottom - _lastTemperature) / _interval);
			_lastTemperature = bottom;
		}

		/// <summary>
		/// Возвращает коэффициент, указывающий на изменение питания.
		/// </summary>
		/// <returns>
		///       1 - необходимо увеличить питание
		/// <br />0 - оставить без изменений
		/// <br />-1 - необходимо уменьшить питание</returns>
		public int GetPowerChangingDirection()
		{
			if (_rate < TemperatureRate - RateRangeStability)
				return 1;
			if (_rate > TemperatureRate + RateRangeStability)
				return -1;
			return 0;
		}
	}
}
