using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Type type;
        private Race? race;
        private SubType? subType;

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
                case CardId.Kappa:
                {
                    name = "Kappa";
                    blueCost = 1;
                    type = Type.Creature;
                    race = Race.Salamander;
                } break;

                case CardId.FrenziedPiranha:
                {
                    name = "Frenzied Piranha";
                    blueCost = 2;
                    type = Type.Creature;
                    race = Race.Fish;
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

        public Cost getCost()
        {
            return cost;
        }

        public void moveTo(Location l)
        {
            Pile p = l.getPile();
            moveTo(p);
        }

        public void moveToOwners(byte i)
        {
            Pile p = Location.getPile(i, location.getSide());
            moveTo(p);
        }

        public bool isCastable()
        {
            return location.getLocation() == Location.HAND;
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

        public String getArchtypeString()
        {
            return type.ToString() + " - " + 
                (race != null ? race.ToString() + " " : "") + 
                (subType != null ? subType.ToString() : "");
        }
    }
    public enum CardId
    {
        Kappa,
        FrenziedPiranha,
    }

    public enum Type
    {
        Creature,
        Instant, 
        Sorcery,
        Relic
    }

    public enum Race
    {
        Human,
        Salamander,
        Fish
    }

    public enum SubType
    {
        Warrior,
    }
}
