using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2108Comparer
{
    static class Constants
    {
        // plot default values
        public const double XAXIS_MIN_DEFAULT = 0;
        public const double XAXIS_MAX_DEFAULT = 35;
        public const double YAXIS_MAX_DEFAULT = 100;
        public const double YAXIS_MIN_DEFAULT = 0;

        // set of colors instead of default order
        public static OxyColor[] Colors = new[]
        {
            OxyColors.Red,
            OxyColors.Green,
            OxyColors.Blue,
            OxyColors.Orange,
            OxyColors.Orchid,
            OxyColors.Brown
        };
    }
}
