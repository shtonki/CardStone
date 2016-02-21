using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace stonekart
{
    public class DraftPanel : DisplayPanel
    {
        private CardPanel choices;
        private Button dealEm;

        private Pile cards;

        public DraftPanel()
        {
            BackColor = Color.DodgerBlue;

            cards = new Pile(new Card[0]);
            choices = new CardPanel(() => new CardButton(new FML(clicked
                )), new LayoutArgs(false, false), cards);
            Controls.Add(choices);

            dealEm = new Button();
            dealEm.Click += (_, __) => dealm();
            Controls.Add(dealEm);
        }

        private void clicked(CardButton b)
        {
            //b.Card.attacking = !b.Card.attacking;
        }

        private void dealm()
        {
            cards.clear();
            CardId[] pack = newPack();
            foreach (CardId i in pack)
            {
                cards.add(new Card(i));
            }
        }

        private CardId[] newPack()
        {
            Random rando = new Random();
            CardId[] r = new CardId[10];
            CardId[] enm = (CardId[])Enum.GetValues(typeof (CardId));
            CardId[] commons = enm.Where(id => Card.rarityOf(id) == Rarity.Common).ToArray();
            CardId[] uncommons = enm.Where(id => Card.rarityOf(id) == Rarity.Uncommon).ToArray();
            CardId[] ebins = enm.Where(id => Card.rarityOf(id) == Rarity.Ebin).ToArray();
            CardId[] legens = enm.Where(id => Card.rarityOf(id) == Rarity.Legendair).ToArray();
            int i = 0;
            while (i < 6)
            {
                r[i++] = commons[rando.Next(commons.Length)];
            }
            while (i < 9)
            {
                r[i++] = uncommons[rando.Next(uncommons.Length)];
            }
            while (i < 10)
            {
                if (rando.Next(13) == 0)
                {
                    r[i++] = legens[rando.Next(legens.Length)];
                }
                else
                {
                    r[i++] = ebins[rando.Next(ebins.Length)];
                }
            }
            return r;
        }
        

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            int width = Size.Width;
            int height = Size.Height;

            choices.Size = new Size(width, height/3);
            choices.Location = new Point(0, 0);

            dealEm.Size = new Size(width/15, height/15);
            dealEm.Location = new Point(width/2, height/2);
        }
    }
}
