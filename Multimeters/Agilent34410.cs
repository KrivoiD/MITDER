using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
#if !WithoutDevice
using NationalInstruments.Visa;
#endif
using Interfaces;
using Services;

namespace Multimeters
{
	public class Agilent34410 : IVoltageMeasurable, IResistanceMeasurable
	{
		private string _resourceName;
		private string _name;

		/// <summary>
		/// Указывает, инициализировано ли устройство.
		/// </summary>
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// Строка, содержащая VISA-адрес устройства.
		/// </summary>
		public string ResourceName
		{
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
			_name = "Agilent 34410A";
#if !WithoutDevices
			InitializeDevice();
#else
			IsInitialized = true;
#endif
		}

		private UsbSession _session;
#if !WithoutDevices
		/// <summary>
		/// Инициализирует устройство перед работой
		/// </summary>
		private void InitializeDevice()
		{
			//_driver = new Agilent34410Class();
			//try
			//{
			//    // Setup IVI-defined initialization options
			//    string standardInitOptions =
			//        "Cache=true, InterchangeCheck=false, QueryInstrStatus=true, RangeCheck=true, RecordCoercions=false, Simulate=false";

			//    _driver.Initialize(_resourceName, false, false, standardInitOptions);

			//    // Set up the DMM for a single reading
			//    _driver.Trigger.TriggerSource = Agilent34410TriggerSourceEnum.Agilent34410TriggerSourceImmediate;
			//    _driver.Trigger.TriggerCount = 5;
			//    //_driver.Trigger.TriggerDelay = 1;
			//    _driver.Trigger.SampleCount = 1;
			//}
			//catch (Exception e)
			//{
			//    System.Diagnostics.Debug.WriteLine(e);
			//    _driver.Close();
			//}
			//finally
			//{
			//}
			_session = new UsbSession(ResourceName);
			//Очищает прибор от ошибок
			_session.FormattedIO.WriteLine("*CLS");

			//Переводит в режим дистанционного управления. Все кнопки не работают, кроме МУ (ручное управление)
			//_session.FormattedIO.WriteLine("SYSTEM:REMOTE");
			//Устанавливает вид работы
			//_session.FormattedIO.WriteLine("FUNCTION \"VOLTAGE:DC\"");
			////Устанавливаем предел в 100 мВ и разрешение 0,1 мкВ
			//_session.FormattedIO.WriteLine("CONFIGURE:VOLTAGE:DC 0.1,1e-7");

			_session.FormattedIO.FlushWrite(true);

			IsInitialized = true;
		}
#endif

#if WithoutDevices
		Random rand = new Random((int)DateTime.Now.Ticks);
		double lastVoltageValue = -0.0058;
		double lastResistanceValue = 10;
		public double valueStep = 0.00001;
		private int _direction = 1;
		public int Direction
		{
			get { return _direction; }
			set { _direction = value; }
		}
#endif

		/// <summary>
		/// Возвращает текущее значение напряжения.
		/// </summary>
		/// <param name="range">Диапазон измерения в Вольтах. Указывает верхнее измеряемое значение.</param>
		/// <returns></returns>
		public double GetVoltage(double range = 0.1)
		{
#if WithoutDevices
			lastVoltageValue += Direction * valueStep + rand.NextDouble() / 100000;
			return lastVoltageValue;
#else
			//try
			//{
			//    var result = _driver.Voltage.DCVoltage.Measure(range, Agilent34410ResolutionEnum.Agilent34410ResolutionDefault);
			//    Trace.TraceInformation("Agilent => Получено напряжение " + result.ToString("0.000000") + "В");
			//    return result;

			//}
			//catch (Exception ex)
			//{
			//    Trace.TraceError("Agilent => При получении напряжения возникло исключение: " + ex.Message + "\n\t\t" + ex.StackTrace);
			//}
			//return double.NaN;

			try
			{
				if (range == 0.1)
					_session.FormattedIO.WriteLine("CONFIGURE:VOLTAGE:DC 0.1,1e-7");
				else
					_session.FormattedIO.WriteLine("CONFIGURE:VOLTAGE:DC " + range.ToString("0.0"));
				_session.FormattedIO.WriteLine("SENSe:VOLTAGE:DC:IMPedance:AUTO 1");
				_session.FormattedIO.WriteLine("SENSe:VOLTAGE:DC:NPLCycles 10");
				_session.FormattedIO.PrintfAndFlush("READ?");
				var result = _session.FormattedIO.ReadLineDouble();
				Logger.Info(_name + " => Получено напряжение " + result.ToString("0.00000000") + "В");
				return result;

			}
			catch (TimeoutException ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Warn(_name + " => При получении напряжения возникло TimeoutException: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(_name + " => При получении напряжения возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			return double.NaN;
#endif
		}

		/// <summary>
		/// Возвращает текущее значение сопротивления.
		/// </summary>
		/// <param name="range">Диапазон измерения в Омах. Указывает верхнее измеряемое значение.</param>
		/// <returns></returns>
		public double GetResistance(double range = 100)
		{
#if WithoutDevices
			lastResistanceValue += rand.NextDouble() * 10 - 3;
			return lastResistanceValue;
#else
			//try
			//{
			//    var result = _driver.Resistance.Measure(range, Agilent34410ResolutionEnum.Agilent34410ResolutionDefault);
			//    Trace.TraceInformation("Agilent => Получено сопротивление " + result.ToString("0.000000") + "Ом");
			//    return result;

			//}
			//catch (Exception ex)
			//{
			//    Trace.TraceError("Agilent => При получении сопротивления возникло исключение: " + ex.Message + "\n\t\t" + ex.StackTrace);
			//}
			//return double.NaN;
			try
			{
				if (range < 100)
					range = 100;
				//_session.FormattedIO.WriteLine("CONFIGURE:FRESISTANCE " + range.ToString("0"));
				_session.FormattedIO.WriteLine("MEASURE:FRESISTANCE? AUTO");
				_session.FormattedIO.PrintfAndFlush("READ?");
				var result = _session.FormattedIO.ReadLineDouble();
				Logger.Info(_name + " => Получено сопротивления " + result.ToString("0.000000") + "Ом");
				return result;

			}
			catch (TimeoutException ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(_name + " => При получении сопротивления возникло TimeoutException: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(_name + " => При получении сопротивления возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			return double.NaN;
#endif
		}
	}
}
