using System;
using System.Globalization;
using System.Windows.Controls;

namespace P2108Comparer.ValidationRules
{
    class TimeValidation : ValidationRule
    {
        private const double MINIMUM = 0;
        private const double MAXIMUM = 100;

        private readonly string InvalidInput = $"Time percentage must be between {MINIMUM} and {MAXIMUM}, exclusive.";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty((string)value) ||
                !Double.TryParse(value.ToString(), out double time) ||
                time <= MINIMUM ||
                time >= MAXIMUM)
                return new ValidationResult(false, InvalidInput);

            return ValidationResult.ValidResult;
        }
    }
}
