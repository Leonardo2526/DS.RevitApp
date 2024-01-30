using DS.ClassLib.VarUtils.Collisions;
using System;
using System.Windows.Data;

namespace DS.MEPCurveTraversability.UI.Converters
{
    public class CollisionIsResolvingConverterInverted : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            CollisionStatus status = (CollisionStatus)value;

            switch (status)
            {
                case CollisionStatus.Resolving:
                    return false;
                case CollisionStatus.Resolved:
                    return false;
                default:
                    break;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
