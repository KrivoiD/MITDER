using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Helpers
{
	public class PowerHelper
	{
		private int _interval;
		private MovableAverage _bottomRateList;
		private MovableAverage _topRateList;
		private double _bottomRate;
		private double _topRate;

		private double _lastBottomTemperature;
		private double _lastTopTemperature;
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
		/// <br /> При попадании текущей скорости изменения температуры в диапазон (<see cref="TemperatureRate"/> +/- указанное значение)Ы не приводит к изменению питания источником.
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

		public PowerHelper(int lastValuesAmount, int interval)
		{
			TemperatureRate = 0.002;
			RateRangeStability = 0.0005;
			_interval = interval;
			_bottomRateList = new MovableAverage(lastValuesAmount);
			_topRateList = new MovableAverage(lastValuesAmount);
		}

		public void AddCurrentTemperature(double bottom)
		{
			_bottomRate = _bottomRateList.AddValue((bottom - _lastBottomTemperature) / _interval);
			_lastBottomTemperature = bottom;
		}

		public void AddCurrentTemperature(double bottom, double top)
		{
			_bottomRate = _bottomRateList.AddValue((bottom - _lastBottomTemperature) / _interval);
			_lastBottomTemperature = bottom;
			_topRate = _topRateList.AddValue((top - _lastTopTemperature) / _interval);
			_lastTopTemperature = top;
		}

		public int GetPowerChangingDirection(bool forTopRate)
		{
			var rate = forTopRate ? _topRate : _bottomRate;

			if (rate < TemperatureRate - RateRangeStability)
				return 1;
			if (rate > TemperatureRate + RateRangeStability)
				return -1;
			return 0;
		}
	}
}
