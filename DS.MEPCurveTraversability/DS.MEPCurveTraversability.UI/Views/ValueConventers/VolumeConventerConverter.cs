using Autodesk.Revit.DB;
using System;
using System.Globalization;

namespace DS.MEPCurveTraversability.UI
{
    public class VolumeConventerConverter : BaseValueConverter<VolumeConventerConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var volume = Math.Round(UnitUtils.ConvertFromInternalUnits((double)value, DisplayUnitType.DUT_CUBIC_CENTIMETERS), 2);
            //var volume = Math.Round(UnitUtils.ConvertFromInternalUnits((double)value, UnitTypeId.CubicCentimeters), 2);
            return $"Объем пересечения: {volume} см3";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
