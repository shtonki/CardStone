using System;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    public class FieldPanel : Panel, Observer
    {
        private CardButton[] buttons;
        private const int BUTTONS = 10;

        public FieldPanel(GameInterface g, bool asHero)
        {
            Size = new Size(900, 320);
            BackColor = Color.DarkKhaki;

            buttons = new CardButton[BUTTONS];

            for (int i = 0; i < BUTTONS; i++)
            {
                SnapCardButton b = new SnapCardButton(g, asHero ? 40 : -40);
                b.Location = new Point(5 + 183 * i, asHero ? 38 : 0);
                buttons[i] = b;
                Controls.Add(buttons[i]);
                buttons[i].setVisible(false);
            }
        }


        public void notifyObserver(Observable o)
        {
            Pile p = (Pile)o;

            int i = 0;
            for (; i < p.cards.Count; i++)
            {
                p.cards[i].setObserver(buttons[i]);
                buttons[i].setVisible(true);
                buttons[i].Invalidate();
            }

            for (; i < BUTTONS; i++)
            {
                buttons[i].setVisible(false);
            }
        }
    }
    
    class SnapCardButton : CardButton, Observer
    {
        private Point def, att;

        private int xdd;

        private bool dirty;

        public SnapCardButton(GameInterface g, int i) : base(g)
        {
            LocationChanged += (sender, args) => setLocation();
            xdd = i;
        }

        public new void notifyObserver(Observable o)
        {
            base.notifyObserver(o);

            Card c = (Card)o;
            Invoke(new Action(() => { dirty = true; Location = c.topped ? att : def; }));
        }
        

        private void setLocation()
        {
            if (dirty)
            {
                dirty = false;
                return;
            }

            int x = Location.X;
            int y = Location.Y;
            def = new Point(x, y);
            att = new Point(x, y + xdd);
        }
    }
}
