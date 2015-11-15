using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    class FieldPanel : Panel, Foo, Observer
    {
        private SnapCardButton[] buttons;
        private const int BUTTONS = 10;

        public FieldPanel()
        {
            BackColor = Color.IndianRed;
            Size = new Size(900, 320);

            buttons = new SnapCardButton[BUTTONS];

            for (int i = 0; i < BUTTONS; i++)
            {
                buttons[i] = new SnapCardButton();
                buttons[i].setLocation(5 + 183 * i, 38);
                Controls.Add(buttons[i]);
                buttons[i].setVisible(false);
            }
        }


        public void notifyObserver(Observable o)
        {
            Pile p = (Pile)o;

            int i = 0;
            for (; i < p.getCards().Count; i++)
            {
                p.getCards()[i].setObserver(buttons[i]);
                buttons[i].setVisible(true);
                buttons[i].Invalidate();
            }

            for (; i < BUTTONS; i++)
            {
                buttons[i].setVisible(false);
            }
        }
    }
}
