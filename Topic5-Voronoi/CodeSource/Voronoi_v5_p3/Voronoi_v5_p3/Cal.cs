using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voronoi_v5_p3
{
    internal class Cal
    {
        public static double CalDis(Point p,double x,double y)
        {
            return Math.Sqrt(Math.Pow(p.X-x,2)+Math.Pow(p.Y-y,2));
        }
        public static List<Edge> CalSingles(List<Triangle> Tris)
        {
            List<Edge> Alledges = Tris.SelectMany(a=>a.edges).ToList();
            return Alledges.GroupBy(a=>a)
                            .Where(a=>a.Count()==1)
                            .Select(a=>a.Key)
                            .ToList();
        }
        public static List<Triangle> CalT(List<Point> points)
        {
            List<Triangle> result = new List<Triangle>();
            double minX = points.Min(a => a.X);
            double maxX = points.Max(a => a.X);
            double minY = points.Min(a => a.Y);
            double maxY = points.Max(a => a.Y);
            Point x0y0 = new Point { Id = "100", X = minX - 1e5, Y = minY - 1e5 };
            Point x1y1 = new Point { Id = "111", X = maxX + 1e5, Y = maxY + 1e5 };
            Point x0y1 = new Point { Id = "101", X = minX - 1e5, Y = minY + 1e5 };
            Point x1y0 = new Point { Id = "110", X = maxX + 1e5, Y = minY - 1e5 };
            result.Add(new Triangle(x0y0, x1y1, x0y1));
            result.Add(new Triangle(x0y0, x1y1, x1y0));

            for (int i = 0; i < points.Count; i++)
            {
                List<Triangle> T0 = new List<Triangle>(result);
                List<Triangle> TrisConPoints = new List<Triangle>();
                foreach (Triangle t in T0)
                {
                    if (CalDis(points[i], t.CircleX, t.CircleY) < t.CircleR)
                    {
                        result.Remove(t);
                        TrisConPoints.Add(t);
                    }
                }
                List<Edge> edge = CalSingles(TrisConPoints);
                foreach (Edge e in edge)
                {
                    result.Add(new Triangle(points[i],e.p1,e.p0));
                }
            }
            return result;
        }
        public static Dictionary<Point, List<Point>> CalV(List<Triangle> T)
        {
            Dictionary<Point, List<Point>> SiteToCenters = new Dictionary<Point, List<Point>>();
            Dictionary<Point, List<Point>> V = new Dictionary<Point, List<Point>>();

            foreach (Triangle t in T)
            {
                Point center = new Point { Id=$"{t.CircleR}",X=t.CircleX,Y=t.CircleY};
                AddToDic(SiteToCenters, t.A, center);
                AddToDic(SiteToCenters, t.B, center);
                AddToDic(SiteToCenters, t.C, center);
            }
            foreach (var kvp in SiteToCenters)
            {
                Point site = kvp.Key;
                List<Point> centers = kvp.Value;
                centers.Sort((p1, p2) => 
                {
                    double a1 = Math.Atan2(p1.Y - site.Y, p1.X - site.X);
                    double a2 = Math.Atan2(p2.Y - site.Y, p2.X - site.X);
                    return a1.CompareTo(a2);
                });
                V.Add(site, centers);
            }
            return V;
        }
        private static void AddToDic(Dictionary<Point, List<Point>> Dic, Point site, Point center)
        {
            if (!Dic.ContainsKey(site))
            {
                Dic[site] = new List<Point>();
            }
            Dic[site].Add(center);
        }
    }
}
