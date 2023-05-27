using System;
using System.Globalization;
using System.Windows.Controls;

namespace P2108Comparer.ValidationRules
{
    class GroundStationHeightValidation : ValidationRule
    {
        private const double MINIMUM = 1;
        private const double MAXIMUM = 30;

        private readonly string InvalidInput = $"Ground station height must be between {MINIMUM} and {MAXIMUM}, inclusive.";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty((string)value) ||
                !Double.TryParse(value.ToString(), out double h__meter) ||
                h__meter < MINIMUM ||
                h__meter > MAXIMUM)
                return new ValidationResult(false, InvalidInput);

            return ValidationResult.ValidResult;
        }
    }
}
