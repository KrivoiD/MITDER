using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Core
{
	/// <summary>
	/// Коллекция, содержащая элементы класса <see cref="StepSetting"/>.
	/// </summary>
	public class StepCollection : Collection<StepSettings>
	{
		/// <summary>
		/// Класс, содержащий данные события <see cref="StepCollection.SelectionChanged"/>
		/// </summary>
		public class StepCollectionEventArgs : EventArgs
		{
			public readonly StepSettings oldStep;
			public readonly StepSettings newStep;

			public StepCollectionEventArgs(StepSettings oldStep, StepSettings newStep)
			{
				this.oldStep = oldStep;
				this.newStep = newStep;
			}

		}

		public delegate void StepChanged(StepCollection collection, StepCollectionEventArgs args);
		public event StepChanged SelectionChanged;

		public StepSettings SellectedStep { get; private set; }

		/// <summary>
		/// Изменяет текущий этап настройки
		/// </summary>
		/// <param name="step"></param>
		/// <returns></returns>
		public bool ChangeSellection(StepSettings step)
		{
			if(!this.Contains(step))
				return false;
			if (this.SellectedStep == step)
				return false;
			var oldStep = SellectedStep;
			SellectedStep = step;
			if (SelectionChanged != null)
				SelectionChanged.Invoke(this, new StepCollectionEventArgs(oldStep, step));
			return true;
		}
	}
}
