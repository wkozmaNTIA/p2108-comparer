using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace P2108Comparer.PropModels
{
    public static class TEMP2Model
    {
        public enum Scenerio : int
        {
            LowRise = 10,
            MidRise = 20,
            HighRise = 30
        }

        public static int AeronauticalStatisticalModel(double f__ghz, double theta__deg, double p, 
            double h__meter, Scenerio scenerio, bool directional, out double L_clt__db)
        {
            double C_e;
            double k;

            switch (scenerio)
            {
                case Scenerio.LowRise:
                    C_e = 0.08 * h__meter - 2;              // [Eqn 21a]
                    k = Math.Pow((h__meter + 10) / 8, 3);   // [Eqn 21b]
                    break;

                case Scenerio.MidRise:
                    C_e = 0.22 * h__meter - 4;              // [Eqn 22a]
                    k = Math.Pow((h__meter - 1) / 8, 2.2);  // [Eqn 22b]
                    break;

                case Scenerio.HighRise:
                    C_e = 0.2 * h__meter - 7;               // [Eqn 23a]
                    k = Math.Pow((h__meter - 1) / 8, 1.2);  // [Eqn 23b]
                    break;

                default:
                    throw new Exception();  // should never happen...
            }

            double numerator = 1 - Math.Pow(Math.E, -(theta__deg + C_e) / 90 * k);
            double denominator = 1 - Math.Pow(Math.E, -(90 + C_e) / 90 * k);
            double LOSp = 100 * numerator / denominator;

            double h_s__meter = (int)scenerio;

            if (h_s__meter - h__meter < 0)
            {
                L_clt__db = 0;
                return 0;
            }
            else if (h_s__meter - h__meter == 0)
            {
                L_clt__db = 0.06 * p * Math.Pow(Math.E, -2 * theta__deg);
                return 0;
            }
            else // h_s__meter - h__meter > 0
            {
                if (p <= LOSp) // LOS
                {
                    L_clt__db = Math.Pow(7, p / LOSp) - 1 - InverseComplementaryCumulativeDistribution(Math.Sqrt(p / LOSp) / 2);

                    if (directional)
                        L_clt__db = Math.Max(L_clt__db, 0);
                }
                else // NLOS
                {
                    double A = 7.78 + 0.23 * theta__deg;                // [Eqn 19]
                    double B = -30 + 8 * Math.Log10(theta__deg + 1);    // [Eqn 18]
                    double C_t = B + A * Math.Log10(h_s__meter);        // [Eqn 17]

                    L_clt__db = C_t * C(p) + L_ces(f__ghz, theta__deg, p) - (C_t * C(LOSp) + L_ces(f__ghz, theta__deg, LOSp)) + 6 
                        - 0.1 * (h__meter - 5) * (p - LOSp) / 100;

                    L_clt__db = Math.Max(L_clt__db, 6);
                }

                return 0;
            }
        }

        private static double C(double p)
        {
            double C_p = 0.7 * p / 100;     // [Eqn 16]

            return C_p;
        }

        private static double L_ces(double f__ghz, double theta__deg, double p)
        {
            // [Eqn 20]
            double K_1 = 93 * Math.Pow(f__ghz, 0.175);
            double A_1 = 0.05;
            // breaking Eqn 20 into parts cause of its size...
            double part1 = -K_1 * Math.Log(1 - p / 100);
            double part2 = cot(A_1 * (1 - theta__deg / 90) + (Math.PI * theta__deg / 180));
            double part3 = 0.5 * (90 - theta__deg) / 90;
            double part4 = 0.6 * InverseComplementaryCumulativeDistribution(p / 100);
            double L_ces__db = Math.Pow(part1 * part2, part3) - 1 - part4;

            return L_ces__db;
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
