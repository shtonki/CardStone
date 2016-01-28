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
    public struct PlacementArgs
    {
        public int paddingX;
        public int paddingY;
        public int paddingLeft;
        public int paddingTop;

        public PlacementArgs(int paddingX, int paddingY, int paddingLeft, int paddingTop)
        {
            this.paddingX = paddingX;
            this.paddingY = paddingY;
            this.paddingLeft = paddingLeft;
            this.paddingTop = paddingTop;
        }
    }

    public struct LayoutArgs
    {
        public readonly int columns;
        public readonly int rows;

        public readonly bool topDown;
        public readonly bool reverseClippingOrder;

        public LayoutArgs(int columns, int rows, bool topDown, bool reverseClippingOrder)
        {
            this.columns = columns;
            this.rows = rows;
            this.topDown = topDown;
            this.reverseClippingOrder = reverseClippingOrder;
        }
    }

    public sealed class CardPanel : Panel, Observer
    {
        //public static int WIDTH = CardButton.WIDTH*6 + 5, HEIGHT = CardButton.HEIGHT + 5;
        public static int WIDTH = 210*6+5, HEIGHT = 285;

        private LayoutArgs layoutArgs;
        
        CardButton[] cardButtons;

        public CardPanel(int buttons, Func<CardButton> buttonGenerator, LayoutArgs args)
        {
            BackColor = Color.Pink;
            layoutArgs = args;
            cardButtons = new CardButton[buttons];

            for (int i = 0; i < buttons; i++)
            {
                int n = 0;
                var button = buttonGenerator();
                cardButtons[i] = button;
                button.BringToFront();
                Controls.Add(button);

                if (true) //bring to front
                {
                    button.MouseEnter += (_, __) =>
                    {
                        n = Controls.GetChildIndex(button);
                        Controls.SetChildIndex(button, 0);
                    };

                    button.MouseLeave += (_, __) =>
                    {
                        Controls.SetChildIndex(button, n);
                    };
                }
            }
        }

        public void placeButtons(PlacementArgs a)
        {
            if (layoutArgs.topDown)
            {
                int rows = layoutArgs.rows == 0 ? int.MaxValue : layoutArgs.rows;
                
                for (int i = 0; i < cardButtons.Length; i++)
                {
                    int x = i / rows;
                    int y = i % rows;
                    cardButtons[i].Location = new Point(a.paddingLeft + a.paddingX * x, a.paddingTop + a.paddingY * y);
                }
            }
            else
            {
                int columns = layoutArgs.columns == 0 ? int.MaxValue : layoutArgs.columns;
                for (int i = 0; i < cardButtons.Length; i++)
                {
                    int x = i%columns;
                    int y = i/columns;
                    cardButtons[i].Location = new Point(a.paddingLeft + a.paddingX * x, a.paddingTop + a.paddingY * y);
                }
            }

            bool interim = layoutArgs.reverseClippingOrder;
            int interimx = cardButtons.Length;
            for (int i = 0; i < interimx; i++)
            {
                cardButtons[interim ? i : interimx - i - 1].BringToFront();
            }
        }


        public void notifyObserver(Observable o)
        {
            Form.CheckForIllegalCrossThreadCalls = false; //todo might be a hack bruh, actually entire function is a hack
            Pile p = (Pile)o;

            int height = Size.Height,
                width = Size.Width;

            if (p.cards.Count > cardButtons.Length)
            {
                throw new SyntaxErrorException();
            }

            int i = 0;
            for (; i < p.cards.Count; i++)
            {
                p.cards[i].setObserver(cardButtons[i]); 
                cardButtons[i].setVisible(true);
                cardButtons[i].Invalidate();
            }

            for (; i < cardButtons.Length; i++)
            {
                cardButtons[i].setVisible(false);
            }
            Form.CheckForIllegalCrossThreadCalls = true;
        }
    }
}
