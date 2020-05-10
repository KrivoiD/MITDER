using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Interfaces
{
	/// <summary>
	/// Интерфейс, имеющий параметры этапа измерения
	/// </summary>
	public interface IMeasurementSettings
	{
		/// <summary>
		/// Значение напряжения в мВ для начала измерения
		/// </summary>
		double From { get; set; }

		/// <summary>
		/// Значение напряжения в мВ для окончания измерения
		/// </summary>
		double To { get; set; }

		/// <summary>
		/// Значение напряжения шага измерения в мВ.
		/// По умолчанию значение равно 0,1 мВ.
		/// НЕ может быть меньше двойного значения <see cref="MeasurementCore.PointRange"/>.
		/// </summary>
		double Step { get; set; }

		/// <summary>
		/// Диапазон в мВ для точки измерения <see cref="MeasurementCore.Next"/>, при попадании в который производится измерения.
		/// Минимальное значение - 0,010 мВ, максимальное значение - 0,1 мВ. По умолчанию - 0,015 мВ.
		/// </summary>
		double PointRange { get; set; }
	}
}
