using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Controls
{
    /// <summary>
    /// Interaction logic for ValueIndicator.xaml
    /// </summary>
    public partial class ValueIndicator : UserControl
    {
        [Category("Format")]
        public double IndicatorDiameter {
            get { return (double)GetValue(IndicatorDiameterProperty); }
            set { SetValue(IndicatorDiameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IndicatorRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndicatorDiameterProperty =
            DependencyProperty.Register(nameof(IndicatorDiameter), typeof(double), typeof(ValueIndicator), new FrameworkPropertyMetadata(30.0));


        [Category("Format")]
        public bool IsLedOn {
            get { return (bool)GetValue(IsLedOnProperty); }
            set { SetValue(IsLedOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLedOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLedOnProperty =
            DependencyProperty.Register(nameof(IsLedOn), typeof(bool), typeof(ValueIndicator), new PropertyMetadata(false));



        [Category("Format")]
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ValueIndicator), new PropertyMetadata("Title"));


        [Category("Format")]
        public string Unit {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Unit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(ValueIndicator), new PropertyMetadata("unit"));


        [Category("Format")]
        public double Value {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(ValueIndicator), new PropertyMetadata(0.0));


        [Category("Format")]
        public string ValueStringFormat {
            get { return (string)GetValue(ValueStringFormatProperty); }
            set { SetValue(ValueStringFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueStringFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueStringFormatProperty =
            DependencyProperty.Register("ValueStringFormat", typeof(string), typeof(ValueIndicator), new PropertyMetadata("{0:e4}"));




        [Category("Format")]
        public double TitleFontSize {
            get { return (double)GetValue(TitleFontSizeProperty); }
            set { SetValue(TitleFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.Register(nameof(TitleFontSize), typeof(double), typeof(ValueIndicator), new FrameworkPropertyMetadata(8.0));


        [Category("Format")]
        public bool IsTitleEnabled {
            get { return (bool)GetValue(IsTitleEnabledProperty); }
            set { SetValue(IsTitleEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTitleEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTitleEnabledProperty =
            DependencyProperty.Register(nameof(IsTitleEnabled), typeof(bool), typeof(ValueIndicator), new FrameworkPropertyMetadata(true));


        [Category("Format")]
        public FontFamily TitleFontFamily {
            get { return (FontFamily)GetValue(TitleFontFamilyProperty); }
            set { SetValue(TitleFontFamilyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleFontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontFamilyProperty =
            DependencyProperty.Register(nameof(TitleFontFamily), typeof(FontFamily), typeof(ValueIndicator), new FrameworkPropertyMetadata(new FontFamily("Segoe UI")));

        [Category("Format")]
        public FontStretch TitleFontStretch {
            get { return (FontStretch)GetValue(TitleFontStretchProperty); }
            set { SetValue(TitleFontStretchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleFontStretch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontStretchProperty =
            DependencyProperty.Register(nameof(TitleFontStretch), typeof(FontStretch), typeof(ValueIndicator), new FrameworkPropertyMetadata(FontStretches.Normal));


        [Category("Format")]
        public FontStyle TitleFontStyle {
            get { return (FontStyle)GetValue(TitleFontStyleProperty); }
            set { SetValue(TitleFontStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleFontStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontStyleProperty =
            DependencyProperty.Register(nameof(TitleFontStyle), typeof(FontStyle), typeof(ValueIndicator), new FrameworkPropertyMetadata(FontStyles.Normal));


        [Category("Format")]
        public FontWeight TitleFontWeight {
            get { return (FontWeight)GetValue(TitleFontWeightProperty); }
            set { SetValue(TitleFontWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleFontWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontWeightProperty =
            DependencyProperty.Register(nameof(TitleFontWeight), typeof(FontWeight), typeof(ValueIndicator), new FrameworkPropertyMetadata(FontWeights.Normal));




        [Category("Format")]
        public double UnitFontSize {
            get { return (double)GetValue(UnitFontSizeProperty); }
            set { SetValue(UnitFontSizeProperty, value); }
        }


        // Using a DependencyProperty as the backing store for TitleFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitFontSizeProperty =
            DependencyProperty.Register(nameof(UnitFontSize), typeof(double), typeof(ValueIndicator), new FrameworkPropertyMetadata(8.0));

        [Category("Format")]
        public bool IsUnitEnabled {
            get { return (bool)GetValue(IsUnitEnabledProperty); }
            set { SetValue(IsUnitEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsUnitEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUnitEnabledProperty =
            DependencyProperty.Register(nameof(IsUnitEnabled), typeof(bool), typeof(ValueIndicator), new FrameworkPropertyMetadata(true));


        [Category("Format")]
        public FontFamily UnitFontFamily {
            get { return (FontFamily)GetValue(UnitFontFamilyProperty); }
            set { SetValue(UnitFontFamilyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitFontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitFontFamilyProperty =
            DependencyProperty.Register(nameof(UnitFontFamily), typeof(FontFamily), typeof(ValueIndicator), new FrameworkPropertyMetadata(new FontFamily("Segoe UI")));

        [Category("Format")]
        public FontStretch UnitFontStretch {
            get { return (FontStretch)GetValue(UnitFontStretchProperty); }
            set { SetValue(UnitFontStretchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitFontStretch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitFontStretchProperty =
            DependencyProperty.Register(nameof(UnitFontStretch), typeof(FontStretch), typeof(ValueIndicator), new FrameworkPropertyMetadata(FontStretches.Normal));


        [Category("Format")]
        public FontStyle UnitFontStyle {
            get { return (FontStyle)GetValue(UnitFontStyleProperty); }
            set { SetValue(UnitFontStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitFontStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitFontStyleProperty =
            DependencyProperty.Register(nameof(UnitFontStyle), typeof(FontStyle), typeof(ValueIndicator), new FrameworkPropertyMetadata(FontStyles.Normal));


        [Category("Format")]
        public FontWeight UnitFontWeight {
            get { return (FontWeight)GetValue(UnitFontWeightProperty); }
            set { SetValue(UnitFontWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitFontWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitFontWeightProperty =
            DependencyProperty.Register(nameof(UnitFontWeight), typeof(FontWeight), typeof(ValueIndicator), new FrameworkPropertyMetadata(FontWeights.Normal));

        [Category("Format")]
        public Brush OnColor {
            get { return (Brush)GetValue(OnColorProperty); }
            set { SetValue(OnColorProperty, value); }
        }

        public static readonly DependencyProperty OnColorProperty =
        DependencyProperty.Register("OnColor", typeof(Brush), typeof(ValueIndicator), new PropertyMetadata(Brushes.Green));

        [Category("Format")]
        public Brush OffColor {
            get { return (Brush)GetValue(OffColorProperty); }
            set { SetValue(OffColorProperty, value); }
        }

        public static readonly DependencyProperty OffColorProperty =
            DependencyProperty.Register("OffColor", typeof(Brush), typeof(ValueIndicator), new PropertyMetadata(Brushes.Red));

        static ValueIndicator()
        {

        }

        public ValueIndicator()
        {
            InitializeComponent();
        }


    }
}
