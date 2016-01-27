﻿using System;
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
        public Location location { get; set; }
        public Player owner { get; private set; }
        public Player controller { get; private set; }

        private bool Attacking;
        private Card DefenderOf;
        private Card DefendedBy;
        
        private bool _topped;
        public bool topped
        {
            get { return _topped; }
            set
            {
                _topped = value;
                notifyObserver();
            }
        }

        private string name;
        private Type type;
        private Race? race;
        private SubType? subType;
        public StackWrapper stackWrapper;

        private int? basePower, baseToughness, CurrentPower, CurrentToughness;
        public int currentPower => CurrentPower.GetValueOrDefault();
        public int currentToughness => CurrentToughness.GetValueOrDefault();
        public bool summoningSick { get; set; }
        public bool attacking
        {
            get { return Attacking; }
            set
            {
                Attacking = value;
                notifyObserver();
            }
        }

        public bool inCombat => defended || defending;
        public bool defended => defendedBy != null;
        public bool defending => defenderOf != null;
        public Card defenderOf
        {
            get { return DefenderOf; }
            set
            {
                DefenderOf = value;
                notifyObserver();
            }
        }

        public Card defendedBy
        {
            get { return DefendedBy; }
            set
            {
                DefendedBy = value;
                notifyObserver();
            }
        }

        private List<Ability> abilities;
        private ManaCoster castingCost;
        private List<KeyAbility> keyAbilities; 
        
        //todo(seba) move this entire constructor to a XML document
        public Card(CardId c)
        {
            cardId = c;
            location = null;

            List<SubEffect> fx = new List<SubEffect>();
            
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
                    basePower = 1;
                    baseToughness = 3;
                    type = Type.Creature;
                    race = Race.Salamander;
                } break;

                case CardId.BearCavalary:
                {
                    greenCost = 2;
                    type = Type.Creature;
                    race = Race.Bear;
                    subType = SubType.Warrior;
                    basePower = 2;
                    baseToughness = 3;
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
                    basePower = 2;
                    baseToughness = 2;
                } break;

                case CardId.PropheticVision:
                {
                    blueCost = 1;
                    type = Type.Sorcery;
                    fx.Add(new OwnerDrawsSubEffect(2));
                } break;

                case CardId.FrothingGoblin:
                {
                    redCost = 1;
                    type = Type.Creature;
                    basePower = 2;
                    baseToughness = 2;
                    keyAbilities.Add(KeyAbility.Fervor);
                } break;

                case CardId.TempleCleric:
                {
                    whiteCost = 1;
                    type = Type.Creature;
                    basePower = 1;
                    EventFilter e = vanillaETB;
                } break;

                default:
                {
                    throw new Exception("pls no");
                }
            }

            if (basePower != null)
            {
                CurrentPower = basePower;
                CurrentToughness = baseToughness;
            }

            abilities = new List<Ability>();

            Effect x = new Effect(fx.ToArray());
            castingCost = new ManaCoster(whiteCost, blueCost, blackCost, redCost, greenCost);
            Cost cc = new Cost(castingCost);
            ActivatedAbility castAbility = new ActivatedAbility(this, cc, x, LocationPile.HAND);
            castAbility.setInstant(type == Type.Instant);

            abilities.Add(castAbility);

            if ((basePower == null) != (baseToughness == null))
            {
                throw new Exception("bad thing b0ss");
            }
        }


        private bool vanillaETB(GameEvent e)
        {
            if (e.type != GameEventType.MOVECARD) { return false; }
            MoveCardEvent moveEvent = (MoveCardEvent)e;

            return moveEvent.card == this && moveEvent.to.pile == LocationPile.FIELD;
        }
        private EventFilter entersBattlefieldFilterLambda(params EventFilter[] fs)
        {


            return null;
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


        public Player getController()
        {
            return owner;
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
                if (!a.castableFrom(location.pile)) { continue; }
                if (!canSorc && !a.isInstant()) { continue; }

                r.Add(a);
            }

            return r;
        }

        
        
        public void damage(int d)
        {
            CurrentToughness -= d;
            notifyObserver();
        }
        

        public bool isTopped()
        {
            return topped;
        }

        public bool hasPT()
        {
            return basePower != null;
        }

        

        public bool isDamaged()
        {
            return CurrentToughness != baseToughness;
        }

        public bool canAttack()
        {
            return location.pile == LocationPile.FIELD && (!summoningSick || has(KeyAbility.Fervor));
        }

        public bool canDefend()
        {
            return location.pile == LocationPile.FIELD;
        }

        public bool has(KeyAbility a)
        {
            return keyAbilities.Contains(a);
        }

        public void moveReset()
        {
            CurrentPower = basePower;
            CurrentToughness = baseToughness;
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
        TempleCleric,
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