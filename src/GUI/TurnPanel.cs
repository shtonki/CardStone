using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace stonekart
{
    internal class TurnPanel : Panel
    {
        private Image[] images = new Image[Enum.GetNames(typeof(Step)).Length];
        private ToggleBox[] toggleBoxes = new ToggleBox[Enum.GetNames(typeof(Step)).Length];

        private int step = 0;
        private bool xd;

        public TurnPanel()
        {
            BackColor = Color.Red;
            //Size = new Size(70, 700);
            /*
            refill = Image.FromFile(Settings.resButton + "refill.png");
            draw = Image.FromFile(Settings.resButton + "draw.png");
            main1 = Image.FromFile(Settings.resButton + "main1.png");
            startCombat = Image.FromFile(Settings.resButton + "startcombat.png");
            attack = Image.FromFile(Settings.resButton + "attack.png");
            block = Image.FromFile(Settings.resButton + "block.png");
            damage = Image.FromFile(Settings.resButton + "damage.png");
            endCombat = Image.FromFile(Settings.resButton + "endcombat.png");
            main2 = Image.FromFile(Settings.resButton + "main2.png");
            end = Image.FromFile(Settings.resButton + "end.png");
            */

            for (int i = 0; i < toggleBoxes.Length; i++)
            {
                ToggleBox b = new ToggleBox();
                Controls.Add(b);
                toggleBoxes[i] = b;
                var i1 = i;
                b.Click += (_, __) =>
                {
                    b.toggle();
                };
            }
        }

        public void setHeight(int i)
        {
            Size = new Size(i/10, i);
            Invalidate();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            padPer = Size.Width;
            int w = padPer - padSides;

            for (int i = 0; i < toggleBoxes.Length; i++)
            {
                toggleBoxes[i].Size = new Size(10, 10);
                toggleBoxes[i].Location = new Point(3, 3+i*padPer);
            }
            
            
            Size s = new Size(padPer, padPer);

            for (int i = 0; i < images.Length; i++)
            {
                images[i] = ImageLoader.getStepImage(((Step)i).ToString(), s);
            }
        }

        public void setStep(int s, bool a)
        {
            step = s;
            xd = a;
            Invalidate();
        }

        private int padSides, padPer;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (images?[0] == null) { return; }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int height = Size.Height;
            int width = Size.Width;
            //todo(seba) find out if this leaks memory
            e.Graphics.FillRectangle(new SolidBrush(Color.DodgerBlue),  0, 0,         70, 140);
            e.Graphics.FillRectangle(new SolidBrush(Color.ForestGreen), 0, 2 * width, 70, 70);
            e.Graphics.FillRectangle(new SolidBrush(Color.DarkRed),     0, 3 * width, 70, 350);
            e.Graphics.FillRectangle(new SolidBrush(Color.ForestGreen), 0, 8 * width, 70, 70);
            e.Graphics.FillRectangle(new SolidBrush(Color.DodgerBlue),  0, 9 * width, 70, 70);
            
            for (int i = 0; i < images.Length; i++)
            {
                e.Graphics.DrawImage(images[i], padSides, padSides + i*padPer);
            }
            /*
            e.Graphics.DrawImage(refill, 4, 4);
            e.Graphics.DrawImage(draw, 4, 74);
            e.Graphics.DrawImage(main1, 4, 144);
            e.Graphics.DrawImage(startCombat, 4, 214);
            e.Graphics.DrawImage(attack, 4, 284);
            e.Graphics.DrawImage(block, 4, 354);
            e.Graphics.DrawImage(damage, 4, 424);
            e.Graphics.DrawImage(endCombat, 4, 494);
            e.Graphics.DrawImage(main2, 4, 564);
            e.Graphics.DrawImage(end, 4, 634);
            */
            
            e.Graphics.DrawRectangle(new Pen(xd ? Color.Gold : Color.LightGray, 4), 1, 1 + step*width, width - 3, width - 3);
        }
        class ToggleBox : Panel
        {
            public bool t { get; private set; }
            private SolidBrush on, off;

            public ToggleBox()
            {
                BackColor = Color.Transparent;
                on = new SolidBrush(Color.DarkOrchid);
                off = new SolidBrush(Color.Lime);
            }

            public void toggle()
            {
                t = !t;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(t ? on : off, 0, 0, Size.Width - 1, Size.Height - 1);
                //e.Graphics.DrawEllipse(new Pen(Color.DarkOrchid, 1), 0, 0, 10, 10);
            }
        }
    }
}
