using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
	class TemperatureHelper
	{
		private Action MeasureResistance;
		
		public WSICollection<StepSettings> StepCollection { get; private set; }

		public double CurrentTemperature { get; private set; }

		public TemperatureHelper(WSICollection<StepSettings> steps, Action measureResistance)
		{
			if (steps == null || measureResistance == null)
				throw new NullReferenceException("Параметры steps/measureResistance не должены быть null.");
			if (!steps.Any())
				throw new ArgumentException("Передаваемая коллекция не должна быть пустой.");

			StepCollection = steps;
			StepCollection.SelectedItemChanged += StepCollection_SelectionChanged;
			StepCollection.ChangeSelection(0);
			MeasureResistance = measureResistance;
		}

		/// <summary>
		/// Указывает на нагрев/охлаждение. Необходим для единной логики проверок.
		/// </summary>
		private int _coeff = 1;

		private void StepCollection_SelectionChanged(WSICollection<StepSettings> collection, ChangedEventArgs<StepSettings> args)
		{
			switch (StepCollection.SelectedItem.Type)
			{
				//TODO: необходимо ли проверять на два этих типа?
				case StepType.NotAssigned:
				case StepType.Done:
					return;
				case StepType.Waiting:
				case StepType.Heating:
					_coeff = 1;
					break;
				case StepType.Cooling:
					_coeff = -1;
					break;
			}
		}

		public void SetCurrentTemperature(double temperature)
		{
			CurrentTemperature = temperature;
			var step = StepCollection.SelectedItem;

			var from = StepCollection.SelectedItem.From;
			var to = StepCollection.SelectedItem.To;

			//проверяем, вышли за конечную точку этапа
			if (_coeff * temperature > _coeff * to)
				StepCollection.ChangeSelection();

			//проверяем, достигли ли отправной точки этапа
			if (_coeff * temperature < _coeff * from)
				return;


		}
	}
}
