using MathNet.Numerics;
using System;

namespace Services
{
	/// <summary>
	/// Класс конверитрует показания термопары типа К из мВ в Кельвины и обратно
	/// </summary>
	public static class ThermocoupleConverter
	{
		//коэффициенты взяты с https://srdata.nist.gov/its90/download/type_k.tab

		/// <summary>
		/// Коэффициенты для преобразования из мВ в Кельвины от -5,891 мВ (-200°С) до 0 мВ (0°С)
		/// </summary>
		private static readonly Polynomial _lowCoeffToCelcius = new Polynomial(new double[] {
																								 0.0000000E+00,
																								 2.5173462E+01,
																								-1.1662878E+00,
																								-1.0833638E+00,
																								-8.9773540E-01,
																								-3.7342377E-01,
																								-8.6632643E-02,
																								-1.0450598E-02,
																								-5.1920577E-04,
																								 0.0000000E+00
																							});
		/// <summary>
		/// Коэффициенты для преобразования из мВ в Кельвины от 0 мВ (0°С) до 20.644 мВ (500°С)
		/// </summary>
		private static readonly Polynomial _middleCoeffToCelcius = new Polynomial(new double[] {
																								 0.000000E+00,
																								 2.508355E+01,
																								 7.860106E-02,
																								-2.503131E-01,
																								 8.315270E-02,
																								-1.228034E-02,
																								 9.804036E-04,
																								-4.413030E-05,
																								 1.057734E-06,
																								-1.052755E-08
																							});
		/// <summary>
		/// Коэффициенты для преобразования из мВ в Кельвины от 20.644 мВ (500°С) до 54.886 мВ (1372°С)
		/// </summary>
		private static readonly Polynomial _highCoeffToCelcius = new Polynomial(new double[] {
																								-1.318058E+02,
																								 4.830222E+01,
																								-1.646031E+00,
																								 5.464731E-02,
																								-9.650715E-04,
																								 8.802193E-06,
																								-3.110810E-08,
																								 0.000000E+00,
																								 0.000000E+00,
																								 0.000000E+00
																							});

		/// <summary>
		/// Преобразует мВ в °С.
		/// </summary>
		/// <param name="mV">Значение в милливольтах</param>
		/// <returns>Значение в градусах Цельсия.</returns>
		/// <remarks>Точный диапазон преобразований от от -5,891 мВ (-200°С) до 54.886 мВ (1372°С). За пределами данного диапазона возможны не логические значения температур.</remarks>
		public static double ToCelcius(double mV)
		{
			if (mV < 0)
			{
				return _lowCoeffToCelcius.Evaluate(mV);
			}
			if (mV < 20.644)
			{
				return _middleCoeffToCelcius.Evaluate(mV);
			}
			return _highCoeffToCelcius.Evaluate(mV);
		}

		/// <summary>
		/// Преобразует мВ в K.
		/// </summary>
		/// <param name="mV">Значение в милливольтах</param>
		/// <returns>Значение в градусах Кельвина.</returns>
		/// <remarks>Точный диапазон преобразований от от -5,891 мВ (73.15К) до 54.886 мВ (1645.15K). За пределами данного диапазона возможны не логические значения температур.</remarks>
		public static double ToKelvin(double mV)
		{
			return ToCelcius(mV) + 275.15;
		}


		public static double ToMilliVolts(double celcius)
		{
			throw new NotImplementedException();
		}
	}
}
