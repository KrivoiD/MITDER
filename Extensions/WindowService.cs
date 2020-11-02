using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Services
{
	/// <summary>
	/// Сервесный класс для установления связи между представлением (ViewModel) и отображением (View).
	/// </summary>
	public static class WindowService
	{
		private static Dictionary<Type, Type> TypeMapping;
		private static Dictionary<object, Window> ObjectsMapping;

		static WindowService()
		{
			TypeMapping = new Dictionary<Type, Type>();
			ObjectsMapping = new Dictionary<object, Window>();
		}

		/// <summary>
		/// Добавляет ассоциацию ViewModel с View.
		/// ViewModel может иметь только одну View.
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <typeparam name="TView"></typeparam>
		public static void AddMapping<TViewModel, TView>() where TViewModel : new() where TView : Window
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
		public static void ShowWindow<TViewModel>() where TViewModel : new()
		{
			var viewModelType = typeof(TViewModel);
			if (!TypeMapping.ContainsKey(viewModelType))
				throw new InvalidOperationException("Для класса " + viewModelType.FullName + " не задан класс отображения.");

			CreateWindow(typeof(TViewModel)).Show();
		}
		
		/// <summary>
		/// Создает и отображает ассоциированную View с ViewModel по указанному типу ViewModel.
		/// Дополнительно устанавливает в <see cref="Application.MainWindow"/> созданную View.
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		public static void ShowMainWindow<TViewModel>() where TViewModel : new()
		{
			var viewModelType = typeof(TViewModel);
			if (!TypeMapping.ContainsKey(viewModelType))
				throw new InvalidOperationException("Для класса " + viewModelType.FullName + " не задан класс отображения.");

			var window = CreateWindow(typeof(TViewModel));
			Application.Current.MainWindow = window;
			window.Show();
		}

		/// <summary>
		/// Создает и отображает ассоциированную View с ViewModel по заданному объекту ViewModel
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <param name="viewModel"></param>
		public static void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : new()
		{
			if (viewModel == null)
				throw new ArgumentNullException();
			var viewModelType = typeof(TViewModel);
			//получаем тип объекта на случай, если в переменной родительского типа хранится объект типа наследника
			var trueModelType = viewModel.GetType();
			if (!TypeMapping.ContainsKey(trueModelType) || !TypeMapping.ContainsKey(typeof(TViewModel)))
				throw new InvalidOperationException("Для класса " + trueModelType.FullName + " не задан класс отображения.");

			CreateWindow(typeof(TViewModel), viewModel).Show();
		}

		/// <summary>
		/// Создает и отображает ассоциированную View с ViewModel по указанному типу ViewModel виде диалога
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		public static bool? ShowDialog<TViewModel>() where TViewModel : new()
		{
			var viewModelType = typeof(TViewModel);
			if (!TypeMapping.ContainsKey(viewModelType))
				throw new InvalidOperationException("Для класса " + viewModelType.FullName + " не задан класс отображения.");

			return CreateWindow(typeof(TViewModel)).ShowDialog();
		}

		/// <summary>
		/// Создает и отображает ассоциированную View с ViewModel по заданному объекту ViewModel виде диалога
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <param name="viewModel"></param>
		public static bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : new()
		{
			if (viewModel == null)
				throw new ArgumentNullException();
			var viewModelType = typeof(TViewModel);
			//получаем тип объекта на случай, если в переменной родительского типа хранится объект типа наследника
			var trueModelType = viewModel.GetType();
			if (!TypeMapping.ContainsKey(trueModelType) || !TypeMapping.ContainsKey(typeof(TViewModel)))
				throw new InvalidOperationException("Для класса " + trueModelType.FullName + " не задан класс отображения.");

			return CreateWindow(typeof(TViewModel), viewModel).ShowDialog();
		}

		/// <summary>
		/// Скрытый метод для создания окна и установки в DataContext объекта ViewModel.
		/// Если переданный объект ViewModel нулевой, то создается объект соответствующего типа.
		/// </summary>
		/// <param name="viewModelType"></param>
		/// <param name="viewModel"></param>
		private static Window CreateWindow(Type viewModelType, object viewModel = null)
		{
			var window = Activator.CreateInstance(TypeMapping[viewModelType]) as Window;
			viewModel = viewModel ?? Activator.CreateInstance(viewModelType);
			window.DataContext = viewModel;
			ObjectsMapping.Add(viewModel, window);
			return window;
		}

		/// <summary>
		/// Закрывает View, ассоциированную с ViewModel. Если не находит по ViewModel, то ничего не происходит.
		/// </summary>
		/// <param name="viewModel"></param>
		/// <param name="isAccept"></param>
		public static void CloseWindow(object viewModel, bool isAccept = false)
		{
			if (viewModel == null)
				throw new ArgumentNullException();
			if (!ObjectsMapping.ContainsKey(viewModel))
				return;

			var window = ObjectsMapping[viewModel];
			window.DialogResult = isAccept;
			ObjectsMapping.Remove(viewModel);
			window.Close();
		}

		public static bool? ShowSaveDialog(out string filename)
		{
			var dialog = new SaveFileDialog() {
				CreatePrompt = false,
				Filter = "Все файлы|*.*|Файл данных|*.dat|Тестовый файл|*txt",
				FilterIndex = 1,
				OverwritePrompt = true,
				Title = "Укажите каталог и название файла для сохранения данных"
			};
			var result = dialog.ShowDialog();
			filename = dialog.FileName;
			return result;
		}

		/// <summary>
		/// Выводит окно с сообщением для пользователя.
		/// </summary>
		/// <param name="message">Текст сообщения</param>
		/// <param name="caption">Строка заглавия</param>
		/// <param name="isError">Указывает, является ли сообщение сообщением об ошибке. Если true, то отображается знак ошибки, иначе знак информации.</param>
		/// <returns>Если true, то пользователь нажал кнопку OK</returns>
		public static bool ShowMessage(string message, string caption, bool isError = false)
		{
			var result = MessageBox.Show(message, caption, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information, MessageBoxResult.OK);
			return result == MessageBoxResult.OK;
		}

		/// <summary>
		/// Выводит окно с вопросом с кнопками Да/Нет или ОК/Отмена
		/// </summary>
		/// <param name="question">Текст вопроса</param>
		/// <param name="caption">Строка заглавия</param>
		/// <param name="isYesNoButtons">Определяет кнопки. Если true, то отображаются Да/Нет, иначе ОК/Отмена</param>
		/// <returns>Если true, то пользователь нажал кнопку ОК/Да. Если false, то пользователь нажал Отмена/Нет. Если же null, то ни одна кнопка не нажата.</returns>
		public static bool? ShowQuestion(string question, string caption, bool isYesNoButtons = false)
		{
			var result = MessageBox.Show(question, caption, isYesNoButtons ? MessageBoxButton.YesNo : MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.None);
			return result == MessageBoxResult.None ? (bool?)null
													: isYesNoButtons ? result == MessageBoxResult.Yes
																	: result == MessageBoxResult.OK;
		}
	}
}
