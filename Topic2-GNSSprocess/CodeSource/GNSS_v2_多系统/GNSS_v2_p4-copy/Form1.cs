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

namespace GNSS_v2_p4_copy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<obs> inputs = new List<obs>();
        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter="文本文件|*.txt",Title="TEXT"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Data.LoadObs(ofd.FileName,inputs);
                richTextBox1.Text = File.ReadAllText(ofd.FileName);
                toolStripStatusLabel1.Text = $"文件:{ofd.FileName}已导入";
            }
        }
        private void 计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = new GnssProcessor(inputs);
            result.DetectCS();
            var sat_groups = inputs.GroupBy(a => a.sat_Id);
            foreach (var sat in sat_groups)
            {
                string name = sat.Key;
                List<obs> obss = sat.ToList();
                richTextBox2.Text += $"卫星:{name}\n";
                richTextBox3.Text += $"卫星:{name}\n";
                richTextBox4.Text += $"卫星:{name}\n";
                foreach (obs o in obss)
                {
                    richTextBox2.Text += $"历元:{o.Epoch},周跳:{o.HasCS1}\n";
                    richTextBox3.Text += $"历元:{o.Epoch},周跳:{o.HasCS12}\n";
                    richTextBox4.Text += $"历元:{o.Epoch},周跳:{o.HasCS123}\n";
                }
                richTextBox2.Text += "\n";
                richTextBox3.Text += "\n";
                richTextBox4.Text += "\n";
            }
            var cal_MP = result.MultiPathError();
            foreach (var sates in cal_MP)
            { 
                string id = sates.Key;
                var epochs = sates.Value;
                richTextBox5.Text += $"{id}\n";
                foreach(var epoch in epochs)
                {
                    int time = epoch.Key;
                    var mps = epoch.Value;
                    richTextBox5.Text += $"历元:{time}    ";
                    foreach (var mp in mps)
                    {
                        richTextBox5.Text += $"{mp.Key}:{mp.Value}  ";
                    }
                    richTextBox5.Text += "\n";
                }
                richTextBox5.Text += "\n";
            }
            var cal_Ha = result.HatchSmooth();
            foreach (var sates in cal_Ha)
            {
                string id = sates.Key;
                var epochs = sates.Value;
                richTextBox6.Text += $"{id}\n";
                foreach (var epoch in epochs)
                { 
                    int time = epoch.Key;
                    var hatchs = epoch.Value;
                    richTextBox6.Text += $"历元:{time} ";
                    foreach (var hatch in hatchs)
                    {
                        richTextBox6.Text += $"{hatch.Key}:{hatch.Value} ";
                    }
                    richTextBox6.Text += "\n";
                }
                richTextBox6.Text += "\n";
            }
            toolStripStatusLabel1.Text = "计算完成";
        }
    }
}
