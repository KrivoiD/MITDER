using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace Controls.Converters
{
    class BooleanToVisibility : ConvertorBase<BooleanToVisibility>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool)value;
            return val ? Visibility.Visible : Visibility.Collapsed; 
        }
    }
}
