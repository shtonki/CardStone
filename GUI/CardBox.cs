using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public class CardBox : Panel, Observer
    {
        private const int BUTTONS = 30;
        private CardButton[] buttons;

        public CardBox(GameInterface g, int width, int height)
        {
            Size = new Size(width, height);
            BackColor = Color.LightGreen;
            buttons = new CardButton[BUTTONS];

            for (int i = 0; i < BUTTONS; i++)
            {
                CardButton b = new CardButton(g);
                buttons[BUTTONS - i - 1] = b;
                b.Location = new Point(5, -10 + 22 * (BUTTONS - i));
                Controls.Add(b);
                //Controls.SetChildIndex(b, BUTTONS - i);
            }
        }

        public void notifyObserver(Observable o)
        {
            Pile p = (Pile)o;
            var cs = p.getCards();
            int i = 0;

            for (; i < cs.Count; i++)
            {
                cs[i].setObserver(buttons[i]);
                buttons[i].setVisible(true);
                buttons[i].Invalidate();
            }

            for (; i < BUTTONS; i++)
            {
                buttons[i].setVisible(false);
                buttons[i].Invalidate();
            }
        }
    }
}
