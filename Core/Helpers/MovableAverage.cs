using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Helpers
{
	/// <summary>
	/// Класс для расчета скользящего среднего значения
	/// </summary>
	public class MovableAverage
	{
		private double[] _values;
		private int _index = 0;
		/// <summary>
		/// Среднее значение
		/// </summary>
		public double Average { get; private set; }
		/// <summary>
		/// Количество элементов для скользящего среднего значения
		/// </summary>
		public int Amount { get; private set; }

		public MovableAverage(int amount = 5)
		{
			Amount = amount;
			_values = new double[amount];
		}

		/// <summary>
		/// Добавление нового значения
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Новое среднее значчение</returns>
		public double AddValue(double value)
		{
			var old = _values[_index];
			Average -= old / Amount;
			_values[_index++] = value;
			Average += value / Amount;
			if (_index == Amount)
				_index = 0;
			return Average;
		}
	}
}
