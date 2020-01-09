﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Core.Interfaces;

namespace Core
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
        IVoltageMeasurable _topThermocouple = null;
        IVoltageMeasurable _bottomThermocouple = null;
        IResistanceMeasurable _resistance = null;

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
        public ObservableCollection<StepSettings> MeasurementSteps { get; private set; }

        private StepSettings _currentStep;

        #endregion

        private MeasurementCore()
        {
            MeasurementSteps = new ObservableCollection<StepSettings>();
            MeasurementSteps.CollectionChanged += MeasurementSteps_CollectionChanged;
            _timer = new System.Timers.Timer(200);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
        }

        private void MeasurementSteps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //назначает текущий этап измерения и свойство Next при добавлении первого объекта в коллекцию.
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && MeasurementSteps.Count == 1 && e.NewItems.Count == 1)
            {
                _currentStep = e.NewItems.Cast<StepSettings>().Single();
                Next = _currentStep.From;
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="topThermocouple">Устройство, снимающее показания с верхней термопары</param>
        /// <param name="bottomThermocouple">Устройство, снимающее показания с нижней термопары</param>
        /// <param name="resistance">Устройство, снимающее сопротивление с образца</param>
        /// <param name="settings">Параметры измерения</param>
        public MeasurementCore(IVoltageMeasurable topThermocouple, IVoltageMeasurable bottomThermocouple, IResistanceMeasurable resistance) : this()
        {
            if (topThermocouple == null || bottomThermocouple == null || resistance == null)
                throw new ArgumentNullException();
            if (!topThermocouple.IsInitialized || !bottomThermocouple.IsInitialized || !resistance.IsInitialized)
                throw new InvalidOperationException("Должны быть инициализированы все устройства.");
            _topThermocouple = topThermocouple;
            _bottomThermocouple = bottomThermocouple;
            _resistance = resistance;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MeasureTemperatureVoltages();
        }

        /// <summary>
        /// Однократно измеряет напряжение на верхней и нижней термопарах.
        /// После измерения генерирует событие <see cref="MeasurementCore.MeasuredVoltage()"/>.
        /// Если устройство, считывающее показания с нижней термопары, не инициализировано, то событие не генерируется.
        /// Значения напряжения в <see cref="MeasurementCore.TopTemperature"/> и <see cref="MeasurementCore.BottomTemperature"/> соответственно
        /// </summary>
        public void MeasureTemperatureVoltages()
        {
            if(_topThermocouple.IsInitialized)
                TopTemperature = _topThermocouple.GetVoltage(0.1) * 1000;
            if (!_bottomThermocouple.IsInitialized)
                return;
            BottomTemperature = _bottomThermocouple.GetVoltage(0.1) * 1000;
            if (MeasuredVoltage != null)
            {
                MeasuredVoltage.Invoke(new MeasuredValues(DateTime.Now) { TopTemperature = this.TopTemperature, BottomTemperature = this.BottomTemperature });
            }

            //определяет следующую точку измерения, если были какие-либо сбои
            if (IsMeasurementStarted && BottomTemperature < _currentStep.To && 
				BottomTemperature > Next + 2 * _currentStep.PointRange)
                while (Next < BottomTemperature)
                {
                    Next += _currentStep.Step;
                }

            if (IsResistanceMeasured)
                MeasureResistanceIfNeed();
        }

        /// <summary>
        /// Проверяет, нужно ли измерить сопротивление при текущих значениях температуры и заданных параметрах
        /// </summary>
        private void MeasureResistanceIfNeed()
        {
			if (BottomTemperature < Next - _currentStep.PointRange || BottomTemperature > Next + _currentStep.PointRange)
                return;

            //определяем диапазон измеряемого значения для омметра
            var integer = (int)Resistance;
            var digitNumber = integer.ToString().Length;
            var range = Math.Pow(10, digitNumber);

            MeasureResistance(range);

			if (Next < _currentStep.To)
				Next += _currentStep.Step;
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
            if (MeasuredResistance != null)
                MeasuredResistance.Invoke(new MeasuredValues(DateTime.Now) { 
                                                TopTemperature = this.TopTemperature, 
                                                BottomTemperature = this.BottomTemperature, 
                                                Resistance = this.Resistance });
            return Resistance;
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
