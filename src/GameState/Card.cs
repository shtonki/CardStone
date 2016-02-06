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
    public enum Colour
    {
        WHITE,
        BLUE,
        BLACK,
        RED,
        GREEN,
        GREY,
        MULTI,
    }

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
                notifyObservers();
            }
        }

        private string name;
        private CardType cardType;
        private Race? race;
        private SubType? subType;
        public Colour colour;
        public StackWrapper stackWrapper;

        private Modifiable<int>[] mods = new Modifiable<int>[Enum.GetNames(typeof(Modifiable)).Count()];

        private Modifiable<int> power
        {
            get { return mods[(int)Modifiable.Power]; }
            set { mods[(int)Modifiable.Power] = value; }
        }

        private Modifiable<int> toughness
        {
            get { return mods[(int)Modifiable.Toughness]; }
            set { mods[(int)Modifiable.Toughness] = value; }
        }
        //private Modifiable<int> power, toughness;
        public int currentPower => power.getValue();
        public int currentToughness => toughness.getValue();
        public bool summoningSick { get; set; }
        public bool attacking
        {
            get { return Attacking; }
            set
            {
                Attacking = value;
                notifyObservers();
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
                notifyObservers();
            }
        }
        public Card defendedBy
        {
            get { return DefendedBy; }
            set
            {
                DefendedBy = value;
                notifyObservers();
            }
        }
        public bool canDefend => location.pile == LocationPile.FIELD && !topped;

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

        private int moveHackInt = 0;

        public List<Aura> auras { get; private set; }

        protected Card()
        {
            baseActivatedAbilities = new List<ActivatedAbility>();
            baseTriggeredAbilities = new List<TriggeredAbility>();
            keyAbilities = new List<KeyAbility>();
            auras = new List<Aura>();
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
            auras = new List<Aura>();
            name = b.ToString();
            baseActivatedAbilities = new List<ActivatedAbility>();
            baseTriggeredAbilities = new List<TriggeredAbility>();

            Colour? forceColour = null;
            int? basePower = null, baseToughness = null;

            switch (cardId)
            {
                case CardId.Kappa:
                {
                    blueCost = 2;
                    basePower = 1;
                    baseToughness = 3;
                    cardType = CardType.Creature;
                    race = Race.Salamander;
                } break;

                case CardId.GrizzlyBear:
                {
                    greenCost = 2;
                    cardType = CardType.Creature;
                    race = Race.Bear;
                    subType = SubType.Warrior;
                    basePower = 3;
                    baseToughness = 3;
                } break;

                case CardId.LightningBolt:
                {
                    redCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new Ping(3, TargetLambda.ZAPPABLE));
                    castDescription = "Deal 3 damage to target player or creature.";
                } break;

                case CardId.ForkedLightning:
                {
                    redCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new Ping(1, TargetLambda.ZAPPABLE));
                    fx.Add(new Ping(1, TargetLambda.ZAPPABLE));
                    castDescription = "Deal 1 damage to 2 target players or creatures.";
                } break;

                case CardId.SolemnAberration:
                {
                    blackCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Zombie;
                    basePower = 2;
                    baseToughness = 1;
                } break;

                case CardId.PropheticVision:
                {
                    blueCost = 2;
                    cardType = CardType.Sorcery;
                    fx.Add(new Draw(false, 2));
                    castDescription = "Draw 2 cards";
                } break;

                case CardId.FrothingGnome:
                {
                    redCost = 1;
                    cardType = CardType.Creature;
                    basePower = 1;
                    baseToughness = 1;
                    keyAbilities.Add(KeyAbility.Fervor);
                } break;

                case CardId.TempleHealer:
                {
                    whiteCost = 3;
                    greyCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Human;
                    subType = SubType.Cleric;
                    basePower = 4;
                    baseToughness = 4;
                    EventFilter e = vanillaETB;
                    baseTriggeredAbilities.Add(new TriggeredAbility(this, 
                        friendlyETB, 
                        underYourControlETBDescription + "gain 1 life.", 
                        LocationPile.FIELD, EventTiming.Post, new GainLife(false, 1)));
                } break;

                case CardId.Rapture:
                {
                    whiteCost = 2;
                    greyCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new MoveTo(LocationPile.EXILE, TargetLambda.ZAPPABLECREATURE));
                    castDescription = "Exile target creature";
                } break;

                case CardId.CallToArms:
                {
                    whiteCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new SummonTokens(TargetLambda.CONTROLLER, 2, CardId.Squire));
                    castDescription = "Summon two Squires.";
                } break;

                case CardId.Squire:
                {
                    cardType = CardType.Token;
                    race = Race.Human;
                    baseToughness = 1;
                    basePower = 1;
                    forceColour = Colour.WHITE;
                    
                    } break;

                case CardId.ShimmeringKoi:
                {
                    blueCost = 2;
                    greyCost = 2;
                    cardType = CardType.Creature;
                    race = Race.Fish;
                    basePower = 2;
                    baseToughness = 3;
                    baseTriggeredAbilities.Add(new TriggeredAbility(this,
                        thisETB(this),
                        thisETBDescription + "draw a card.",
                        LocationPile.FIELD, EventTiming.Post,
                        new Draw(false, 1)
                        ));
                } break;

                case CardId.Belwas:
                {
                    basePower = 3;
                    baseToughness = 2;
                    whiteCost = 2;
                    greyCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Human;
                    Aura a = new Aura(
                        (crd) => crd.controller == this.controller && crd.colour == Colour.WHITE && crd != this,
                        Modifiable.Power,
                        1,
                        "Other white creatures you control get +1/+0");
                    auras.Add(a);
                } break;

                case CardId.AlterTime:
                {
                    blueCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new Timelapse(2));
                    fx.Add(new Draw(false, 1));
                    castDescription = "Timelapse 2\nDraw a card.";
                } break;

                case CardId.GrizzlyCub:
                {
                    greenCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Bear;
                    basePower = 2;
                    baseToughness = 2;
                } break;

                case CardId.EvolveFangs:
                {
                    greenCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new ModifyUntil(TargetLambda.ZAPPABLECREATURE, Modifiable.Power, untilEndOfTurn, 3));
                    castDescription = "Target creature gets +3/+0" + untilEOTDescription;
                } break;

                case CardId.IlasGambit:
                {
                    name = "Ila's Gambit";
                    blackCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new Duress((_) => true));
                    fx.Add(new GainLife(false, -2));
                    castDescription =
                        "Look at target players hand and choose 1 card from it. The chosen card is discarded.\nLose 2 life.";
                } break;

                case CardId.YungLich:
                {
                    blackCost = 1;
                    blueCost = 1;
                    greyCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Zombie;
                    subType = SubType.Wizard;
                    basePower = 2;
                    baseToughness = 2;
                    triggeredAbilities.Add(new TriggeredAbility(this, thisDies(this), thisDiesDescription + "draw a card.", LocationPile.GRAVEYARD, EventTiming.Post, new Draw(false, 1)));
                } break;

                case CardId.Unmake:
                {
                    blueCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new MoveTo(LocationPile.HAND, TargetLambda.ZAPPABLECREATURE));
                    castDescription = "Return target creature to its owners hand";
                } break;

                case CardId.GnomishCannoneer:
                {
                    redCost = 2;
                    cardType = CardType.Creature;
                    race = Race.Gnome;
                    basePower = 2;
                    baseToughness = 2;
                        triggeredAbilities.Add(new TriggeredAbility(this, thisETB(this), thisETBDescription + " deal 1 damage to target player or creature.", 
                            LocationPile.FIELD, EventTiming.Post, new Ping(1, TargetLambda.ZAPPABLE) ));
                } break;

                case CardId.SteamBolt:
                {
                    redCost = 1;
                    blueCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new Ping(1, TargetLambda.ZAPPABLE));
                    fx.Add(new Draw(false, 1));
                    castDescription = "Deal 1 damage to target creature or player.\nDraw a card.";
                } break;

                default:
                {
                    throw new Exception("pls no" + c.ToString());
                }
            }

            if (basePower != null)
            {
                power = new Modifiable<int>(add, sub);
                power.setBaseValue(basePower.Value);
                toughness = new Modifiable<int>(add, sub);
                toughness.setBaseValue(baseToughness.Value);
            }


            Effect x = new Effect(fx.ToArray());
            castingCost = new ManaCost(whiteCost, blueCost, blackCost, redCost, greenCost, greyCost);
            Cost cc = new Cost(castingCost);
            ActivatedAbility castAbility = new ActivatedAbility(this, cc, x, LocationPile.HAND, castDescription);
            castAbility.setInstant(cardType == CardType.Instant);

            baseActivatedAbilities.Add(castAbility);

            if ((basePower == null) != (baseToughness == null))
            {
                throw new Exception("bad thing b0ss");
            }

            var vs = castingCost.costsEnumerable;
            List<int> n = new List<int>();
            int ctr = 0;
            foreach (var v in vs)
            {
                if (v != 0)
                {
                    n.Add(ctr);
                }
                if (++ctr == 5)
                {
                    break;
                }
            }
            if (n.Count() == 0)
            {
                if (!forceColour.HasValue)
                {
                    colour = Colour.GREY;
                }
                else
                {
                    colour = forceColour.Value;
                }
            }
            else if (n.Count() == 1)
            {
                colour = (Colour)n.First();
            }
            else
            {
                colour = Colour.MULTI;
            }
        }

        

        #region commonEventFilters

        private static Modifiable<int>.Operator add = (a, b) => a + b;
        private static Modifiable<int>.Operator sub = (a, b) => a - b;

        private static Clojurex never = () => false;

        private bool untilEndOfTurn()
        {
            return owner.gameState.currentStep == Step.END;
        }
        private const string untilEOTDescription = " until end of turn.";

        private static bool vanillaETB(GameEvent e)
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

        private const string thisETBDescription = "Whenever this card enters the battlefield, ";
        private static EventFilter thisETB(Card c)
        {
            return @e =>
            {
                if (e.type != GameEventType.MOVECARD) { return false; }
                MoveCardEvent moveEvent = (MoveCardEvent)e;

                return moveEvent.to.pile == LocationPile.FIELD && moveEvent.card == c;
            };
        }
        private const string thisDiesDescription = "Whenever this card enters a graveyard from the battlefield, ";
        private static EventFilter thisDies(Card c)
        {
            return @e =>
            {
                if (e.type != GameEventType.MOVECARD) { return false; }
                MoveCardEvent moveEvent = (MoveCardEvent)e;

                return moveEvent.card == c && moveEvent.to.pile == LocationPile.GRAVEYARD && moveEvent.from.pile == LocationPile.FIELD;
            };
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
        



        public CardType getType()
        {
            return cardType;
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
            modify(Modifiable.Toughness, -d, never);
        }
        

        public bool isTopped()
        {
            return topped;
        }

        public bool hasPT()
        {
            return power != null;
        }

        public void modify(Modifiable m, int v, Clojurex c)
        {
            mods[(int)m].addModifier(v, c);
            notifyObservers();
        }

        public bool isDamaged()
        {
            return false;
        }

        public bool canAttack()
        {
            return location.pile == LocationPile.FIELD && (!summoningSick || has(KeyAbility.Fervor));
        }


        public bool has(KeyAbility a)
        {
            return keyAbilities.Contains(a);
        }

        public void moveReset()
        {
            moveHackInt++;
            power?.clear();
            toughness?.clear();
            summoningSick = true;
        }

        //hack assumes that abilities looks the same for both the players
        //if this ever bugs that is most likely why
        public int getAbilityIndex(Ability a)
        {
            int r = 0;
            List<Ability> abs = isDummy ? dummyFor.card.abilities : abilities;


            foreach (Ability v in abs)
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
            r.cardType = CardType.Ability;
            r.colour = b.colour;

            return r;
        }

        public void checkModifiers()
        {
            power?.check();
            toughness?.check();

            notifyObservers();
        }
        

        public string getArchtypeString()
        {
            return cardType.ToString() +
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

            foreach (var v in auras)
            {
                b.AppendLine(v.description);
            }

            return b.ToString();
        }

    }
    public enum CardId
    {
        Kappa,
        //FrenziedPiranha,
        GrizzlyBear,
        LightningBolt,
        SolemnAberration,
        PropheticVision,
        ForkedLightning,
        FrothingGnome,
        TempleHealer,
        Rapture,
        Squire,
        CallToArms,
        ShimmeringKoi,
        Belwas,
        AlterTime,
        EvolveFangs,
        GrizzlyCub,
        IlasGambit,
        YungLich,
        Unmake,
        GnomishCannoneer,
        SteamBolt,
    }

    public enum CardType
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
        Gnome,
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

    public enum Modifiable
    {
        Power,
        Toughness,

    }

    public struct Aura
    {
        public readonly Func<Card, bool> filter;
        public readonly Modifiable attribute;
        public readonly int value;
        public readonly string description;

        public Aura(Func<Card, bool> filter, Modifiable attribute, int value, string description)
        {
            this.filter = filter;
            this.attribute = attribute;
            this.value = value;
            this.description = description;
        }
    }
}