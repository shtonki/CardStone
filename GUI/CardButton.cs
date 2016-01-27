using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace stonekart
{
    public class CardButton : Button, GameUIElement, Observer
    {
        public static int WIDTH = 180, HEIGHT = 280;
        private readonly Size size = new Size(WIDTH, HEIGHT);

        private static FontFamily fontFamilyA;

        private GameInterface gameInterface;
        private Card card;

        private Pen borderPen;

        static CardButton()
        {
            //todo(seba) actually use the FontLoader class
            var a = new PrivateFontCollection();

            try
            {
                //privet.AddFontFile(@"res/FONT/mangalb.ttf");
                a.AddFontFile(@"res/FONT/MatrixBold.ttf");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            fontFamilyA = a.Families[0];
        }

        public CardButton(GameInterface g)
        {
            brushes[(int)ManaColour.WHITE] = new SolidBrush(Color.White);
            brushes[(int)ManaColour.RED] = new SolidBrush(Color.Red);
            brushes[(int)ManaColour.BLACK] = new SolidBrush(Color.Black);
            brushes[(int)ManaColour.BLUE] = new SolidBrush(Color.Blue);
            brushes[(int)ManaColour.GREEN] = new SolidBrush(Color.Green);
            brushes[(int)ManaColour.GREY] = new SolidBrush(Color.Gray);

            gameInterface = g;
            Visible = true;
            Size = size;
            
            MouseEnter += (sender, args) =>
            {
                if (card?.stackWrapper?.targets != null)
                {
                    foreach (Target t in card.stackWrapper.targets)
                    {
                        gameInterface.addArrow(this, targetToGameElement(t));
                    }
                }

                if (card?.defenderOf != null)
                {
                    gameInterface.addArrow(this, gameInterface.getCardButton(card.defenderOf));
                }

                if (card != null && card.isDummy)
                {
                    gameInterface.addArrow(this, gameInterface.getCardButton(card.dummyFor.card));
                }
            };

            MouseLeave += (sender, args) =>
            {
                gameInterface.clearArrows();
            };

            Click += clicked;
        }

        private void clicked(object o, EventArgs a)
        {
            gameInterface.gameElementPressed(getElement());
        }

        public GameElement getElement()
        {
            return new GameElement(card);
        }

        private GameUIElement targetToGameElement(Target t)
        {
            if (t.isCard())
            {
                return gameInterface.getCardButton(t.getCard());
            }
            else
            {
                return gameInterface.getPlayerButton(t.getPlayer());
            }
        }
        

        //todo(seba) make this a property
        public Card getCard()
        {
            return card;
        }

        public void setVisible(bool v)
        {
            if (InvokeRequired) { Invoke(new Action(() => { Visible = v; })); }
            else { Visible = v; }
        }

        public void setBorder(Color? c)
        {
            borderPen = c == null ? null : new Pen(c.Value, 8);
            //borderPen = c == null ? null : borderPen == null ? new Pen(c.Value, 8) : null;
            Invalidate();
        }

        private const int w = 33;

        private static Rectangle flavorTextRectangle = new Rectangle(13, 183, 160, 70);

        private Brush[] brushes = new Brush[6];

        protected override void OnPaint(PaintEventArgs pevent)
        {
            //base.OnPaint(pevent);
            if (card != null)
            {
                Brush b = new SolidBrush(Color.Black);
                Font cardNameFont = new Font(fontFamilyA,
                16,
                FontStyle.Regular,
                GraphicsUnit.Pixel),

                archTypeFont = new Font(fontFamilyA,
                13,
                FontStyle.Regular,
                GraphicsUnit.Pixel),

                PTFont = new Font(fontFamilyA,
                30,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pevent.Graphics.DrawImage(card.getFrame(), new Point(0, 0));
                pevent.Graphics.DrawImage(card.getArt(), new Point(15, 25));
                pevent.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                //Brush b = (Brush)blackBrush.Clone();
                pevent.Graphics.DrawString(card.getName(), cardNameFont, b, 4, 5);
                pevent.Graphics.DrawString(card.getArchtypeString(), archTypeFont, b, 15, 165);
                pevent.Graphics.DrawString(card.getAbilitiesString(), archTypeFont, b, flavorTextRectangle);



                int[] mc = card.getManaCost().costs;


                Pen manaBallPen = new Pen(b, 4);


                int c = 0;
                for (int i = 0; i < 5; i++)
                {
                    b = brushes[i];
                    while (mc[i]-- > 0)
                    {

                        pevent.Graphics.DrawEllipse(manaBallPen, 159 - c * 15, 7, 10, 10);
                        pevent.Graphics.FillEllipse(b, 159 - c * 15, 7, 10, 10);
                        c++;
                    }
                }

                if (mc[(int)ManaColour.GREY] != 0)
                {
                    b = brushes[(int)ManaColour.GREY];
                    pevent.Graphics.FillEllipse(b, 156 - c * 15, 5, 14, 14);
                    b = brushes[(int)ManaColour.BLACK];
                    pevent.Graphics.DrawString(mc[(int)ManaColour.GREY].ToString(), cardNameFont, b, 158 - c * 15, 4);
                }
                
                if (card.hasPT())
                {
                    Brush p = new SolidBrush(Color.Silver);
                    Brush pt = new SolidBrush(Color.Black);
                    Brush damaged = new SolidBrush(Color.DarkRed);

                    pevent.Graphics.FillEllipse(p, -w, 280 - w, 2 * w, 2 * w);
                    pevent.Graphics.FillEllipse(p, 180 - w, 280 - w, 2 * w, 2 * w);
                    pevent.Graphics.DrawString(card.currentPower.ToString(), PTFont, pt, 4, 250);
                    pevent.Graphics.DrawString(card.currentToughness.ToString(), PTFont, card.isDamaged() ? damaged : pt, 154, 250);
                }

                if (borderPen != null)
                {
                    pevent.Graphics.DrawRectangle(borderPen, 0, 0, WIDTH, HEIGHT);                    
                }

                /*
                pevent.Graphics.FillEllipse(new SolidBrush(Color.LightSlateGray), 158 - i * 15, 5, 14, 14);
                Font gcFont = new Font(fontFamilyA, 18, FontStyle.Regular, GraphicsUnit.Pixel);
                pevent.Graphics.DrawString("2", gcFont, new SolidBrush(Color.Black), 159 - i * 15, 2);
                 */
            }
        }

        public void notifyObserver(Observable o)
        {
            card = (Card)o;

            if (card.inCombat)
            {
                setBorder(Color.Blue);
            }
            else if (card.attacking)
            {
                setBorder(Color.Red);
            }
            else
            {
                setBorder(null);
            }
            
            if (InvokeRequired)
            {
                Invoke(new Action(() => { Visible = card != null; }));
            }
            else
            {
                Visible = card != null;       
            }
            Invalidate();
        }
    }
}