using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voronoi_v5_p3
{
    internal class Data
    {
        public static List<Point> LoadPoints(string filepath)
        {
            StreamReader sr = new StreamReader(filepath);
            sr.ReadLine();
            List<Point> points= new List<Point>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[]parts = line.Split(',');
                Point p = new Point { Id = parts[0], X = double.Parse(parts[1]), Y = double.Parse(parts[2]) };
                points.Add(p);
            }
            return points;
        }
    }
    class Point
    {
        public string Id;
        public double X;
        public double Y;
    }
    class Edge
    {
        public Point p0;
        public Point p1;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())//！！！逻辑判断
                return false;
            Edge other = (Edge)obj;
            return (p0.Id == other.p1.Id && p1.Id ==other.p0.Id) || 
                (p0.Id ==other.p0.Id && p1.Id==other.p1.Id);
        }
        public override int GetHashCode()
        {
            return p0.Id.GetHashCode()^p1.Id.GetHashCode();
        }
    }
    class Triangle
    {
        public Point A;
        public Point B;
        public Point C;
        public List<Edge> edges = new List<Edge>();

        public double CircleR;
        public double CircleX;
        public double CircleY;
        public Triangle(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;

            Edge e0 = new Edge { p0 = a, p1 = b };
            Edge e1 = new Edge { p0 = b, p1 = c };
            Edge e2 = new Edge { p0 = c, p1 = a };
            edges.Add(e0);
            edges.Add(e1);
            edges.Add(e2);

            double x1 = A.X;
            double x2 = B.X;
            double x3 = C.X;
            double y1 = A.Y;
            double y2 = B.Y;
            double y3 = C.Y;
            CircleX = ((y2 - y1) * (y3 * y3 - y1 * y1 + x3 * x3 - x1 * x1) - (y3 - y1) * (y2 * y2 - y1 * y1 + x2 * x2 - x1 * x1))
                / (2 * (x3 - x1) * (y2 - y1) - 2 * (x2 - x1) * (y3 - y1));
            CircleY = ((x2 - x1) * (x3 * x3 - x1 * x1 + y3 * y3 - y1 * y1) - (x3 - x1) * (x2 * x2 - x1 * x1 + y2 * y2 - y1 * y1))
                / (2 * (y3 - y1) * (x2 - x1) - 2 * (y2 - y1) * (x3 - x1));
            CircleR = Math.Sqrt(Math.Pow(x1 - CircleX, 2) + Math.Pow(y1 - CircleY, 2));
        }
    }
}
