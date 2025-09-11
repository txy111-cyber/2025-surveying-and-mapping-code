using System.Collections.Generic;
using System.IO;

namespace pointcloud
{
    internal class Data
    {
        public static List<Point> LoadData(string filepath)
        { 
            var lines = File.ReadAllLines(filepath);
            var results = new List<Point>();
            int i = 1;
            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                results.Add(new Point {
                    Id = i,
                    X = double.Parse(parts[0]),
                    Y = double.Parse(parts[1]),
                    Z = double.Parse(parts[2])
                });
                i++;
            }
            return results;
        }
    }
    class Point
    {
        public int Id {  get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int i {  get; set; }
        public int j { get; set; }
        public int k { get; set; }
        //public List<Point> prev { get; set; }
        public List<Point> neighbors { get; set; }
        public double miu {  get; set; }
        public double sigma {  get; set; }
        public bool noisy {  get; set; }
        public Point()
        {
            //prev = new List<Point>();
            neighbors = new List<Point>();
        }
    }
}
