using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace stonekart
{
    internal class TurnPanel : Panel
    {
        private Image 
            refill,
            draw,
            main1,
            startCombat,
            attack;
        private int step = 0;

        public TurnPanel()
        {
            BackColor = Color.LightSeaGreen;
            Size = new Size(70, 400);

            refill = Image.FromFile(@"../../res/BTN/refill.png");
            draw = Image.FromFile(@"../../res/BTN/draw.png");
            main1 = Image.FromFile(@"../../res/BTN/main1.png");
            //startCombat = Image.FromFile(@"../../res/BTN/startCombat.png");
            attack = Image.FromFile(@"../../res/BTN/attack.png");

            //MouseClick += (sender, args) => { advanceStep(); };
        }

        public void advanceStep()
        {
            step = (step + 1) % 4;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.DrawImage(refill, 4, 4);
            e.Graphics.DrawImage(draw, 4, 74);
            e.Graphics.DrawImage(main1, 4, 144);
            e.Graphics.DrawImage(attack, 4, 214);

            e.Graphics.DrawRectangle(new Pen(Color.Gold, 4), 1, 1 + step*70, 67, 67);
        }
    }
}
