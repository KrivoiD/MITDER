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
		private int _selectedIndex;

		public StepSettings SelectedStep { get; private set; }

		/// <summary>
		/// Изменяет текущий этап настройки на следующий.
		/// </summary>
		/// <returns></returns>
		internal bool ChangeSelection()
		{
			if (_selectedIndex >= this.Count)
				return false;
			return ChangeSelection(_selectedIndex + 1);
		}
		
		/// <summary>
		/// Изменяет текущий этап настройки на указанный этап.
		/// </summary>
		/// <param name="step"></param>
		/// <returns></returns>
		internal bool ChangeSelection(StepSettings step)
		{
			if(this.Count == 0)
				return false;

			if(step != null && (!this.Contains(step) || this.SelectedStep == step))
				return false;

			return ChangeSelection(this.IndexOf(step));
		}

		/// <summary>
		/// Изменяет текущий этап настройки на этап по его индексу
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal bool ChangeSelection(int index)
		{
			if (index < 0 || index >= this.Count)
				throw new ArgumentOutOfRangeException("index", "Разрешенный диапазон индекса - [0; " + (this.Count - 1) + "].");
			if (index == _selectedIndex)
				return false;

			var oldStep = SelectedStep;
			_selectedIndex = index;
			SelectedStep = this[index];
			if (SelectionChanged != null)
				SelectionChanged.Invoke(this, new StepCollectionEventArgs(oldStep, SelectedStep));
			return true;
		}

		//internal StepSettings GetNextStep()
		//{
		//	var index = this.IndexOf(SelectedStep);
		//	if (this.Count == 0 || this.SelectedStep == null || index == this.Count)
		//		return null;

		//	return this[index + 1];
		//}
	}
}
