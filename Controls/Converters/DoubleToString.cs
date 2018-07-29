using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Controls.Converters
{
    class DoubleToString : ConvertorBase<DoubleToString>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double)value;
            string res = string.Empty;
            if (parameter is string)
                res = val.ToString((string)parameter);
            else
                res = val.ToString();
            return res;
        }
    }
}
