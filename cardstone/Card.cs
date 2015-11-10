using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public class Card : Observable
    {
        private static int idCtr = 0;

        private int id;
        private CardId cardId;
        private Location location;

        private string name;
        private Cost cost;

        public Card(CardId c, Location l) : this(c)
        {
            location = l;
        }

        public Card(CardId c)
        {
            cardId = c;
            location = new Location(Location.NOWHERE);
            id = idCtr++;

            int redCost = 0, greenCost = 0, whiteCost = 0, blackCost = 0, blueCost = 0;

            switch (cardId)
            {
                case CardId.KAPPA:
                {
                    name = "Kappa";
                    redCost = 1;
                } break;

                case CardId.KEEPO:
                {
                    name = "Keepo";
                    whiteCost = 1;
                } break;
            }

            ManaCost mc = new ManaCost(whiteCost, blueCost, blackCost, redCost, greenCost);
            cost = new Cost(mc);
        }

        public int getId()
        {
            return id;
        }

        public string getName()
        {
            return name;
        }

        public ManaCost getManaCost()
        {
            return cost.getManaCost();
        }

        public void moveTo(Location l)
        {
            Pile p = l.getPile();
            moveTo(p);
        }

        public void moveTo(Pile d)
        {
            Pile p = location.getPile();
            if (p != null) { p.remove(this); }
            d.add(this);
            location = d.getLocation();
        }

        public Image getArt()
        {
            return ImageLoader.getCardArt(cardId);
        }

        public Image getFrame()
        {
            return ImageLoader.getFrame();
        }
    }
    public enum CardId
    {
        KAPPA,
        KEEPO,
    }
}
