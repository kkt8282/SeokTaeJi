using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
        private void button2_Click_1(object sender, EventArgs e)

        {
            if (Username.Text == "석태지" && Password.Text == "1234")
            {
                this.DialogResult = DialogResult.OK;

            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                MessageBox.Show("로그인에 실패했습니다.");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        //private void Form3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Keys   == Key.Enter)
        //    {
        //        if (Username.Text == "석태지" && Password.Text == "1234")
        //        {
        //            this.DialogResult = DialogResult.OK;

        //        }
        //        else
        //        {
        //            this.DialogResult = DialogResult.Cancel;
        //            MessageBox.Show("로그인에 실패했습니다.");
        //        }
        //    }
        //}

        private void Username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2_Click_1(sender, e);
            }
        }
    }
}
