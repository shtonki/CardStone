using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace stonekart
{
    public class CardButton : Button, GameUIElement, Observer, Resolutionable
    {
        public static GameInterface dummy = new GameInterface(GUI.nonsense);
        //private static FontFamily fontFamilyA;

        private GameInterface gameInterface;
        private Card card;

        private Pen borderPen;
        private Brush[] brushes = new Brush[6];
        private Font cardNameFont, textFont, PTFont;
        

        public CardButton(GameInterface g)
        {
            updateResolution();

            brushes[(int)ManaColour.WHITE] = new SolidBrush(Color.White);
            brushes[(int)ManaColour.RED] = new SolidBrush(Color.Red);
            brushes[(int)ManaColour.BLACK] = new SolidBrush(Color.Black);
            brushes[(int)ManaColour.BLUE] = new SolidBrush(Color.Blue);
            brushes[(int)ManaColour.GREEN] = new SolidBrush(Color.Green);
            brushes[(int)ManaColour.GREY] = new SolidBrush(Color.Gray);

            gameInterface = g;
            Visible = true;
            
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

        public CardButton(CardId c) : this(dummy)
        {
            card = new Card(c);
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
            if (borderPen != null)
            {
                borderPen.Dispose();
            }

            borderPen = c == null ? null : new Pen(c.Value, 8);
            //borderPen = c == null ? null : borderPen == null ? new Pen(c.Value, 8) : null;
            Invalidate();
        }


        private static Rectangle flavorTextRectangle = new Rectangle(13, 183, 160, 70);


        protected override void OnPaint(PaintEventArgs pevent)
        {
            //base.OnPaint(pevent);
            if (card != null)
            {
                
                int width = Size.Width,
                    height = Size.Height;

                Brush b = brushes[(int)ManaColour.BLACK];
                
                
                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                pevent.Graphics.DrawImage(ImageLoader.getFrame(card), new Point(0, 0));

                pevent.Graphics.DrawImage(ImageLoader.getCardArt(card.cardId), 
                    Resolution.get(ElementDimensions.CardButtonArtLocationX), 
                    Resolution.get(ElementDimensions.CardButtonArtLocationY));
                
                pevent.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                pevent.Graphics.DrawString(card.getName(), cardNameFont, b, 
                    Resolution.get(ElementDimensions.CardButtonNameLocationX), 
                    Resolution.get(ElementDimensions.CardButtonNameLocationY));

                pevent.Graphics.DrawString(card.getArchtypeString(), textFont, b, 
                    Resolution.get(ElementDimensions.CardButtonTypeTextLocationX),
                    Resolution.get(ElementDimensions.CardButtonTypeTextLocationY));

                pevent.Graphics.DrawString(card.getAbilitiesString(), textFont, b, 
                    new Rectangle(
                            Resolution.get(ElementDimensions.CardButtonTextLocationX),
                            Resolution.get(ElementDimensions.CardButtonTextLocationY),
                            Resolution.get(ElementDimensions.CardButtonTextWidth),
                            Resolution.get(ElementDimensions.CardButtonTextHeight)
                        ));
                


                int[] mc = card.getManaCost().costs;


                Pen manaBallPen = new Pen(b, 2);


                int c = 0;
                for (int i = 0; i < 5; i++)
                {
                    b = brushes[i];
                    while (mc[i]-- > 0)
                    {

                        pevent.Graphics.DrawEllipse(manaBallPen, 
                            Resolution.get(ElementDimensions.CardButtonManaOrbLocationX) - c * Resolution.get(ElementDimensions.CardButtonManaOrbPadding),
                            Resolution.get(ElementDimensions.CardButtonManaOrbLocationY), 
                            Resolution.get(ElementDimensions.CardButtonManaOrbSize),
                            Resolution.get(ElementDimensions.CardButtonManaOrbSize));

                        pevent.Graphics.FillEllipse(b, 
                            Resolution.get(ElementDimensions.CardButtonManaOrbLocationX) - c * Resolution.get(ElementDimensions.CardButtonManaOrbPadding),
                            Resolution.get(ElementDimensions.CardButtonManaOrbLocationY),
                            Resolution.get(ElementDimensions.CardButtonManaOrbSize),
                            Resolution.get(ElementDimensions.CardButtonManaOrbSize));

                        c++;
                    }
                }
                
                if (mc[(int)ManaColour.GREY] != 0)
                {
                    b = brushes[(int)ManaColour.GREY];
                    pevent.Graphics.FillEllipse(b, 
                        Resolution.get(ElementDimensions.CardButtonManaOrbLocationX) - c * Resolution.get(ElementDimensions.CardButtonManaOrbPadding) - 1,
                            Resolution.get(ElementDimensions.CardButtonManaOrbLocationY) - 1,
                            Resolution.get(ElementDimensions.CardButtonManaOrbSize) + 2,
                            Resolution.get(ElementDimensions.CardButtonManaOrbSize) + 2);
                    b = brushes[(int)ManaColour.BLACK];
                    pevent.Graphics.DrawString(mc[(int)ManaColour.GREY].ToString(), cardNameFont, b,
                        Resolution.get(ElementDimensions.CardButtonGreyCostLocationX) + Resolution.get(ElementDimensions.CardButtonManaOrbLocationX) - c * Resolution.get(ElementDimensions.CardButtonManaOrbPadding) + 1,
                        Resolution.get(ElementDimensions.CardButtonGreyCostLocationY) + Resolution.get(ElementDimensions.CardButtonManaOrbLocationY));
                        
                }
                
                if (card.hasPT())
                {
                    Brush p = new SolidBrush(Color.Silver);
                    Brush pt = new SolidBrush(Color.Black);
                    Brush damaged = new SolidBrush(Color.DarkRed);

                    int w = Resolution.get(ElementDimensions.CardButtonPTAreaSize);

                    pevent.Graphics.FillEllipse(p, -w, height - w, 2 * w, 2 * w);
                    pevent.Graphics.FillEllipse(p , width - w, height - w, 2 * w, 2 * w);

                    pevent.Graphics.DrawString(card.currentPower.ToString(), PTFont, pt, 
                        Resolution.get(ElementDimensions.CardButtonPTTextLocationP),
                        Resolution.get(ElementDimensions.CardButtonPTTextLocationY));

                    pevent.Graphics.DrawString(card.currentToughness.ToString(), PTFont, card.isDamaged() ? damaged : pt,
                        Resolution.get(ElementDimensions.CardButtonPTTextLocationT),
                        Resolution.get(ElementDimensions.CardButtonPTTextLocationY));
                }

                if (borderPen != null)
                {
                    pevent.Graphics.DrawRectangle(borderPen, 0, 0, width, height);                    
                }

                manaBallPen.Dispose();
                
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

        public void updateResolution()
        {
            if (cardNameFont != null)
            {
                cardNameFont.Dispose();
                textFont.Dispose();
                PTFont.Dispose();
            }

            cardNameFont = FontLoader.getFont(FontLoader.MANGALB, Resolution.get(ElementDimensions.CardButtonNameFontSize));
            textFont = FontLoader.getFont(FontLoader.MANGALB, Resolution.get(ElementDimensions.CardButtonTextFontSize));
            PTFont = FontLoader.getFont(FontLoader.MANGALB, Resolution.get(ElementDimensions.CardButtonPTFontSize));
            
            int height = Resolution.get(ElementDimensions.CardButtonHeight),
                width =  Resolution.get(ElementDimensions.CardButtonWidth);
            Size = new Size(width, height);

            Invalidate();
        }
    }
}