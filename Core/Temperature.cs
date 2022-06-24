using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public class Temperature
	{
		public TemperatureUnit Unit { get; private set; }
		public double Value { get; set; }

		public Temperature(TemperatureUnit unit)
		{
			Unit = unit;
		}

		public Temperature(TemperatureUnit unit, double value) : this(unit)
		{
			Value = value;
		}

		public void SetUnit(TemperatureUnit newUnit)
		{
			if(Unit == newUnit)
			{
				return;
			}
			switch (newUnit)
			{
				case TemperatureUnit.MilliVolt:

					break;
				case TemperatureUnit.Kelvin:
					break;
				case TemperatureUnit.Celcius:
					break;
				default:
					break;
			}
		}

	}
}
