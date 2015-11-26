using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using WMPLib;

namespace stonekart
{
    internal class TurnPanel : Panel
    {
        private const string resPath = @"res/IMG/button/";
        private const int STEPS = 10;

        private Image
            refill,
            draw,
            main1,
            startCombat,
            attack,
            block,
            damage,
            endCombat,
            main2,
            end;

        private int step = 0;
        private bool xd;

        public TurnPanel()
        {
            BackColor = Color.Black;
            Size = new Size(70, 700);

            refill = Image.FromFile(resPath + "refill.png");
            draw = Image.FromFile(resPath + "draw.png");
            main1 = Image.FromFile(resPath + "main1.png");
            startCombat = Image.FromFile(resPath + "startcombat.png");
            attack = Image.FromFile(resPath + "attack.png");
            block = Image.FromFile(resPath + "block.png");
            damage = Image.FromFile(resPath + "damage.png");
            endCombat = Image.FromFile(resPath + "endcombat.png");
            main2 = Image.FromFile(resPath + "main2.png");
            end = Image.FromFile(resPath + "end.png");


            for (int i = 0; i < 20; i++)
            {
                ToggleBox b = new ToggleBox();
                b.Location = new Point(i%2 * 60, (i/2)*70);
                Controls.Add(b);
                var i1 = i;
                b.Click += (_, __) =>
                {
                    System.Console.WriteLine(i1);
                    b.toggle();
                };
            }
        }

        public void advanceStep()
        {
            step = (step + 1) % STEPS;
            xd = xd ^ step == 0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.FillRectangle(new SolidBrush(Color.DodgerBlue),  0, 0,   70, 140);
            e.Graphics.FillRectangle(new SolidBrush(Color.ForestGreen), 0, 140, 70, 70);
            e.Graphics.FillRectangle(new SolidBrush(Color.DarkRed),     0, 210, 70, 350);
            e.Graphics.FillRectangle(new SolidBrush(Color.ForestGreen), 0, 560,   70, 70);
            e.Graphics.FillRectangle(new SolidBrush(Color.DodgerBlue),  0, 630,   70, 70);

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
            
            
            e.Graphics.DrawRectangle(new Pen(xd ? Color.Gold : Color.LightGray, 4), 1, 1 + step*70, 67, 67);
        }

        class ToggleBox : Panel
        {
            private bool t;

            public ToggleBox()
            {
                Size = new Size(10, 10);
                BackColor = Color.Transparent;
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
                e.Graphics.FillEllipse(new SolidBrush(t ? Color.DarkOrchid : Color.Lime), 0, 0, 9, 9);
                //e.Graphics.DrawEllipse(new Pen(Color.DarkOrchid, 1), 0, 0, 10, 10);
            }
        }
    }
}
