using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Core.Helpers;
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
        IVoltageMeasurable _thermoEDF = null;
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
        /// Последнее измеренное значение термоЭДС
        /// </summary>
        public double ThermoEDF { get; set; }

        /// <summary>
        /// Указывает, производить ли измерения сопротивления в заданном интервале [<see cref="MeasurementCore.From"/>;<see cref="MeasurementCore.To"/>]
        /// с заданным шагом <see cref="MeasurementCore.Step"/>.
        /// </summary>
        public bool IsResistanceMeasured { get; set; }
        
        /// <summary>
        /// Указывает, производить ли измерения термоЭДС в заданном интервале [<see cref="MeasurementCore.From"/>;<see cref="MeasurementCore.To"/>]
        /// с заданным шагом <see cref="MeasurementCore.Step"/>.
        /// </summary>
        public bool IsThermoEDFMeasured { get; set; }

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
            MeasurementSteps.SelectedItemChanged += MeasurementSteps_SelectedItemChanged;
            _tempHelper = new TemperatureHelper(MeasurementSteps);
            _timer = new System.Timers.Timer(200);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
        }

        private void MeasurementSteps_SelectedItemChanged(WSICollection<StepSettings> collection, ChangedEventArgs<StepSettings> args)
        {
            if (collection.SelectedItem == null)
                return;
#if WithoutDevices
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
#else
#endif
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
            //Прибор, измеряющий сопротивление, измеряет еще и термоЭДС
            _thermoEDF = _resistance as IVoltageMeasurable;
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
            if (_tempHelper.IsMeasureResistance(BottomTemperature))
                MeasureResistanceIfNeed();

            Next = _tempHelper.NextTemperature;
        }

        /// <summary>
        /// Проверяет, нужно ли измерить сопротивление и термоЭДС при текущих значениях температуры и заданных параметрах
        /// </summary>
        private void MeasureResistanceIfNeed()
        {
            if (!IsResistanceMeasured)
                return;

            //определяем диапазон измеряемого значения для омметра
            var integer = (int)Resistance;
            var digitNumber = integer.ToString().Length;
            var range = Math.Pow(10, digitNumber);

            MeasureResistance(range);

            if(IsThermoEDFMeasured)
			{
                //определяем диапазон измеряемого значения для омметра
                integer = (int)ThermoEDF;
                digitNumber = integer.ToString().Length;
                range = Math.Pow(10, digitNumber);

                MeasureThermoEDF(range);
			}

            if (MeasuredResistance != null)
                MeasuredResistance.Invoke(new MeasuredValues(DateTime.Now)
                {
                    TopTemperature = this.TopTemperature,
                    BottomTemperature = this.BottomTemperature,
                    Resistance = this.Resistance,
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
            
            return Resistance;
        }

        /// <summary>
        /// Возвращает измерянное сопротивление в указанном диапазоне.
        /// Если прибор не инициализирован, то возвращает <see cref="Double.NaN"/>
        /// и не генерируется событие <see cref="MeasurementCore.MeasuredResistance"/>
        /// </summary>
        /// <param name="range">Диапазон для измеряемого значения</param>
        /// <returns>Измерянное сопротивление</returns>
        public double MeasureThermoEDF(double range)
        {
            if (!_thermoEDF.IsInitialized)
                return double.NaN;
            ThermoEDF = _thermoEDF.GetVoltage(range);

            return ThermoEDF;
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
