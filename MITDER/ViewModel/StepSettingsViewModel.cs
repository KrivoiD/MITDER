using Core;
using MITDER.ViewModelClasses;

using Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MITDER.ViewModel
{
	public class StepSettingsViewModel : ViewModelBase
	{
		public StepSettings StepSettigs { get; set; }

		public double From {
			get{ return StepSettigs.From; }
			set { StepSettigs.From = value; }
		}
		public double To {
			get { return StepSettigs.To; }
			set { StepSettigs.To = value; }
		}
		public double Step {
			get { return StepSettigs.Step; }
			set { StepSettigs.Step = value; }
		}
		public StepType Type {
			get { return StepSettigs.Type; }
			set { StepSettigs.Type = value; }
		}

		/// <summary>
		/// Комманда сохранения, указывающая на закрытиие окна
		/// </summary>
		public RelayCommand Save {
			get { return new RelayCommand(SaveStepSettings); }
		}

		/// <summary>
		/// Закрывает окно с параметрами
		/// </summary>
		private void SaveStepSettings(object obj)
		{
			WindowService.CloseWindow(this, true);
		}


		public StepSettingsViewModel()
		{
			StepSettigs = new StepSettings();
		}

		public StepSettingsViewModel(StepSettings step)
		{
			StepSettigs = step;
		}

		public override void Dispose()
		{
			
		}
	}
}
