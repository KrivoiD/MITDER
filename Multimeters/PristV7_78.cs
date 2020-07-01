using System;
using System.Diagnostics;

#if !WithoutDevice
using NationalInstruments.Visa;

#endif
using Core.Interfaces;
using Services;
using System.Collections.Generic;

namespace Multimeters
{
	public class PristV7_78 : IVoltageMeasurable, IResistanceMeasurable
	{
		public bool IsInitialized { get; private set; }
		public string ResourceName { get; protected set; }
		/// <summary>
		/// Удобное название устройства. Назначается пользователем.
		/// </summary>
		public string Name { get; set; }
		private UsbSession _session;
		public PristV7_78(string resource)
		{
			ResourceName = resource;
			IsInitialized = true;
			Name = "PristV7/78";
#if !WithoutDevices
			InitializeDevice();
#endif
		}


		public double GetResistance(double range)
		{
			throw new NotImplementedException();
		}

#if WithoutDevices
		Random rand = new Random((int)DateTime.Now.Ticks);
		double lastVoltageValue = -0.0058;
		public double valueStep = 0.00001;
		private int _direction = 1;
		public int Direction {
			get { return _direction; }
			set { _direction = value; } 
		}
#endif
		public double GetVoltage(double range = 0.1)
		{
			//возможны ошибка по timeout
			//TODO: реализовать очищение от ошибки и продолжить измерения
			//TODO: реализовать закрытие с переходом на ручное управление и статус инициализации
			//TODO: реализовать логирование всего процесса
			//TODO: реализовать отказоустойчивость
			//TODO: реализовать переключение заданий
			//TODO: реализовать сохранение заданий

#if WithoutDevices
			lastVoltageValue += Direction * valueStep + rand.NextDouble() / 100000;
			return lastVoltageValue;
#else
			try
			{
				_session.FormattedIO.PrintfAndFlush("READ?");
				var result = _session.FormattedIO.ReadLineDouble();
				//Logger.Info(Name + "  => Получено напряжение " + result.ToString("0.000000") + "В");
				return result;

			}
			catch (TimeoutException ex)
			{
				var error = GetErrorsResult();
				Logger.Warn(Name + " => При получении напряжения возникло TimeoutException: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);				
			}
			catch (Exception ex)
			{
				var error = GetErrorsResult();
				Logger.Error(Name + " => При получении напряжения возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			return double.NaN;
			////_session.FormattedIO.PrintfAndFlush($"MEASURE:VOLTAGE:DC? {range.ToString("0.#", CultureInfo.InvariantCulture)},1e-7");
#endif
		}

		private void InitializeDevice()
		{
			_session = new UsbSession(ResourceName);
			//Очищает прибор от ошибок
			_session.FormattedIO.WriteLine("*CLS");

			//Переводит в режим дистанционного управления. Все кнопки не работают, кроме МУ (ручное управление)
			_session.FormattedIO.WriteLine("SYSTEM:REMOTE");
			//Устанавливает вид работы
			_session.FormattedIO.WriteLine("FUNCTION \"VOLTAGE:DC\"");
			//Устанавливаем предел в 100 мВ и разрешение 0,1 мкВ
			_session.FormattedIO.WriteLine("CONFIGURE:VOLTAGE:DC 0.1,1e-7");

			_session.FormattedIO.FlushWrite(true);

		}

		public List<string> GetErrors()
		{
			var errors = new List<string>(4);
			var error = string.Empty;
			do
			{
				_session.FormattedIO.PrintfAndFlush("SYSTEM:ERROR?");
				error = _session.FormattedIO.ReadString();
				errors.Add(error);
			} while (!error.Contains("No error"));
			//удаляем последнию запись, которая указывает, что отсутствуют ошибки
			errors.RemoveAt(errors.Count - 1);
			return errors;
		}

		public string GetErrorsResult()
		{
			var errorResult = string.Empty;
			foreach (var error in GetErrors())
				errorResult += error;
			return errorResult;
		}
	}
}
