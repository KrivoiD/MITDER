using MITDER.ViewModelClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MITDER
{
	/// <summary>
	/// Сервесный класс для установления связи между представлением (ViewModel) и отображением (View).
	/// </summary>
	public static class WindowService
	{
		private static Dictionary<Type, Type> TypeMapping;
		private static Dictionary<ViewModelBase, Window> ObjectsMapping;

		static WindowService()
		{
			TypeMapping = new Dictionary<Type, Type>();
			ObjectsMapping = new Dictionary<ViewModelBase, Window>();
		}

		/// <summary>
		/// Добавляет ассоциацию ViewModel с View.
		/// ViewModel может иметь только одну View.
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <typeparam name="TView"></typeparam>
		public static void AddMapping<TViewModel, TView>() where TViewModel : ViewModelBase where TView : Window
		{
			var viewModelType = typeof(TViewModel);
			var viewType = typeof(TView);
			
			if (!TypeMapping.ContainsKey(viewModelType))
				TypeMapping[viewModelType] = viewType;
		}

		/// <summary>
		/// Создает и отображает ассоциированную View с ViewModel по указанному типу ViewModel
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		public static void ShowWindow<TViewModel>() where TViewModel : ViewModelBase
		{
			var viewModelType = typeof(TViewModel);
			if (!TypeMapping.ContainsKey(viewModelType))
				throw new InvalidOperationException("Для класса " + viewModelType.FullName + " не задан класс отображения.");

			var viewModel = Activator.CreateInstance(viewModelType);
			CreateWindow<TViewModel>();
		}

		/// <summary>
		/// Создает и отображает ассоциированную View с ViewModel по заданному объекту ViewModel
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <param name="viewModel"></param>
		public static void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
		{
			if (viewModel == null)
				throw new ArgumentNullException();
			var viewModelType = viewModel.GetType();
			if (!TypeMapping.ContainsKey(viewModelType))
				throw new InvalidOperationException("Для класса " + viewModelType.FullName + " не задан класс отображения.");

			CreateWindow<TViewModel>(viewModel);
		}

		/// <summary>
		/// Скрытый метод для создания окна и установки в DataContext объекта ViewModel.
		/// Если переданный объект ViewModel нулевой, то создается объект соответствующего типа.
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <param name="viewModel"></param>
		private static void CreateWindow<TViewModel>(TViewModel viewModel = null) where TViewModel : ViewModelBase
		{
			var window = Activator.CreateInstance(TypeMapping[typeof(TViewModel)]) as Window;
			viewModel = viewModel ?? Activator.CreateInstance(typeof(TViewModel)) as TViewModel;
			window.DataContext = viewModel ?? Activator.CreateInstance(typeof(TViewModel));
			ObjectsMapping.Add(viewModel, window);
			window.Show();
		}
	}
}
