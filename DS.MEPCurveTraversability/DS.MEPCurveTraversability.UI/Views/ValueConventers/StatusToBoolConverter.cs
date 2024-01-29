using DS.ClassLib.VarUtils.Collisions;
using System;
using System.Windows.Data;

namespace DS.MEPCurveTraversability.UI.Converters
{
    public class StatusToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {

            CollisionStatus status = (CollisionStatus)value;
            switch (status)
            {
                case CollisionStatus.ToResolve:
                    break;
                case CollisionStatus.Resolved:
                    return false;
                case CollisionStatus.Unresolved:
                    break;
                case CollisionStatus.Stopped:
                    break;
                case CollisionStatus.Resolving:
                    return false;
                case CollisionStatus.AwaitingToApply:
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
