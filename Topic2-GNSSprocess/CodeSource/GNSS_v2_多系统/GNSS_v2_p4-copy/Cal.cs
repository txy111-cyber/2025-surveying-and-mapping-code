using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace GNSS_v2_p4_copy
{
    class GnssProcessor
    {
        private const double C = 299792458.0;
        private const int WINDOW_Max = 100;
        private List<obs> inputs;
        private Dictionary<string, List<obs>> sat_groups;
        private Dictionary<string, FreqConfig> frqCon;
        public GnssProcessor(List<obs> inputobs)
        {
            inputs = inputobs;
            sat_groups = inputs.GroupBy(a => a.sat_Id).ToDictionary(a => a.Key,a=>a.OrderBy(o=>o.Epoch).ToList());
            frqCon = new Dictionary<string, FreqConfig>();
            foreach (var o in inputobs)
            {
                string sys = o.sat_Id.Substring(0, 1);
                if (!frqCon.ContainsKey(o.sat_Id))
                {
                    if(FreqConfig.Systems.TryGetValue(sys, out FreqConfig config))
                        frqCon[o.sat_Id] = config;
                }
            }
        }
        private double CalMW(double L1, double P1, double L2, double P2, double f1, double f2)
        {
            double Lw = (f1 * L1 - f2 * L2) / (f1 - f2);
            double Pn = (f1 * P1 + f2 * P2) / (f1 + f2);
            return Lw - Pn / (C / (f1 - f2));
        }
        public void DetectCS()
        {
            foreach (var sat in sat_groups)
            {
                string sat_id = sat.Key;
                List<obs> obs = sat.Value;
                FreqConfig config = frqCon[sat_id];
                for (int i = 1;i<obs.Count;i++)
                {
                    obs prev = obs[i - 1];
                    obs curr = obs[i];
                    double DelPhi = curr.Car["L1"]-prev.Car["L1"];
                    if (Math.Abs(DelPhi) > 2.0)
                    {
                        curr.HasCS1 = true;
                    }
                }//单频
                for (int i = 1;i<obs.Count;i++)
                {
                    obs prev = obs[i - 1];
                    obs curr = obs[i];

                    double MWprev = CalMW(prev.Car["L1"], prev.Pse["P1"], prev.Car["L2"], prev.Pse["P2"], config.f1, config.f2);
                    double MWcurr = CalMW(curr.Car["L1"], curr.Pse["P1"], curr.Car["L2"], curr.Pse["P2"], config.f1, config.f2);
                    double delMW = MWcurr- MWprev;
                    if (Math.Abs(delMW) > 0.5)
                    {
                        curr.HasCS12 = true;
                    }
                }//双频
                for (int i = 1; i < obs.Count; i++)
                {
                    var prev = obs[i - 1];
                    var curr = obs[i];

                    double gf12Prev = prev.Car["L1"] - prev.Car["L2"];
                    double gf13Prev = prev.Car["L1"] - prev.Car["L5"];
                    double gf23Prev = prev.Car["L2"] - prev.Car["L5"];

                    double gf12Curr = curr.Car["L1"] - curr.Car["L2"];
                    double gf13Curr = curr.Car["L1"] - curr.Car["L5"];
                    double gf23Curr = curr.Car["L2"] - curr.Car["L5"];

                    double deltaGf12 = gf12Curr - gf12Prev;
                    double deltaGf13 = gf13Curr - gf13Prev;
                    double deltaGf23 = gf23Curr - gf23Prev;
                    if (Math.Abs(deltaGf12) > 0.1 || Math.Abs(deltaGf13) > 0.1 || Math.Abs(deltaGf23) > 0.1)
                    {
                        curr.HasCS123 = true;
                    }
                }//三频
            }
        }
        public Dictionary<string, Dictionary<int, Dictionary<string, double>>> MultiPathError()
        {
            var results = new Dictionary<string, Dictionary<int, Dictionary<string, double>>>();
            foreach (var sat in sat_groups)
            { 
                string id = sat.Key;
                List<obs> epochs = sat.Value;
                FreqConfig config = frqCon[id];

                double alpha = Math.Pow(config.f1 / config.f2, 2);
                double gamma = Math.Pow(config.f5 / config.f1, 2);

                if (!results.ContainsKey(id))
                    results[id] = new Dictionary<int, Dictionary<string, double>>();
                foreach (obs epoch in epochs)
                {
                    results[id][epoch.Epoch] = new Dictionary<string, double>();

                    double L1term1 = (1 + 2 / (alpha - 1)) * (epoch.Car["L1"] * C / config.f1);
                    double L1term2 = (2 * alpha / (alpha - 1)) * (epoch.Car["L2"] * C / config.f2);
                    results[id][epoch.Epoch]["MP1"] = epoch.Pse["P1"] - L1term1 - L1term2;

                    double L2term1 = (1 + 2 / (alpha - 1)) * (epoch.Car["L2"] * C / config.f2);
                    double L2term2 = (2 / (alpha - 1)) * (epoch.Car["L1"] * C / config.f1);
                    results[id][epoch.Epoch]["MP2"] = epoch.Pse["P2"] - L2term1 - L2term2;

                    double L3term1 = (1 + 2 / (gamma - 1)) * (epoch.Car["L5"] * C / config.f5);
                    double L3term2 = (2 * gamma / (gamma - 1)) * (epoch.Car["L1"] * C / config.f1);
                    results[id][epoch.Epoch]["MP5"] = epoch.Pse["P5"] - L3term1 - L3term2;
                }
            }
            return results;
        }
        
        private Dictionary<string, Dictionary<string, double>> SmoothPse = new Dictionary<string, Dictionary<string, double>>();
        private Dictionary<string, Dictionary<string, int>> WindowCounts = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<int, Dictionary<string, double>>> HatchSmooth()
        {
            var result = new Dictionary<string, Dictionary<int, Dictionary<string, double>>>();
            foreach (var sate in sat_groups)
            {
                string sat_id = sate.Key;
                List<obs> epochs = sate.Value;
                FreqConfig config = frqCon[sat_id];
                if (!SmoothPse.ContainsKey(sat_id))
                    SmoothPse[sat_id] = new Dictionary<string, double>();
                if (!WindowCounts.ContainsKey(sat_id))
                    WindowCounts[sat_id] = new Dictionary<string, int>();
                if (!result.ContainsKey(sat_id))
                    result[sat_id] = new Dictionary<int, Dictionary<string, double>>();
                obs firstobs = epochs[0];
                result[sat_id][firstobs.Epoch] = new Dictionary<string, double>();
                foreach (var freq in new[] { "P1", "P2", "P5" })
                {
                    if (firstobs.Pse.ContainsKey(freq))
                    {
                        SmoothPse[sat_id][freq] = firstobs.Pse[freq];
                        WindowCounts[sat_id][freq] = 1;
                        result[sat_id][firstobs.Epoch][freq] = firstobs.Pse[freq];
                    }
                }
                for (int i = 1;i<epochs.Count;i++)
                { 
                    obs prev = epochs[i-1];
                    obs curr = epochs[i];
                    result[sat_id][curr.Epoch] = new Dictionary<string, double>();
                    foreach (var freq in new[] { "P1", "P2", "P5" })
                    {
                        string conver = freq.Replace("P","L");
                        Getf(config,conver,out double f);
                        double lamb = C / f;

                        bool rest = false;
                        if(freq == "P1"&&(curr.HasCS1||curr.HasCS12||curr.HasCS123))rest=true;
                        if (freq == "P2" && (curr.HasCS12 || curr.HasCS123)) rest = true;
                        if (freq == "P3" && curr.HasCS123) rest = true;

                        if (rest)
                        {
                            SmoothPse[sat_id][freq] = curr.Pse[freq];
                            WindowCounts[sat_id][freq] = 1;
                            result[sat_id][firstobs.Epoch][freq] = curr.Pse[freq];
                        }
                        else 
                        {
                            int n = Math.Min(WindowCounts[sat_id][freq] + 1,WINDOW_Max);
                            WindowCounts[sat_id][freq] = n;

                            double prevSmoothed = SmoothPse[sat_id][freq];
                            double deltconver = curr.Car[conver] - prev.Car[conver];

                            double smoothed =(1.0/n)*curr.Pse[freq]+((n-1.0)/n)*(prevSmoothed+lamb*deltconver);

                            SmoothPse[sat_id][freq] = smoothed;
                            result[sat_id][curr.Epoch][freq] = smoothed;
                        }
                    }
                }
            }
            return result;
        }
        private static void Getf(FreqConfig GCF, string L, out double f)
        {
            f = 0.0;
            if (L == "L1")
            {
                f = GCF.f1;
            }
            if (L == "L2")
            {
                f = GCF.f2;
            }
            if (L == "L5")
            {
                f = GCF.f5;
            }
        }
    }
}
