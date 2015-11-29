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
        private int id;
        private CardId cardId;
        private Location location;
        private Player owner, controller;

        private bool attacking;

        private string name;
        //private Cost cost;
        private Type type;
        private Race? race;
        private SubType? subType;
        private Effect resolveEffect;

        private int? power, toughness;
        private bool summoningSick;

        private Ability[] abilities;
        private Ability castAbility;

        public Card(CardId c, Location l)
            : this(c)
        {
            location = l;
        }

        public Card(CardId c)
        {
            cardId = c;
            location = new Location(Location.NOWHERE);

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

                case CardId.SolemnAberration:
                {
                    name = "Solemn Aberration";
                    blackCost = 1;
                    type = Type.Creature;
                    race = Race.Zombie;
                    power = 2;
                    toughness = 2;
                    //todo can't block
                } break;
            }


            ManaCoster mc = new ManaCoster(whiteCost, blueCost, blackCost, redCost, greenCost);
            CastingCost cost = new CastingCost(mc);
            Effect e = new Effect(new StackResolve());
            castAbility = new Ability(cost, e);

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

        public void setId(int i)
        {
            id = i;
        }

        public string getName()
        {
            return name;
        }

        public ManaCoster getManaCost()
        {
            return getCastingCost().getManaCost();
        }

        public bool isCastable()
        {
            return location.getLocation() == Location.HAND;
        }

        public CastingCost getCastingCost()
        {
            return castAbility.cost as CastingCost;
        }

        public Player getOwner()
        {
            return owner;
        }

        public Player getController()
        {
            return owner;
        }

        public Location getLocation()
        {
            return location;
        }

        public void resolve(Game g)
        {
            moveTo(owner.getField());
        }

        public Type getType()
        {
            return type;
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

        public void moveTo(Pile d)
        {
            Pile p = location.getPile();
            if (p != null) { p.remove(this); }
            d.add(this);
            location = d.getLocation();
            summoningSick = true;
        }

        public void setLocationRaw(Location l)
        {
            location = l;
        }

        public void setOwner(Player p)
        {
            owner = p;
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
            return type.ToString() +
                (race != null ? " - " + race.ToString() + " " : "") +
                (subType != null ? subType.ToString() : "");
        }

    }
    public enum CardId
    {
        Kappa,
        //FrenziedPiranha,
        BearCavalary,
        LightningBolt,
        SolemnAberration,
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
        Bear,
        Zombie
    }

    public enum SubType
    {
        Warrior,
        Wizard,
    }
}