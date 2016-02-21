using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace stonekart
{
    public struct FML
    {
        public readonly Action<CardButton> clickCallBack;
        public readonly Action<CardButton, IEnumerable<Target>> addArrows;
        public readonly Action clearArrows;
        public readonly Action<CardButton> mouseEnters;
        
        public FML(Action<CardButton> clickCallBack)
        {
            this.clickCallBack = clickCallBack;
            addArrows = (_, __) => { };
            clearArrows = () => { };
            mouseEnters = (_) => { };
        }

        public FML(Action<CardButton> clickCallBack, Action<CardButton, IEnumerable<Target>> addArrows, Action clearArrows, Action<CardButton> mouseEnters)
        {
            this.clickCallBack = clickCallBack;
            this.addArrows = addArrows;
            this.clearArrows = clearArrows;
            this.mouseEnters = mouseEnters;
        }

        public FML(Action<CardButton> clickCallBack, Action<CardButton> mouseEnters)
        {
            this.clickCallBack = clickCallBack;
            this.addArrows = (_, __) => { };
            this.clearArrows = () => { };
            this.mouseEnters = mouseEnters;
        }
    }

    public class CardButton : Panel, GameUIElement, Observer
    {
        protected bool valid => Card != null && cardNameFont != null;
        public Card Card { get; private set; }
        #region drawshit
        protected Colour colour;
        protected CardId art;
        protected string name;
        protected string archtype;
        protected string abilityText;
        protected int[] costs;
        protected bool hasPT;
        protected string power;
        protected string toughness;
        protected bool isDamaged;
        protected Rarity rarity;
        public bool isExhausted { get; private set; }
        private IEnumerable<Target> targets;


        protected int CardWidth;
        protected int CardHeight;
        protected int ArtHeight;
        protected int ArtWidth;
        protected int ArtLocationX;
        protected int ArtLocationY;
        protected int NameLocationX;
        protected int NameLocationY;
        protected int NameFontSize;
        protected int TextFontSize;
        protected int PTFontSize;
        protected int TypeTextLocationX;
        protected int TypeTextLocationY;
        protected int TextLocationX;
        protected int TextLocationY;
        protected int TextWidth;
        protected int TextHeight;
        protected int ManaOrbLocationX;
        protected int ManaOrbLocationY;
        protected int ManaOrbSize;
        protected int ManaOrbPadding;
        protected int GreyCostLocationX;
        protected int GreyCostLocationY;
        protected int PTAreaSize;
        protected int PTTextLocationP;
        protected int PTTextLocationY;
        protected int PTTextLocationT;
        protected Rectangle TextRectangle;
        protected Color? borderColor;
        #endregion
        
        protected Brush[] brushes = new Brush[6];
        protected Brush[] rarityBrushes = new Brush[10];
        protected Font cardNameFont, textFont, PTFont;

        public CardButton()
        {
            //FlatStyle = FlatStyle.Flat;
            //FlatAppearance.BorderColor = Color.Red;
            //FlatAppearance.BorderSize = 0;

            brushes[(int)Colour.WHITE] = new SolidBrush(Color.White);
            brushes[(int)Colour.RED] = new SolidBrush(Color.Red);
            brushes[(int)Colour.BLACK] = new SolidBrush(Color.Black);
            brushes[(int)Colour.BLUE] = new SolidBrush(Color.Blue);
            brushes[(int)Colour.GREEN] = new SolidBrush(Color.Green);
            brushes[(int)Colour.GREY] = new SolidBrush(Color.Gray);

            rarityBrushes[(int)Rarity.Common] = brushes[(int)Colour.GREY];
            rarityBrushes[(int)Rarity.Uncommon] = new SolidBrush(Color.DodgerBlue);
            rarityBrushes[(int)Rarity.Ebin] = brushes[(int)Colour.RED];
            rarityBrushes[(int)Rarity.Legendair] = new SolidBrush(Color.Goldenrod);
            rarityBrushes[(int)Rarity.Xperimental] = new SolidBrush(Color.Fuchsia);

            Visible = true;
            int i = 0;
            MouseEnter += (_, __) =>
            {
                i = Parent.Controls.GetChildIndex(this);
                Parent.Controls.SetChildIndex(this, 0);
            };

            MouseLeave += (_, __) =>
            {
                if (Parent == null) { return; }
                Parent.Controls.SetChildIndex(this, i);
            };
        }
        

        public CardButton(FML arg) : this()
        {
            Click += (object o, EventArgs a) =>
            {
                arg.clickCallBack(this);
            };
            
            MouseEnter += (sender, args) =>
            {
                arg.addArrows(this, targets);
                arg.mouseEnters(this);
            };

            MouseLeave += (sender, args) =>
            {
                arg.clearArrows();
            };
        }

        public virtual void setHeight(int h)
        {
            Size = new Size((int)(0.642f * h), h);
        }

        public void setWidth(int h)
        {
            Size = new Size(h, (int)(h / 0.654f));
        }

        public CardButton(CardId c) : this()
        {
            Card card = new Card(c);
            card.addObserver(this);
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
        
        public void close()
        {
            Card.removeObserver(this);
        }
        
        public virtual void notifyObserver(Observable o, object args)
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
            isExhausted = card.exhausted;
            rarity = card.rarity;
            if (card.attacking)
            {
                borderColor = Color.Red;
            }
            else
            {
                borderColor = null;
            }
            if (card.hasPT())
            {
                hasPT = true;
                power = card.currentPower.ToString();
                toughness = card.currentToughness.ToString();
                isDamaged = card.isDamaged();
            }

            targets = card.stackWrapper?.targets ?? (IEnumerable<Target>)notahack;

            Invalidate();
        }
        
        protected override void OnResize(EventArgs e)
        {
            //base.OnResize(e);

            CardHeight = Size.Height;
            CardWidth = Size.Width;
            ArtHeight =         (int)Math.Round(CardHeight * 0.483857142857143f);
            ArtWidth =          (int)Math.Round(CardHeight * 0.519142857142857f);
            ArtLocationX =      (int)Math.Round(CardHeight * 0.0648571428571429f);
            ArtLocationY =      (int)Math.Round(CardHeight * 0.104142857142857f);
            NameLocationX =     (int)Math.Round(CardHeight * 0.0321428571428571f);
            NameLocationY =     (int)Math.Round(CardHeight * 0.025f);
            NameFontSize =      (int)Math.Round(CardHeight * 0.05f);
            TextFontSize =      (int)Math.Round(CardHeight * 0.0357142857142857f);
            PTFontSize =        (int)Math.Round(CardHeight * 0.1f);
            TypeTextLocationX = (int)Math.Round(CardHeight * 0.0607142857142857f);
            TypeTextLocationY = (int)Math.Round(CardHeight * 0.6f);
            TextLocationX =     (int)Math.Round(CardHeight * 0.075f);
            TextLocationY =     (int)Math.Round(CardHeight * 0.646428571428571f);
            TextWidth =         (int)Math.Round(CardHeight * 0.464285714285714f);
            TextHeight =        (int)Math.Round(CardHeight * 0.192142857142857f);
            ManaOrbLocationX =  (int)Math.Round(CardHeight * 0.55f);
            ManaOrbLocationY =  (int)Math.Round(CardHeight * 0.025f);
            ManaOrbSize =       (int)Math.Round(CardHeight * 0.0392857142857143f);
            ManaOrbPadding =    (int)Math.Round(CardHeight * 0.0504285714285714f);
            GreyCostLocationX = (int)Math.Round(CardHeight * 0.00357142857142857f);
            GreyCostLocationY = (int)Math.Round(CardHeight * -0.00357142857142857f);
            PTAreaSize =        (int)Math.Round(CardHeight * 0.117857142857143f);
            PTTextLocationP =   (int)Math.Round(CardHeight * 0.013f);
            PTTextLocationY =   (int)Math.Round(CardHeight * 0.892857142857143f);
            PTTextLocationT =   (int)Math.Round(CardHeight * 0.554142857142857f);

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
        
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            if (valid)
            {

                Brush b = brushes[(int)Colour.BLACK];

                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                pevent.Graphics.DrawImage(ImageLoader.getFrame(colour, new Size(CardWidth, CardHeight)), new Point(0, 0));
                pevent.Graphics.DrawImage(ImageLoader.getCardArt(art, new Size(ArtWidth, ArtHeight)), ArtLocationX, ArtLocationY);

                pevent.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                pevent.Graphics.DrawString(name, cardNameFont, b, NameLocationX, NameLocationY);
                pevent.Graphics.DrawString(archtype, textFont, b, TypeTextLocationX, TypeTextLocationY);
                pevent.Graphics.DrawString(abilityText, textFont, b, TextRectangle);
                pevent.Graphics.DrawString("A", PTFont, rarityBrushes[(int)rarity] ?? rarityBrushes[(int)Rarity.Xperimental], (PTTextLocationP + PTTextLocationT)/2, PTTextLocationY + PTTextLocationY/55);

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

                    //pevent.Graphics.FillEllipse(p, -w, height - w, 2 * w, 2 * w);
                    //pevent.Graphics.FillEllipse(p, width - w, height - w, 2 * w, 2 * w);

                    pevent.Graphics.DrawString(power, PTFont, pt,
                        PTTextLocationP,
                        PTTextLocationY);

                    pevent.Graphics.DrawString(toughness, PTFont, isDamaged ? damaged : pt,
                        PTTextLocationT,
                        PTTextLocationY);
                }

                if (borderColor != null)
                {
                    using (Pen borderPen = new Pen(borderColor.Value, 8))
                    {
                        pevent.Graphics.DrawRectangle(borderPen, 0, 0, CardWidth, CardHeight);
                    }
                }

                manaBallPen.Dispose();

            }
        }
        private static List<Target> notahack = new List<Target>();
    }
    

    public class SnapCardButton : CardButton
    {
        private const float snapDistance = 0.6f;
        private bool invert;
        public SnapCardButton(FML f, bool invert) : base(f)
        {
            this.invert = invert;
        }
        
        public override void setHeight(int h)
        {
            Size = new Size((int)(0.642f*snapDistance * h), h);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CardHeight = (int)(Size.Height*snapDistance);
            
            CardWidth = (int)(Size.Width);
            ArtHeight = (int)Math.Round(CardHeight * 0.483857142857143f);
            ArtWidth = (int)Math.Round(CardHeight * 0.513142857142857f);
            ArtLocationX = (int)Math.Round(CardHeight * 0.0648571428571429f);
            ArtLocationY = (int)Math.Round(CardHeight * 0.104142857142857f);
            NameLocationX = (int)Math.Round(CardHeight * 0.0321428571428571f);
            NameLocationY = (int)Math.Round(CardHeight * 0.025f);
            NameFontSize = (int)Math.Round(CardHeight * 0.05f);
            TextFontSize = (int)Math.Round(CardHeight * 0.0357142857142857f);
            PTFontSize = (int)Math.Round(CardHeight * 0.1f);
            TypeTextLocationX = (int)Math.Round(CardHeight * 0.0607142857142857f);
            TypeTextLocationY = (int)Math.Round(CardHeight * 0.6f);
            TextLocationX = (int)Math.Round(CardHeight * 0.075f);
            TextLocationY = (int)Math.Round(CardHeight * 0.646428571428571f);
            TextWidth = (int)Math.Round(CardHeight * 0.464285714285714f);
            TextHeight = (int)Math.Round(CardHeight * 0.132142857142857f);
            ManaOrbLocationX = (int)Math.Round(CardHeight * 0.55f);
            ManaOrbLocationY = (int)Math.Round(CardHeight * 0.025f);
            ManaOrbSize = (int)Math.Round(CardHeight * 0.0392857142857143f);
            ManaOrbPadding = (int)Math.Round(CardHeight * 0.0504285714285714f);
            GreyCostLocationX = (int)Math.Round(CardHeight * 0.00357142857142857f);
            GreyCostLocationY = (int)Math.Round(CardHeight * -0.00357142857142857f);
            PTAreaSize = (int)Math.Round(CardHeight * 0.117857142857143f);
            PTTextLocationP = (int)Math.Round(CardHeight * 0.012f);
            PTTextLocationY = (int)Math.Round(CardHeight * 0.892857142857143f);
            PTTextLocationT = (int)Math.Round(CardHeight * 0.555142857142857f);

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
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Matrix transformation = new Matrix();
            if (isExhausted != invert)
            {
                transformation.Translate(0, (int)(Size.Height*(1 - snapDistance)));
            }
            //transformation.Scale(1, 0.8f);
            pevent.Graphics.Transform = transformation;
            base.OnPaint(pevent);
        }
    }

}
