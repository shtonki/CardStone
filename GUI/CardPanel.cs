using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public struct CardPanelArgs
    {
        public readonly int width;
        public readonly int height;
        public readonly int paddingX;
        public readonly int paddingY;
        public readonly int borderLeft;
        public readonly int borderRight;
        public readonly int borderTop;
        public readonly int borderBottom;
        public readonly int columns;
        public readonly int rows;
    }

    public sealed class CardPanel : Panel, Observer
    {
        //public static int WIDTH = CardButton.WIDTH*6 + 5, HEIGHT = CardButton.HEIGHT + 5;
        public static int WIDTH = 210*6+5, HEIGHT = 285;

        

        private const int NOOFBUTTONS = 20;
        CardButton[] cardButtons;

        public CardPanel(int buttons, CardPanelArgs args, Func<CardButton> buttonGenerator)
        {
            BackColor = Color.Pink;
            Size = new Size(WIDTH, HEIGHT);

            cardButtons = new CardButton[NOOFBUTTONS];

            for (int i = 0; i < NOOFBUTTONS; i++)
            {
                cardButtons[i] = buttonGenerator();
                cardButtons[i].Location = new Point(5 + 183 * i, 0);
                Controls.Add(cardButtons[i]);
            }
        }

        public void notifyObserver(Observable o)
        {
            Form.CheckForIllegalCrossThreadCalls = false; //todo might be a hack bruh, actually entire function is a hack
            Pile p = (Pile)o;

            int height = Size.Height,
                width = Size.Width;
            

            int i = 0;
            for (; i < p.cards.Count; i++)
            {
                p.cards[i].setObserver(cardButtons[i]); 
                cardButtons[i].setVisible(true);
                cardButtons[i].Invalidate();
            }

            for (; i < NOOFBUTTONS; i++)
            {
                cardButtons[i].setVisible(false);
            }
            Form.CheckForIllegalCrossThreadCalls = true;
        }
    }
}
