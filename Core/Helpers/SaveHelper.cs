using Interfaces;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Helpers
{
	/// <summary>
	/// Вспомогательный класс для сохранения в файл данных
	/// </summary>
	public class SaveHelper<T> : IDisposable
	{
		private ObservableCollection<T> _collection;
		private StreamWriter _streamWriter;

		public SaveHelper(ObservableCollection<T> collection, string filePath)
		{
			_streamWriter = new StreamWriter(filePath);
			var objType = typeof(T);
			if(typeof(ISavingHeader).IsAssignableFrom(objType))
			{
				try
				{
					var obj = Activator.CreateInstance(objType, true) as ISavingHeader;
					_streamWriter.WriteLine(obj.GetHeader());
					_streamWriter.Flush();
				}
				catch (Exception e)
				{
					Trace.TraceError(e.Message);
				}
			}
			_collection = collection;
			_collection.CollectionChanged += Collection_CollectionChanged;
		}

		public void Dispose()
		{
			_collection.CollectionChanged -= Collection_CollectionChanged;
			_streamWriter.Flush();
			_streamWriter.Close();
			_streamWriter.Dispose();
		}

		private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						_streamWriter.WriteLine(item.ToString());
						_streamWriter.Flush();
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					break;
				default:
					break;
			}
		}
	}
}
