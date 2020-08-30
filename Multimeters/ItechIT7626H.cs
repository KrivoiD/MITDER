using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces;
using NationalInstruments.Visa;
using Services;

namespace Multimeters
{
	public class ItechIT7626H : IPowerSupply
	{
		public bool IsInitialized { get; private set; }
		public string ResourceName { get; protected set; }
		/// <summary>
		/// Удобное название устройства. Назначается пользователем.
		/// </summary>
		public string Name { get; set; }
		private UsbSession _session;
		public ItechIT7626H(string resource)
		{
			ResourceName = resource;
			IsInitialized = true;
			Name = "Itech IT7626H";
#if !WithoutDevices
			InitializeDevice();
#endif
		}

		private void InitializeDevice()
		{
			_session = new UsbSession(ResourceName);
			//Очищает прибор от ошибок
			_session.FormattedIO.WriteLine("*CLS");

			//Переводит в режим дистанционного управления. Все кнопки не работают, кроме МУ (ручное управление)
			_session.FormattedIO.WriteLine("SYSTEM:REMOTE");
			//Проверка подачи сигнала
			_session.FormattedIO.WriteLine("SYSTem:BEEPer");

			_session.FormattedIO.FlushWrite(true);

		}

		public void SetCurrent(double value)
		{
#if !WithoutDevices
			try
			{
				var curr = currency.ToString("0.00");
				_session.FormattedIO.PrintfAndFlush("CURRent " + curr);
				Logger.Info(Name + " => Установлено значение тока " + curr + "А");
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При установке значения тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
#endif
		}

		public void SetPower(double voltage, double currency)
		{
#if !WithoutDevices
			try
			{
				var volt = voltage.ToString("0.0");
				var curr = currency.ToString("0.00");
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

		public void SetVoltage(double value)
		{
#if !WithoutDevices
			try
			{
				var volt = value.ToString("0.0");
				_session.FormattedIO.PrintfAndFlush("VOLTage " + volt);
				Logger.Info(Name + " => Установлено значение напряжения " + volt + "В");
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
#if WithoutDevices
			try
			{
				var state = isOn ? "ON" : "OFF";
				_session.FormattedIO.PrintfAndFlush("OUTPut " + state);
				Logger.Info(Name + " => Выходное питание переведено в состояние" + state);
				return true;
			}
			catch (Exception ex)
			{
				var error = SessionHelper.GetErrorsResult(_session);
				Logger.Error(Name + " => При установке значения тока возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
				return false;
			}
#endif
		}
	}
}
