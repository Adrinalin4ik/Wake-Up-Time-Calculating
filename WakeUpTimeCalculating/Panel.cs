using System;
using System.Drawing;
using System.Windows.Forms;

namespace WakeUpTimeCalculating
{
    /// 
    /// A transparent control.
    /// 

    public sealed class TransparentPanel : Panel
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 0, 0)), this.ClientRectangle);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }
    }
}
