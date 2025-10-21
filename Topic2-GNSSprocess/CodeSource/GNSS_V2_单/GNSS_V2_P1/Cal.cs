using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNSS_V2_P1
{
    internal class Cal
    {
        public static List<SateToObs> CalSTOa(List<obs> obsS)
        {
            List<SateToObs> stoa = new List<SateToObs>();

            var sto = obsS.GroupBy(a => a.sat_id);
            foreach (var kvp in sto)
            {
                string id = kvp.Key;
                SateToObs stos = new SateToObs(id);
                List<obs> obs = kvp.OrderBy(a => a.freq_id).ToList();
                stos.obsS = new List<obs>(obs);
                stoa.Add(stos);
            }
            return stoa;
        }//利用.GroupBy按卫星分组
    }
    class CSDete
    {
        static double gfThreshold = 0.005; // GF可调阈值，单位：米
        public static List<ProArc> DetectCycleSlipsWithGF(SateToObs STO)
        {
            List<ProArc> arcs = new List<ProArc>();
            // 按时间戳排序观测值
            var sortedObs = STO.obsS.OrderBy(a => a.timestamp).ToList();
            var groupedByEpoch = sortedObs.GroupBy(o => o.timestamp)
                                          .OrderBy(g => g.Key)
                                          .ToList();

            ProArc currentArc = null;
            double? prevGF = null;

            foreach (var epoch in groupedByEpoch)
            {
                var epochObs = epoch.ToList();
                var obs1 = epochObs.FirstOrDefault(o => o.freq_id == 1);
                var obs2 = epochObs.FirstOrDefault(o => o.freq_id == 2);

                // 如果缺少双频数据，视为周跳
                if (obs1 == null || obs2 == null)
                {
                    FinalizeCurrentArc(ref currentArc, arcs, epoch.Key);
                    prevGF = null;
                    continue;
                }

                double currentGF = obs1.carrier - obs2.carrier;//单位

                // 检测周跳条件
                bool cycleSlipDetected = prevGF.HasValue &&
                                        Math.Abs(currentGF - prevGF.Value) > gfThreshold;

                if (cycleSlipDetected)
                {
                    FinalizeCurrentArc(ref currentArc, arcs, epoch.Key);//确认弧段
                }

                // 创建新弧段（如果需要）
                if (currentArc == null)
                {
                    currentArc = new ProArc(epoch.Key);
                }

                // 添加当前历元的所有观测值到弧段
                currentArc.Epochs.AddRange(epochObs);
                currentArc.EndTime = epoch.Key; // 更新结束时间
                prevGF = currentGF; // 更新前一个GF值
            }

            // 处理最后一个弧段
            FinalizeCurrentArc(ref currentArc, arcs, null);
            // 对每个弧段应用处理
            foreach (var arc in arcs)
            {
                arc.CalculateMultipath();  // 计算多路径误差
                arc.ApplyHatchFilter();     // 应用Hatch滤波
            }
            return arcs;
        }
        private static void FinalizeCurrentArc(ref ProArc currentArc, List<ProArc> arcs, DateTime? nextEpochTime)
        {
            if (currentArc != null)
            {
                // 设置弧段结束时间
                if (currentArc.Epochs.Count > 0 && !currentArc.EndTime.HasValue)
                {
                    currentArc.EndTime = nextEpochTime ?? currentArc.Epochs.Last().timestamp;
                }
                arcs.Add(currentArc);
                currentArc = null;
            }
        }
    }
}
