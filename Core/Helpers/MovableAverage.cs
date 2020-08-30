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
		private int _realAmount = 0;
		private double _total = 0;
		private bool _isFull = false;
		/// <summary>
		/// Скользящее среднее значение
		/// </summary>
		public double Average
		{
			get
			{
				return _realAmount == 0 ? Double.NaN : _total / _realAmount;
			}
		}
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
			_total -= _values[_index];
			_values[_index] = value;
			_total += value;
			if (!_isFull)
				_realAmount++;
			if (++_index >= Amount)
			{
				_isFull = true;
				_index = 0;
			}

			return Average;
		}
	}
}
