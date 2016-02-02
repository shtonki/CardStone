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
        //public static GameInterface dummy = new GameInterface();
        //private static FontFamily fontFamilyA;

        //private GameInterface gameInterface;
        //private Card card;

        private bool valid => Card != null;
        public Card Card { get; private set; }

        private Colour colour;
        private CardId art;
        private string name;
        private string archtype;
        private string abilityText;
        private int[] costs;
        private bool hasPT;
        private string power;
        private string toughness;
        private bool isDamaged;
        private IEnumerable<Target> targets;

        private int ArtHeight;
        private int ArtWidth;
        private int ArtLocationX;
        private int ArtLocationY;
        private int NameLocationX;
        private int NameLocationY;
        private int NameFontSize;
        private int TextFontSize;
        private int PTFontSize;
        private int TypeTextLocationX;
        private int TypeTextLocationY;
        private int TextLocationX;
        private int TextLocationY;
        private int TextWidth;
        private int TextHeight;
        private int ManaOrbLocationX;
        private int ManaOrbLocationY;
        private int ManaOrbSize;
        private int ManaOrbPadding;
        private int GreyCostLocationX;
        private int GreyCostLocationY;
        private int PTAreaSize;
        private int PTTextLocationP;
        private int PTTextLocationY;
        private int PTTextLocationT;
        private Rectangle TextRectangle;


        private Pen borderPen;
        private Brush[] brushes = new Brush[6];
        private Font cardNameFont, textFont, PTFont;
        

        public CardButton()
        {
            //updateResolution();
            
            brushes[(int)Colour.WHITE] = new SolidBrush(Color.White);
            brushes[(int)Colour.RED] = new SolidBrush(Color.Red);
            brushes[(int)Colour.BLACK] = new SolidBrush(Color.Black);
            brushes[(int)Colour.BLUE] = new SolidBrush(Color.Blue);
            brushes[(int)Colour.GREEN] = new SolidBrush(Color.Green);
            brushes[(int)Colour.GREY] = new SolidBrush(Color.Gray);

            Visible = true;
            /*
            MouseEnter += (sender, args) =>
            {
                foreach (Target t in targets)
                {
                    IEnumerable<GameUIElement> ret;
                    List<GameUIElement> r = new List<GameUIElement>();

                    if (t.isCard())
                    {
                        ret = g.getCardButtons(t.getCard());
                    }
                    else
                    {
                        ret = g.getPlayerButton(t.getPlayer());
                    }
                    var ts = ret;
                    foreach (var v in ts)
                    {
                        g.addArrow(this, v);
                    }
                    
                }
            };

            MouseLeave += (sender, args) =>
            {
                g.clearArrows();
            };
            */
        }

        public CardButton(GameInterface g, int height) : this()
        {
            setHeight(height);
            Click += (object o, EventArgs a) =>
            {
                g.gameElementPressed(getElement());
            };
        }

        public void setHeight(int h)
        {
            Size = new Size((int)(0.642f * h), h);
        }

        public void setWidth(int h)
        {
            Size = new Size(h, (int)(h / 0.654f));
        }

        public CardButton(CardId c, int height) : this()
        {
            Card card = new Card(c);
            card.addObserver(this);
            setHeight(height);
        }
        

        public GameElement getElement()
        {
            return new GameElement(Card);
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

        public void close()
        {
            Card.removeObserver(this);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            //base.OnPaint(pevent);
            if (valid)
            {
                int width = Size.Width,
                    height = Size.Height;

                Brush b = brushes[(int)Colour.BLACK];

                
                
                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                pevent.Graphics.DrawImage(ImageLoader.getFrame(colour, new Size(width, height)), new Point(0, 0));

                pevent.Graphics.DrawImage(ImageLoader.getCardArt(art, new Size(ArtWidth, ArtHeight)), ArtLocationX, ArtLocationY);
                
                pevent.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                pevent.Graphics.DrawString(name, cardNameFont, b, NameLocationX, NameLocationY);

                pevent.Graphics.DrawString(archtype, textFont, b, TypeTextLocationX, TypeTextLocationY);

                pevent.Graphics.DrawString(abilityText, textFont, b, TextRectangle);
                


                int[] mc = costs;


                Pen manaBallPen = new Pen(b, 2);


                int c = 0;
                for (int i = 0; i < 5; i++)
                {
                    b = brushes[i];
                    int v = mc[i];
                    while (v-- > 0)
                    {

                        pevent.Graphics.DrawEllipse(manaBallPen, 
                            ManaOrbLocationX - c * ManaOrbPadding,
                            ManaOrbLocationY, 
                            ManaOrbSize,
                            ManaOrbSize);

                        pevent.Graphics.FillEllipse(b,
                            ManaOrbLocationX - c * ManaOrbPadding,
                            ManaOrbLocationY,
                            ManaOrbSize,
                            ManaOrbSize);

                        c++;
                    }
                }
                
                if (mc[(int)Colour.GREY] != 0)
                {
                    b = brushes[(int)Colour.GREY];
                    pevent.Graphics.FillEllipse(b, 
                        ManaOrbLocationX - c * ManaOrbPadding - 1,
                        ManaOrbLocationY - 1,
                        ManaOrbSize + 2,
                        ManaOrbSize + 2);
                    b = brushes[(int)Colour.BLACK];
                    pevent.Graphics.DrawString(mc[(int)Colour.GREY].ToString(), cardNameFont, b,
                        GreyCostLocationX + ManaOrbLocationX - c * ManaOrbPadding + 1,
                        GreyCostLocationY + ManaOrbLocationY);
                        
                }
                
                if (hasPT)
                {
                    Brush p = new SolidBrush(Color.Silver);
                    Brush pt = new SolidBrush(Color.Black);
                    Brush damaged = new SolidBrush(Color.DarkRed);

                    int w = PTAreaSize;

                    pevent.Graphics.FillEllipse(p, -w, height - w, 2 * w, 2 * w);
                    pevent.Graphics.FillEllipse(p , width - w, height - w, 2 * w, 2 * w);

                    pevent.Graphics.DrawString(power, PTFont, pt, 
                        PTTextLocationP,
                        PTTextLocationY);

                    pevent.Graphics.DrawString(toughness, PTFont, isDamaged ? damaged : pt,
                        PTTextLocationT,
                        PTTextLocationY);
                }

                if (borderPen != null)
                {
                    pevent.Graphics.DrawRectangle(borderPen, 0, 0, width, height);                    
                }

                manaBallPen.Dispose();
                
            }
        }

        public void notifyObserver(Observable o, object args)
        {
            Card card = (Card)o;

            Card = card;
            colour = card.colour;
            art = card.cardId;
            name = card.getName();
            archtype = card.getArchtypeString();
            abilityText = card.getAbilitiesString();
            costs = card.getManaCost().costs;
            hasPT = false;
            if (card.hasPT())
            {
                hasPT = true;
                power = card.currentPower.ToString();
                toughness = card.currentToughness.ToString();
                isDamaged = card.isDamaged();
            }
            
            targets = new List<Target>(); //hack

            Invalidate();
        }
        
        protected override void OnResize(EventArgs e)
        {
            //base.OnResize(e);

            int height = Size.Height;
            ArtHeight =         (int)Math.Round(height * 0.483857142857143f);
            ArtWidth =          (int)Math.Round(height * 0.513142857142857f);
            ArtLocationX =      (int)Math.Round(height * 0.0648571428571429f);
            ArtLocationY =      (int)Math.Round(height * 0.104142857142857f);
            NameLocationX =     (int)Math.Round(height * 0.0321428571428571f);
            NameLocationY =     (int)Math.Round(height * 0.025f);
            NameFontSize =      (int)Math.Round(height * 0.05f);
            TextFontSize =      (int)Math.Round(height * 0.0357142857142857f);
            PTFontSize =        (int)Math.Round(height * 0.1f);
            TypeTextLocationX = (int)Math.Round(height * 0.0607142857142857f);
            TypeTextLocationY = (int)Math.Round(height * 0.6f);
            TextLocationX =     (int)Math.Round(height * 0.075f);
            TextLocationY =     (int)Math.Round(height * 0.646428571428571f);
            TextWidth =         (int)Math.Round(height * 0.464285714285714f);
            TextHeight =        (int)Math.Round(height * 0.132142857142857f);
            ManaOrbLocationX =  (int)Math.Round(height * 0.55f);
            ManaOrbLocationY =  (int)Math.Round(height * 0.025f);
            ManaOrbSize =       (int)Math.Round(height * 0.0392857142857143f);
            ManaOrbPadding =    (int)Math.Round(height * 0.0504285714285714f);
            GreyCostLocationX = (int)Math.Round(height * 0.00357142857142857f);
            GreyCostLocationY = (int)Math.Round(height * -0.00357142857142857f);
            PTAreaSize =        (int)Math.Round(height * 0.117857142857143f);
            PTTextLocationP =   (int)Math.Round(height * -0.00357142857142857f);
            PTTextLocationY =   (int)Math.Round(height * 0.892857142857143f);
            PTTextLocationT =   (int)Math.Round(height * 0.557142857142857f);

            TextRectangle = new Rectangle(TextLocationX, TextLocationY, TextWidth, TextHeight);

            if (cardNameFont != null)
            {
                cardNameFont.Dispose();
                textFont.Dispose();
                PTFont.Dispose();
            }
            //todo(seba) make this fit something
            cardNameFont = FontLoader.getFont(FontLoader.MANGALB, NameFontSize);
            textFont = FontLoader.getFont(FontLoader.MANGALB, TextFontSize);
            PTFont = FontLoader.getFont(FontLoader.MANGALB, PTFontSize);
            /*
            int height = Resolution.get(ElementDimensions.CardButtonHeight),
                width =  Resolution.get(ElementDimensions.CardButtonWidth);
            Size = new Size(width, height);
            */
            Invalidate();
        }
    }


    class SnapCardButton : CardButton
    {
        private Point def, att;

        private int xdd;

        private bool dirty;

        public SnapCardButton(GameInterface g, int i) : base(g, 0000000000)
        {
            throw new NotImplementedException(); //00000000
            LocationChanged += (sender, args) => setLocation();
            xdd = i;
        }

        public new void notifyObserver(Observable o, object[] args)
        {
            base.notifyObserver(o, args);

            Card c = (Card)o;
            Invoke(new Action(() => { dirty = true; Location = c.topped ? att : def; }));
        }


        private void setLocation()
        {
            if (dirty)
            {
                dirty = false;
                return;
            }

            int x = Location.X;
            int y = Location.Y;
            def = new Point(x, y);
            att = new Point(x, y + xdd);
        }
    }

}
