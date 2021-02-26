using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class TestLD : Form
    {
        modbusMaster master;

        public TestLD()
        {
            InitializeComponent();
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            string port = comboBox1.Text;
            int tensNum;
            if (checkBox1.Checked && checkBox2.Checked)
            {
                tensNum = 3;
            }
            else if (checkBox1.Checked)
            {
                tensNum = 1;
            }
            else if (checkBox2.Checked)
            {
                tensNum = 2;
            }
            else
                tensNum = 0;

            for (int j = 0; j < 1; j++)
            {
                master.setParameters(port, tensNum);
                textBox4.Text = "Проверка связи с регистратором";
                textBox1.Text = " ";
                textBox2.Text = " ";
                textBox3.Text = " ";
                button1.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                if (master.ReadyToMeasure())
                {
                    textBox4.Text = master.errorText;
                    master.takeAMeasure();
                    button1.Enabled = true;
                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                    textBox4.Text = master.errorText;
                    if (!master.error)
                    {
                        textBox1.Text = master.holding_register[2].ToString();
                        textBox2.Text = master.holding_register[3].ToString();
                        textBox3.Text = master.holding_register[1].ToString();
                    }

                }
                else
                {
                    textBox4.Text = master.errorText;
                    button1.Enabled = true;
                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                }
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 1; i <= 20; i++)
                this.comboBox1.Items.Add("COM" + i.ToString());
            comboBox1.SelectedIndex = 0;
            textBox4.Text = "Ошибок нет";
            checkBox1.Checked = true;
            master = new modbusMaster(1);
        }
    }
}
