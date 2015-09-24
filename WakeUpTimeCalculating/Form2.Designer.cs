namespace WakeUpTimeCalculating
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.OpenSoftTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // OpenSoftTimer
            // 
            this.OpenSoftTimer.Interval = 10;
            this.OpenSoftTimer.Tick += new System.EventHandler(this.OpenSoftTimer_Tick);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(244, 493);
            this.Name = "Form2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = " ";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer OpenSoftTimer;

    }
}