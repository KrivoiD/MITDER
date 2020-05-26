using System;
using System.Diagnostics;

#if !WithoutDevice
using NationalInstruments.Visa;

#endif
using Core.Interfaces;

namespace Multimeters
{
	public class PristV7_78 : IVoltageMeasurable, IResistanceMeasurable
	{
		public bool IsInitialized => true;
		public string ResourceName { get; protected set; }

		private UsbSession _session;
		public PristV7_78(string resource)
		{
			ResourceName = resource;
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
		public int Direction { get; set; } = 1;
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
                _session.FormattedIO.PrintfAndFlush($"READ?");
				var result = _session.FormattedIO.ReadLineDouble();
				Trace.TraceInformation("PristV7_78 => Получено напряжение " + result.ToString("0.000000") + "В");
                return result;

			}
			catch (TimeoutException ex)
			{
                _session.FormattedIO.PrintfAndFlush("SYSTEM:ERROR?");
				var error = _session.FormattedIO.ReadString();
				Trace.TraceError("PristV7_78 => При получении напряжения возникло TimeoutException: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
			}
			catch (Exception ex)
			{
				_session.FormattedIO.PrintfAndFlush("SYSTEM:ERROR?");
				var error = _session.FormattedIO.ReadString();
				Trace.TraceError("PristV7_78 => При получении напряжения возникло исключение: " + ex.Message + "\n\t\tОшибка по прибору: " + error + "\n\t\tStackTrace" + ex.StackTrace);
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
	}
}
