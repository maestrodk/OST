using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverloadServerTool
{
    public class CustomCheckBox : CheckBox
    {
        int boxSize = 0;

        public CustomCheckBox()
        {
            Appearance = Appearance.Button;
            FlatStyle = FlatStyle.Flat;
            TextAlign = ContentAlignment.MiddleCenter;
            AutoSize = false;
            FlatAppearance.BorderSize = 0;

            Height = new CheckBox().Height;

            ForeColor = SystemColors.ControlText;
            BackColor = SystemColors.Control;
        }

        protected override void OnPaint(PaintEventArgs paintEvent)
        {
            // base.OnPaint(pevent);

            paintEvent.Graphics.Clear(BackColor);

            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                paintEvent.Graphics.DrawString(Text, Font, brush, 27, 4);
            }

            Point pt = new Point(4, 4);
            Rectangle rect = new Rectangle(pt, Size);

            paintEvent.Graphics.FillRectangle(SystemBrushes.Control, rect);

            if (Checked)
            {
                using (SolidBrush brush = new SolidBrush(Color.LightCyan))
                {
                    using (Font wing = new Font("Wingdings", 10f))
                    {
                        paintEvent.Graphics.DrawString("ü", wing, brush, 1, 2);
                    }
                }
            }

            paintEvent.Graphics.DrawRectangle(Pens.DarkSlateBlue, rect);

            Rectangle fRect = ClientRectangle;

            if (Focused)
            {
                fRect.Inflate(-1, -1);

                using (Pen pen = new Pen(Brushes.Gray) { DashStyle = DashStyle.Dot })
                {
                    paintEvent.Graphics.DrawRectangle(pen, fRect);
                }
            }
        }
    }
}
