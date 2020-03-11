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
		private int _selectedIndex = -1;

		/// <summary>
		/// Индекс текущего выделенного элемента коллекции.
		/// Если не были ни разу вызван метод <see cref="ChangeSelection()"/> или его перегрузки, то возвращает значение -1.
		/// </summary>
		public int SelectedIndex
		{
			get
			{
				return _selectedIndex;
			}
		}

		/// <summary>
		/// Текущий выделенный элемент коллекции
		/// </summary>
		public T SelectedItem
		{
			get
			{
				return _selectedIndex < 0 ? (T)GetDefault() : this[_selectedIndex];
			}
		}

		/// <summary>
		/// Изменяет текущий элемент на следующий в коллекции.
		/// </summary>
		/// <returns></returns>
		public bool ChangeSelection()
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
		public bool ChangeSelection(T item)
		{
			if (this.Count == 0)
				return false;

			if (item != null && (!this.Contains(item) || this.SelectedItem.Equals(item)))
				return false;

			return ChangeSelection(this.IndexOf(item));
		}

		/// <summary>
		/// Изменяет индекс текущего элемента на указанный индекс.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool ChangeSelection(int index)
		{
			if (index < 0 || index >= this.Count)
				throw new ArgumentOutOfRangeException("index", "Разрешенный диапазон индекса - [0; " + (this.Count - 1) + "].");
			if (index == _selectedIndex)
				return false;

			ChangeSelectedIndex(index);
			return true;
		}

		/// <summary>
		/// Обнуляет текущий выделенный индекс.
		/// </summary>
		public void ResetSelection()
		{
			ChangeSelectedIndex(-1);
		}

		/// <summary>
		/// Изменяет текущий выделенный индекс на указанный.
		/// Для внутренних применений.
		/// </summary>
		/// <param name="index"></param>
		private void ChangeSelectedIndex(int index)
		{
			var oldItem = SelectedItem;
			_selectedIndex = index;
			RaiseselectedItemChanged(oldItem);
		}

		/// <summary>
		/// Генерирует событие <see cref="SelectedItemChanged"/>.
		/// </summary>
		/// <param name="oldItem"></param>
		private void RaiseselectedItemChanged(T oldItem)
		{
			if (SelectedItemChanged != null)
				SelectedItemChanged.Invoke(this, new ChangedEventArgs<T>(oldItem, SelectedItem));
		}

		/// <summary>
		/// Возвращает значение по умолчанию для типа коллекции
		/// </summary>
		/// <returns></returns>
		private static object GetDefault()
		{
			var type = typeof(T);
			if (type.IsValueType)
			{
				return (T)Activator.CreateInstance(type);
			}
			return null;
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			ChangeSelectedIndex(-1);
		}

		protected override void RemoveItem(int index)
		{
			if (_selectedIndex != -1 && _selectedIndex == index)
				throw new InvalidOperationException("Нельзя удалять текущий выделенный элемент. Измените выделенный элемент методами ChangeSelection() или ResetSelection() и выполните операцию повторно.");
			base.RemoveItem(index);
			if (_selectedIndex > index)
				_selectedIndex--;
		}

		protected override void InsertItem(int index, T item)
		{
			base.InsertItem(index, item);
			if (_selectedIndex >= index)
				_selectedIndex++;
		}

		protected override void SetItem(int index, T item)
		{
			if(_selectedIndex == index)
				throw new InvalidOperationException("Нельзя заменить текущий выделенный элемент. Измените выделенный элемент методами ChangeSelection() или ResetSelection() и выполните операцию повторно.");
			
			base.SetItem(index, item);
		}
	}
}
