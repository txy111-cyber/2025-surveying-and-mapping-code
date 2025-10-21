using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNSS_V2_P1
{
    internal class Data
    {
        public static List<obs> LoadObs(string filepath)
        {
            StreamReader sr = new StreamReader(filepath);
            sr.ReadLine();
            List<obs> obsS = new List<obs>();
            while (!sr.EndOfStream)
            {
                string lines = sr.ReadLine();
                string[] parts = lines.Split(',');
                string[] timeParts = parts[0].Trim().Split(' ');
                obs o = new obs
                {
                    timestamp = new DateTime(
                            int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]),
                            int.Parse(timeParts[3]), int.Parse(timeParts[4]), int.Parse(timeParts[5])),//"2025 01 22 00 00 00"
                    sat_id = parts[1],
                    freq_id = double.Parse(parts[2]),
                    pseud = double.Parse(parts[3]),
                    carrier = double.Parse(parts[4]),
                    lambda = double.Parse(parts[5]),
                    f = 299792458 / double.Parse(parts[5]),
                };
                obsS.Add(o);
            }
            return obsS;
        }
    }
    class obs
    {
        public DateTime timestamp;
        public string sat_id;
        public double freq_id;
        public double pseud;
        public double carrier;
        public double lambda;
        public double f;
    }
    class SateToObs
    {
        public List<obs> obsS = new List<obs>();
        public string sat_id;
        public SateToObs(string sat)
        {
            sat_id = sat;
        }
    }
    class ProArc
    {
        public DateTime StartTime;
        public DateTime? EndTime;
        public List<obs> Epochs = new List<obs>();
        public bool HasCycleSlip { get; set; } = false;
        public List<double> MultipathErrors { get; set; } = new List<double>();  // 多路径误差
        public List<double> SmoothedPseudoranges { get; set; } = new List<double>();  // 平滑后的伪距

        public ProArc(DateTime startTime)
        {
            StartTime = startTime;
        }

        // 计算伪距多路径误差
        public void CalculateMultipath()
        {
            if (Epochs.Count == 0) return;

            // 按时间戳分组（每个历元一组）
            var epochGroups = Epochs.GroupBy(e => e.timestamp)
                                   .OrderBy(g => g.Key)
                                   .ToList();

            foreach (var epoch in epochGroups)
            {
                var epochObs = epoch.ToList();
                var obs1 = epochObs.FirstOrDefault(o => o.freq_id == 1);
                var obs2 = epochObs.FirstOrDefault(o => o.freq_id == 2);

                if (obs1 == null || obs2 == null)
                {
                    MultipathErrors.Add(double.NaN);  // 缺失数据用NaN表示
                    continue;
                }

                // 计算多路径误差 (MP1)
                double alpha = Math.Pow(obs1.f / obs2.f, 2);  // α = (f1/f2)^2
                double mp1 = obs1.pseud -
                            (1 + 2 / (alpha - 1)) * (obs1.carrier * obs1.lambda) +
                            (2 / (alpha - 1)) * (obs2.carrier * obs2.lambda);

                MultipathErrors.Add(mp1);
            }
        }

        // Hatch滤波平滑伪距
        public void ApplyHatchFilter()
        {
            if (Epochs.Count == 0) return;

            // 按时间戳排序
            var sortedEpochs = Epochs.GroupBy(e => e.timestamp)
                                    .OrderBy(g => g.Key)
                                    .ToList();

            double? prevSmoothed = null;
            int windowSize = 0;

            //obs prevObs1 = null;
            foreach (var epoch in sortedEpochs)
            {
                var epochObs = epoch.ToList();
                var obs1 = epochObs.FirstOrDefault(o => o.freq_id == 1);

                if (obs1 == null)
                {
                    SmoothedPseudoranges.Add(double.NaN);
                    continue;
                }

                // 首次处理
                if (prevSmoothed == null)
                {
                    SmoothedPseudoranges.Add(obs1.pseud);
                    prevSmoothed = obs1.pseud;
                    windowSize = 1;
                    continue;
                }

                // 查找前一历元的载波相位观测值
                var prevEpoch = sortedEpochs
                    .Where(e => e.Key < epoch.Key)
                    .OrderByDescending(e => e.Key)
                    .FirstOrDefault();

                var prevObs1 = prevEpoch?.FirstOrDefault(o => o.freq_id == 1);

                if (prevObs1 == null)
                {
                    SmoothedPseudoranges.Add(obs1.pseud);
                    prevSmoothed = obs1.pseud;
                    windowSize = 1;
                    continue;
                }

                // Hatch滤波公式
                windowSize = Math.Min(windowSize + 1, 100);  // 限制窗口大小
                double weight = 1.0 / windowSize;

                double smoothed = weight * obs1.pseud +
                                 (1 - weight) * (prevSmoothed.Value +
                                 obs1.lambda * (obs1.carrier - prevObs1.carrier));

                SmoothedPseudoranges.Add(smoothed);
                prevSmoothed = smoothed;
            }
        }
    }//!!!
}
