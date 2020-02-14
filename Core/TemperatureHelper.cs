using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
	class TemperatureHelper
	{
		private Action MeasureResistance;

		public StepCollection StepCollection { get; private set; }

		public double CurrentTemperature { get; private set; }

		public TemperatureHelper(StepCollection steps, Action measureResistance)
		{
			if (steps == null || measureResistance == null)
				throw new NullReferenceException("Параметры steps/measureResistance не должены быть null.");
			if (!steps.Any())
				throw new ArgumentException("Передаваемая коллекция не должна быть пустой.");

			StepCollection = steps;
			StepCollection.ChangeSelection(0);
			MeasureResistance = measureResistance;
		}

		public void SetCurrentTemperature(double temperature)
		{
			CurrentTemperature = temperature;
			var step = StepCollection.SelectedStep;

			switch (step.Type)
			{
				case StepType.NotAssigned:
				case StepType.Done:
					return;
				case StepType.Waiting:
					break;
				case StepType.Heating:
					break;
				case StepType.Cooling:
					break;
			}

			if (temperature < step.From)
				return;
			//учитывать направление движения
		}
	}
}
