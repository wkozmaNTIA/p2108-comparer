using System;

namespace P2108Comparer.PropModels
{
    internal static class P2108
    {
        const int SUCCESS = 0;
        const int FREQUENCY_ERROR = 1;
        const int THETA_ERROR = 2;
        const int PERCENTAGE_ERROR = 3;

        /// <summary>
        /// The Earth-space and aeronautical statistical clutter loss model as described in Section 3.3.
        /// </summary>
        /// <param name="f__ghz">Frequency, in GHz</param>
        /// <param name="theta__deg">Elevation angle, in degrees</param>
        /// <param name="p">Percentage of locations, in %</param>
        /// <param name="L_ces__db">Additional loss (clutter loss), in dB</param>
        /// <returns>Error code</returns>
        public static int AeronauticalStatisticalModel(double f__ghz, double theta__deg, double p, out double L_ces__db)
        {
            var rtn = InputValidation(f__ghz, theta__deg, p);
            if (rtn != SUCCESS)
            {
                L_ces__db = -1;
                return rtn;
            }

            double A_1 = 0.05;
            double K_1 = 93 * Math.Pow(f__ghz, 0.175);

            double part1 = Math.Log(1 - p / 100.0);
            double part2 = A_1 * (1 - theta__deg / 90.0) + Math.PI * theta__deg / 180.0;
            double part3 = 0.5 * (90.0 - theta__deg) / 90.0;
            double part4 = 0.6 * InverseComplementaryCumulativeDistribution(p / 100);

            L_ces__db = Math.Pow(-K_1 * part1 * cot(part2), part3) - 1 - part4;

            return SUCCESS;
        }

        private static int InputValidation(double f__ghz, double theta__deg, double p)
        {
            if (f__ghz < .7 || f__ghz > 100)
                return FREQUENCY_ERROR;

            if (theta__deg < 0 || theta__deg > 90)
                return THETA_ERROR;

            if (p <= 0 || p >= 100)
                return PERCENTAGE_ERROR;

            return SUCCESS;
        }

        private static double cot(double x)
        {
            return 1 / Math.Tan(x);
        }

        private static double InverseComplementaryCumulativeDistribution(double q)
        {
            double C_0 = 2.515517;
            double C_1 = 0.802853;
            double C_2 = 0.010328;
            double D_1 = 1.432788;
            double D_2 = 0.189269;
            double D_3 = 0.001308;

            double x = q;
            if (q > 0.5)
                x = 1.0 - x;

            double T_x = Math.Sqrt(-2.0 * Math.Log(x));

            double zeta_x = ((C_2 * T_x + C_1) * T_x + C_0) / (((D_3 * T_x + D_2) * T_x + D_1) * T_x + 1.0);

            double Q_q = T_x - zeta_x;

            if (q > 0.5)
                Q_q = -Q_q;

            return Q_q;
        }
    }
}
