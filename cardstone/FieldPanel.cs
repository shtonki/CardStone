using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    class FieldPanel : Panel, Foo, Observer
    {
        private CardButton[] buttons;
        private const int BUTTONS = 10;

        public FieldPanel()
        {
            BackColor = Color.IndianRed;
            Size = new Size(900, 300);

            buttons = new CardButton[BUTTONS];

            for (int i = 0; i < BUTTONS; i++)
            {
                buttons[i] = new CardButton();
                buttons[i].Location = new Point(5 + 183 * i, 5);
                Controls.Add(buttons[i]);
            }
        }


        public void notifyObserver(Observable o)
        {
            /*
            Pile p = (Pile)o;

            int i = 0;
            for (; i < p.getCards().Count; i++)
            {
                p.getCards()[i].setObserver(cardButtons[i]);
                cardButtons[i].Location = new Point(padding * i, 0);
                cardButtons[i].setVisible(true);
                cardButtons[i].Invalidate();
            }

            for (; i < NOOFBUTTONS; i++)
            {
                cardButtons[i].setVisible(false);
            }
             */
        }
    }
}
