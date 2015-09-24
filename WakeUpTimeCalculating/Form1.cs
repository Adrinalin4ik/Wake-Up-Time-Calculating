using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Net.Mail;
using System.Net;


namespace WakeUpTimeCalculating
{
    public partial class Form1 : Form
    {
        public int WM_SYSCOMMAND = 0x0112;
        public int SC_MONITORPOWER = 0xF170;
        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);
        


         
        int ShowHours = 8;
        int Hours = 0;
        int Minutes = 0;
        int Seconds = 0;

        //a =85;
        //b = 120;
        string buf = null;// переменная для промежуточного сохранения данных листбокса

        int[] AH=new int[9]; // массивы чисел для времени Alarm
        int[] AM=new int[9];

   
        int item; // переменная хранящяю номер выделенного элемента в лист бокс
        int effectiveness = 0;


        int HoursUp = 0; //переменная для удлинения шкалы( перехода чиселна еденицу по достяжению конца графика)
        int Phase = 1; // переменная для фаз сна
        int timer=0;
        int timerMin = 0;
        int timerHour = 0;
        DateTime DT = DateTime.Now; // инициализация времени



        private System.Media.SoundPlayer player = new System.Media.SoundPlayer();
        private bool PlayingNow = false;


        bool Start;
        bool activatedAlarm;


        int MXold, MYold;
        bool Windowmoving;

        const int DISTANCE = 10;



        public Form1()
        {
            InitializeComponent();
            Start = false;
            PlayingNow = false;
            OpasitySoft();
            Windowmoving = false;
            activatedAlarm = false;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            groupBox4.AllowDrop = true;
            StreamReader sr = new StreamReader("SettingPhase.txt");
            numericUpDown1.Value = Convert.ToInt32(sr.ReadLine());
            sr.Close();
            ParentChange(pictureBox2);
            label7.Text = "Starting";
            updateDT();
            pictureBox2.Load(ParseFile("SettingUpload.xml", "/Setting/AppConfig/BackgroundImage"));
           
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            listBox1.Visible = false;
            //label15.Text = Convert.ToString(Phase = 0);

            SettingStart();


            toolTip1.SetToolTip(button13, "Затемнение экрана на время сна");
            toolTip1.SetToolTip(button12, "Изменение фонового изображения");
            toolTip1.SetToolTip(button16, "Будильник");
            toolTip1.SetToolTip(listBox1, "Нажмите дважды на время чтобы завести будильник");
            toolTip1.SetToolTip(button1, "Рассчитать оптимальное время сна , для бодрого пробуждения");

            toolTip1.SetToolTip(pictureBox11, "Короткий сон: наилучшее время для подъема");
            toolTip1.SetToolTip(pictureBox12, "Короткий сон: наилучшее время для подъема");
            toolTip1.SetToolTip(pictureBox13, "Короткий сон: наилучшее время для подъема");
            toolTip1.SetToolTip(pictureBox14, "Короткий сон: наилучшее время для подъема");
            toolTip1.SetToolTip(pictureBox15, "Короткий сон: наилучшее время для подъема");
            toolTip1.SetToolTip(pictureBox3, "Затемнение экрана");
            toolTip1.SetToolTip(label6, "Глобальное время");
            toolTip1.SetToolTip(label7, "Глобальное время");
        }
        public void updateDT()
    {
         label10.Text = Convert.ToString(DT.TimeOfDay);
    }

