using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Refm.ViewModelClasses
{
	public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
	{
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		public void RaisePropertyChanged<T>(string propertyName, ref T field, T newValue)
		{
			if (EqualityComparer<T>.Default.Equals(field, newValue))
				return;
			field = newValue;
			OnPropertyChanged(propertyName);
		}

		public abstract void Dispose();
	}
}
