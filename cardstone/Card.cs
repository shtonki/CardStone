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

        private int? power, toughness, currentPower, currentToughness;
        private bool summoningSick;

        private List<Ability> abilities;
        private ManaCoster castingCost;
        private List<KeyAbility> keyAbilities; 

        public Card(CardId c, Location l)
            : this(c)
        {
            location = l;
        }

        public Card(CardId c)
        {
            cardId = c;
            location = new Location(Location.NOWHERE);

            List<Effecter> fx = new List<Effecter>();
            
            keyAbilities = new List<KeyAbility>();

            int redCost = 0, greenCost = 0, whiteCost = 0, blackCost = 0, blueCost = 0;

            string s = cardId.ToString();
            StringBuilder b = new StringBuilder();
            b.Append(s[0]);
            for (int i = 1; i < s.Length; i++)
            {
                char ch = s[i];
                if ((byte)ch <= 90) { b.Append(' '); }

                b.Append(ch);
            }

            name = b.ToString();

            switch (cardId)
            {
                case CardId.Kappa:
                {
                    blueCost = 2;
                    power = 1;
                    toughness = 3;
                    type = Type.Creature;
                    race = Race.Salamander;
                } break;

                case CardId.BearCavalary:
                {
                    greenCost = 2;
                    type = Type.Creature;
                    race = Race.Bear;
                    subType = SubType.Warrior;
                    power = 2;
                    toughness = 3;
                } break;

                case CardId.LightningBolt:
                {
                    redCost = 1;
                    type = Type.Instant;
                    fx.Add(new PingN(1,3));
                } break;

                case CardId.ForkedLightning:
                {
                    redCost = 1;
                    type = Type.Sorcery;
                    fx.Add(new PingN(2, 1));
                } break;

                case CardId.SolemnAberration:
                {
                    blackCost = 1;
                    type = Type.Creature;
                    race = Race.Zombie;
                    power = 2;
                    toughness = 2;
                    //todo can't block
                } break;

                case CardId.PropheticVision:
                {
                    blueCost = 1;
                    type = Type.Sorcery;
                    fx.Add(new OwnerDrawsEffecter(2));
                } break;

                case CardId.FrothingGoblin:
                {
                    redCost = 1;
                    type = Type.Creature;
                    power = 1;
                    toughness = 1;
                    keyAbilities.Add(KeyAbility.Fervor);
                } break;

            }

            if (power != null)
            {
                currentPower = power;
                currentToughness = toughness;
            }

            abilities = new List<Ability>();

            Effect x = new Effect(fx.ToArray());
            castingCost = new ManaCoster(whiteCost, blueCost, blackCost, redCost, greenCost);
            Cost cc = new Cost(castingCost);
            ActivatedAbility castAbility = new ActivatedAbility(this, cc, x);
            castAbility.setInstant(type == Type.Instant);

            abilities.Add(castAbility);

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
            return castingCost;
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

        public Ability getAbility(int i)
        {
            return abilities[i];
        }

        public bool isDummy()
        {
            return false;
        }

        public void setLocationRaw(Location l)
        {
            location = l;
        }

        public void setOwner(Player p)
        {
            owner = p;
        }



        public Type getType()
        {
            return type;
        }

        public List<ActivatedAbility> getAvailableActivatedAbilities(bool canSorc)
        {
            List<ActivatedAbility> r = new List<ActivatedAbility>();

            foreach (var v in abilities)
            {
                if (!(v is ActivatedAbility)) { continue; }
                ActivatedAbility a = v as ActivatedAbility;
                if (!a.castableFrom(location.getLocation())) { continue; }
                if (!canSorc && !a.isInstant()) { continue; }

                r.Add(a);
            }

            return r;
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

        public void damage(int d)
        {
            currentToughness -= d;
            notifyObserver();
        }


        
        public bool isAttacking()
        {
            return attacking;
        }

        public bool hasPT()
        {
            return power != null;
        }

        public int getCurrentPower()
        {
            return currentPower.GetValueOrDefault();
        }

        public int getCurrentToughness()
        {
            return currentToughness.GetValueOrDefault();
        }

        public bool isDamaged()
        {
            return currentToughness != toughness;
        }

        public bool canAttack()
        {
            return location.getLocation() == Location.FIELD && (!summoningSick || has(KeyAbility.Fervor));
        }

        public bool has(KeyAbility a)
        {
            return keyAbilities.Contains(a);
        }


        public int getAbilityIndex(Ability a)
        {
            return abilities.FindIndex(v => v == a);
        }

        public Ability getAbilityByIndex(int ix)
        {
            return abilities[ix];
        }

        public Image getArt()
        {
            return ImageLoader.getCardArt(cardId);
        }

        public Image getFrame()
        {
            return ImageLoader.getFrame();
        }

        public string getArchtypeString()
        {
            return type.ToString() +
                (race != null ? " - " + race.ToString() + " " : "") +
                (subType != null ? subType.ToString() : "");
        }

        public string getAbilitiesString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(abilities[0].getExplanation());

            foreach (var v in keyAbilities)
            {
                b.AppendLine(v.ToString());
            }

            return b.ToString();
        }

    }
    public enum CardId
    {
        Kappa,
        //FrenziedPiranha,
        BearCavalary,
        LightningBolt,
        SolemnAberration,
        PropheticVision,
        ForkedLightning,
        FrothingGoblin,
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
        Zombie,
        Goblin,
    }

    public enum SubType
    {
        Warrior,
        Wizard,
    }

    public enum KeyAbility
    {
        Fervor,
    }
}