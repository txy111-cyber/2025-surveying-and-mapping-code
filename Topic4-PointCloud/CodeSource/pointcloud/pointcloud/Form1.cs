using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace pointcloud
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<Point> input = new List<Point>();
        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter="文本文件|*.txt",Title="TEXT"};
            if (op.ShowDialog() == DialogResult.OK)
            {
                input = Data.LoadData(op.FileName);
                richTextBox1.Text = File.ReadAllText(op.FileName);
                toolStripStatusLabel1.Text = $"{op.FileName}导入成功";
            }
        }

        private void 计算结果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (input.Count > 3)
            {
                var result = new Cal(input);
                result.cal_1_20();
                result.cal_21_29();
                richTextBox2.Text = result.output;
                toolStripStatusLabel1.Text = "计算完成,可查看并保存结果";
            }
            else 
            {
                MessageBox.Show("请先打开正确的数据文件!","提示");
            }
        }

        private void 保存文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sv = new SaveFileDialog { Filter="文本文件|*.txt",Title="TEXT"};
            if (sv.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sv.FileName, richTextBox2.Text);
                MessageBox.Show($"结果已保存至{sv.FileName}","提示");
                toolStripStatusLabel1.Text = $"计算结果已保存:{sv.FileName}";
            }
        }
    }
}
