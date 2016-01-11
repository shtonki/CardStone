using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public sealed class CardPanel : Panel, Observer
    {
        //public static int WIDTH = CardButton.WIDTH*6 + 5, HEIGHT = CardButton.HEIGHT + 5;
        public static int WIDTH = 210*6+5, HEIGHT = 285;

        private const int NOOFBUTTONS = 20;
        CardButton[] cardButtons;

        public CardPanel()
        {
            BackColor = Color.Pink;
            Size = new Size(WIDTH, HEIGHT);

            cardButtons = new CardButton[NOOFBUTTONS];

            for (int i = 0; i < NOOFBUTTONS; i++)
            {
                cardButtons[i] = new CardButton();
                Controls.Add(cardButtons[i]);
                cardButtons[i].dbg = i;
            }
        }

        public void notifyObserver(Observable o)
        {
            Form.CheckForIllegalCrossThreadCalls = false; //todo might be a hack bruh, actually entire function is a hack
            Pile p = (Pile)o;

            int padding = 5 + (CardButton.WIDTH < WIDTH/(1 + p.getCards().Count) ? CardButton.WIDTH : WIDTH/(1+p.getCards().Count));

            int i = 0;
            for (; i < p.getCards().Count; i++)
            {
                p.getCards()[i].setObserver(cardButtons[i]); 
                cardButtons[i].Location = new Point(padding*i, 0);
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
