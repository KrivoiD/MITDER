using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces
{
    /// <summary>
    /// Интерфейс, позволяющий получать напряжение с устройства
    /// </summary>
    public interface IVoltageMeasurable
    {
        /// <summary>
        /// Указывает, инициализированно ли устройство
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Возвращает текущее значение напряжения.
        /// </summary>
        /// <param name="range">Диапазон измерения в Вольтах. Указывает верхнее измеряемое значение.</param>
        /// <returns></returns>
        double GetVoltage(double range);

#if WithoutDevices
        int Direction { get; set; }
#endif
    }
}
