using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace stonekart
{
    class CardButton : Button, Foo, Observer
    {
        public static int WIDTH = 180, HEIGHT = 280;
        private static Size HIDE = new Size(0, 0), SHOW = new Size(WIDTH, HEIGHT);

        private static Font cardNameFont;/* = new Font(
            new FontFamily("Comic Sans MS"),
            14,
            FontStyle.Regular,
            GraphicsUnit.Pixel);
        */

        private static Font PTFont;/* = new Font(
            new FontFamily("Comic Sans MS"), 
            30, 
            FontStyle.Bold,
            GraphicsUnit.Pixel);
        */
        private static Brush
            whiteBrush = new SolidBrush(Color.White),
            blueBrush = new SolidBrush(Color.Blue),
            blackBrush = new SolidBrush(Color.Black),
            redBrush = new SolidBrush(Color.Red),
            greenBrush = new SolidBrush(Color.Green);


        private static Brush[] brushes = new[] {whiteBrush, blueBrush, blackBrush, redBrush, greenBrush};

        private static int idCtr = 0;

        private int prevIndex;  //for draw order purposes

        private int id;
        private Card card;

        static CardButton()
        {
            PrivateFontCollection privet = new PrivateFontCollection();
             
            try { privet.AddFontFile(@"../../res/FONT/mangalb.ttf"); }
            catch(Exception e) { System.Console.WriteLine(e.Message); }
            FontFamily[] horfamilj = privet.Families;

            PTFont = new Font(horfamilj[0],
                16,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            cardNameFont = new Font(horfamilj[0],
                30,
                FontStyle.Bold,
                GraphicsUnit.Pixel);
        }

        public CardButton()
        {
            Visible = true;
            id = idCtr++;
            Size = SHOW;

            MouseEnter += (sender, args) =>
            {
                prevIndex = Parent.Controls.GetChildIndex(this);
                Parent.Controls.SetChildIndex(this, 0);
            };

            MouseLeave += (sender, args) =>
            {
                Parent.Controls.SetChildIndex(this, prevIndex);
            };

            Click += (sender, args) =>
            {
                GameController.fooPressed(this);
            };
        }

        public Card getCard()
        {
            return card;
        }

        public void setVisible(bool v)
        {
            Size = v ? SHOW : HIDE;
        }

        private static Pen manaBallPen = new Pen(blackBrush, 4);
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            if (card != null)
            {
                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pevent.Graphics.DrawImage(card.getFrame(), new Point(0, 0));
                pevent.Graphics.DrawImage(card.getArt(), new Point(15, 25));
                pevent.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                pevent.Graphics.DrawString(card.getName(), cardNameFont, blackBrush, 15, 3);

                int[] mc = card.getManaCost().getColors();
                int i = 0;

                for (int c = 0; c < 5; c++)
                {
                    for (int j = 0; j < mc[c]; j++)
                    {
                        pevent.Graphics.DrawEllipse(manaBallPen, 149 - i * 18, 5, 13, 13);
                        pevent.Graphics.FillEllipse(brushes[c], 149 - i * 18, 5, 13, 13);
                        i++;
                    }
                }
            }
        }

        public void notifyObserver(Observable o)
        {
            card = (Card)o;
            Visible = card != null;
        }
    }
}
