using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if WithoutDevice
using Ivi.Visa.Interop;
using Agilent.Agilent34410.Interop;
#endif
using Core.Interfaces;

namespace Multimeters
{
    public class Agilent34410 : IVoltageMeasurable, IResistanceMeasurable
    {
#if WithoutDevice
        private IAgilent34410 _driver;
#endif
        private string _resourceName;
        private string _name;

        /// <summary>
        /// Указывает, инициализировано ли устройство.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
#if WithoutDevices
                return true;
#else
                return _driver.Initialized;
#endif
            }
        }

        /// <summary>
        /// Строка, содержащая VISA-адрес устройства.
        /// </summary>
        public string ResourceName {
            get { return _resourceName; }
        }

        /// <summary>
        /// Удобное название устройства. Назначается пользователем.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="resource">Строка, содержащая VISA-адрес устройства</param>
        public Agilent34410(string resource)
        {
            _resourceName = resource;
#if !WithoutDevices
            InitializeDevice();
#endif
        }

#if !WithoutDevices
        /// <summary>
        /// Инициализирует устройство перед работой
        /// </summary>
        private void InitializeDevice()
        {
            _driver = new Agilent34410Class();
            try
            {
                // Setup IVI-defined initialization options
                string standardInitOptions =
                    "Cache=true, InterchangeCheck=false, QueryInstrStatus=true, RangeCheck=true, RecordCoercions=false, Simulate=false";

                _driver.Initialize(_resourceName, false, false, standardInitOptions);

                // Set up the DMM for a single reading
                _driver.Trigger.TriggerSource = Agilent34410TriggerSourceEnum.Agilent34410TriggerSourceImmediate;
                _driver.Trigger.TriggerCount = 5;
                //_driver.Trigger.TriggerDelay = 1;
                _driver.Trigger.SampleCount = 1;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            finally
            {
                _driver.Close();
            }
        }
#endif

#if WithoutDevices
        Random rand = new Random(DateTime.Now.Second);
        double lastVoltageValue = -0.0058;
        double lastResistanceValue = 10;
        public double valueStep = 0.00001;
#endif

        /// <summary>
        /// Возвращает текущее значение напряжения.
        /// </summary>
        /// <param name="range">Диапазон измерения в Вольтах. Указывает верхнее измеряемое значение.</param>
        /// <returns></returns>
        public double GetVoltage(double range = 0.01)
        {
#if WithoutDevices
            lastVoltageValue += valueStep + rand.NextDouble() / 100000;
            return  lastVoltageValue;
#else
            return _driver.Voltage.DCVoltage.Measure(range, Agilent34410ResolutionEnum.Agilent34410ResolutionDefault);
#endif
        }

        /// <summary>
        /// Возвращает текущее значение сопротивления.
        /// </summary>
        /// <param name="range">Диапазон измерения в Омах. Указывает верхнее измеряемое значение.</param>
        /// <returns></returns>
        public double GetResistance(double range)
        {
#if WithoutDevices
            lastResistanceValue += rand.NextDouble() * 10 - 3;
            return lastResistanceValue;
#else
            return _driver.Resistance.Measure(range, Agilent34410ResolutionEnum.Agilent34410ResolutionDefault);
#endif
        }

    }
}
