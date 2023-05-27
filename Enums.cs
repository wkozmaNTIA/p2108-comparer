using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2108Comparer
{
    enum PlotMode
    {
        Single,
        MultipleFrequencies,
        MultipleBaseStationHeights,
        MultipleElevationAngles,
        MultipleSheildingHeights
    }

    enum Scenerio : int
    {
        LowRise = 10,
        MidRise = 20,
        HighRise = 30
    }
}
