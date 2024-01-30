using DS.ClassLib.VarUtils.Collisions;
using System;
using System.Windows.Data;

namespace DS.MEPCurveTraversability.UI.Converters
{
    public class ApplyVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            CollisionStatus status = (CollisionStatus)value;
            switch (status)
            {
                case CollisionStatus.AwaitingToApply:
                    return "Visible";
                default:
                    break;
            }
           
            return "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
