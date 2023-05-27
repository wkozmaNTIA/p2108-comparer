using P2108Comparer.PropModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace P2108Comparer.Converters
{
    class ScenerioConverter : IValueConverter
    {
        /// <summary>
        /// Convert from GUI Polarization to P528.Polarization
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vs = (Scenerio[])value;
            var ls = new TEMP2Model.Scenerio[3];

            for (int i = 0; i < vs.Length; i++)
                ls[i] = (TEMP2Model.Scenerio)((int)vs[i]);

            return ls;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
