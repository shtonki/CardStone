﻿using System;
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

        private static FontFamily fontFamilyA;

        private static int idCtr = 0;

        private int prevIndex;  //for draw order purposes

        private int id;
        private Card card;

        static CardButton()
        {
            var a = new PrivateFontCollection();

            try
            {
                //privet.AddFontFile(@"res/FONT/mangalb.ttf");
                a.AddFontFile(@"res/FONT/MatrixBold.ttf");
            }
            catch (Exception e) { System.Console.WriteLine(e.Message); }
            fontFamilyA = a.Families[0];
            /*
            PTFont = new Font(horfamilj[0],
                20,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            cardNameFont = new Font(horfamilj[0],
                16,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            archTypeFont = new Font(horfamilj[0],
                13,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            textFont = new Font(horfamilj[0],
                13,
                FontStyle.Regular,
                GraphicsUnit.Pixel);
             */
        }

        public CardButton()
        {
            Visible = true;
            id = idCtr++;
            Size = SHOW;
            /*
            MouseEnter += (sender, args) =>
            {
                prevIndex = Parent.Controls.GetChildIndex(this);
                Parent.Controls.SetChildIndex(this, 0);
            };

            MouseLeave += (sender, args) =>
            {
                Parent.Controls.SetChildIndex(this, prevIndex);
            };
            */
            Click += (sender, args) =>
            {
                GameController.currentGame.fooPressed(this);
            };
        }

        public Card getCard()
        {
            return card;
        }

        public void setVisible(bool v)
        {
            if (InvokeRequired) { Invoke(new Action(() => { Size = v ? SHOW : HIDE; })); }
            else { Size = v ? SHOW : HIDE; }
        }

        private const int w = 33;

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
                //pevent.Graphics.DrawString("Flying", textFont, blackBrush, 13, 193);



                int[] mc = card.getManaCost().getColors();
                int i = 0;


                Pen manaBallPen = new Pen(b, 4);



                for (int c = 0; c < 5; c++)
                {
                    switch (c)
                    {
                        case 0:
                            {
                                b = new SolidBrush(Color.White);
                            } break;

                        case 1:
                            {
                                b = new SolidBrush(Color.Blue);
                            } break;

                        case 2:
                            {
                                b = new SolidBrush(Color.Black);
                            } break;

                        case 3:
                            {
                                b = new SolidBrush(Color.Red);
                            } break;

                        case 4:
                            {
                                b = new SolidBrush(Color.Green);
                            } break;

                    }
                    for (int j = 0; j < mc[c]; j++)
                    {
                        pevent.Graphics.DrawEllipse(manaBallPen, 159 - i * 15, 7, 10, 10);
                        pevent.Graphics.FillEllipse(b, 159 - i * 15, 7, 10, 10);
                        i++;
                    }
                }

                if (card.hasPT())
                {
                    Brush p = new SolidBrush(Color.Silver);
                    b = new SolidBrush(Color.Black);

                    pevent.Graphics.FillEllipse(p, -w, 280 - w, 2 * w, 2 * w);
                    pevent.Graphics.FillEllipse(p, 180 - w, 280 - w, 2 * w, 2 * w);
                    pevent.Graphics.DrawString(card.getPower().ToString(), PTFont, b, 4, 250);
                    pevent.Graphics.DrawString(card.getToughness().ToString(), PTFont, b, 154, 250);
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
            if (InvokeRequired)
            {
                Invoke(new Action(() => { Visible = card != null; }));
            }
            else
            {
                Visible = card != null;       
            }
        }
    }

    class SnapCardButton : CardButton, Observer
    {
        private bool snapped;
        private Point def, att;

        public new void notifyObserver(Observable o)
        {
            base.notifyObserver(o);

            Card c = (Card)o;
            Invoke(new Action(() => { Location = c.isAttacking() ? att : def; }));
        }

        public void setLocation(int x, int y)
        {
            def = new Point(x, y);
            att = new Point(x, y - 40);
            Location = def;
        }
    }
}