using System;
using System.Globalization;
using System.Windows.Controls;

namespace P2108Comparer.ValidationRules
{
    class ElevationAngleValidation : ValidationRule
    {
        private const double MINIMUM = 0;
        private const double MAXIMUM = 90;

        private readonly string InvalidInput = $"Elevation angle must be between {MINIMUM} and {MAXIMUM}, inclusive.";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty((string)value) ||
                !Double.TryParse(value.ToString(), out double theta__deg) ||
                theta__deg < MINIMUM ||
                theta__deg > MAXIMUM)
                return new ValidationResult(false, InvalidInput);

            return ValidationResult.ValidResult;
        }
    }
}
