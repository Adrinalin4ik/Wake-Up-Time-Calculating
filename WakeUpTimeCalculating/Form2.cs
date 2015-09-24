using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace WakeUpTimeCalculating
{
    public partial class Form2 : Form
    {
        int i = 0;
        int initial=0;
        public Form2()
        {
            InitializeComponent();

            //Opacity = 0.5;
            
        }
        Form1 f1 = new Form1();
        private void Form2_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            this.Visible = false;
            OpenSoftTimer.Enabled = true;
            
        }

        


        private void softPanelOpen()
        {
            
            
            //
            i++;
            if (this.Width < 260)
            {
                //this.Size = new Size(this.Size.Width + i, this.Size.Height);
                //this.Location = new Point((f1.Location.X + f1.Size.Width)-i-10, f1.Location.Y);
                this.Width += 8;
                this.Left -= 17;
                Opacity+=0.06;
            }
        }

        private void OpenSoftTimer_Tick(object sender, EventArgs e)
        {
            
            initial++;
            //Opacity = 0.5;
            if (initial > 20)
            {
                //Opacity = 0.5;
                softPanelOpen();
                OpenSoftTimer.Interval++;
            }
            else
            {
                this.Location = new Point(f1.Location.X + f1.Size.Width, f1.Location.Y);
                this.Size = new Size(0,531);
            }
        }

    }
}
