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

    
    public partial class Form3 : Form
    {

        


  
        int TimeSec;
        int speed = 100;
        bool scene1,scene2;
        public Form3()
        {
            
            TimeSec = 0;
            InitializeComponent();

            OpasitySoft();
            timer1.Start();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            //pictureBox1.Load("Alarm.jpg");
            pictureBox1.Load("Alarm.jpg");
            scene1 = true;
            this.Size = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            this.FormBorderStyle = FormBorderStyle.None;
            this.AllowTransparency = true;
            this.BackColor = Color.AliceBlue;//цвет фона  
            this.TransparencyKey = this.BackColor;//он же будет заменен на прозрачный цвет

            pictureBox1.Location = new Point(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width/2, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height/2);


            Form1 form1 = new Form1();
            form1.ShowDialog();
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (TimeSec <= 4 && scene1)
            {
                //this.Size = new Size(this.Size.Width + speed, this.Size.Height + speed);
                pictureBox1.Size = new Size(pictureBox1.Size.Width + speed, pictureBox1.Size.Height + speed);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Location = new Point(pictureBox1.Location.X - speed / 2, pictureBox1.Location.Y - speed / 2);
                speed += 6;

                scene2 = true;
                //TimeSec = 0;
                // Opacity -= 0.08d;


            }
            else
            {
                
                if (scene1)
                {
                    //OpasitySoft();
                    scene1 = false;
                    TimeSec = 0;
                    
                    this.Hide();
                    
                }
                if (TimeSec <= 0 && scene2 && !scene1)
                {
                    //pictureBox2.Size = new Size(this.Size.Width,this.Size.Height);
                    //pictureBox2.Location = new Point(0,50);

                    //pictureBox2.Image = Image.FromFile("Rgif.gif");
                }


            

            
            }
        }

        private void TimerSec_Tick(object sender, EventArgs e)
        {
            TimeSec++;
        }


        private void OpasitySoft()
        {
            Opacity = 0;
            Timer timer = new Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                if ((Opacity += 0.02d) == 0.1) timer.Stop();
            });
            timer.Interval = 2;
            timer.Start();
        }
    }

}
