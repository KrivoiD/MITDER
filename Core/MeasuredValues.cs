using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    /// <summary>
    /// Класс, хранящий значения напряжений и сопротивления одного измерения.
    /// </summary>
    public class MeasuredValues
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="date">Дата и время измерения значений</param>
        public MeasuredValues(DateTime date)
        {
            Date = date;
        }

        /// <summary>
        /// Значение напряжения в мВ на верхней термопаре.
        /// </summary>
        public double TopTemperature { get; set; }
        /// <summary>
        /// Значение напряжения в мВ на нижней термопаре.
        /// </summary>
        public double BottomTemperature { get; set; }
        /// <summary>
        /// Значение сопротивления в Ом
        /// </summary>
        public double Resistance { get; set; }
        /// <summary>
        /// Дата и время измерения
        /// </summary>
        public DateTime Date { get; private set; }
    }
}
