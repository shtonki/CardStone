using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public struct CardPanelArgs
    {
        public int width;
        public int height;
        public int paddingX;
        public int paddingY;
        public int borderLeft;
        public int borderRight;
        public int borderTop;
        public int borderBottom;
        public int columns;
        public int rows;

        public CardPanelArgs(int width, int height, int paddingX, int paddingY, int borderLeft, int borderRight, int borderTop, int borderBottom, int columns, int rows)
        {
            this.width = width;
            this.height = height;
            this.paddingX = paddingX;
            this.paddingY = paddingY;
            this.borderLeft = borderLeft;
            this.borderRight = borderRight;
            this.borderTop = borderTop;
            this.borderBottom = borderBottom;
            this.columns = columns;
            this.rows = rows;
        }
    }

    public sealed class CardPanel : Panel, Observer
    {
        //public static int WIDTH = CardButton.WIDTH*6 + 5, HEIGHT = CardButton.HEIGHT + 5;
        public static int WIDTH = 210*6+5, HEIGHT = 285;


        
        CardButton[] cardButtons;

        public CardPanel(int buttons, Func<CardButton> buttonGenerator)
        {
            BackColor = Color.Pink;

            cardButtons = new CardButton[buttons];

            for (int i = 0; i < buttons; i++)
            {
                cardButtons[i] = buttonGenerator();
                Controls.Add(cardButtons[i]);
            }
        }

        public void layoutButtons(CardPanelArgs a)
        {
            Size = new Size(a.width, a.height);

            for (int i = 0; i < cardButtons.Length; i++)
            {
                cardButtons[i].Location = new Point(5 + 183 * i, 0);
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
