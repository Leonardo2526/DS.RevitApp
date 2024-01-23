using System;
using System.Collections;
using System.Windows.Data;

namespace DS.MEPCurveTraversability.UI.Converters
{
    public class EnumerableToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var val = (ICollection)value;
            
            return val is not null && val.Count > 0 ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