        private void ListBoxActive()
        {
            listBox1.Visible = true;
            panel141.Visible = true;
            panel142.Visible = true;
            panel143.Visible = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {


            ListBoxActive();

            effectiveness = 0;
            listBox1.Items.Clear();
            //label7.Text = Convert.ToString(DT);
            if (checkBox1.Checked)
            {
                currentTimeSolve();
                Graph();
                SleepTimer.Enabled = true;
                label5.Text = "Засыпает";
                label18.Text = "Бодрствование";
                Start = true;
            }
            else TimeSolve();
            
        }

        private void Time_Tick(object sender, EventArgs e)
        {
            DateTime DT1 = DateTime.Now;
            string MinUp = "";
            string SecUp = "";

            label7.Text = Convert.ToString(DT1.TimeOfDay);
            
            if (Start)
            {
                if (DT1.Minute < 10) MinUp = "0"; else MinUp = "";
                if (DT1.Second < 10) SecUp = "0"; else SecUp = "";
                label2.Text = Convert.ToString(DT1.Hour) + ":" + MinUp + Convert.ToString(DT1.Minute) + ":" + SecUp + Convert.ToString(DT1.Second);

                timer++;

                if (timer < 60)
                {
                    label28.Text = Convert.ToString(timer + " сек");
                 }
                else
                {
                    
                    timer = 0;
                    if (timerMin < 60)
                    {
                        timerMin++;
                        label29.Text = Convert.ToString(timerMin + " мин");
                    }
                    if (timerMin == 60)
                    {
                        timerMin = 0;
                        timerHour++;
                        label33.Text = Convert.ToString(timerHour +" ч ");
                    }

                }

            }
            if (activatedAlarm)
            {
                Alarm();
            }
        }

        public void TimeSolve()
        {
            string MUp = "";
            string HUp = "";

           listBox1.Items.Add("                  Ч | М |       | Эффективность");
           for (int i = 1; i <= ShowHours; i++)
            {
                int sr = Convert.ToInt32(numericUpDown1.Value*i);

                if (DT.AddMinutes(90 * i + Hours * 60 + Minutes).Minute < 10) MUp = "0"; else MUp = "";
                if (DT.AddMinutes(90 * i + Hours * 60 + Minutes).Hour < 10) HUp = "0"; else HUp = "";

                AH[i] = DT.AddMinutes(90 * i + Hours * 60 + Minutes).Hour;
                AM[i] = DT.AddMinutes(90 * i + Hours * 60 + Minutes).Minute;
                listBox1.Items.Add("Время " + i + " |  " + HUp + DT.AddMinutes(90 * i + Hours * 60 + Minutes).Hour + ":" + MUp + DT.AddMinutes(90 * i + Hours * 60 + Minutes).Minute);   
            } 
        }

        public void Alarm()
        {
            DateTime DT1 = DateTime.Now;

            TestLabel.Text = Convert.ToString(AM[item]);
            if (DT1.Hour == AH[item] && DT1.Minute==AM[item] )
            {
                TestLabel.Text = "true";
                if (!PlayingNow)
                {
                    display(-1);
                    playSound("Alarm.wav");
                    PlayingNow = true;
                    SendMail("smtp.list.ru", "soild@list.ru", "Hi73s6dL", "tolkova1992@mail.ru", "Будильник", "Будильник прозвенел в " + label7.Text);
          

                    DialogResult dialogResult = MessageBox.Show(label7.Text + "\n" + "Отложить на 10 мин ?", "Будильник", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        display(2);
                        WakeUpRepeatTimer.Enabled = true;
                        player.Stop();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        WakeUpRepeatTimer.Enabled = false;
                        player.Stop();
                        activatedAlarm = false;
                        //PlayingNow = false;
                    }
                }
                

                
            }
            
        }
        public void currentTimeSolve()
        {
            DateTime DT1 = DateTime.Now;
            string MUp = "";
            string HUp = "";

            Graph();
            listBox1.Items.Add("                  Ч | М |        | Эффективность");
            for (int i = 1; i <= ShowHours; i++)
            {
                int sr = Convert.ToInt32(numericUpDown1.Value*i);

               // listBox1.Items.Add("Время "+i+" |  " + DT.AddMinutes(sr).TimeOfDay);

             //   if (DT1.AddMinutes(90 * i).Minute < 10) MUp = "0"; else MUp = "";
             //   if (DT1.AddMinutes(90 * i).Hour < 10) HUp = "0"; else HUp = "";

             //       AH[i] = DT1.AddMinutes(90 * i ).Hour;
             //       AM[i] = DT1.AddMinutes(90 * i ).Minute;


            //        listBox1.Items.Add("Время " + i + " |  " + HUp + DT1.AddMinutes(90 * i).TimeOfDay.Hours +":"+ MUp +DT1.AddMinutes(90 * i).TimeOfDay.Minutes + "     |       " + (effectiveness));
                if (DT1.AddMinutes(sr).Minute < 10) MUp = "0"; else MUp = "";
                if (DT1.AddMinutes(sr).Hour < 10) HUp = "0"; else HUp = "";

                AH[i] = DT1.AddMinutes(sr).Hour;
                AM[i] = DT1.AddMinutes(sr).Minute;


                listBox1.Items.Add("Время " + i + " |  " + HUp + DT1.AddMinutes(sr).TimeOfDay.Hours + ":" + MUp + DT1.AddMinutes(sr).TimeOfDay.Minutes + "     |       " + (effectiveness));
         
            
            }  
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Hours++;
            label10.Text = Convert.ToString(DT.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).TimeOfDay);
           

        }

