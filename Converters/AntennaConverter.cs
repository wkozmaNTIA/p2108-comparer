using P2108Comparer.PropModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace P2108Comparer.Converters
{
    class AntennaConverter : IValueConverter
    {
        /// <summary>
        /// Convert from GUI Antenna to Model.Antenna
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vs = (Antenna[])value;
            var ls = new TEMP2Model.Antenna[2];

            for (int i = 0; i < vs.Length; i++)
                ls[i] = (TEMP2Model.Antenna)((int)vs[i]);

            return ls;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
