using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Indicators
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CurrentValueIndicator : UserControl
    {
        public CurrentValueIndicator()
        {
            InitializeComponent();
        }

        #region Dependency Properties and its valuechanged methods

        [Category("Values")]
        public double Value {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(CurrentValueIndicator), new PropertyMetadata(0.0, new PropertyChangedCallback(ValueChanged)));

        private static void ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var indicator = sender as CurrentValueIndicator;
            indicator.ValueTextBlock.Text = ((double)args.NewValue).ToString("0.00000");
            var currentSize = new Size(indicator.ValueTextBlock.ActualWidth, indicator.ValueTextBlock.ActualHeight);
            indicator.ValueTextBlock.FontSize = CalculateFontSize(currentSize, indicator.ValueTextBlock);
        }

        [Category("Values")]
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CurrentValueIndicator), new PropertyMetadata("Title", new PropertyChangedCallback(TitleChanged)));

        private static void TitleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var indicator = sender as CurrentValueIndicator;
            indicator.TitleTextBlock.Text = ((string)args.NewValue);
            var currentSize = new Size(indicator.TitleTextBlock.ActualWidth, indicator.TitleTextBlock.ActualHeight);
            indicator.TitleTextBlock.FontSize = CalculateFontSize(currentSize, indicator.TitleTextBlock);
        }

        [Category("Values")]
        public string Unit {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(CurrentValueIndicator), new PropertyMetadata("Unit", new PropertyChangedCallback(UnitChanged)));

        private static void UnitChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var indicator = sender as CurrentValueIndicator;
            indicator.UnitTextBlock.Text = ((string)args.NewValue);
            var currentSize = new Size(indicator.UnitTextBlock.ActualWidth, indicator.UnitTextBlock.ActualHeight);
            indicator.UnitTextBlock.FontSize = CalculateFontSize(currentSize, indicator.UnitTextBlock);
        }

        [Category("Values"), Description("Значение true отображается зеленным, false - красным, неопределенное состояние - черным")]
        public bool? State {
            get { return (bool?)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(bool?), typeof(CurrentValueIndicator), new PropertyMetadata(null, new PropertyChangedCallback(StateChanged)));

        private static void StateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var indicator = sender as CurrentValueIndicator;
            indicator.StateIndicator.Fill = ConvertBoolToBrush((bool?)args.NewValue);
        }

        /// <summary>
        /// Преобразует тип <see cref="bool?"/> в тип <see cref="Brush"/>
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static Brush ConvertBoolToBrush(Nullable<bool> state)
        {
            if (state == null)
                return Brushes.Black;
            return state.Value ? Brushes.Green : Brushes.Red;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeInfo">содержит требуемый новый размер всего контрола</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            //для каждого TextBlock-а указываем соотношение колонки и строки, в которой он находится в Grid-е
            TitleTextBlock.FontSize = CalculateFontSize(new Size(sizeInfo.NewSize.Width * 7 / 8, sizeInfo.NewSize.Height * 1 / 3), TitleTextBlock);// 7/8 - размер соотношения колонки в разметке Grid-а
            ValueTextBlock.FontSize = CalculateFontSize(new Size(sizeInfo.NewSize.Width * 7 / 8, sizeInfo.NewSize.Height * 2 / 3), ValueTextBlock);
            UnitTextBlock.FontSize = CalculateFontSize(new Size(sizeInfo.NewSize.Width * 1 / 8, sizeInfo.NewSize.Height * 2 / 3), UnitTextBlock);
        }

        /// <summary>
        /// Расчитывает необходимый размер шрифта для оптимального заполнения пространства по высоте и ширине
        /// </summary>
        /// <param name="newSize">Новый требуемый размер</param>
        /// <param name="tb"></param>
        /// <returns></returns>
        private static double CalculateFontSize(Size newSize, TextBlock tb)
        {
            double fontSize = tb.FontSize;

            // get desired size with fontsize = MaxFontSize
            Size desiredSize = MeasureText(tb);
            double widthMargins = tb.Margin.Left + tb.Margin.Right;
            double heightMargins = tb.Margin.Top + tb.Margin.Bottom;

            double factor = (newSize.Height - heightMargins) / (desiredSize.Height - heightMargins);
            double factor2 = (newSize.Width - widthMargins) / (desiredSize.Width - widthMargins);
            fontSize = fontSize * Math.Min(factor, factor2);
            //fontSize = Math.Min(fontSize, 1000);
            //System.Diagnostics.Debugger.Log(1, "debug", $"New    {newSize.Width.ToString("N1")}, {newSize.Height.ToString("N1")}" + Environment.NewLine +
            //                                   $"Actual {desiredWidth.ToString("N1")}, {desiredHeight.ToString("N1")}" + Environment.NewLine +
            //                                   $"Factor {factor.ToString("N1")}, factor2 {factor2.ToString("N1")}" + Environment.NewLine);
            // apply fontsize (always equal fontsizes)
            return fontSize;
        }

        // Measures text size of textblock
        private static Size MeasureText(TextBlock tb)
        {
            var formattedText = new FormattedText(tb.Text, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize, Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