        private void button7_Click(object sender, EventArgs e)
        {
            Hours--;
            label10.Text = Convert.ToString(DT.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).TimeOfDay);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Minutes++;
            label10.Text = Convert.ToString(DT.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).TimeOfDay);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Minutes--;
            label10.Text = Convert.ToString(DT.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).TimeOfDay);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Seconds++;
            label10.Text = Convert.ToString(DT.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).TimeOfDay);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Seconds--;
            label10.Text = Convert.ToString(DT.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).TimeOfDay);
        }


       
        public void Graph()
        {
            string MinUp = "";

            if (DT.Minute < 10) MinUp = "0"; else MinUp = "";


            label20.Text = Convert.ToString(DT.AddHours(1 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);

            label21.Text = Convert.ToString(DT.AddHours(2 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);

            label22.Text = Convert.ToString(DT.AddHours(3 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);

            label23.Text = Convert.ToString(DT.AddHours(4 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);

            label24.Text = Convert.ToString(DT.AddHours(5 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);

            label25.Text = Convert.ToString(DT.AddHours(6 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);

            label26.Text = Convert.ToString(DT.AddHours(7 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);

            label27.Text = Convert.ToString(DT.AddHours(8 + HoursUp).Hour) + ":" + MinUp + Convert.ToString(DT.Minute);
        }

        private void SleepTimer_Tick(object sender, EventArgs e)
        {
            panel10.Location = new Point(panel10.Location.X+1,panel10.Location.Y);
           
            label2.Location = new Point(label2.Location.X + 1,label2.Location.Y);
            // переход на предыдущую фазу


            if (panel10.Location.X == 952)
            {

                HoursUp++;
                panel10.Location = new Point(782, panel10.Location.Y);
                label2.Location = new Point(792, label2.Location.Y);
                Graph();
            }

            ///
            if (panel10.Location.X == 137)
            {
                panel10.BackColor = Color.Black;
                label18.Text = "Бодрствование";
                //
                label15.Text = Convert.ToString(Phase);
                Phase++;
                
            }
            ///
            if (panel10.Location.X == 144)
            {
                panel10.BackColor = Color.Lime;
                label18.Text = "Первая стадия";
            }

            if (panel10.Location.X == 155)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
                label5.Text = "Глубокий сон";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 189)
            {
                panel10.BackColor = Color.Gold;
                label18.Text = "Третья стадия";
            }

            if (panel10.Location.X == 206)
            {
                panel10.BackColor = Color.Red;
                label18.Text = "Четвертая стадия";
                label17.Text = "очень тяжело";
            }

            if (panel10.Location.X == 255)
            {
                panel10.BackColor = Color.Gold;
                label18.Text = "Третья стадия";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 272)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
            }

            if (panel10.Location.X == 306)
            {
                panel10.BackColor = Color.DeepSkyBlue;
                label5.Text = "Видит сон";
                label18.Text = "Быстрый сон";
                label15.Text = Convert.ToString(Phase++);
                label17.Text = "легко";
            }
            //////Фаза 2

            if (panel10.Location.X == 323)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
                label5.Text = "Глубокий сон";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 357)
            {
                panel10.BackColor = Color.Gold;
                label18.Text = "Третья стадия";
            }

            if (panel10.Location.X == 374)
            {
                panel10.BackColor = Color.Red;
                label18.Text = "Четвертая стадия";
                label17.Text = "очень тяжело";
            }

            if (panel10.Location.X == 408)
            {
                panel10.BackColor = Color.Gold;
                label18.Text = "Третья стадия";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 425)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
            }

            if (panel10.Location.X == 459)
            {
                panel10.BackColor = Color.DeepSkyBlue;
                label5.Text = "Видит сон";
                label18.Text = "Быстрый сон";
                label15.Text = Convert.ToString(Phase++);
                label17.Text = "легко";
            }

            //////////фаза 3

            if (panel10.Location.X == 493)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
                label5.Text = "Глубокий сон";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 527)
            {
                panel10.BackColor = Color.Gold;
                label18.Text = "Третья стадия";
            }

            if (panel10.Location.X == 544)
            {
                panel10.BackColor = Color.Red;
                label18.Text = "Четвертая стадия";
                label17.Text = "очень тяжело";
            }

            if (panel10.Location.X == 561)
            {
                panel10.BackColor = Color.Gold;
                label18.Text = "Третья стадия";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 578)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
            }

            if (panel10.Location.X == 612)
            {
                panel10.BackColor = Color.DeepSkyBlue;
                label5.Text = "Видит сон";
                label18.Text = "Быстрый сон";
                label15.Text = Convert.ToString(Phase++);
                label17.Text = "легко";
            }
            ///////// фаза 4

            if (panel10.Location.X == 663)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
                label5.Text = "Глубокий сон";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 697)
            {
                panel10.BackColor = Color.Gold;
                label18.Text = "Третья стадия";
            }

            if (panel10.Location.X == 731)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
            }

            if (panel10.Location.X == 765)
            {
                panel10.BackColor = Color.DeepSkyBlue;
                label5.Text = "Видит сон";
                label18.Text = "Быстрый сон";
                label15.Text = Convert.ToString(Phase++); ;
                label17.Text = "легко";
            }

            ////фаза 5

            if (panel10.Location.X == 833)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
                label5.Text = "Глубокий сон";
                label17.Text = "тяжело";
            }

            if (panel10.Location.X == 901)
            {
                panel10.BackColor = Color.DeepSkyBlue;
                label5.Text = "Видит сон";
                label17.Text = "легко";
                label15.Text = Convert.ToString(Phase++);
            }
            ///// фаза 6

            if (panel10.Location.X == 988)
            {
                panel10.BackColor = Color.Green;
                label18.Text = "Вторая стадия";
                label5.Text = "Глубокий сон";
                label17.Text = "тяжело";
            }



        }

        private void button8_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SleepTimer.Interval = 50;
            button1.PerformClick();
            
        }


        private void playSound(string path)
        {

                player.SoundLocation = path;
                player.Load();
                player.Play();

        }
        private void listBox1_DoubleClick(object sender, EventArgs e)
        { 
            if (buf != null)
            {
                
                listBox1.Items.RemoveAt(item);
                listBox1.Items.Insert(item, buf);
                activatedAlarm = false;
                
            }

            if (listBox1.SelectedIndex != -1)
            {
                DialogResult dialogResult = MessageBox.Show("Завести будильник", "Будильник", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    item = listBox1.SelectedIndex;
                    buf = Convert.ToString(listBox1.Items[item]);
                    listBox1.Items.RemoveAt(item);
                    listBox1.Items.Insert(item, " < " + buf + " > ");
                    System.Threading.Thread thread1 = new System.Threading.Thread(SendMailAlarm);
                    thread1.Start();
                    activatedAlarm = true;
                }

            }
           
        }

        private void WakeUpRepeatTimer_Tick(object sender, EventArgs e)
        {
            display(-1);
                playSound("Alarm.wav");
                

                DialogResult dialogResult = MessageBox.Show(label7.Text + "\n" + "Отложить на 10 мин ?", "Будильник", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    display(2);
                    WakeUpRepeatTimer.Enabled = true;
                    player.Stop();
                }
                else if (dialogResult == DialogResult.No)
                {
                    WakeUpRepeatTimer.Enabled = false;
                    player.Stop();
                }
            
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            display(2);
        }


        private void display(int mode)
        {
            //mode = 2 выключение
            //mode = -1 включение
            SendMessage(this.Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, mode);
        }


        public void ParentChange(PictureBox pb)
        {

            label1.Parent = pb;
            label2.Parent = pb;
            label6.Parent = pb;
            label7.Parent = pb;
            label9.Parent = pb;
            label11.Parent = pb;
            label12.Parent = pb;
            label13.Parent = pb;
            label14.Parent = pb;

            label20.Parent = pb;
            label21.Parent = pb;
            label22.Parent = pb;
            label23.Parent = pb;
            label24.Parent = pb;
            label25.Parent = pb;
            label26.Parent = pb;
            label27.Parent = pb;
            label30.Parent = pb;


            groupBox1.Parent = pb;
            groupBox2.Parent = pb;
            //groupBox3.Parent = pb;
            groupBox4.AllowDrop = true;
            groupBox5.AllowDrop = true;

            button8.Parent = pb;
            button9.Parent = pb;
            button10.Parent = pb;
            button11.Parent = pb;
            button12.Parent = pb;
            button13.Parent = pb;
            button16.Parent = pb;

        }

        private void OpasitySoft()
        {
            Opacity = 0;
            Timer timer = new Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                //if ((Opacity += 0.007d) == 1) timer.Stop();
                if ((Opacity += 0.03) == 1) timer.Stop();
            });
            timer.Interval = 2;
            timer.Start();
        }

        private void OpasitySoftClose()
        {
            Opacity = 1;
            Timer timer = new Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                //if ((Opacity += 0.007d) == 1) timer.Stop();
                if ((Opacity -= 0.01d) == 0) timer.Stop();
            });
            timer.Interval = 2;
            timer.Start();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            
            System.Diagnostics.Process.GetCurrentProcess().Kill();
           
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            Windowmoving = true;

            MXold = e.X;
            MYold = e.Y;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (Windowmoving)
            {
                Point tmp = new Point(this.Location.X, this.Location.Y);


                tmp.X += e.X - MXold;
                tmp.Y += e.Y - MYold;

                this.Location = tmp;
            }
        
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            
            Windowmoving = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0046 /* WM_WINDOWPOSCHANGING */)
            {
                Rectangle workArea = SystemInformation.WorkingArea;
                Rectangle rect = (Rectangle)Marshal.PtrToStructure((IntPtr)(IntPtr.Size * 2 + m.LParam.ToInt64()), typeof(Rectangle));

                if (rect.X <= workArea.Left + DISTANCE)
                    Marshal.WriteInt32(m.LParam, IntPtr.Size * 2, workArea.Left);

                if (rect.X + rect.Width >= workArea.Width - DISTANCE)
                    Marshal.WriteInt32(m.LParam, IntPtr.Size * 2, workArea.Right - rect.Width);

                if (rect.Y <= workArea.Top + DISTANCE)
                    Marshal.WriteInt32(m.LParam, IntPtr.Size * 2 + 4, workArea.Top);

                if (rect.Y + rect.Height >= workArea.Height - DISTANCE)
                    Marshal.WriteInt32(m.LParam, IntPtr.Size * 2 + 4, workArea.Bottom - rect.Height);
            }

            base.WndProc(ref m);
        }



        private void button12_Click(object sender, EventArgs e)
        {
            //label visible

            groupBox4.Size = new Size(groupBox4.Size.Width, 211);
            label34.Visible = true;
            label35.Visible = true;
            numericUpDown1.Visible = true;

            label32.Visible = false;
            label31.Visible = false;
            button22.Visible = false;
            button21.Visible = false;
            button19.Visible = false;
            button18.Visible = false;
            button17.Visible = false;

            //

            button14.Visible = true;
            groupBox3.Text = "Настройки";
            groupBox4.Text = "Перенесите ваше фоновое изображение на панель";

            if (!settingOpened)
            {
                settingOpen();
            }
            else
            {
                settingClose();
                StreamWriter sw = File.CreateText("SettingPhase.txt");
                sw.WriteLine(numericUpDown1.Value);
                sw.Close();
            }
        }

        bool settingOpened=false;
        public void settingOpen()
        {

                
                groupBox3.Visible = true;
                Timer timer = new Timer();
                timer.Tick += new EventHandler((sender, e) =>
                {
                    if (groupBox3.Width <= 301)
                    {
                        groupBox3.Width += 10;
                        groupBox3.Left -= 10;

                    }
                    else if (groupBox3.Height <= 292)
                    {
                        groupBox3.Height += 10;
                    }
                    else { timer.Stop(); settingOpened = true; }

                    if (timer.Interval > 2)
                        timer.Interval -= 2;


                });
                timer.Interval = 20;
                timer.Start();
           
        }

        private void settingClose()
        {

                
                Timer timer = new Timer();
                timer.Tick += new EventHandler((sender, e) =>
                {

                    if (groupBox3.Height >= 30)
                    {
                        groupBox3.Height -= 10;
                    }
                    
                    else if (groupBox3.Width >= 20)
                          {
                            groupBox3.Width -= 10;
                            groupBox3.Left += 10;
                          }
                    else { timer.Stop(); groupBox3.Visible = false; settingOpened = false; }

                    if (timer.Interval > 2)
                        timer.Interval -= 2;


                });
                timer.Interval = 20;
                timer.Start();
           
        }
        private void SettingStart()
        {
            groupBox3.Visible = false;

            groupBox3.Location = new Point(this.Location.X+780, this.Location.Y+15);
            groupBox3.Size = new Size(20, 20);
        }



        private void groupBox4_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void groupBox4_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {

                Item item = new Item();
                item.values = new List<string>();
                item.values.Add(file);
                SaveXml(item, "SettingUpload.xml");

                
                //pictureBox2.Load(file);
            }

            pictureBox2.Load(ParseFile("SettingUpload.xml", "/Setting/AppConfig/BackgroundImage"));
            //MessageBox.Show(ParseFile("SettingUpload.xml", "/Setting/AppConfig/BackgroundImage"));
        }

        [XmlRoot("Setting")]
        public class Item
        {
            [XmlArray("AppConfig")]
            [XmlArrayItem("BackgroundImage")]
            public List<string> values;
            [XmlIgnore]
            public bool CurrentStatus;
        }
        public static bool SaveXml(object obj, string filename)
        {
            bool result = false;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                try
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    XmlSerializer serializer = new XmlSerializer(obj.GetType());
                    serializer.Serialize(writer, obj, ns);
                    result = true;
                }
                catch (Exception)
                {
                    // Логирование
                }
                finally
                {
                    writer.Close();
                }
            }
            return result;
        }
        public static object LoadXml(Type type, string filename)
        {
            object result = null;
            using (StreamReader reader = new StreamReader(filename))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(type);
                    result = serializer.Deserialize(reader);
                }
                catch (Exception)
                {
                    // Логирование
                }
                finally
                {
                    reader.Close();
                }
            }
            return result;
        }

        public string ParseFile(string PathFile, string xPath)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(PathFile);
            foreach (XmlNode node1 in xml.SelectNodes(xPath))
            {
                return  node1.InnerText;
            }
            return "background.jpg";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            display(2);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Item item = new Item();
            item.values = new List<string>();
            item.values.Add("background.jpg");
            SaveXml(item, "SettingUpload.xml");
            pictureBox2.Load(ParseFile("SettingUpload.xml", "/Setting/AppConfig/BackgroundImage"));
        }

        public void SendMail(string smtpServer, string from, string password,
string mailto, string caption, string message)
        {

            try
            {
                
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(new MailAddress(mailto));
                mail.Subject = caption;
                mail.Body = message;

                


                SmtpClient client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(from.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                mail.Dispose();

                //MessageBox.Show("Сообщение отправлено");

            }
            catch (Exception e)
            {

                //throw new Exception("Mail.Send: " + e.Message);
               // MessageBox.Show("Сбой отправки сообщения : " + e.Message);
            }

        }
         
        public void SendMailAlarm()
        {
            string smtpServer = "smtp.list.ru";
            string from = "soild@list.ru";
            string password = "Hi73s6dL";
            string mailto = "soild@mail.ru";
            string caption="Будильник заведен " + label7.Text;
            string message="Будильник был заведен на время " + AH[item] + ":" + AM[item];
            SendMail("smtp.list.ru", "soild@list.ru", "Hi73s6dL", "soild@mail.ru", "Будильник заведен " + label7.Text, "Будильник был заведен на время " + AH[item] + ":" + AM[item]);
          
            try
            {

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(new MailAddress(mailto));
                mail.Subject = caption;
                mail.Body = message;




                SmtpClient client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(from.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                mail.Dispose();

                //MessageBox.Show("Сообщение отправлено");

            }
            catch (Exception e)
            {

                //throw new Exception("Mail.Send: " + e.Message);
                // MessageBox.Show("Сбой отправки сообщения : " + e.Message);
            }

        }


        int hour= 10;
        int minute = 10;
        string hour0;
        string minute0;
        private void button16_Click(object sender, EventArgs e)
        {
            //label visible
            groupBox4.Size = new Size(groupBox4.Size.Width, 247);
            label34.Visible = false;
            label35.Visible = false;
            numericUpDown1.Visible = false;
            label32.Visible = true;
            label31.Visible = true;
            button22.Visible = true;
            button21.Visible = true;
            button19.Visible = true;
            button18.Visible = true;
            button17.Visible = true;
            //
           //button14.Visible = false;
            button14.Visible = false;
            groupBox3.Text = "Будильник";
            groupBox4.Text = "Выбирите время";
            if (!settingOpened)
            {
                settingOpen();
            }
            else settingClose();
        }

        void AlarmTime()
        {
            label31.Text = hour0+hour + ":" + minute0+minute;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (hour < 23)
            {
                hour++;
            }
            else hour = 0;

            if (hour < 10)
            {
                hour0 = "0";
            }
            else hour0 = "";

            AlarmTime();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (hour < 24)
            {
                hour--;
            }
            else hour = 0;

            if (hour < 10 && hour >= 0)
            {
                hour0 = "0";
            }
            else hour0 = "";

            if (hour < 0) hour = 23;

            AlarmTime();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (minute < 59)
            {
                minute++;
            }
            else minute = 0;

            if (minute < 10 )
            {
                minute0 = "0";
            }
            else minute0 = "";

            AlarmTime();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (minute < 60)
            {
                minute--;
            }
            else minute = 0;

            if (minute < 10 && minute>=0)
            {
                minute0 = "0";
            }
            else minute0 = "";
            if (minute < 0) minute = 59;

            AlarmTime();
        }


        private void AlarmAllTimer_Tick(object sender, EventArgs e)
        {
            DateTime DT2 = DateTime.Now;

            if (hour == DT2.Hour && minute == DT2.Minute)
            {
                if (!PlayingNow)
                {
                    display(-1);
                    playSound("Alarm.wav");
                    PlayingNow = true;
                    SendMail("smtp.list.ru", "soild@list.ru", "Hi73s6dL", "tolkova1992@mail.ru", "Будильник", "Будильник прозвенел в " + label7.Text);

                    DialogResult dialogResult = MessageBox.Show(label7.Text + "\n" + "Отложить на 10 мин ?", "Будильник", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        display(2);
                        WakeUpRepeatTimer.Enabled = true;
                        player.Stop();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        WakeUpRepeatTimer.Enabled = false;
                        player.Stop();
                        PlayingNow = false;
                        AlarmAllTimer.Enabled = false;
                    }
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            AlarmAllTimer.Enabled = true;
            button20.Visible = true;
            button17.Visible = false;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            AlarmAllTimer.Enabled = false;
            button20.Visible = false;
            button17.Visible = true;
            
        }

        private void numericUpDown1_MouseClick(object sender, MouseEventArgs e)
        {
            StreamWriter sw = File.CreateText("SettingPhase.txt");
            sw.WriteLine(numericUpDown1.Value);
            sw.Close();
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


       
 
      
    }
}
