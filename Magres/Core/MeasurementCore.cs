using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

using Core;
using Core.Helpers;

using Interfaces;

namespace Magres.Core
{
	/// <summary>
	/// Делегат метода, обрабатывающий события <see cref="MeasurementCore.MeasuredVoltage" /> и <see cref="MeasurementCore.MeasuredResistance" /> 
	/// в классе <see cref="MeasurementCore" />
	/// </summary>
	/// <param name="value"></param>
	public delegate void MeasuredValueHandler(MeasuredValues value);

	public class MeasurementCore : IDisposable
	{
		#region Properties and Variables

		//таймер для обновления данных с устройств
		System.Timers.Timer _timer;
		IVoltageMeasurable _thermocouple = null;
		IVoltageMeasurable _voltage = null;
		//IResistanceMeasurable _resistance = null;
		MovableAverage _temperatureRate = null;

		/// <summary>
		/// Интервал (в миллисекундах) измерения напряжения на верхней и нижней термопарах, контролирующих температуру.
		/// По умолчанию 250 мс. Диапазон устанавливаемых значений от 200 мс до 10 с.
		/// Установка нового значения перезапускает таймер измерений.
		/// </summary>
		public double Interval
		{
			get { return _timer.Interval; }
			set
			{
				if (value < 200)
					value = 200;
				if (value > 10000)
					value = 10000;
				if (_timer.Enabled)
					_timer.Stop();
				_timer.Interval = value;
				_timer.Start();
			}
		}

		/// <summary>
		/// Следующее значение в мВ, при котором измеряется сопротивление
		/// </summary>
		public double Next { get; set; }

		/// <summary>
		/// Температура термопары в мВ
		/// </summary>
		public double Temperature { get; private set; }

		/// <summary>
		/// Последнее измеренное значение сопротивления
		/// </summary>
		public double Resistance { get; set; }

		/// <summary>
		/// Последнее измеренное значение напряжения
		/// </summary>
		public double Voltage { get; set; }

		/// <summary>
		/// Указывает, производить ли измерения сопротивления в заданном интервале [<see cref="MeasurementCore.From"/>;<see cref="MeasurementCore.To"/>]
		/// с заданным шагом <see cref="MeasurementCore.Step"/>.
		/// </summary>
		public bool IsResistanceMeasured { get; set; }

		/// <summary>
		/// Указывает, запущен ли процесс измерения.
		/// </summary>
		public bool IsMeasurementStarted { get; set; }

		/// <summary>
		/// Событие измерения напряжения. Передает последнее измеренное напряжение с нижней термопары
		/// </summary>
		public event MeasuredValueHandler MeasuredVoltage;
		/// <summary>
		/// Событие измерения сопротивления. Передает последнее измеренное сопротивление
		/// </summary>
		public event MeasuredValueHandler MeasuredResistance;

		/// <summary>
		/// Коллекция, содержащая параметры этапов измерения.
		/// </summary>
		public WSICollection<StepSettings> MeasurementSteps { get; private set; }

		/// <summary>
		/// Объект, реализующий логику проверок при изменении текущей температуры.
		/// </summary>
		private TemperatureHelper _tempHelper;

		#endregion

		private MeasurementCore()
		{
			MeasurementSteps = new WSICollection<StepSettings>();
#if WithoutDevices
			MeasurementSteps.SelectedItemChanged += MeasurementSteps_SelectedItemChanged;
#endif
			_tempHelper = new TemperatureHelper(MeasurementSteps);
			_timer = new System.Timers.Timer(200);
			_timer.AutoReset = true;
			_timer.Elapsed += _timer_Elapsed;
			_temperatureRate = new MovableAverage(10);
		}

#if WithoutDevices
		private void MeasurementSteps_SelectedItemChanged(WSICollection<StepSettings> collection, ChangedEventArgs<StepSettings> args)
		{
			if (collection.SelectedItem == null)
				return;
			//изменяет направление симуляции измерений
			switch (collection.SelectedItem.Type)
			{
				case StepType.Heating:
					_thermocouple.Direction = 1;
					break;
				case StepType.Cooling:
					_thermocouple.Direction = -1;
					break;
			}
		}
#endif

