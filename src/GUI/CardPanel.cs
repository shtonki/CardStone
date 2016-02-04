using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{

    public struct LayoutArgs
    {
        public readonly bool topDown;
        public readonly bool reverseClippingOrder;
        public readonly int snap;
        public readonly double maxPaddingFactor;

        public LayoutArgs(bool topDown, bool reverseClippingOrder, double maxPaddingFactor = 1.0, int snap = 0)
        {
            this.topDown = topDown;
            this.reverseClippingOrder = reverseClippingOrder;
            this.maxPaddingFactor = maxPaddingFactor;
            this.snap = snap;
        }
    }

    public sealed class CardPanel : Panel, Observer
    {
        private LayoutArgs layoutArgs;
        
        private List<CardButton> cardButtons;

        private Func<CardButton> buttonGenerator;

        public CardPanel(Func<CardButton> buttonGenerator, LayoutArgs args, Pile p = null)
        {
            BackColor = Color.Pink;
            layoutArgs = args;
            cardButtons = new List<CardButton>();
            this.buttonGenerator = buttonGenerator;

            p?.addObserver(this);
        }

        private const int sidePadding = 5;
        private const int maxPerRow = 1000;
        private const double snapDistance = 0.9;

        public void placeButtons()
        {
            if (cardButtons.Count == 0) { return; }

            int width = Size.Width;
            int height = Size.Height;
            int cardWidth = cardButtons[0].Width;
            int cardHeight = cardButtons[0].Height;
            int padding;
            if (cardButtons.Count == 1)
            {
                padding = 0;
            }
            else
            {   
                if (layoutArgs.topDown)
                {
                    padding = Math.Min((height - cardHeight - (sidePadding << 1)) / (cardButtons.Count - 1), (int)(cardHeight*layoutArgs.maxPaddingFactor));
                }
                else
                {
                    padding = Math.Min((width - cardWidth - (sidePadding << 1)) / (cardButtons.Count - 1), (int)(cardWidth * layoutArgs.maxPaddingFactor));
                }
            }

            for (int i = 0; i < cardButtons.Count; i++)
            {
                int x = i % maxPerRow;
                int y = i / maxPerRow;

                if (layoutArgs.topDown)
                {
                    int t = y;
                    y = x;
                    x = t;
                }
                cardButtons[i].Location = new Point(sidePadding + padding * x, sidePadding + padding * y); //hack using one padding
            }

            /*
            if (layoutArgs.topDown)
            {
                int rows = layoutArgs.rows == 0 ? int.MaxValue : layoutArgs.rows;
                
                for (int i = 0; i < cardButtons.Count; i++)
                {
                    int x = i / rows;
                    int y = i % rows;
                    cardButtons[i].Location = new Point(paddingLeft + paddingX * x, paddingTop + paddingY * y);
                }
            }
            else
            {
                int columns = layoutArgs.columns == 0 ? int.MaxValue : layoutArgs.columns;
                for (int i = 0; i < cardButtons.Count; i++)
                {
                    int x = i%columns;
                    int y = i/columns;
                    cardButtons[i].Location = new Point(paddingLeft + paddingX * x, paddingTop + paddingY * y);
                }
            }
            */
            bool interim = layoutArgs.reverseClippingOrder;
            int interimx = cardButtons.Count;
            for (int i = 0; i < interimx; i++)
            {
                cardButtons[interim ? i : interimx - i - 1].BringToFront();
            }
        }

        private void sizeButton(CardButton b)
        {
            int height = Size.Height;
            int width = Size.Width;
            if (layoutArgs.topDown)
            {
                b.setWidth(width - sidePadding * 2);
            }
            else
            {
                b.setHeight(height - sidePadding * 2);
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            //base.OnResize(eventargs);
            
            foreach (CardButton b in cardButtons)
            {
                sizeButton(b);
            }

            placeButtons();
        }

        public void notifyObserver(Observable o, object args)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => invokeMe(o as Pile, args)));
            }
            else
            {
                invokeMe(o as Pile, args);
            }
        }

        private void invokeMe(Pile pile, object args)
        {
            if (args == null) //draw from scratch
            {
                args = new Object[] {pile.cards.ToArray(), true};
                invokeMe(pile, args);
                return;
                //Form.CheckForIllegalCrossThreadCalls = false;
                foreach (CardButton b in cardButtons)
                {
                    Controls.Remove(b);
                }
                cardButtons.Clear();
                foreach (Card c in pile.cards)
                {
                    CardButton b = buttonGenerator();
                    cardButtons.Add(b);
                    Controls.Add(b);
                }
                
                if (pile.cards.Count > cardButtons.Count)
                {
                    throw new SyntaxErrorException();
                }
                
                for (int i = 0; i < pile.cards.Count; i++)
                {
                    pile.cards[i].addObserver(cardButtons[i]);
                    cardButtons[i].setVisible(true);
                    cardButtons[i].Invalidate();
                }
                
            }
            else if (args is object[])
            {
                object[] azs = (object[])args;
                Card[] addUs;
                if (azs[0] is Card[])
                {
                    addUs = (Card[])azs[0];
                }
                else
                {
                    addUs = new Card[] { azs[0] as Card };
                }
                bool add = (bool)azs[1];

                foreach (Card card in addUs)
                {
                    if (add)
                    {
                        CardButton b = buttonGenerator();
                        cardButtons.Add(b);
                        Controls.Add(b);
                        card.addObserver(b);
                        sizeButton(b);
                    }
                    else
                    {
                        for (int i = 0; i < cardButtons.Count; i++)
                        {
                            if (cardButtons[i].Card == card)
                            {
                                Controls.Remove(cardButtons[i]);
                                cardButtons[i].close();
                                cardButtons.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            placeButtons();
        }

        public void close()
        {
            foreach (CardButton c in cardButtons)
            {
                c.close();
            }
        }
        
    }
    
}
