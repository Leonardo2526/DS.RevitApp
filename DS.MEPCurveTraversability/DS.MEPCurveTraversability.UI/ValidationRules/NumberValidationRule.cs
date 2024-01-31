using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Strings;
using System;
using System.Windows.Controls;

namespace DS.MEPCurveTraversability.UI
{
    public class NumberValidationRule : ValidationRule
    {
        public override ValidationResult Validate
          (object value, System.Globalization.CultureInfo cultureInfo)
        {
            string stringValue = value as string;
            string errorMessage = "Ошибка ввода!";

            if (String.IsNullOrEmpty(stringValue))
            {
                return new ValidationResult(false, $"{errorMessage} Значение не может быть пустым.");
            }
            else if (!stringValue.IsInt())
            {
                return new ValidationResult(false, $"{errorMessage} Значение должно быть целым числом.");
            }

            int maxValue = 10000;
            int minValue = 0;
            int devisibleNum = 1;

            int.TryParse(stringValue, out int intValue);
            if (!intValue.IsDevisible(devisibleNum))
            {
                return new ValidationResult(false, $"{errorMessage} Значение должно быть кратным {devisibleNum}.");
            }
            else if (intValue < minValue || intValue > maxValue)
            {
                return new ValidationResult(false, $"{errorMessage} Значение должно быть в пределах от {minValue} до {maxValue}.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
