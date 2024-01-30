using DS.ClassLib.VarUtils.Collisions;
using System;
using System.Windows.Data;

namespace DS.MEPCurveTraversability.UI.Converters
{
    public class StatusToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            //if (targetType != typeof(Enum))
            //    throw new InvalidOperationException("The target must be a 'CollisionStatus'");

            CollisionStatus status = (CollisionStatus)value;

            switch (status)
            {
                case CollisionStatus.ToResolve:
                    break;
                case CollisionStatus.Resolved:
                    return "РЕШЕНА";
                case CollisionStatus.Unresolved:
                    return "НЕ УДАЛОСЬ ИСПРАВИТЬ";
                case CollisionStatus.Stopped:
                    return "ИСПРАВЛЕНИЕ ОСТАНОВЛЕНО";
                case CollisionStatus.Resolving:
                    return "ИСПРАВЛЯЕТСЯ";
                case CollisionStatus.AwaitingToApply:
                    return "ОЖИДАЕТ ПОДТВЕРЖДЕНИЯ";
                default:
                    break;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
