using System;
using System.Collections.Generic;
using System.Linq;

namespace pointcloud
{
    internal class Cal
    {
        public string output = "序号,说明,答案\n";
        public List<Point> points { get; set; }//原始点云
        public int number { get; set; }//原始点云总数

        public double xmin1, ymin1, zmin1;
        public double xmax1, ymax1, zmax1;
        public double miuAver {  get; set; }
        public double sigmaAver {  get; set; }

        private Dictionary<(int, int, int), List<Point>> gridDict = new Dictionary<(int, int, int), List<Point>>();
        public Cal(List<Point> input) 
        {
            points = input;
        }
        public void cal_1_20()//完成程序正确性1——20
        {
            foreach (var p in points)
            {
                if (p.Id == 1)
                {
                    output += $"1,点P1的x坐标,{p.X:f3}\n";
                }
                if (p.Id == 6)
                {
                    output += $"2,点P6的y坐标,{p.Y:f3}\n";
                }
                if (p.Id == 789)
                {
                    output += $"3,点P789的z坐标,{p.Z:f3}\n";
                }
            }
            number = points.Count;
            double xmax, ymax,zmax,xmin,ymin,zmin;
            xmax = points.Max(a => a.X);xmin = points.Min(a => a.X);
            ymax = points.Max(a => a.Y);ymin = points.Min(a => a.Y);
            zmax = points.Max(a => a.Z);zmin = points.Min(a => a.Z);

            output += $"4,原始点云的总点数,{number}\n";
            output += $"5,点云x的最大值xmax,{xmax:f3}\n";
            output += $"6,点云y的最大值ymax,{ymax:f3}\n";
            output += $"7,点云z的最大值zmax,{zmax:f3}\n";

            xmax1 = (((xmax - xmin) / 3.0) + 1.0) * 3.0 + xmin;
            ymax1 = (((ymax - ymin) / 3.0) + 1.0) * 3.0 + ymin;
            zmax1 = (((zmax - zmin) / 3.0) + 1.0) * 3.0 + zmin;
            xmin1 = xmin;
            ymin1 = ymin;
            zmin1 = zmin;

            output += $"8,格网xmim1,{xmin:f3}\n";
            output += $"9,格网xmax1,{xmax1:f3}\n";
            output += $"10,格网ymim1,{ymin:f3} \n";
            output += $"11,格网ymax1,{ymax1:f3} \n";
            output += $"12,格网zmim1,{zmin:f3} \n";
            output += $"13,格网zmax1,{zmax1:f3}  \n";

            List<Point> index000 = new List<Point>();
            foreach (Point p in points)
            {
                p.i = (int)Math.Floor((p.X - xmin) / 3);
                p.j = (int)Math.Floor((p.Y - ymin) / 3);
                p.k = (int)Math.Floor((p.Z - zmin) / 3);
                var key = (p.i, p.j, p.k);
                if (!gridDict.ContainsKey(key))
                    gridDict[key] = new List<Point>();
                gridDict[key].Add(p);
            }
            if (gridDict.ContainsKey((0, 0, 0)))
            { 
                index000 = gridDict[(0, 0, 0)]; 
            }
            output += $"14,格网(0,0,0)内的点个数,{index000.Count}\n";
            output += $"15,点P1的网格索引(i,j,k)中i分量,{points.Find(a=>a.Id==1).i}\n";
            output += $"16,点P6的网格索引(i,j,k)中j分量,{points.Find(a => a.Id == 6).j}\n";
            // 为每个点寻找候选点（27个相邻网格）
            foreach (var p in points)
            {
                var candidatePoints = new List<Point>();
                for (int di = -1; di <= 1; di++)
                {
                    for (int dj = -1; dj <= 1; dj++)
                    {
                        for (int dk = -1; dk <= 1; dk++)
                        {
                            var key = (p.i + di, p.j + dj, p.k + dk);
                            if (gridDict.ContainsKey(key))
                                candidatePoints.AddRange(gridDict[key]);
                        }
                    }
                }
                // 计算距离并排序，取前6个（不包括自身）
                candidatePoints.Sort((a, b) =>
                {
                    double da = CalDis(a, p);
                    double db = CalDis(b, p);
                    return da.CompareTo(db);
                });

                // 移除自身（第一个），取接下来6个
                candidatePoints.RemoveAll(pt => pt.Id == p.Id);
                p.neighbors = candidatePoints.Take(6).ToList();

                if (p.Id == 1)
                {
                    output += $"17,点P1的候选点总数,{candidatePoints.Count}\n";
                    output += $"19,点P1的6个邻近点序号最大值,{p.neighbors.Max(n => n.Id)}\n";
                }
                if (p.Id == 6)
                {
                    output += $"18,点P6的候选点总数,{candidatePoints.Count}\n";
                    output += $"20,点P6的6个邻近点序号最大值,{p.neighbors.Max(n => n.Id)}\n";
                }
            }
            //Dictionary<(Point,Point),double> p2D = new Dictionary<(Point,Point),double>();
            //int limit = 0;
            //foreach (Point p in points)
            //{
            //    int i = p.i;
            //    int j = p.j;
            //    int k = p.k;
            //    List<Point> tmp = new List<Point>();
            //    foreach (Point p2 in points)
            //    {
            //        int delti = Math.Abs(p2.i - i);
            //        int deltj = Math.Abs(p2.j - j);
            //        int deltk = Math.Abs(p2.k - k);
            //        if (Math.Sqrt(Math.Pow(delti,2)+ Math.Pow(deltj, 2)+Math.Pow(deltk, 2))< 6)
            //        {
            //            tmp.Add(p2);
            //        }
            //    }
            //    tmp.Sort((p1,p2)=> 
            //    {
            //        double dis1 = CalDis(p1, p);
            //        double dis2 = CalDis(p2, p);
            //        return dis1.CompareTo(dis2);
            //    });
            //    tmp.Remove(tmp.First());
            //    for (int m = 0; m < 6; m++)
            //    {
            //        p.prev.Add(tmp[m]);
            //    }
            //    if (p.Id == 1)
            //    {
            //        output += $"17,点P1的候选点总数,{tmp.Count}\n";
            //    }
            //    if (p.Id == 6)
            //    {
            //        output += $"18,点P6的候选点总数,{tmp.Count}\n";
            //        output += $"19,点P1的6个邻近点序号最大值,{points.Find(a=>a.Id==1).prev.Find(a=>a.Id== points.Find(b => b.Id == 1).prev.Max(b=>b.Id)).Id}\n";
            //        output += $"20,点P6的6个邻近点序号最大值,{p.prev.Find(a=>a.Id==p.prev.Max(b=>b.Id)).Id}\n";
            //    }
            //    limit++;//程序运行时间过长崩溃,手动添加跳出循环的limit
            //    if (limit > 500) break;
            //}//比赛时理解错误的编译
        }
        private static double CalDis(Point p1,Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2));
        }//计算两点间的距离
        public void cal_21_29()
        {
            //    double allmiu = 0.0;
            //    double allsigma = 0.0;
            //    foreach (Point p in points)
            //    {
            //        List<Point> around = p.prev;
            //        double dis = 0.0;
            //        for (int i = 0; i < around.Count; i++)
            //        {
            //           dis += CalDis(p, around[i]);
            //        }
            //        p.miu = dis / around.Count;

            //        double sum = 0.0;
            //        for (int i = 0; i < around.Count; i++)
            //        {
            //            double disij = CalDis(p, around[i]);
            //            sum += Math.Pow(disij - p.miu, 2);
            //        }
            //        p.sigma = Math.Sqrt(sum/around.Count);
            //        if (p.Id == 1) {output += $"21,点P1的邻域平均距离u1,{p.miu:f3}\n"; output += $"22,点P1的邻域距离标准差σ1,{p.sigma:f3}\n"; }
            //        if (p.Id == 6) {output += $"23,点P6的邻域平均距离u1,{p.miu:f3}\n"; output += $"24,点P6的邻域距离标准差σ1,{p.sigma:f3}\n"; }
            //        allmiu += p.miu;
            //        allsigma += p.sigma;    
            //    }
            //    miuAver = allmiu / number;
            //    sigmaAver = allsigma / number;
            //    output += $"25,全局平均距离均值μ,{miuAver:f3}\n";
            //    output += $"26,全局距离标准差σ,{sigmaAver:f3}\n";
            //    double judge = miuAver + 2 * sigmaAver;
            //    foreach (Point p in points)
            //    {
            //        if (p.miu > judge)
            //        {
            //            p.noisy = true;
            //        }
            //        else 
            //        {
            //            p.noisy = false;
            //        }
            //    }
            //    output += $"27,点P1是否为噪声点,{points.Find(a=>a.Id==1).noisy}\n";
            //    output += $"28,点P6是否为噪声点,{points.Find(a=>a.Id==6).noisy}\n";
            //    List<Point> bag = points.FindAll(p => p.noisy == false);
            //    output += $"29,去噪后保留的点云总数,{bag.Count}\n";//比赛中编译
            double sumMiu = 0;
            double sumSigma = 0;

            foreach (var p in points)
            {
                if (p.neighbors.Count == 0) continue;

                double sumDist = 0;
                foreach (var n in p.neighbors)
                    sumDist += CalDis(p, n);
                p.miu = sumDist / p.neighbors.Count;

                double sumVar = 0;
                foreach (var n in p.neighbors)
                {
                    double dist = CalDis(p, n);
                    sumVar += Math.Pow(dist - p.miu, 2);
                }
                p.sigma = Math.Sqrt(sumVar / p.neighbors.Count);

                sumMiu += p.miu;
                sumSigma += p.sigma;

                if (p.Id == 1)
                {
                    output += $"21,点P1的邻域平均距离u1,{p.miu:f3}\n";
                    output += $"22,点P1的邻域距离标准差σ1,{p.sigma:f3}\n";
                }
                if (p.Id == 6)
                {
                    output += $"23,点P6的邻域平均距离u6,{p.miu:f3}\n";
                    output += $"24,点P6的邻域距离标准差σ6,{p.sigma:f3}\n";
                }
            }

            miuAver = sumMiu / points.Count;
            sigmaAver = sumSigma / points.Count; // 注意：这里是所有点σ的平均值，而非全局σ
            output += $"25,全局平均距离均值μ,{miuAver:f3}\n";
            output += $"26,全局距离标准差σ,{sigmaAver:f3}\n";

            double threshold = miuAver + 2 * sigmaAver;
            int noiseCount = 0;

            foreach (var p in points)
            {
                p.noisy = p.miu > threshold;
                if (p.noisy) noiseCount++;
            }

            output += $"27,点P1是否为噪声点,{(points[0].noisy ? 1 : 0)}\n";
            output += $"28,点P6是否为噪声点,{(points[5].noisy ? 1 : 0)}\n";
            output += $"29,去噪后保留的点云总数,{points.Count - noiseCount}\n";

        }//完成程序正确性21-29
    }
}
