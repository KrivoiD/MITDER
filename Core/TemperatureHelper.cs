using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
	class TemperatureHelper
	{
		/// <summary>
		/// Действие, вызываемое для измерения сопротивления
		/// </summary>
		private Action MeasureResistance;
		
		/// <summary>
		/// Коллекция, содержащая этапы измерения
		/// </summary>
		public WSICollection<StepSettings> StepCollection { get; private set; }

		/// <summary>
		/// Текущая температура (последнее установленное значение)
		/// </summary>
		public double CurrentTemperature { get; private set; }

		/// <summary>
		/// Температура, при которой происходит следующее измерение
		/// </summary>
		public double NextTemperature { get; private set; }

		/// <summary>
		/// Коструктор
		/// </summary>
		/// <param name="steps">Коллекция, содержащая этапы измерения</param>
		/// <param name="measureResistance">Действие, вызываемое для измерения сопротивления</param>
		public TemperatureHelper(WSICollection<StepSettings> steps, Action measureResistance)
		{
			if (steps == null || measureResistance == null)
				throw new NullReferenceException("Параметры steps/measureResistance не должны быть null.");
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
		/// <summary>
		/// Текущий этап измерения
		/// </summary>
		private StepSettings _step = null;

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
			_step = collection.SelectedItem;
		}

		public void SetCurrentTemperature(double temperature)
		{
			CurrentTemperature = temperature;
			
			//проверяем, достигли ли отправной точки этапа
			if (_coeff * temperature < _coeff * _step.From)
				return;

			//проверяем, надо ли проводить измерение сопротивления
			if (_coeff * temperature >= _coeff * (NextTemperature - _step.PointRange)
				&& _coeff * temperature <= _coeff * (NextTemperature + _step.PointRange))
				MeasureResistance();

			//проверяем, вышли за конечную точку этапа - переходим к следующему этапу
			if (_coeff * temperature > _coeff * _step.To)
				StepCollection.ChangeSelection();

			//расчитываем следующую точку для измерения
			if (_coeff * temperature > _coeff * (NextTemperature + _step.PointRange))
				NextTemperature += _coeff * _step.Step;
		}
	}
}
