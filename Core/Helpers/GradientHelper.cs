using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Helpers
{
    /// <summary>
	/// Класс, следящий за градиентом температур (разница между верхней и нижней температур) и указывающий на направление изменения питания.
	/// Для получения коэффициента изменения использовать <see cref="GradientHelper.GetPowerChangingDirection"/>
	/// </summary>
	public class GradientHelper
    {
        /// <summary>
        /// Размер градиента в мВ
        /// </summary>
        public double GradientSize { get; set; }
        /// <summary>
        /// Диапазон стабильности в мВ
        /// </summary>
        public double StabilityRange { get; set; }

        /// <summary>
        /// Конструткор. <see cref="StabilityRange"/> устанавливается 10% от <see cref="GradientSize"/> по умолчанию
        /// </summary>
        /// <param name="gradientSize">Размер градиента в мВ</param>
        public GradientHelper(double gradientSize)
        {
            GradientSize = gradientSize;
            StabilityRange = gradientSize * 0.1;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="gradientSize">Величина градиента (разница температур) в мВ</param>
        /// <param name="stabilityRange">Диапазон стабильности в мВ</param>
        public GradientHelper(double gradientSize, double stabilityRange) : this(gradientSize)
        {
            StabilityRange = stabilityRange;
        }

        /// <summary>
		/// Возвращает коэффициент, указывающий на изменение питания.
		/// </summary>
		/// <returns>
		///  1 - необходимо увеличить питание<br /> 
		///  0 - оставить без изменений<br />
		/// -1 - необходимо уменьшить питание</returns>
		public int GetPowerChangingDirection(double bottomTemp, double topTemp)
        {
            var diff = topTemp - bottomTemp;
            if (diff < GradientSize - StabilityRange)
                return 1;
            if (diff > GradientSize + StabilityRange)
                return -1;
            return 0;
        }
    }
}
