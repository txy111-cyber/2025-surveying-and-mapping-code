using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace singlePic_v6_final
{
    internal class Cal
    {
        public static double[,] CalR(Photo input)
        {
            double cosO = Cos(input.ome);double sinO = Sin(input.ome);
            double cosP = Cos(input.phi);double sinP = Sin(input.phi);
            double cosK = Cos(input.kap);double sinK = Sin(input.kap);
            return new double[3, 3] {
                {cosP*cosK ,sinO*sinP*cosK+cosO*sinK ,-cosO*sinP*cosK+sinO*sinK},
                {-cosP*sinK,-sinO*sinP*sinK+cosO*cosK,cosO*sinP*sinK+sinO*cosK},
                {sinP      ,-sinO*cosP               ,cosO*cosP}
            };
        }
        public static void CalJS(Photo input)
        {
            List<Point> points = input.points;
            double Xs0 = input.Xs0;
            double Ys0 = input.Ys0;
            double Zs0 = input.Zs0;
            double f = input.f;

            input.R=CalR(input);
            double a1 = input.R[0, 0]; double a2 = input.R[0, 1]; double a3 = input.R[0, 2];
            double b1 = input.R[1, 0]; double b2 = input.R[1, 1]; double b3 = input.R[1, 2];
            double c1 = input.R[2, 0]; double c2 = input.R[2, 1]; double c3 = input.R[2, 2];

            foreach (Point p in points)
            {
                double X = p.X; double Y = p.Y; double Z = p.Z;
                double Zba = a3 * (X - Xs0) + b3 * (Y - Ys0) + c3 * (Z - Zs0);

                p.xJs = -f * (a1 * (X - Xs0) + b1 * (Y - Ys0) + c1 * (Z - Zs0)) / Zba;
                p.yJs = -f * (a2 * (X - Xs0) + b2 * (Y - Ys0) + c2 * (Z - Zs0)) / Zba;
            }
        }
        public static void CalL(Photo input)
        {
            int n = input.points.Count;
            input.L = new double[2 * n, 1];
            int i = 0;
            foreach (Point p in input.points)
            {
                input.L[2 * i, 0] = p.x - p.xJs;
                input.L[2 * i+1, 0] = p.y- p.yJs;
                i++;
            }
        }
        public static void CalA(Photo photo) 
        {
            int n = photo.points.Count();
            double a1 = photo.R[0, 0]; double a2 = photo.R[0, 1]; double a3 = photo.R[0, 2];
            double b1 = photo.R[1, 0]; double b2 = photo.R[1, 1]; double b3 = photo.R[1, 2];
            double c1 = photo.R[2, 0]; double c2 = photo.R[2, 1]; double c3 = photo.R[2, 2];
            double w = photo.ome; double phi = photo.phi; double kpa = photo.kap;
            double Xs0 = photo.Xs0;
            double Ys0 = photo.Ys0;
            double Zs0 = photo.Zs0;
            double f = photo.f;

            photo.A = new double[2 * n, 6];
            double[,] A = new double[2 * n, 6];
            int i = 0;
            foreach (Point p in photo.points)
            {
                double X = p.X; double Y = p.Y; double Z = p.Z;
                double x = p.x; double y = p.y;
                double Zba = a3 * (X - Xs0) + b3 * (Y - Ys0) + c3 * (Z - Zs0);
                A[2 * i, 0] = (a1 * f + a3 * x) / Zba;
                A[2 * i, 1] = (b1 * f + b3 * x) / Zba;
                A[2 * i, 2] = (c1 * f + c3 * x) / Zba;
                A[2 * i, 3] = y * Sin(w) - (x / f * (x * Cos(kpa) - y * Sin(kpa)) + f * Cos(kpa)) * Cos(w);
                A[2 * i, 4] = -f * Sin(kpa) - x / f * (x * Sin(kpa) + y * Cos(kpa));
                A[2 * i, 5] = y;

                A[i * 2 + 1, 0] = (a2 * f + a3 * y) / Zba;
                A[i * 2 + 1, 1] = (b2 * f + b3 * y) / Zba;
                A[i * 2 + 1, 2] = (c2 * f + c3 * y) / Zba;
                A[i * 2 + 1, 3] = -x * Sin(w) - (y / f * (x * Cos(kpa) - y * Sin(kpa)) - f * Sin(kpa)) * Cos(w);
                A[i * 2 + 1, 4] = -f * Cos(kpa) - y / f * (x * Sin(kpa) + y * Cos(kpa));
                A[i * 2 + 1, 5] = -x;
                i++;
            }
            photo.A = A;
        }
    }
}
