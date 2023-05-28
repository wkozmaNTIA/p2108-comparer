﻿using P2108Comparer.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace P2108Comparer.ValidationRules
{
    class XAxisLimitsValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is BindingGroup bindingGroup))
                return new ValidationResult(false, "Invalid use of validation rule.");

            if (bindingGroup.Items.Count == 0)
                return ValidationResult.ValidResult;

            var wndw = (SetAxisWindow)bindingGroup.Items[0];

            if (wndw.XAxisMaximum <= wndw.XAxisMinimum)
                return new ValidationResult(false, "X-Axis values are invalid");

            return ValidationResult.ValidResult;
        }
    }
}
