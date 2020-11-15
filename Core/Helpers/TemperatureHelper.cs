using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Helpers
{
	/// <summary>
	/// Вспомогательный класс, следящий за температурой и указывающий в нужный момент на необходимость измерения сопротивления,
	/// а также меняет на следующий этап измерения при достижении точки окончания действия текущего этапа.
	/// </summary>
	public class TemperatureHelper
	{
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
		public TemperatureHelper(WSICollection<StepSettings> steps)
		{
			if (steps == null)
				throw new ArgumentNullException("Параметр steps не должен быть null.");
			
			StepCollection = steps;
			StepCollection.SelectedItemChanged += StepCollection_SelectionChanged;
		}

		/// <summary>
		/// Указывает на нагрев/охлаждение. Необходим для единной логики проверок.
		/// </summary>
		private int _coeff = 1;
		/// <summary>
		/// Текущий этап измерения
		/// </summary>
		private StepSettings _step = null;
		/// <summary>
		/// Признак окончания измерений, т.е. достигли конца коллекции этапов.
		/// </summary>
		private bool isDone = false;
		
		private void StepCollection_SelectionChanged(WSICollection<StepSettings> collection, ChangedEventArgs<StepSettings> args)
		{
			isDone = false;
			_step = collection.SelectedItem;
			
			//Установка текущего индекса в -1 означает, что достигли конца коллекции 
			// или был вызван метод ResetSelection(), т.е. перезапустили этапы
			if(collection.SelectedIndex == -1)
			{
				isDone = true;
				return;
			}

			switch (collection.SelectedItem.Type)
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

			NextTemperature = _step.From;
			while (_coeff * NextTemperature < _coeff * CurrentTemperature)
			{
				NextTemperature += _coeff * _step.Step;
			}
		}

		/// <summary>
		/// Возвращает признак необходимости измерения сопротивления, проверяет выполнения условий этапа, 
		/// при необходимости расчитывает следующую точку для измерений.
		/// </summary>
		/// <param name="temperature"></param>
		public bool IsMeasureResistance(double temperature)
		{
			var isMeasure = false;
			CurrentTemperature = temperature;

			if (isDone || _step == null)
				return isMeasure;

			var temper = _coeff * temperature;
			//проверяем, достигли ли отправной точки этапа
			if (temper < _coeff * _step.From)
				return isMeasure;

			//проверяем, надо ли проводить измерение сопротивления
			if (temperature >=  (NextTemperature - _step.PointRange)
				&& temperature <=  (NextTemperature + _step.PointRange))
				isMeasure = true;

			//расчитываем следующую точку для измерения
			if (temper > _coeff * (NextTemperature + _step.PointRange) || isMeasure)
				NextTemperature += _coeff * _step.Step;

			//проверяем, вышли за конечную точку этапа - переходим к следующему этапу
			if (temper > _coeff * _step.To)
				StepCollection.ChangeSelection();

			return isMeasure;
		}
	}
}
