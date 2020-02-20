using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Core
{
	/// <summary>
	/// Класс, содержащий данные о старом и новом значениях для событий
	/// </summary>
	public class ChangedEventArgs<T> : EventArgs
	{
		public readonly T oldValue;
		public readonly T newValue;

		public ChangedEventArgs(T oldValue, T newValue)
		{
			this.oldValue = oldValue;
			this.newValue = newValue;
		}
	}

	/// <summary>
	/// Коллекция с указанием на текущий элемент
	/// (WSI - with selected item)
	/// </summary>
	public class WSICollection<T> : Collection<T>
	{
		public delegate void SelectionChangedDelegate(WSICollection<T> collection, ChangedEventArgs<T> args);
		public event SelectionChangedDelegate SelectedItemChanged;
		private int _selectedIndex;

		public T SelectedItem { get; private set; }

		/// <summary>
		/// Изменяет текущий элемент на следующий в коллекции.
		/// </summary>
		/// <returns></returns>
		internal bool ChangeSelection()
		{
			if (_selectedIndex >= this.Count)
				return false;
			return ChangeSelection(_selectedIndex + 1);
		}
		
		/// <summary>
		/// Изменяет текущий элемент на указанный.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		internal bool ChangeSelection(T item)
		{
			if(this.Count == 0)
				return false;

			if(item != null && (!this.Contains(item) || this.SelectedItem.Equals(item)))
				return false;

			return ChangeSelection(this.IndexOf(item));
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

			var oldItem = SelectedItem;
			_selectedIndex = index;
			SelectedItem = this[index];
			if (SelectedItemChanged != null)
				SelectedItemChanged.Invoke(this, new ChangedEventArgs<T>(oldItem, SelectedItem));
			return true;
		}
	}
}
