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
        public CardId cardId { get; private set; }
        public Location location { get; set; }
        public Player owner { get; set; }
        public Player controller { get; set; }

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
        public readonly ManaColour colour;
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

        public bool isDummy => dummyFor != null;
        public Ability dummyFor { get; private set; }

        //private List<Ability> abilities;
        private readonly List<ActivatedAbility> baseActivatedAbilities;
        private readonly List<TriggeredAbility> baseTriggeredAbilities;
        public ManaCost castingCost { get; private set; }
        private List<KeyAbility> keyAbilities;

        public List<Ability> abilities => ((IEnumerable<Ability>)activatedAbilities).Concat(triggeredAbilities).ToList();
        public List<ActivatedAbility> activatedAbilities => baseActivatedAbilities;
        public List<TriggeredAbility> triggeredAbilities => baseTriggeredAbilities;

        protected Card()
        {
            baseActivatedAbilities = new List<ActivatedAbility>();
            baseTriggeredAbilities = new List<TriggeredAbility>();
            keyAbilities = new List<KeyAbility>();
        }

        //todo(seba) move this entire constructor to a XML document
        public Card(CardId c)
        {
            cardId = c;
            location = null;

            List<SubEffect> fx = new List<SubEffect>();
            
            keyAbilities = new List<KeyAbility>();
            string castDescription = "";
            int redCost = 0, greenCost = 0, whiteCost = 0, blackCost = 0, blueCost = 0, greyCost = 0;

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
            baseActivatedAbilities = new List<ActivatedAbility>();
            baseTriggeredAbilities = new List<TriggeredAbility>();

            ManaColour? forceColour = null;

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
                    castDescription = "Deal 3 damage to target player or creature.";
                } break;

                case CardId.ForkedLightning:
                {
                    redCost = 1;
                    type = Type.Sorcery;
                    fx.Add(new PingN(2, 1));
                    castDescription = "Deal 2 damage to 2 target players or creatures.";
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
                    fx.Add(new OwnerDraws(2));
                    castDescription = "Draw 2 cards";
                } break;

                case CardId.FrothingGoblin:
                {
                    redCost = 1;
                    type = Type.Creature;
                    basePower = 2;
                    baseToughness = 2;
                    keyAbilities.Add(KeyAbility.Fervor);
                } break;

                case CardId.TempleHealer:
                {
                    whiteCost = 3;
                    greyCost = 1;
                    type = Type.Creature;
                    race = Race.Human;
                    subType = SubType.Cleric;
                    basePower = 4;
                    baseToughness = 4;
                    EventFilter e = vanillaETB;
                    baseTriggeredAbilities.Add(new TriggeredAbility(this, 
                        friendlyETB, 
                        underYourControlETBDescription + "gain 1 life.", 
                        LocationPile.FIELD, EventTiming.Post, new GainLife(1)));
                } break;

                case CardId.Rapture:
                {
                    whiteCost = 3;
                    type = Type.Instant;
                    fx.Add(new ExileTarget());
                    castDescription = "Exile target creature";
                } break;

                case CardId.CallToArms:
                {
                    whiteCost = 1;
                    type = Type.Sorcery;
                    fx.Add(new SummonNTokens(2, CardId.Squire));
                    castDescription = "Summon two Squires.";
                } break;

                case CardId.Squire:
                {
                    type = Type.Token;
                    race = Race.Human;
                    baseToughness = 1;
                    basePower = 1;
                    forceColour = ManaColour.WHITE;
                } break;

                case CardId.ShimmeringKoi:
                {
                    blueCost = 2;
                    greyCost = 2;
                    type = Type.Creature;
                    race = Race.Fish;
                    basePower = 2;
                    baseToughness = 3;
                        baseTriggeredAbilities.Add(new TriggeredAbility(this,
                            thisETB(this),
                            thisETBDescription + "draw a card.",
                            LocationPile.FIELD, EventTiming.Post,
                            new OwnerDraws(1)
                            ));
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


            Effect x = new Effect(fx.ToArray());
            castingCost = new ManaCost(whiteCost, blueCost, blackCost, redCost, greenCost, greyCost);
            Cost cc = new Cost(castingCost);
            ActivatedAbility castAbility = new ActivatedAbility(this, cc, x, LocationPile.HAND, castDescription);
            castAbility.setInstant(type == Type.Instant);

            baseActivatedAbilities.Add(castAbility);

            if ((basePower == null) != (baseToughness == null))
            {
                throw new Exception("bad thing b0ss");
            }

            var vs = castingCost.costsEnumerable;
            int n = vs.TakeWhile(v => v == 0).Count();  //spooky LINQ that probably breaks at some point 
            if (n == 6)
            {
                if (!forceColour.HasValue)
                {
                    throw new Exception();
                }
                colour = forceColour.Value;
            }
            else
            {
                colour = (ManaColour)n;
            }
        }

        #region commonEventFilters

        private bool vanillaETB(GameEvent e)
        {
            if (e.type != GameEventType.MOVECARD) { return false; }
            MoveCardEvent moveEvent = (MoveCardEvent)e;

            return moveEvent.to.pile == LocationPile.FIELD;
        }

        private const string underYourControlETBDescription =
            "Whenever a creature enters the battlefield under your control ";
        private bool friendlyETB(GameEvent e)
        {
            if (e.type != GameEventType.MOVECARD) { return false; }
            MoveCardEvent moveEvent = (MoveCardEvent)e;

            return moveEvent.to.pile == LocationPile.FIELD && moveEvent.card.controller == this.controller;
        }

        private string thisETBDescription = "Whenever this card enters the battlefield, ";
        private EventFilter thisETB(Card c)
        {
            return new EventFilter(@e =>
            {
                if (e.type != GameEventType.MOVECARD) { return false; }
                MoveCardEvent moveEvent = (MoveCardEvent)e;

                return moveEvent.to.pile == LocationPile.FIELD && moveEvent.card == c;
            });
        }

        #endregion

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

        public ManaCost getManaCost()
        {
            return castingCost;
        }


        public void setLocationRaw(Location l)
        {
            location = l;
        }
        



        public Type getType()
        {
            return type;
        }

        public IList<ActivatedAbility> getAvailableActivatedAbilities(bool canSorc)
        {
            return activatedAbilities.Where(@a =>
                (canSorc || a.isInstant()) &&
                a.castableFrom(location.pile)).ToList();

            /*
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
            */
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
            summoningSick = true;
        }

        //hack assumes that abilities looks the same for both the players
        //if this ever bugs that is most likely why
        public int getAbilityIndex(Ability a)
        {
            int r = 0;
            foreach (Ability v in abilities)
            {
                if (a == v)
                {
                    return r;
                }
                r++;
            }

            throw new ArgumentException();
        }

        public Ability getAbilityByIndex(int ix)
        {
            foreach (var a in abilities)
            {
                if (ix-- == 0)
                {
                    return a;
                }
            }

            throw new TimeZoneNotFoundException();
        }

        public static Card createDummy(Ability a)
        {
            Card b = a.card;
            Card r = new Card();

            r.name = b.name;
            r.cardId = b.cardId;
            r.castingCost = b.castingCost;
            r.dummyFor = a;
            r.controller = b.controller;
            r.owner = b.owner;
            r.type = Type.Ability;

            return r;
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

            if (isDummy)
            {
                b.Append(dummyFor.description);
            }

            foreach (Ability v in abilities)
            {
                if (v.description.Length != 0) { b.AppendLine(v.description); }
            }

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
        TempleHealer,
        Rapture,
        Squire,
        CallToArms,
        Ragnarok,
        ShimmeringKoi,
    }

    public enum Type
    {
        Creature,
        Instant,
        Sorcery,
        Relic,
        Ability,
        Token,
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
        Cleric,
    }

    public enum KeyAbility
    {
        Fervor,
    }
}