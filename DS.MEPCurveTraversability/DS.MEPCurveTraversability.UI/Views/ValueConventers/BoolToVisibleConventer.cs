using System;
using System.Globalization;

namespace DS.MEPCurveTraversability.UI
{
    public class BoolToVisibleConventer : BaseValueConverter<BoolToVisibleConventer>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
