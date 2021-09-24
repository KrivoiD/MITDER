using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Interfaces;
using NationalInstruments.Visa;
using Services;

namespace Multimeters
{
	public class ItechIT7626H : IPowerSupply, IDisposable
	{
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// Максимальная сила тока для установки, в Амперах
		/// </summary>
		public double MaxCurrent { get; private set; }

		/// <summary>
		/// Максимальное напряжение для установки, в Вольтах
		/// </summary>
		public double MaxVoltage { get; private set; }

		public string ResourceName { get; protected set; }

		/// <summary>
		/// Удобное название устройства. Назначается пользователем.
		/// </summary>
		public string Name { get; set; }

		private UsbSession _session;

		public ItechIT7626H(string resource, double maxVoltage, double maxCurrent)
		{
			ResourceName = resource;
			IsInitialized = true;
			Name = "Itech IT7626H";
			MaxVoltage = maxVoltage;
			MaxCurrent = maxCurrent;
#if !WithoutDevices
			InitializeDevice();
#else
			IsInitialized = true;
#endif
		}

		private void InitializeDevice()
		{
			_session = new UsbSession(ResourceName);
			//Очищает прибор от ошибок
			_session.FormattedIO.WriteLine("*CLS");

			//Переводит в режим дистанционного управления. Все кнопки не работают, кроме Local
			_session.FormattedIO.WriteLine("SYSTEM:REMOTE");
			//Проверка подачи сигнала
			_session.FormattedIO.WriteLine("SYSTem:BEEPer");

			_session.FormattedIO.FlushWrite(true);

			IsInitialized = true;
		}

		public double? SetCurrent(double value)
		{
			var settedValue = GetCurrent();
			if (settedValue is null)
				return null;
			if (value == settedValue.Value)
				return value;
			if (value > MaxCurrent)
				value = MaxCurrent;
			value = Math.Max(value, 0);
#if !WithoutDevices
			try
			{
				var curr = value.ToString("0.00", CultureInfo.InvariantCulture);
				_session.FormattedIO.PrintfAndFlush("CURRent " + curr);
				Logger.Info(Name + " => Установлено значение силы тока " + curr + "А");
				return value;
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При установке значения силы тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
#endif
			return null;
		}

		public double? SetVoltage(double value)
		{
			var settedValue = GetVoltage();
			if (settedValue is null)
				return null;
			if (value == settedValue.Value)
				return value;
			if (value > MaxVoltage)
				value = MaxVoltage;
			value = Math.Max(value, 0);
#if !WithoutDevices
			try
			{
				var volt = value.ToString("0.0", CultureInfo.InvariantCulture);
				_session.FormattedIO.PrintfAndFlush("VOLTage " + volt);
				Logger.Info(Name + " => Установлено значение напряжения " + volt + "В");
				return value;
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При установке значения напряжения тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
#endif
			return null;
		}

		public void SetPower(double voltage, double currency)
		{
			voltage = Math.Min(voltage, MaxVoltage);
			currency = Math.Min(currency, MaxCurrent);
#if !WithoutDevices
			try
			{
				var volt = voltage.ToString("0.0", CultureInfo.InvariantCulture);
				var curr = currency.ToString("0.00", CultureInfo.InvariantCulture);
				_session.FormattedIO.PrintfAndFlush("APPLy " + volt + ", " + curr);
				Logger.Info(Name + " => Установлено значение напряжения " + volt + "В" + "и тока " + curr + "А");
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При установке значения тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
#endif
		}

		public bool TurnPower(bool isOn)
		{
#if !WithoutDevices
			try
			{
				var state = isOn ? "ON" : "OFF";
				_session.FormattedIO.PrintfAndFlush("OUTPut " + state);
				Logger.Info(Name + " => Выходное питание переведено в состояние " + state);
				return true;
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При установке значения тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
#endif
			return false;
		}

		private double? GetCurrent()
		{
			try
			{
				_session.FormattedIO.PrintfAndFlush("CURRent?");
				var curr = _session.FormattedIO.ReadDouble();
				Logger.Info(Name + " => Текущее заданное значение силы тока " + curr + "А");
				return curr;
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При чтении значения силы тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			return null;
		}

		private double? GetVoltage()
		{
			try
			{
				_session.FormattedIO.PrintfAndFlush("VOLTage?");
				var volt = _session.FormattedIO.ReadDouble();
				Logger.Info(Name + " => Текущее заданное значение напряжения " + volt + "В");
				return volt;
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При чтении значения напряжения тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			return null;
		}


		public void Dispose()
		{
			if(!_session.IsDisposed)
			{
				TurnPower(false);
				_session.Dispose();
			}
		}
	}
}
