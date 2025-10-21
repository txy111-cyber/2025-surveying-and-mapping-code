using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GNSS_v2_p4_copy//50
{
    internal class Data
    {
        public static void LoadObs(string filepath,List<obs> epochs)
        {
            var lines = File.ReadAllLines(filepath).Skip(5);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                obs o = new obs { Epoch = int.Parse(parts[0]),sat_Id = parts[1].Trim()};
                o.Pse["P1"] = double.Parse(parts[2]);
                o.Car["L1"] = double.Parse(parts[3]);
                o.Pse["P2"] = double.Parse(parts[4]);
                o.Car["L2"] = double.Parse(parts[5]);
                o.Pse["P5"] = double.Parse(parts[6]);
                o.Car["L5"] = double.Parse(parts[7]);
                epochs.Add(o);
            }
        }
    }
    public class obs
    {
        public string sat_Id { get; set; }
        public int Epoch { get; set; }
        public Dictionary<string, double> Pse { get; set; }
        public Dictionary<string, double> Car { get; set; }
        public bool HasCS1 { get;set; }
        public bool HasCS12 { get; set; }
        public bool HasCS123 { get; set; }
        public obs()
        {
            Pse = new Dictionary<string, double>();
            Car = new Dictionary<string, double>();
        }
    }
    public class FreqConfig
    {
        public string System { get;set; }
        public double f1 { get;set; }
        public double f2 { get;set; }
        public double f5 { get;set; }

        public static Dictionary<string, FreqConfig> Systems = new Dictionary<string, FreqConfig>
        {
            { "G", new FreqConfig { System = "G", f1 = 1575.42e6, f2 = 1227.60e6, f5 = 1176.45e6 } }, 
            { "R", new FreqConfig { System = "R", f1 = 1602.00e6, f2 = 1246.00e6, f5 = 0 } },         
            { "E", new FreqConfig { System = "E", f1 = 1575.42e6, f2 = 1278.75e6, f5 = 1176.45e6 } },
            { "C", new FreqConfig { System = "C", f1 = 1575.42e6, f2 = 1176.45e6, f5 = 1207.14e6 } }  
        };
    }
}
