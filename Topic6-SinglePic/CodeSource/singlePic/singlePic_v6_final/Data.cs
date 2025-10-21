using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace singlePic_v6_final
{
    internal class Data
    {
        public static Photo LoadData(string filepath)
        {
            Photo result = new Photo();
            StreamReader sr = new StreamReader(filepath);
            sr.ReadLine();
            var photoparts = sr.ReadLine().Split(',');
            result.m = double.Parse(photoparts[0]);
            result.f = double.Parse(photoparts[1]);
            result.x0 = double.Parse(photoparts[2]);
            result.y0 = double.Parse(photoparts[3]);
            sr.ReadLine();
            sr.ReadLine();
            result.points = new List<Point>();
            while (!sr.EndOfStream)
            {
                var parts = sr.ReadLine().Split(',');
                result.points.Add(new Point { 
                    Id = parts[0],
                    X = double.Parse(parts[1]),
                    Y = double.Parse(parts[2]),
                    Z = double.Parse(parts[3]), 
                    x = double.Parse(parts[4]),
                    y = double.Parse(parts[5]),
                    });
            }
            result.ome = 0.0;
            result.phi = 0.0;
            result.kap = 0.0;
            result.Xs0 = result.points.Average(a => a.X);
            result.Ys0 = result.points.Average(a => a.Y);
            result.Zs0 = result.m * result.f;
            return result;
        }
    }
    class Photo
    {
        public double X {  get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double ome {  get; set; }
        public double phi {  get; set; }
        public double kap {  get; set; }
        public double m {  get; set; }
        public double f { get; set; }
        public double x0 { get; set; }
        public double y0 { get; set; }
        public double Xs0 { get; set; }//初始值
        public double Ys0 { get; set; }
        public double Zs0 {  get; set; }
        public List<Point> points { get; set; }
        public double[,] R { get; set; }
        public double[,] A { get; set; }
        public double[,] L { get; set; }
    }
    class Point
    {
        public string Id {  get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }//m
        public double x {  get; set; }//mm
        public double y { get; set; }
        public double xJs { get; set; }
        public double yJs { get; set; }
        public double zJs { get; set; }
    }
}
