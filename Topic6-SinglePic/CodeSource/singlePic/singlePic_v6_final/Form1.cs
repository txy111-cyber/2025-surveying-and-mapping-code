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
using System.Security;

namespace singlePic_v6_final
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Photo photo = new Photo();
        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter="文本文件|*.txt",Title="TEXT"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                photo = Data.LoadData(ofd.FileName);
                richTextBox1.Text = File.ReadAllText(ofd.FileName);
                toolStripStatusLabel1.Text = $"{ofd.FileName}导入成功";
            }
        }
        private static double[,] Trans(double[,] A)
        {
            int row = A.GetLength(0);//行
            int col = A.GetLength(1);//列
            double[,] result = new double[col,row];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    result[j,i] = A[i,j];
                }
            }
            return result;
        }
        private static double[,] Multiply(double[,] left, double[,] right)
        {
            int row = left.GetLength(0);
            int col = right.GetLength(1);
            int same = left.GetLength(1);
            double[,]result = new double[row,col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    for (int k = 0; k < same; k++)
                    {
                        result[i, j] += left[i,k] * right[k,j]; 
                    }
                }
            }
            return result;
        }
        private static double[] Inverse(double[,] A, double[,] L)
        {
            int row = A.GetLength(1);
            int col = L.GetLength(1);
            double[] result = new double[row];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                int rowIndex = i;
                double Max = Math.Abs(A[i,i]);
                for (int j = i; j < A.GetLength(0); j++)
                {
                    if (Math.Abs(A[j,i])>Max)
                    {
                        rowIndex = j;
                        Max = Math.Abs(A[j,i]);
                    }
                }//寻找主元行
                for (int j = i; j < A.GetLength(1); j++)
                {
                    double temp = A[rowIndex,j];
                    A[rowIndex, j] = A[i,j];
                    A[i,j] = temp;
                }//交换行
                double tempL = L[rowIndex, 0];
                L[rowIndex, 0] = L[i,0];
                L[i,0] = tempL;
                for (int j = i+1; j<A.GetLength(0); j++)
                {
                    double factor = A[j,i] / A[i,i];
                    for (int k = i;k<A.GetLength(1); k++)
                    {
                        A[j,k] -= factor*A[i,k];
                    }
                    L[j,0] -= factor*L[i,0];
                }//消元
            }
            for (int i = A.GetLength(0)-1; i >=0 ; i--)
            {
                double sum = 0.0;
                for (int j = i+1;j<A.GetLength(0); j++)
                {
                    sum += A[i,j] * result[j];
                }
                result[i] = (L[i, 0] - sum) / A[i, i];
            }
            return result;
        }
        private void 计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = 0;
            do
            {
                Cal.CalR(photo);
                Cal.CalJS(photo);
                Cal.CalA(photo);
                Cal.CalL(photo);

                double[,] At = Trans(photo.A);

                double[,] right = Multiply(At, photo.L);

                double[,] left = Multiply(At, photo.A);

                double[] adjust = Inverse(left, right);

                photo.Xs0 += adjust[0];
                photo.Ys0 += adjust[1];
                photo.Zs0 += adjust[2];
                photo.phi += adjust[3];
                photo.ome += adjust[4];
                photo.kap += adjust[5];

                richTextBox2.Text += $"第{i + 1}次迭代" + "\n";
                richTextBox2.Text += "改正数为\n";
                richTextBox2.Text += $"{adjust[0]:f4}" + "\n";
                richTextBox2.Text += $"{adjust[1]:f4}" + "\n";
                richTextBox2.Text += $"{adjust[2]:f4}" + "\n";
                richTextBox2.Text += $"{adjust[3]:f4}" + "\n";
                richTextBox2.Text += $"{adjust[4]:f4}" + "\n";
                richTextBox2.Text += $"{adjust[5]:f4}" + "\n";
                richTextBox2.Text += "解算得到外方位元素各值为:\n";
                richTextBox2.Text += $"{photo.Xs0:f4}" + "\n";
                richTextBox2.Text += $"{photo.Ys0:f4}" + "\n";
                richTextBox2.Text += $"{photo.Zs0:f4}" + "\n";
                richTextBox2.Text += $"{photo.phi:f4}" + "\n";
                richTextBox2.Text += $"{photo.ome:f4}" + "\n";
                richTextBox2.Text += $"{photo.kap:f4}" + "\n";

                i++;
                if (adjust[0] < 1e-5 || adjust[1] < 1e-5 || adjust[2] < 1e-5 || adjust[3] < 1e-5 || adjust[4] < 1e-5 || adjust[5] < 1e-5)
                    break;
            } while (true);
        }
    }
}