		/// <summary>
		/// Конструктор класса
		/// </summary>
		/// <param name="thermocouple">Устройство, снимающее показания с верхней термопары</param>
		/// <param name="voltage">Устройство, снимающее показания с нижней термопары</param>
		/// <param name="resistance">Устройство, снимающее сопротивление с образца</param>
		/// <param name="settings">Параметры измерения</param>
		public MeasurementCore(IVoltageMeasurable thermocouple, IVoltageMeasurable voltage/*, IResistanceMeasurable resistance*/) : this()
		{
			if (thermocouple == null || voltage == null /*|| resistance == null*/)
				throw new ArgumentNullException();
			if (!thermocouple.IsInitialized || !voltage.IsInitialized /*|| !resistance.IsInitialized*/)
				throw new InvalidOperationException("Должны быть инициализированы все устройства.");

			_thermocouple = thermocouple;
			_voltage = voltage;
			//_resistance = resistance;
			
			_timer.Start();
		}

		void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			MeasureTemperatureVoltages();
		}

		/// <summary>
		/// Однократно измеряет напряжение на термопаре.
		/// После измерения генерирует событие <see cref="MeasurementCore.MeasuredVoltage()"/>.
		/// Если устройство, считывающее показания с термопары, не инициализировано, то событие не генерируется.
		/// Значения напряжения в <see cref="MeasurementCore.TopTemperature"/> и <see cref="MeasurementCore.Temperature"/> соответственно
		/// </summary>
		public void MeasureTemperatureVoltages()
		{
			if (!_thermocouple.IsInitialized)
				return;
			var oldTemperature = Temperature;
			Temperature = _thermocouple.GetVoltage(0.1) * 1000;
			_temperatureRate.AddValue(Temperature - oldTemperature);
			if (MeasuredVoltage != null)
			{
				MeasuredVoltage.Invoke(new MeasuredValues(DateTime.Now) { Temperature = this.Temperature });
			}
			if ( _tempHelper.IsTakeMeasurement(Temperature))
				TakeMeasurement();

			Next = _tempHelper.NextTemperature;
		}

		/// <summary>
		/// Проверяет, нужно ли измерить сопротивление и термоЭДС при текущих значениях температуры и заданных параметрах
		/// </summary>
		private void TakeMeasurement()
		{
			if (!IsResistanceMeasured)
				return;

			//определяем диапазон измеряемого значения
			var integer = Math.Ceiling(Voltage) == 0 ? 1 : Math.Ceiling(Voltage);
			var digitNumber = integer.ToString().Length;
			var range = Math.Pow(10, digitNumber);
			MeasureVoltage(range);

			//MeasureResistance(range);

			if (MeasuredResistance != null)
				MeasuredResistance.Invoke(new MeasuredValues(DateTime.Now)
				{
					Temperature = this.Temperature,
					Voltage = this.Voltage,
					//Resistance = this.Resistance
				});
		}

		/// <summary>
		/// Возвращает измерянное сопротивление в указанном диапазоне.
		/// Если прибор не инициализирован, то возвращает <see cref="Double.NaN"/>
		/// и не генерируется событие <see cref="MeasurementCore.MeasuredResistance"/>
		/// </summary>
		/// <param name="range">Диапазон для измеряемого значения</param>
		/// <returns>Измерянное сопротивление</returns>
		//public double MeasureResistance(double range)
		//{
		//	if (!_resistance.IsInitialized)
		//		return double.NaN;
		//	Resistance = _resistance.GetResistance(range);

		//	return Resistance;
		//}

		/// <summary>
		/// Возвращает измерянное напряжение в указанном диапазоне.
		/// Если прибор не инициализирован, то возвращает <see cref="Double.NaN"/>
		/// </summary>
		/// <param name="range">Диапазон для измеряемого значения</param>
		/// <returns>Измерянное термоЭДС в В</returns>
		public double MeasureVoltage(double range)
		{
			if (!_voltage.IsInitialized)
				return double.NaN;
			Voltage = _voltage.GetVoltage(range);

			return Voltage;
		}

		/// <summary>
		/// Синхронизатор освобождения ресурсов 
		/// </summary>
		private static ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
		/// <summary>
		/// Освобождает ресурсы, используемые <see cref="MeasurementCore"/>.
		/// </summary>
		public void Dispose()
		{
			if (_timer == null)
				return;
			_timer.Disposed += _timer_Disposed;
			if (_timer.Enabled)
				_timer.Stop();
			_timer.Elapsed -= _timer_Elapsed;
			_timer.Dispose();
			//ожидает полного освобождения ресурсов _timer;
			_manualResetEvent.WaitOne();
		}

		/// <summary>
		/// Сигнализирует <see cref="MeasurementCore"/> об окончании полном освобождении ресурсов объекта <see cref="System.Timers.Timer"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _timer_Disposed(object sender, EventArgs e)
		{
			_manualResetEvent.Set();
		}
	}
}
