using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Helpers
{
	/// <summary>
	/// Класс, следящий за градиентом температур (разница между верхней и нижней температур) и указывающий на направление изменения питания.
	/// Для получения коэффициента изменения использовать <see cref="GradientHelper.GetPowerChangingDirection"/>
	/// </summary>
	public class GradientHelper
	{
		/// <summary>
		/// Величина скорости изменения температуры по умолчанию в мВ/с
		/// </summary>
		public const double DEFAULT_TEMPERATURE_RATE = 0.0013;
		/// <summary>
		/// Величина диапазона стабильности скорости по умолчанию в мВ/С
		/// </summary>
		public const double DEFAULT_RATE_STABILITY_RANGE = 0.0002;

		private double _gradientSize;
		private double _stabilityRange;
		private MovableAverage _gradientAverage;
		private int _measurementQty = 0;
		private int _measurementCounter = 0;

		private double _interval;
		private double _lastTemperature;
		private double _temperatureRate;
		private double _rateStabilityRange;
		private MovableAverage _rateList;
		private double _rate;

		/// <summary>
		/// Необходимая скорость изменения температуры в мВ/с.
		/// <br /> По умолчанию установлено 0,005 мВ/с, что примерно 450 град/час
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
		/// Размер градиента в мВ
		/// </summary>
		/// <remarks>
		/// Если устанавливаемое значение является отрицательным, то устанавливается значение по модулю.
		/// </remarks>
		public double GradientSize
		{
			get => _gradientSize;
			set
			{
				if (value < 0)
				{
					value = Math.Abs(value);
					Logger.Warn("Устанавливаемый значение размера градиента отрицательное. Установленно значение размера, взятое по модулю.");
				}
				_gradientSize = value;
			}
		}
		/// <summary>
		/// Диапазон стабильности в мВ
		/// </summary>
		/// <remarks>
		/// Если устанавливаемое значение является отрицательным, то устанавливается значение по модулю.
		/// </remarks>
		public double StabilityRange
		{
			get => _stabilityRange;
			set
			{
				if (value < 0)
				{
					value = Math.Abs(value);
					Logger.Warn("Устанавливаемый значение размера градиента отрицательное. Установленно значение размера, взятое по модулю.");
				}
				_stabilityRange = value;
			}
		}

		/// <summary>
		/// Конструткор. По умолчанию <see cref="StabilityRange"/> устанавливается 10% от <see cref="GradientSize"/>. 
		/// </summary>
		/// <param name="interval">Интервал измерений показаний температур, в секундах</param>
		/// <param name="gradientSize">Размер градиента в мВ</param>
		public GradientHelper(double interval, double gradientSize)
		{
			TemperatureRate = DEFAULT_TEMPERATURE_RATE;
			RateStabilityRange = DEFAULT_RATE_STABILITY_RANGE;
			_interval = interval;

			GradientSize = gradientSize;
			StabilityRange = gradientSize * 0.1;

			//TODO: переделать КОСТЫЛЬ
			//выжидаем 5 секунд, прежде чем даем реальную команду на изменение питания
			//т.е. каждый 5-секундный запрос будет вычислять возвращаемый коэффициент, в остальных случая всегда ноль. 
			_measurementQty = (int)(10 / interval);
			_gradientAverage = new MovableAverage(_measurementQty);

			_rateList = new MovableAverage(_measurementQty);
		}

		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="gradientSize">Величина градиента (разница температур) в мВ</param>
		/// <param name="stabilityRange">Диапазон стабильности в мВ</param>
		public GradientHelper(double interval, double gradientSize, double stabilityRange) : this(interval, gradientSize)
		{
			StabilityRange = stabilityRange;
		}

		/// <summary>
		/// Возвращает коэффициент, указывающий на изменение питания.
		/// </summary>
		/// <param name="bottomTemp">Температура нижней термопары в мВ</param>
		/// <param name="topTemp">Температура верхней термопары в мВ</param>
		/// <returns>
		///  1 - необходимо увеличить питание<br /> 
		///  0 - оставить без изменений<br />
		/// -1 - необходимо уменьшить питание</returns>
		public int GetPowerChangingDirection(double bottomTemp, double topTemp)
		{
			_rate = _rateList.AddValue(Math.Abs(topTemp - _lastTemperature) / _interval);
			_lastTemperature = topTemp;
			var diff = topTemp - bottomTemp;
			diff = _gradientAverage.AddValue(diff);
			if (++_measurementCounter < _measurementQty)
				return 0;
			else
				_measurementCounter = 0;

			/*Таблица результата выбора направления изменения питания
			 * Строки - величина градиент от заданной
			 * Столбцы - скорость изменения температуры от заданной
			 *
			 *		|	<	 =	 >
			 *	----|-------------
			 *	<	|	1	 1	-1
			 *	=	|	1	 0	-1
			 *	>	|	-1	-1	-1
			 */
			var result = 0 * GetGradientChangingDirection(diff) + GetRateChangingDirection();
			return Math.Sign(result);
		}

		private int GetGradientChangingDirection(double gradient)
		{
			if (gradient < GradientSize - StabilityRange)
				return 1;
			if (gradient > GradientSize + StabilityRange)
				return -1;//коэффициент такой, чтобы итоговый результат соотвествовал таблице выше
			return 0;
		}

		private int GetRateChangingDirection()
		{
			if (_rate < TemperatureRate - RateStabilityRange)
				return 1;
			if (_rate > TemperatureRate + RateStabilityRange)
				return -1;//коэффициент такой, чтобы итоговый результат соотвествовал таблице выше
			return 0;
		}
	}
}
