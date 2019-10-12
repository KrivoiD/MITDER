using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Interfaces
{
    /// <summary>
    /// Интерфейс, позволяющий получать сопротивление с устройства
    /// </summary>
    public interface IResistanceMeasurable
    {
        /// <summary>
        /// Указывает, инициализированно ли устройство
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Возвращает текущее значение сопротивления.
        /// </summary>
        /// <param name="range">Диапазон измерения в Омах. Указывает верхнее измеряемое значение.</param>
        /// <returns></returns>
        double GetResistance(double range);
    }
}
