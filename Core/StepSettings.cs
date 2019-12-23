﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Interfaces;

namespace Core
{
    /// <summary>
    /// Класс для описания этапа измерения.
    /// </summary>
	public class StepSettings : IMeasurementSettings
	{
		double _from = 0;
		double _pointRange = 0.05;
		double _step = 0.1;

		/// <summary>
		/// Значение напряжения в мВ для начала измерения
		/// </summary>
		public double From
		{
			get { return _from; }
			set
			{
				_from = value;
			}
		}

		/// <summary>
		/// Значение напряжения в мВ для окончания измерения
		/// </summary>
		public double To { get; set; }

		/// <summary>
		/// Значение напряжения шага измерения в мВ.
		/// По умолчанию значение равно 0,1 мВ.
		/// НЕ может быть меньше двойного значения <see cref="MeasurementCore.PointRange"/>.
		/// </summary>
		public double Step
		{
			get { return _step; }
			set
			{
				if (value <= PointRange)
					value = 2 * PointRange;
				_step = value;
			}
		}

		/// <summary>
		/// Диапазон в мВ для точки измерения <see cref="MeasurementCore.Next"/>, при попадании в который производится измерения.
		/// Минимальное значение - 0,010 мВ, максимальное значение - 0,1 мВ. По умолчанию - 0,015 мВ.
		/// </summary>
		public double PointRange
		{
			get { return _pointRange; }
			set
			{
				if (value < 0.010)
					value = 0.010;
				if (value > 0.1)
					value = 0.1;
				_pointRange = value;
			}
		}

		/// <summary>
		/// Тип этапа измерения. 
		/// По окончанию этапа тип меняется на <see cref="StepType.Done"/>.
		/// </summary>
		public StepType Type { get; set; }
	}

	/// <summary>
	/// Определяет тип этапа измерения.
	/// </summary>
	public enum StepType
	{
		/// <summary>
		/// Не проводятся измерения.
		/// </summary>
		NotMeasuring = 0,
		/// <summary>
		/// Ожидание заданной температуры.
		/// </summary>
		Waiting,
		/// <summary>
		/// Производятся измерения с подъемом температуры.
		/// </summary>
		Heating,
		/// <summary>
		/// Производятся измерения с понижением температуры.
		/// </summary>
		Cooling,
		/// <summary>
		/// Данный этап измерения окончен.
		/// </summary>
		Done,
	}
}