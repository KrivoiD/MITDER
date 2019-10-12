using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Core.Interfaces;

namespace Core
{
    public class MeasurementCore : IDisposable
    {
        #region Properties and Variables

        //таймер для обновления данных с устройств
        Timer _timer;
        IVoltageMeasurable _topThermocouple = null;
        IVoltageMeasurable _bottomThermocouple = null;
        IResistanceMeasurable _resistance = null;
        double _from = 0;
        double _pointRange = 0.05;
        double _step = 0.1;

        /// <summary>
        /// Интервал (в миллисекундах) измерения напряжения на верхней и нижней термопарах, контролирующих температуру.
        /// По умолчанию 250 мс. Диапазон устанавливаемых значений от 200 мс до 10 с.
        /// Установка нового значения перезапускает таймер измерений.
        /// </summary>
        public double Interval {
            get { return _timer.Interval; }
            set {
                if(value < 200)
                    value = 200;
                if(value > 10000)
                    value = 10000;
                if (_timer.Enabled)
                    _timer.Stop();
                _timer.Interval = value;
                _timer.Start();
            }
        }

        /// <summary>
        /// Температура верхней термопары в мВ
        /// </summary>
        public double TopTemperature { get; private set; }

        /// <summary>
        /// Температура нижней термопары в мВ
        /// </summary>
        public double BottomTemperature { get; private set; }

        /// <summary>
        /// Значение напряжения в мВ для начала измерения
        /// </summary>
        public double From {
            get { return _from; }
            set {
                _from = value;
                Next = value;
            }
        }

        /// <summary>
        /// Значение напряжения в мВ для окончания измерения
        /// </summary>
        public double To { get; set; }

        /// <summary>
        /// Значение напряжения шага измерения в мВ.
        /// По умолчанию значение равно 0,1 мВ.
        /// НЕ может быть меньше двойного значения <see cref="MeasurementCore.PointRange"/>.
        /// </summary>
        public double Step {
            get { return _step; }
            set {
                if (value <= PointRange)
                    value = 2 * PointRange;
                _step = value;
            }
        }

        /// <summary>
        /// Следующее значение в мВ, при котором измеряется сопротивление
        /// </summary>
        public double Next { get; private set; }

        /// <summary>
        /// Диапазон в мВ для точки измерения <see cref="MeasurementCore.Next"/>, при попадании в который производится измерения.
        /// Минимальное значение - 0,010 мВ, максимальное значение - 0,1 мВ. По умолчанию - 0,015 мВ.
        /// </summary>
        public double PointRange {
            get { return _pointRange; }
            set
            {
                if (value < 0.010)
                    value = 0.010;
                if (value > 0.1)
                    value = 0.1;
                _pointRange = value;
            }
        }

        /// <summary>
        /// Последнее измеренное значение сопротивления
        /// </summary>
        public double Resistance { get; set; }

        /// <summary>
        /// Указывает, производить ли измерения сопротивления в заданном интервале [<see cref="MeasurementCore.From"/>;<see cref="MeasurementCore.To"/>]
        /// с заданным шагом <see cref="MeasurementCore.Step"/>.
        /// </summary>
        public bool IsResistanceMeasured { get; set; }

        public delegate void MeasuredValue(double value);
        /// <summary>
        /// Событие измерения напряжения. Передает последнее измеренное напряжение с нижней термопары
        /// </summary>
        public event MeasuredValue MeasuredVoltage;
        /// <summary>
        /// Событие измерения сопротивления. Передает последнее измеренное сопротивление
        /// </summary>
        public event MeasuredValue MeasuredResistance;

        #endregion

        private MeasurementCore()
        {
            _timer = new Timer(200);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="topThermocouple">Устройство, снимающее показания с верхней термопары</param>
        /// <param name="bottomThermocouple">Устройство, снимающее показания с нижней термопары</param>
        /// <param name="resistance">Устройство, снимающее сопротивление с образца</param>
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
                MeasuredVoltage.Invoke(BottomTemperature);
            }

            if (BottomTemperature > Next + 2 * PointRange)
                while (Next < BottomTemperature)
                {
                    Next += Step;
                }

            if (IsResistanceMeasured)
                MeasureResistanceIfNeed();
        }

        /// <summary>
        /// Проверяет, нужно ли измерить сопротивление при текущих значениях температуры и заданных параметрах
        /// </summary>
        private void MeasureResistanceIfNeed()
        {            
            if(BottomTemperature < Next - PointRange || BottomTemperature > Next + PointRange)
                return;
            
            //определяем диапазон измеряемого значения для омметра
            var integer = (int)Resistance;
            var digitNumber = integer.ToString().Length;
            var range = Math.Pow(10, digitNumber);

            MeasureResistance(range);

            Next += Step;
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
                MeasuredResistance.Invoke(Resistance);
            return Resistance;
        }

        /// <summary>
        /// Освобождает ресурсы, используемые <see cref="MeasurementCore"/>.
        /// </summary>
        public void Dispose()
        {
            if (_timer == null)
                return;
            if (_timer.Enabled)
                _timer.Stop();
            _timer.Elapsed -= _timer_Elapsed;
            _timer.Dispose();            
        }
    }
}
