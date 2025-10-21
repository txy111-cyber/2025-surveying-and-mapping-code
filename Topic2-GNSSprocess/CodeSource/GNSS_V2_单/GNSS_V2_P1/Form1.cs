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


namespace GNSS_V2_P1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<obs> obsS=new List<obs>();
        List<SateToObs> STOa = new List<SateToObs>();
        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter="文本文件|*.txt",Title="TEXT"};
            if (op.ShowDialog() == DialogResult.OK)
            { 
                obsS = Data.LoadObs(op.FileName);
                richTextBox1.Text =File.ReadAllText (op.FileName);
                toolStripStatusLabel1.Text = "数据已导入";
            }
        }

        private void 计算结果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            STOa = Cal.CalSTOa(obsS);//按照卫星分组.
            foreach (var sat in STOa)
            {
                List<ProArc> arcs = CSDete.DetectCycleSlipsWithGF(sat);
                // 处理检测结果
                richTextBox2.Text += $"Sat: {sat.sat_id}\n";
                foreach (var arc in arcs)
                {
                    //周跳
                    richTextBox2.Text += $"{arc.StartTime} | " + $"Epochs: {arc.Epochs.Count} \nCycleSlip: {arc.HasCycleSlip}\n";
                    // 输出多路径误差
                    richTextBox2.Text += "Multipath Errors:";
                    foreach (var mp in arc.MultipathErrors)
                    {
                        richTextBox2.Text += ($"{mp:F3}m\n");
                    }
                    // 输出平滑后的伪距
                    richTextBox2.Text += ("Smoothed Pseudoranges:");
                    foreach (var sp in arc.SmoothedPseudoranges)
                    {
                        richTextBox2.Text += ($"{sp:F3}m\n");
                    }
                }
                richTextBox2.Text += "\n";
            }
        }

        private void 保存文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog  sv=new SaveFileDialog {Filter= "文本文件|*.txt",Title="TEXT"};
            if (sv.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sv.FileName, richTextBox2.Text);
                toolStripStatusLabel1.Text = "数据已保存";
                MessageBox.Show("保存成功！");
            }
        }
    }
}
