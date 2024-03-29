﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

using Core;
using Core.Helpers;

using Interfaces;
using Services;
using UsbRelayNet.RelayLib;

namespace Remf.Core
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
		/// <summary>
		/// Минимальный интервал между измерениями в мс
		/// </summary>
		public const int MIN_INTERVAL = 200;
		/// <summary>
		/// Максимальный интервал между измерениями в мс
		/// </summary>
		public const int MAX_INTERVAL = 10_000;

		//таймер для обновления данных с устройств
		System.Timers.Timer _timer;
		IVoltageMeasurable _topThermocouple = null;
		IVoltageMeasurable _bottomThermocouple = null;
		IVoltageMeasurable _thermoEDF = null;
		IResistanceMeasurable _resistance = null;
		IPowerSupply _gradPower = null;
		IPowerSupply _furnacePower = null;
		Relay _relay = null;
		GradientHelper _gradHelper = null;
		PowerHelper _furnaceHelper = null;

		/// <summary>
		/// Интервал (в миллисекундах) измерения напряжения на верхней и нижней термопарах, контролирующих температуру.
		/// По умолчанию 500 мс. Диапазон устанавливаемых значений от 200 мс до 10 с.
		/// Установка нового значения перезапускает таймер измерений.
		/// </summary>
		public double Interval
		{
			get { return _timer.Interval; }
			set
			{
				if (value < MIN_INTERVAL)
					value = MIN_INTERVAL;
				if (value > MAX_INTERVAL)
					value = MAX_INTERVAL;
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
		/// Температура верхней термопары в мВ
		/// </summary>
		public double TopTemperature { get; private set; }

		/// <summary>
		/// Температура нижней термопары в мВ
		/// </summary>
		public double BottomTemperature { get; private set; }

		/// <summary>
		/// Последнее измеренное значение сопротивления
		/// </summary>
		public double Resistance { get; set; }

		/// <summary>
		/// Последнее измеренное значение сопротивления при протекании обратного тока
		/// </summary>
		public double ReverseResistance { get; set; }

		/// <summary>
		/// Последнее измеренное значение термоЭДС в мкВ
		/// </summary>
		public double ThermoEDF { get; set; }

		/// <summary>
		/// Указывает, производить ли измерения сопротивления в заданном интервале [<see cref="MeasurementCore.From"/>;<see cref="MeasurementCore.To"/>]
		/// с заданным шагом <see cref="MeasurementCore.Step"/>.
		/// </summary>
		public bool IsResistanceMeasured { get; set; }

		private bool _isMeasureThermoEDF;

		/// <summary>
		/// Указывает, производить ли измерения термоЭДС в заданном интервале [<see cref="MeasurementCore.From"/>;<see cref="MeasurementCore.To"/>]
		/// с заданным шагом <see cref="MeasurementCore.Step"/>.
		/// </summary>
		public bool IsMeasureThermoEDF
		{
			get => _isMeasureThermoEDF;
			set => _isMeasureThermoEDF = value && _gradPower != null;
		}

		/// <summary>
		/// Указывает, запущен ли процесс измерения.
		/// </summary>
		public bool IsMeasurementStarted { get; private set; }

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

		private double _gradientCurrent;
		/// <summary>
		/// Сила тока на градиентной спирали.
		/// </summary>
		public double GradientCurrent
		{
			get => _gradientCurrent;
			private set => _gradientCurrent = value < 0 ? 0 : value;
		}

		private double _gradientVoltage = 220;
		/// <summary>
		/// Напряжение тока на градиентной спирали. По умолчанию 220 В.
		/// </summary>
		public double GradientVoltage
		{
			get => _gradientVoltage;
			private set => _gradientVoltage = value < 0 ? 0 : value;
		}

		private double _furnaceCurrent;
		/// <summary>
		/// Сила тока на печи.
		/// </summary>
		public double FurnaceCurrent
		{
			get => _furnaceCurrent;
			private set => _furnaceCurrent = value < 0 ? 0 : value;
		}

		private double _furnaceVoltage = 220;
		/// <summary>
		/// Напряжение тока на печи. По умолчанию 220 В.
		/// </summary>
		public double FurnaceVoltage
		{
			get => _furnaceVoltage;
			private set => _furnaceVoltage = value < 0 ? 0 : value;
		}

		#endregion

		private MeasurementCore()
		{
			MeasurementSteps = new WSICollection<StepSettings>();
#if WithoutDevices
			MeasurementSteps.SelectedItemChanged += MeasurementSteps_SelectedItemChanged;
#endif
			_tempHelper = new TemperatureHelper(MeasurementSteps);
			_timer = new System.Timers.Timer(500);
			_timer.AutoReset = true;
			_timer.Elapsed += _timer_Elapsed;

			if (!double.TryParse(ConfigurationManager.AppSettings["GradientSize"], NumberStyles.Float, CultureInfo.InvariantCulture, out var gradSize))
			{
				WindowService.ShowMessage("В файле app.config для ключа GradientSize ожидалось числовое значение.", "Неверные настройки", true);
				throw new ArgumentException("Неверный формат значения для ключа настройки GradientSize. Указанное значение " + ConfigurationManager.AppSettings["GradientSize"]);
			}
			_gradHelper = new GradientHelper(gradSize);
			_furnaceHelper = new PowerHelper(30, Interval / 1000.0);
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
					_bottomThermocouple.Direction = 1;
					_topThermocouple.Direction = 1;
					break;
				case StepType.Cooling:
					_bottomThermocouple.Direction = -1;
					_topThermocouple.Direction = -1;
					break;
			}
		}
#endif

		/// <summary>
		/// Конструктор класса
		/// </summary>
		/// <param name="topThermocouple">Устройство, снимающее показания с верхней термопары</param>
		/// <param name="bottomThermocouple">Устройство, снимающее показания с нижней термопары</param>
		/// <param name="resistance">Устройство, снимающее сопротивление с образца</param>
		public MeasurementCore(IVoltageMeasurable topThermocouple, IVoltageMeasurable bottomThermocouple, IResistanceMeasurable resistance, IPowerSupply furnacePower, IPowerSupply gradPower = null) : this()
		{
			if (topThermocouple == null || bottomThermocouple == null || resistance == null || furnacePower == null)
				throw new ArgumentNullException();
			if (!topThermocouple.IsInitialized || !bottomThermocouple.IsInitialized
				|| !resistance.IsInitialized || !furnacePower.IsInitialized || (gradPower != null && !gradPower.IsInitialized))
				throw new InvalidOperationException("Должны быть инициализированы все устройства.");
#if !WithoutDevices
			InitializeUsbRelay();
#endif
			_topThermocouple = topThermocouple;
			_bottomThermocouple = bottomThermocouple;
			_resistance = resistance;
			_gradPower = gradPower;
			_furnacePower = furnacePower;
			//Прибор, измеряющий сопротивление, измеряет еще и термоЭДС
			_thermoEDF = _resistance as IVoltageMeasurable;
			//Задаем выходное напряжение на градиентную спираль
			if(_gradPower != null)
				_gradPower.SetVoltage(GradientVoltage);
			_furnacePower.SetVoltage(FurnaceVoltage);
			_timer.Start();
		}

		private void InitializeUsbRelay()
		{
			var relayEnumerator = new RelaysEnumerator();
			var relays = relayEnumerator.CollectInfo().ToList();
			if (relays.Count == 0)
				throw new InvalidOperationException("Ожидалось одно USB-реле. Проверьте подключение USB-реле.");
			if (relays.Count > 1)
				throw new Exception("Ожидалось одно USB-реле, а не " + relays.Count().ToString());
			_relay = new Relay(relays.Single());
			if (_relay.ChannelsCount < 2)
				throw new Exception("Ожидалось USB-реле хотя бы с двумя каналами");
			if (!_relay.IsOpened)
				_relay.Open();
		}

		void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			MeasureTemperatureVoltages();
			if (IsMeasurementStarted)
			{
				AdjustGradientPower();
				AdjustFurnacePower();
			}
		}

		private void AdjustGradientPower()
		{
			var direction = _gradHelper.GetPowerChangingDirection(BottomTemperature, TopTemperature);
			if (IsMeasureThermoEDF && IsMeasurementStarted && direction != 0)
			{
				var value = _gradPower.SetCurrent(GradientCurrent + direction * 0.01);
				if (value.HasValue)
					GradientCurrent = value.Value;
			}
		}

		private void AdjustFurnacePower()
		{
			var direction = _furnaceHelper.AddCurrentTemperature(BottomTemperature);
			if (IsMeasurementStarted && direction != 0)
			{
				var value = _furnacePower.SetCurrent(FurnaceCurrent + direction * 0.01);
				if (value.HasValue)
					FurnaceCurrent = value.Value;
			}
		}

		/// <summary>
		/// Однократно измеряет напряжение на верхней и нижней термопарах.
		/// После измерения генерирует событие <see cref="MeasurementCore.MeasuredVoltage()"/>.
		/// Если устройство, считывающее показания с нижней термопары, не инициализировано, то событие не генерируется.
		/// Значения напряжения в <see cref="MeasurementCore.TopTemperature"/> и <see cref="MeasurementCore.BottomTemperature"/> соответственно
		/// </summary>
		public void MeasureTemperatureVoltages()
		{
			if (_topThermocouple.IsInitialized)
				TopTemperature = _topThermocouple.GetVoltage(0.1) * 1000;
			if (!_bottomThermocouple.IsInitialized)
				return;
			BottomTemperature = _bottomThermocouple.GetVoltage(0.1) * 1000;
			if (MeasuredVoltage != null)
			{
				MeasuredVoltage.Invoke(new MeasuredValues(DateTime.Now) { TopTemperature = this.TopTemperature, BottomTemperature = this.BottomTemperature });
			}

			MeasureResistanceIfNeed();

			Next = _tempHelper.NextTemperature;
		}

		/// <summary>
		/// Проверяет, нужно ли измерить сопротивление и термоЭДС при текущих значениях температуры и заданных параметрах
		/// </summary>
		private void MeasureResistanceIfNeed()
		{
			if (!IsResistanceMeasured || !_tempHelper.IsTakeMeasurement(BottomTemperature))
				return;

			//определяем диапазон измеряемого значения для омметра
			var integer = Math.Ceiling(Resistance) != 0 ? Math.Ceiling(Resistance) : 1_000_000;
			var digitNumber = integer.ToString().Length;
			var range = Math.Pow(10, digitNumber);

			MeasureResistance(range);

			if (IsMeasureThermoEDF)
			{
				//определяем диапазон измеряемого значения для вольтметра
				//integer = (int)ThermoEDF;
				//digitNumber = integer.ToString().Length;
				//range = Math.Pow(10, digitNumber);

				MeasureThermoEDF(0.1);
			}

			if (MeasuredResistance != null)
				MeasuredResistance.Invoke(new MeasuredValues(DateTime.Now)
				{
					TopTemperature = this.TopTemperature,
					BottomTemperature = this.BottomTemperature,
					Resistance = this.Resistance,
					ReverseResistance = this.ReverseResistance,
					ThermoEDF = this.ThermoEDF
				});
		}

		/// <summary>
		/// Возвращает измерянное сопротивление в указанном диапазоне.
		/// Если прибор не инициализирован, то возвращает <see cref="Double.NaN"/>
		/// и не генерируется событие <see cref="MeasurementCore.MeasuredResistance"/>
		/// </summary>
		/// <param name="range">Диапазон для измеряемого значения</param>
		/// <returns>Измерянное сопротивление</returns>
		public double MeasureResistance(double range)
		{
			if (!_resistance.IsInitialized)
				return double.NaN;
			Resistance = _resistance.GetResistance(range);
#if !WithoutDevices
			_relay.WriteChannels(true);
#endif
			ReverseResistance = _resistance.GetResistance(range);
#if !WithoutDevices
			_relay.WriteChannels(false);
#endif

			return Resistance;
		}

		/// <summary>
		/// Возвращает измерянное термоЭДС в указанном диапазоне.
		/// Если прибор не инициализирован, то возвращает <see cref="Double.NaN"/>
		/// </summary>
		/// <param name="range">Диапазон для измеряемого значения</param>
		/// <returns>Измерянное термоЭДС в мВ</returns>
		public double MeasureThermoEDF(double range)
		{
			if (!_thermoEDF.IsInitialized)
				return double.NaN;
			ThermoEDF = _thermoEDF.GetVoltage(range) * 1e6;

			return ThermoEDF;
		}

		/// <summary>
		/// Запускает процесс измерения сопротивления и термоЭДС
		/// </summary>
		/// <returns>Возвращает состояние процесса измерения <see cref="IsMeasurementStarted"/></returns>
		public bool StartMesuarements()
		{
			return TurnMesuarements(true);
		}

		/// <summary>
		/// Останавливает процесс измерения сопротивления и термоЭДС
		/// </summary>
		/// <returns>Возвращает состояние процесса измерения <see cref="IsMeasurementStarted"/></returns>
		public bool StoptMesuarements()
		{
			return TurnMesuarements(false);
		}

		private bool TurnMesuarements(bool isOn)
		{
			if (_gradPower?.IsInitialized ?? false)
			{
				_gradPower.TurnPower(isOn && IsMeasureThermoEDF);
			}

			if (_furnacePower.IsInitialized)
			{
				_furnacePower.TurnPower(isOn);
			}

			return IsMeasurementStarted = isOn;
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
#if !WithoutDevices
			if (_relay.IsOpened)
				_relay.Close();
#endif
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
