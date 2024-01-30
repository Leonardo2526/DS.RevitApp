using System;
using System.Windows.Data;

namespace DS.MEPCurveTraversability.UI.Converters
{
    public class CountConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            int count = (int)value;           
            return count > 0 ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
