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
        private bool attacking;

        private string name;
        private Cost cost;
        private Type type;
        private Race? race;
        private SubType? subType;
        private Effect resolveEffect;

        private int? power, toughness;
        private bool summoningSick;

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
                    blueCost = 2;
                    power = 1;
                    toughness = 3;
                    type = Type.Creature;
                    race = Race.Salamander;
                } break;

                case CardId.BearCavalary:
                {
                    name = "Bear Cavalary";
                    greenCost = 2;
                    type = Type.Creature;
                    race = Race.Bear;
                    subType = SubType.Warrior;
                    power = 2;
                    toughness = 3;
                } break;

                case CardId.LightningBolt:
                {
                    name = "Lightning Bolt";
                    redCost = 1;
                    type = Type.Instant;
                    //Effect = 
                } break;
            }


            ManaCost mc = new ManaCost(whiteCost, blueCost, blackCost, redCost, greenCost);
            cost = new Cost(mc);

            if ((power == null) != (toughness == null))
            {
                throw new Exception("bad thing b0ss");
            }
        }


        public void setAttacking(bool a)
        {
            attacking = a;
            notifyObserver();
        }

        public void toggleAttacking()
        {
            setAttacking(!attacking);
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

        public bool isCastable()
        {
            return location.getLocation() == Location.HAND;
        }

        public Cost getCost()
        {
            return cost;
        }


        public void unTop()
        {
            setAttacking(false);
            summoningSick = false;
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

        public void moveTo(Pile d)
        {
            Pile p = location.getPile();
            if (p != null) { p.remove(this); }
            d.add(this);
            location = d.getLocation();
            summoningSick = true;
        }


        public bool isAttacking()
        {
            return attacking;
        }

        public bool isInstant()
        {
            return type == Type.Instant;
        }

        public bool hasPT()
        {
            return power != null;
        }

        public int getPower()
        {
            return power.GetValueOrDefault();
        }

        public int getToughness()
        {
            return toughness.GetValueOrDefault();
        }
        
        public bool canAttack()
        {
            return !summoningSick;
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
        //FrenziedPiranha,
        BearCavalary,
        LightningBolt,
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
        Fish,
        Bear
    }

    public enum SubType
    {
        Warrior,
    }
}
