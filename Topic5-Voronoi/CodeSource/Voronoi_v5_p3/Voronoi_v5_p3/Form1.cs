using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Imaging;

namespace Voronoi_v5_p3//2025.7.22  9：20--11:00
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<Point> points = new List<Point>();
        List<Triangle> T = new List<Triangle>();
        Dictionary<Point,List<Point>> V = new Dictionary<Point,List<Point>>();
        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter="文本文件|*.txt",Title="TEXT"};
            if (op.ShowDialog() == DialogResult.OK)
            { 
                points = Data.LoadPoints(op.FileName);
                richTextBox1.Text = File.ReadAllText(op.FileName);
                toolStripStatusLabel1.Text = "数据导入成功";
            }
        }
        private void 计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("请先导入数据!", "提示");
            }
            else 
            {
                T = Cal.CalT(points);
                DrawTri(T);
                V = Cal.CalV(T);
                DrawVor(V);
                toolStripStatusLabel1.Text = "计算完成";
            }
        }
        double minX, minY, maxX, maxY;
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chart1 != null)
            {
                SaveFileDialog sv = new SaveFileDialog { Filter = "PNG文件|*.png", Title = "PNG" };
                if (sv.ShowDialog() == DialogResult.OK)
                {
                    this.WindowState = FormWindowState.Maximized;
                    ImageFormat format = ImageFormat.Png;
                    chart2.SaveImage(sv.FileName,format);

                    MessageBox.Show($"图片已保存至{sv.FileName}","提示");
                    toolStripStatusLabel1.Text = $"{sv.FileName}保存成功";
                }
            }
            else 
            {
                MessageBox.Show("请先导入数据进行计算!","提示");
            }
        }
        private void DrawTri(List<Triangle> Tri)
        {
            chart1.Series.Clear();
            minX = chart1.ChartAreas[0].AxisX.Minimum = points.Min(a => a.X) - 5;
            maxX = chart1.ChartAreas[0].AxisX.Maximum = points.Max(a => a.X) + 5;
            minY = chart1.ChartAreas[0].AxisY.Minimum = points.Min(a => a.Y) - 5;
            maxY = chart1.ChartAreas[0].AxisY.Maximum = points.Max(a => a.Y) + 5;
            List<Triangle> ST = new List<Triangle>(Tri);
            foreach (Triangle tri in Tri)
            { 
                List<Point> tp = new List<Point>();
                tp.Add(tri.A);
                tp.Add(tri.B);
                tp.Add(tri.C);
                foreach (Point p in tp)
                {
                    if (p.Id == "100" || p.Id == "111" || p.Id == "110" || p.Id == "101")
                    {
                        ST.Remove(tri);
                    }
                }
            }
            foreach (Triangle t in ST)
            {
                Series st = new Series();
                st.IsVisibleInLegend = false;
                st.ChartType = SeriesChartType.Line;
                st.Color = Color.Gray;
                st.BorderWidth = 1;
                st.Points.AddXY(t.A.X, t.A.Y); st.Points.AddXY(t.B.X, t.B.Y);
                st.Points.AddXY(t.C.X, t.C.Y); st.Points.AddXY(t.A.X, t.A.Y);
                chart1.Series.Add(st);

                Series sp = new Series();
                sp.IsVisibleInLegend = false;
                sp.ChartType = SeriesChartType.Point;
                sp.Color = Color.Red;
                sp.BorderWidth = 2;
                sp.Points.AddXY(t.A.X, t.A.Y);
                sp.Points.AddXY(t.B.X, t.B.Y);
                sp.Points.AddXY(t.C.X, t.C.Y); 
                chart1.Series.Add(sp);
            }
        }
        private void DrawVor(Dictionary<Point, List<Point>> Vor)
        {
            chart2.Series.Clear();
            chart2.ChartAreas[0].AxisX.Minimum = minX;
            chart2.ChartAreas[0].AxisX.Maximum = maxX;
            chart2.ChartAreas[0].AxisY.Minimum = minY;
            chart2.ChartAreas[0].AxisY.Maximum = maxY;
            foreach (var kvp in Vor)
            {
                Point site = kvp.Key;
                List<Point> centers = kvp.Value;

                Series ss = new Series();
                ss.IsVisibleInLegend = false;
                ss.ChartType = SeriesChartType.Point;
                ss.Color = Color.Red;
                ss.BorderWidth = 2;
                ss.Label = site.Id;
                ss.Points.AddXY(site.X,site.Y);
                chart2.Series.Add(ss);

                Series sv = new Series();
                sv.IsVisibleInLegend = false;
                sv.ChartType = SeriesChartType.Line;
                sv.Color = Color.Blue;
                sv.BorderWidth = 1;
                foreach (var p in centers)
                {
                    sv.Points.AddXY(p.X, p.Y);
                }
                chart2.Series.Add(sv);
            }
        }
    }
}
