using System;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    public class FieldPanel : Panel, GameUIElement, Observer
    {
        private SnapCardButton[] buttons;
        private const int BUTTONS = 10;

        public FieldPanel(GameInterface g, bool asHero)
        {
            Size = new Size(900, 320);
            BackColor = Color.DarkKhaki;

            buttons = new SnapCardButton[BUTTONS];

            for (int i = 0; i < BUTTONS; i++)
            {
                buttons[i] = new SnapCardButton(g, asHero);
                buttons[i].setLocation(5 + 183 * i, asHero ? 38 : 0);
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

    //todo seba move this to where it should be
    class SnapCardButton : CardButton, Observer
    {
        private Point def, att;

        private int xdd;

        public SnapCardButton(GameInterface g, bool b) : base(g)
        {
            xdd = b ? -40 : 40;
        }

        public new void notifyObserver(Observable o)
        {
            base.notifyObserver(o);

            Card c = (Card)o;
            Invoke(new Action(() => { Location = c.topped ? att : def; }));
        }

        public void setLocation(int x, int y)
        {
            def = new Point(x, y);
            att = new Point(x, y + xdd);
            Location = def;
        }
    }
}
