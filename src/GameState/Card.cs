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

        public readonly bool isToken;
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
        public bool isCreature => cardType == CardType.Creature;
        public bool isDummy => dummyFor != null;
        public Ability dummyFor { get; private set; }
        public readonly bool isExperimental;

        //private List<Ability> abilities;
        private readonly List<ActivatedAbility> baseActivatedAbilities;
        private readonly List<TriggeredAbility> baseTriggeredAbilities;
        public readonly ActivatedAbility castAbility;
        public ManaCost castingCost { get; private set; }
        //private List<KeyAbility> keyAbilities;
        public List<KeyAbility> keyAbilities { get; private set; }
        public List<Ability> abilities => ((IEnumerable<Ability>)activatedAbilities).Concat(triggeredAbilities).ToList();
        public List<ActivatedAbility> activatedAbilities => baseActivatedAbilities;
        public List<TriggeredAbility> triggeredAbilities => baseTriggeredAbilities;

        public string flavourText { get; private set; }

        private int moveHackInt = 0;

        public List<Aura> auras { get; private set; }

        protected Card()
        {
            baseActivatedAbilities = new List<ActivatedAbility>();
            baseTriggeredAbilities = new List<TriggeredAbility>();
            keyAbilities = new List<KeyAbility>();
            auras = new List<Aura>();
        }

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

            List<SubCost> castingCosts = new List<SubCost>();

            Colour? forceColour = null;
            int? basePower = null, baseToughness = null;

            switch (cardId)
            {
                #region Kappa
                case CardId.Kappa:
                {
                    blueCost = 1;
                    basePower = 1;
                    baseToughness = 3;
                    cardType = CardType.Creature;
                    race = Race.Salamander;
                    activatedAbilities.Add(new ActivatedAbility(this,
                        new Cost(new ManaCost(0, 1, 0, 0, 0, 2)),
                        new Effect(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1)),
                        true,
                        LocationPile.FIELD, 
                        "2BB: Target player draws a card."));
                    } break;
                #endregion
                #region GrizzlyBear
                case CardId.GrizzlyBear:
                {
                    greenCost = 2;
                    cardType = CardType.Creature;
                    race = Race.Bear;
                    subType = SubType.Warrior;
                    basePower = 3;
                    baseToughness = 3;
                } break;
                #endregion
                #region LightningBolt
                case CardId.LightningBolt:
                {
                    redCost = 1;
                    greyCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 3));
                    castDescription = "Deal 3 damage to target player or creature.";
                } break;
                #endregion
                #region ForkedLightning
                case CardId.ForkedLightning:
                {
                    redCost = 1;
                    greyCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new Ping(new FilterTargetRule(2, FilterLambda.ZAPPABLE), 1));
                    castDescription = "Deal 1 damage to 2 target players or creatures.";
                } break;
                #endregion
                #region SolemnAberration
                case CardId.SolemnAberration:
                {
                    blackCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Zombie;
                    basePower = 2;
                    baseToughness = 1;
                } break;
                #endregion
                #region PropheticVision
                case CardId.PropheticVision:
                {
                    blueCost = 2;
                    cardType = CardType.Sorcery;
                    fx.Add(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 2));
                    castDescription = "Draw 2 cards";
                } break;
                #endregion
                #region DragonHatchling
                case CardId.DragonHatchling:
                {
                    redCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Dragon;
                    basePower = 1;
                    baseToughness = 1;
                    keyAbilities.Add(KeyAbility.Fervor);
                    } break;
                #endregion
                #region TempleHealer
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
                        LocationPile.FIELD, EventTiming.Post, new GainLife(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1)));
                } break;
                #endregion
                #region Rapture
                case CardId.Rapture:
                {
                    whiteCost = 2;
                    greyCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.EXILE));
                    castDescription = "Exile target creature";
                } break;
                #endregion
                #region CallToArms
                case CardId.CallToArms:
                {
                    whiteCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new SummonTokens(new ResolveTargetRule(ResolveTarget.CONTROLLER), CardId.Squire, CardId.Squire));
                    castDescription = "Summon two Squires.";
                } break;
                #endregion
                #region Squire
                case CardId.Squire:
                {
                    isToken = true;
                    race = Race.Human;
                    baseToughness = 1;
                    basePower = 1;
                    forceColour = Colour.WHITE;
                    
                } break;
                #endregion
                #region ShimmeringKoi
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
                        new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1)
                        ));
                } break;
                #endregion
                #region Belwas
                case CardId.Belwas:
                {
                    whiteCost = 2;
                    greyCost = 1;
                    basePower = 3;
                    baseToughness = 2;
                    cardType = CardType.Creature;
                    race = Race.Human;
                    Aura a = new Aura(
                        (crd) => crd.controller == this.controller && crd.colour == Colour.WHITE && crd != this,
                        Modifiable.Power,
                        1,
                        "Other white creatures you control get +1/+0");
                    auras.Add(a);
                } break;
                #endregion
                #region AlterTime
                case CardId.AlterTime:
                {
                    blueCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new Timelapse(2));
                    fx.Add(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1));
                    castDescription = "Timelapse 2 " + timelapseReminder2 + "\nDraw a card.";
                } break;
                #endregion
                #region GrizzlyCub
                case CardId.GrizzlyCub:
                {
                    greenCost = 1;
                    cardType = CardType.Creature;
                    race = Race.Bear;
                    basePower = 2;
                    baseToughness = 2;
                } break;
                #endregion
                #region EvolveFangs
                case CardId.EvolveFangs:
                {
                    greenCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new ModifyUntil(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), Modifiable.Power, never, 2));
                    castDescription = "Target creature gets +2/+0.";
                } break;
                #endregion
                #region IlasGambit
                case CardId.IlasGambit:
                {
                    name = "Ila's Gambit";
                    blackCost = 1;
                    castingCosts.Add(new PayLifeCost(3));
                    cardType = CardType.Sorcery;
                    fx.Add(
                        new MoveTo(new SelectFromTargetRule(
                            new ResolveTargetRule(ResolveTarget.CONTROLLER), 
                            new FilterTargetRule(1, FilterLambda.PLAYER), 
                            p => p.hand.cards.ToArray()),
                        LocationPile.GRAVEYARD) );
                    castDescription =
                        "As an additional cost to casting this card pay 3 life.\nLook at target players hand and choose 1 card from it. The chosen card is discarded.";
                } break;
                #endregion
                #region YungLich
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
                    triggeredAbilities.Add(new TriggeredAbility(this, thisDies(this), 
                        thisDiesDescription + "draw a card.", 
                        LocationPile.GRAVEYARD, EventTiming.Post, 
                        new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1)));
                } break;
                #endregion
                #region Unmake
                case CardId.Unmake:
                {
                    blueCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.HAND));
                    castDescription = "Return target creature to its owners hand";
                } break;
                #endregion
                #region EnragedDragon
                case CardId.EnragedDragon:
                {
                    redCost = 2;
                    cardType = CardType.Creature;
                    race = Race.Dragon;
                    basePower = 3;
                    baseToughness = 2;
                        triggeredAbilities.Add(new TriggeredAbility(this, thisETB(this), thisETBDescription + " deal 1 damage to target player or creature.", 
                            LocationPile.FIELD, EventTiming.Post,
                            () => true, 
                            new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 1)));
                } break;
                #endregion
                #region SteamBolt
                case CardId.SteamBolt:
                {
                    redCost = 1;
                    blueCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 1));
                    fx.Add(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1));
                    castDescription = "Deal 1 damage to target creature or player.\nDraw a card.";
                } break;
                #endregion
                #region IlasGravekeeper
                case CardId.IlasGravekeeper:
                {
                    name = "Ila's Gravekeeper";
                    blackCost = 3;
                    basePower = 0;
                    baseToughness = 4;
                    cardType = CardType.Creature;
                    race = Race.Zombie;
                    auras.Add(new DynamicAura((a) => a == this, Modifiable.Power, () => owner.field.cards.Count(card => card.race == Race.Zombie), "Ila's Gravekeeper gets +1/+0 for each zombie under your control."));
                } break;
                #endregion
                #region RottingZombie
                //todo: phrasing and balance
                case CardId.RottingZombie: 
                {
                   //todo: phrasing and balance
                    blackCost = 2;
                    greyCost = 1;
                    basePower = 2;
                    baseToughness = 3;
                    cardType = CardType.Creature;
                    race = Race.Zombie;

                    EventFilter f = (e) =>
                    {
                        if (e.type != GameEventType.MOVECARD) return false;
                        MoveCardEvent mevent = (MoveCardEvent) e;
                        return mevent.from?.pile == LocationPile.FIELD && mevent.to?.pile == LocationPile.GRAVEYARD &&
                               mevent.card.owner.isHero && mevent.card.isCreature && mevent.card != this;
                    };

                    triggeredAbilities.Add(new TriggeredAbility(this, f, " gets +2/+2 when a friendly creature dies ", LocationPile.FIELD, EventTiming.Post,
                        new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Power, () => false, 1),
                        new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Toughness, () => false, 1)));
                } break;
                #endregion
                #region Infiltrator
                case CardId.Infiltrator:
                {
                    blueCost = 3;
                    basePower = 2;
                    baseToughness = 2;
                    cardType = CardType.Creature;
                    keyAbilities.Add(KeyAbility.Fervor);
                    EventFilter f = (e) =>
                    {
                        if (e.type != GameEventType.DAMAGEPLAYER) { return false; }
                        DamagePlayerEvent devent = (DamagePlayerEvent)e;
                        return devent.source == this; 
                    };
                    triggeredAbilities.Add(new TriggeredAbility(this, f, " DO STUFF ", LocationPile.FIELD, EventTiming.Post,
                        new Mill(new ResolveTargetRule(ResolveTarget.OPPONENT), 1)));
                } break;
                #endregion
                #region ProtectiveSow
                //todo seba: PHRASING
                case CardId.ProtectiveSow: //todo fixa så att det bara kortet som summade cubsen dör och inte alla sows dör. och vise versa. Och översätt denna texten till engelska så att seba inte blir arg
                {
                    name = "Protective Sow";
                    greenCost = 1;
                    cardType = CardType.Creature;
                    basePower = 2;
                    baseToughness = 4;
                    triggeredAbilities.Add(new TriggeredAbility(this, thisETB(this), thisETBDescription + " summon two cubs. If a cub dies: give +2/0 to this card. If this card dies: cubs die.", 
                        LocationPile.FIELD, EventTiming.Post, () => true, new SummonTokens(new ResolveTargetRule(ResolveTarget.CONTROLLER), CardId.Cub, CardId.Cub)));
                    //add kill cubs deathrattle thingy
                } break;
                #endregion
                #region Cub
                case CardId.Cub:
                {
                    cardType = CardType.Creature;
                    //cardType = CardType.Token;
                    forceColour = Colour.GREEN;
                    baseToughness = 1;
                    basePower = 1;

                    //todo jaseba: fix my targetrule, so that it actually targets protective sow and now whatever its targeting right now
                    triggeredAbilities.Add(new TriggeredAbility(this, thisDies(this), thisDiesDescription + " give +2/0 to protective sow.",
                            LocationPile.GRAVEYARD, EventTiming.Post, () => owner.field.cards.All(sow => sow.cardId == CardId.ProtectiveSow), new ModifyUntil(new ResolveTargetRule(ResolveTarget.LAST), Modifiable.Power, never, 2)));//new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1)));
                } break;
                #endregion
                #region RiderOfDeath
                case CardId.RiderOfDeath:
                {
                    name = "Rider of Death";
                    blackCost = 3;
                    greyCost = 2;
                    cardType = CardType.Creature;
                    basePower = 5;
                    baseToughness = 4;
                    triggeredAbilities.Add(new TriggeredAbility(this, thisETB(this), thisETBDescription + "kill target creature.",
                        LocationPile.FIELD, EventTiming.Post, () => true, new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.HAND)));
                } break;
                #endregion
                #region IlatianWineMerchant
                case CardId.IlatianWineMerchant:
                {
                    blackCost = 1;
                    greyCost = 2;
                    cardType = CardType.Creature;
                    basePower = 1;
                    baseToughness = 2;

                    activatedAbilities.Add(new ActivatedAbility(this, new Cost(new DiscardCost(1)), new Effect(new GainLife(new ResolveTargetRule(ResolveTarget.CONTROLLER), 3)), true, LocationPile.FIELD, "Discard a card: Gain 3 life."));
                        //triggeredAbilities.Add(new ActivatedAbility(this, new Cost(), ));
                        /*
                        Card c = fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.INHAND), LocationPile.GRAVEYARD)); //todo jasin: take cost of creature and put it in gainlife
                        fx.Add(new GainLife(new ResolveTargetRule(ResolveTarget.CONTROLLER), c.manacost));
                        */
                } break;
                #endregion
                #region MeteorRain
                case CardId.MeteorRain: //todo: seba review
                {
                    redCost = 2;
                    greyCost = 1;
                    cardType = CardType.Sorcery;
                    castDescription = "Deal 3 damage to all creatures.";
                    fx.Add(new Pyro(new ResolveTargetRule(ResolveTarget.OPPONENT), 3, crd => true));
                    fx.Add(new Pyro(new ResolveTargetRule(ResolveTarget.CONTROLLER), 3, crd => true));
                } break;
                #endregion
                #region FuryOfTheRighteous
                case CardId.FuryOfTheRighteous: //todo: seba review
                {
                    name = "Fury of the Righteous";
                    whiteCost = 2;
                    greyCost = 2;
                    cardType = CardType.Sorcery;
                    castDescription = "Deal 2 damage to all non-white creatures";
                    fx.Add(new Pyro(new ResolveTargetRule(ResolveTarget.OPPONENT), 2, crd => crd.colour != Colour.WHITE));
                    fx.Add(new Pyro(new ResolveTargetRule(ResolveTarget.CONTROLLER), 2, crd => crd.colour != Colour.WHITE));
                } break;
                #endregion
                #region Extinguish
                case CardId.Extinguish: //todo: seba review
                {
                    blackCost = 2;
                    cardType = CardType.Instant;
                    castDescription = "Kill target creature.";
                    flavourText = "Be gone!";
                    fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.GRAVEYARD));
                } break;
                #endregion
                #region Jew
                case CardId.Jew: //todo: seba review
                    {
                    blueCost = 4;
                    cardType = CardType.Creature;
                    basePower = 2;
                    baseToughness = 2;
                        EventFilter f = (gameEvent) =>
                        {
                            if (gameEvent.type != GameEventType.STEP) return false;
                            StepEvent stepevent = (StepEvent)gameEvent;
                            return stepevent.step == Step.DRAW && owner.hand.count >= 5 && stepevent.activePlayer == owner;
                        };
                        triggeredAbilities.Add(new TriggeredAbility(this, f, "If you have five or more cards in your hand at beginning of your draw step, draw a card.",
                        LocationPile.FIELD, EventTiming.Post, new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1)));
                } break;
                #endregion
                #region VikingMushroom
                case CardId.VikingMushroom: //todo: seba review
                {
                    redCost = 2;
                    cardType = CardType.Sorcery;
                    castDescription = "Give target creature Fervor and +2/+0, deal 1 damage to it.";
                    fx.Add(new ModifyUntil(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), Modifiable.Power, never, 2));
                    fx.Add(new Ping(new ResolveTargetRule(ResolveTarget.LAST), 1));
                } break;
                #endregion
                #region Tree
                case CardId.Tree: //todo serious balance and flavor issues
                {
                    greenCost = 1;
                    basePower = 1;
                    baseToughness = 2;
                    cardType = CardType.Creature;
                    activatedAbilities.Add(new ActivatedAbility(this, new Cost(new ManaCost(0,0,0,0,1,1)),
                        new Effect(new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Power, never, 1),
                        new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Toughness, never, 1)), true,
                        LocationPile.FIELD, "1G: gain +1/+1"));
                } break;
                #endregion
                #region EssenceOfDemise
                //todo: each time player casts spell deal one damage to face
                case CardId.EssenceOfDemise:
                {
                    name = "Essence of Demise";
                    blackCost = 3;
                    cardType = CardType.Relic;
                    auras.Add(new Aura((crd) => crd.isCreature, Modifiable.Power, -1, "All creatures get -1/-1"));
                    auras.Add(new Aura((crd) => crd.isCreature, Modifiable.Toughness, -1, ""));
                } break;
                #endregion
                #region Counterspell
                case CardId.Counterspell:
                {
                    blueCost = 2;
                    greyCost = 1;
                    cardType = CardType.Instant;
                    castDescription = "Counter target spell.";
                    fx.Add(new CounterSpell(new FilterTargetRule(1, FilterLambda.ONSTACK)));
                } break;
                #endregion
                #region EssenceOfRage
                case CardId.EssenceOfRage:
                {
                    name = "Essence of Rage";
                    redCost = 3;
                    greyCost = 1;
                    cardType = CardType.Relic;

                    triggeredAbilities.Add(new TriggeredAbility(this, stepFilter(Step.END), "At the beginning of each end step deal 1 damage to both players.",
                        LocationPile.FIELD, EventTiming.Post, new Ping(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1), new Ping(new ResolveTargetRule(ResolveTarget.OPPONENT), 1)));
                } break;
                #endregion
                #region EssenceOfClarity
                case CardId.EssenceOfClarity:
                {
                    name = "Essence of Clarity";
                    blueCost = 3;
                    greyCost = 1;
                    cardType = CardType.Relic;

                    triggeredAbilities.Add(new TriggeredAbility(this, stepFilter(Step.END), "At the beginning of each end step the active player draws a card.",
                        LocationPile.FIELD, EventTiming.Post, new Draw(new ResolveTargetRule(ResolveTarget.ACTIVE), 1)));
                } break;
                #endregion
                #region EssenceOfWilderness


                /* 
                case CardId.EssenceOfWilderness:
                {
                    name = "Essence of Wilderness";
                    greenCost = 3;
                    cardType = CardType.Relic;

                    EventFilter f = (gevent) =>
                    {
                        if (gevent.type != GameEventType.MOVECARD) return false;
                        MoveCardEvent mevent = (MoveCardEvent) gevent;
                        return mevent.to.pile == LocationPile.FIELD &&mevent.card.cardType == CardType.Creature;
                    };

                    triggeredAbilities.Add(new TriggeredAbility(this, ));
                } break;
                */
                #endregion
                #region EssenceOfValor
                /*
                case CardId.EssenceOfValor:
                {
                    name = "Essence of Valor";
                    whiteCost = 3;
                    cardType = CardType.Relic;

                    //creatures with more than 3 damage cannot attack
                } break;
                */
                #endregion
                #region IlasMagicLamp
                /*
                case CardId.IlasMagicLamp:
                {
                    name = "Ila's Magic Lamp";
                    blackCost = 1;
                    cardType = CardType.Sorcery;
                    
                    //has three charges, get card from deck and shuffle deck

                } break;
                */
                #endregion
                #region AngryCoolDragonX
                case CardId.AngryCoolDragonX:
                {
                    redCost = 2;
                    baseToughness = 1;
                    basePower = 5;
                    cardType = CardType.Creature;;

                    keyAbilities.Add(KeyAbility.Fervor);
                    triggeredAbilities.Add(new TriggeredAbility(this, stepFilter(Step.END), "dies and end of turn.",
                        LocationPile.FIELD, EventTiming.Post, new MoveTo(new ResolveTargetRule(ResolveTarget.SELF), LocationPile.GRAVEYARD)));
                } break;
                #endregion
                #region MorenianMedic
                case CardId.MorenianMedic:
                {
                    whiteCost = 2;
                    basePower = 2;
                    baseToughness = 2;
                    activatedAbilities.Add(new ActivatedAbility(this, 
                        new Cost(new ManaCost(1, 0, 0, 0, 0, 1)), 
                        new Effect(new GainLife(new ResolveTargetRule(ResolveTarget.CONTROLLER), 2)), 
                        true,
                        LocationPile.FIELD, 
                        "1W: Gain 2 life."));
                } break;
                #endregion
                #region MattysGambit
                case CardId.MattysGambit:
                    {
                        name = "Matty's Gambit";
                        redCost = 1;
                        castingCosts.Add(new PayLifeCost(3));
                        castDescription =
                            "As an additional cost to casting this card pay 3 life.\nDeal 3 damage to target creature or player.";
                        cardType = CardType.Instant;
                        fx.Add(new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 4));
                    }
                    break;
                #endregion
                #region BelwasGambit
                case CardId.BelwasGambit:
                    {
                        name = "Belwas's Gambit";
                        whiteCost = 1;
                        castingCosts.Add(new PayLifeCost(3));
                        castDescription =
                            "As an additional cost to casting this card pay 3 life.\nTarget creature gets +4/+4.";
                        cardType = CardType.Instant;
                        fx.Add(new ModifyUntil(new FilterTargetRule(1, FilterLambda.CREATURE, FilterLambda.ONFIELD), Modifiable.Power, never, 4));
                        fx.Add(new ModifyUntil(new ResolveTargetRule(ResolveTarget.LAST), Modifiable.Toughness, never, 4));
                    }
                    break;
                #endregion
                #region GreenFourDropThatDoesCoolShit
                //todo balance and name and stuff and flavor and stuff
                case CardId.GreenFourDropThatDoesCoolShit:
                    {
                        greenCost = 1;
                        cardType = CardType.Creature;
                        triggeredAbilities.Add(new TriggeredAbility(this, thisETB(this),
                            thisETBDescription + " heal target creature for 3hp",
                            LocationPile.FIELD, EventTiming.Post,
                            new Ping(new FilterTargetRule(1, FilterLambda.ONFIELD), -3)));
                    }
                    break;
                #endregion
                #region SumHyenas
                case CardId.SumHyenas:
                    {
                        greenCost = 1;
                        cardType = CardType.Instant;
                        fx.Add(new SummonTokens(new ResolveTargetRule(ResolveTarget.CONTROLLER), CardId.Hyena, CardId.Hyena));
                        castDescription = "Summon two hyenas. Grants +1/+1 to all other hyenas.";
                    }
                    break;
                #endregion
                #region Hyena
                case CardId.Hyena:
                    {
                        cardType = CardType.Creature;
                        isToken = true;
                        baseToughness = 2;
                        basePower = 2;
                        Aura a = new Aura(
                            (crd) => crd.cardId == CardId.Hyena && crd != this,
                            Modifiable.Power, 1,
                            "Other hyenas get +1/+1");
                        Aura aa = new Aura(
                            (crd) => crd.cardId == CardId.Hyena && crd != this,
                            Modifiable.Toughness, 1,
                            "");
                        auras.Add(a);
                        auras.Add(aa);
                    }
                    break;
                #endregion
                #region FireTornadoYeti
                case CardId.FireTornadoYeti:
                    {
                        cardType = CardType.Creature;
                        greenCost = 4;
                        basePower = 4;
                        baseToughness = 5;
                    }
                    break;
                #endregion
                #region ItsAllOgre
                case CardId.ItsAllOgre:
                    {
                        cardType = CardType.Creature;
                        greenCost = 4;
                        greyCost = 2;
                        basePower = 6;
                        baseToughness = 7;
                    }
                    break;
                #endregion

                #region default
                case CardId.BelwasGambit:
                {
                    name = "Belwas's Gambit";
                    whiteCost = 1;
                    castingCosts.Add(new PayLifeCost(3));
                    castDescription =
                        "As an additional cost to casting this card pay 3 life.\nTarget creature gets +4/+4.";
                        cardType = CardType.Instant;
                    fx.Add(new ModifyUntil(new FilterTargetRule(1, FilterLambda.CREATURE, FilterLambda.ONFIELD), Modifiable.Power, never, 4));
                    fx.Add(new ModifyUntil(new ResolveTargetRule(ResolveTarget.LAST), Modifiable.Toughness, never, 4));
                } break;

                default: 
                {
                    throw new Exception("pls no" + c.ToString());
                }
                #endregion
            }
            //
            if (basePower != null)
            {
                power = new Modifiable<int>(add, sub);
                power.setBaseValue(basePower.Value);
                toughness = new Modifiable<int>(add, sub);
                toughness.setBaseValue(baseToughness.Value);
            }


            Effect x = new Effect(fx.ToArray());
            castingCost = new ManaCost(whiteCost, blueCost, blackCost, redCost, greenCost, greyCost);
            castingCosts.Add(castingCost);
            Cost cc = new Cost(castingCosts.ToArray());
            castAbility = new ActivatedAbility(this,
                cc,
                new Effect(fx.ToArray()), 
                cardType == CardType.Instant, 
                LocationPile.HAND, castDescription);
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
        private static Modifiable<bool>.Operator addb = (a, b) => b;
        private static Modifiable<bool>.Operator subb = (a, b) => a;

        private static Clojurex never = () => false;

        private bool untilEndOfTurn()
        {
            return owner.gameState.currentStep == Step.END;
        }
        private const string untilEOTDescription = " until end of turn.";

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

        //todo add description
        private static EventFilter stepFilter(Step step)
        {
            return @e =>
            {
                if (e.type != GameEventType.STEP) return false;
                StepEvent stepEvent = (StepEvent)e;
                return stepEvent.step == step;
            };
        }

        private const string timelapseReminder1 = "(Look at the top card of your deck, you may shuffle your deck)";
        private const string timelapseReminder2 = "(Look at the top two cards of your deck, you may shuffle your deck)";
        private const string timelapseReminder3 = "(Look at the top three cards of your deck, you may shuffle your deck)";
        private const string timelapseReminder4 = "(Look at the top four cards of your deck, you may shuffle your deck)";

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
                (canSorc || a.instant) &&
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

        public bool isCast(Ability a)
        {
            return a == castAbility;
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
        GrizzlyBear,
        LightningBolt,
        SolemnAberration,
        PropheticVision,
        ForkedLightning,
        DragonHatchling,
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
        EnragedDragon,
        SteamBolt,
        IlasGravekeeper,
        FuryOfTheRighteous,
        MeteorRain,
        RiderOfDeath, 
        Extinguish,
        RottingZombie,
        EssenceOfDemise,
        EssenceOfRage,
        EssenceOfClarity,
        MorenianMedic,
        MattysGambit,
        BelwasGambit,
        //EssenceOfWilderness,
        //EssenceOfValor,
        //IlasMagicLamp,
        AngryCoolDragonX,
        Tree,
        Counterspell,
        Infiltrator,
        ProtectiveSow,
        Cub,
        IlatianWineMerchant,
        Jew,
        VikingMushroom,
        BelwasGambit,
        GreenFourDropThatDoesCoolShit,
        SumHyenas,
        Hyena,
        ItsAllOgre,
        FireTornadoYeti,
    }

    public enum CardType
    {
        Creature,
        Instant,
        Sorcery,
        Relic,
        Ability,
    }

    public enum Race
    {
        Human,
        Salamander,
        Fish,
        Bear,
        Zombie,
        Dragon,
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
    //JAOIJAOJAJ
    public class Aura
    {
        public Func<Card, bool> filter { get; private set; }
        public Modifiable attribute { get; private set; }
        public virtual int value { get; private set; }
        public string description { get; private set; }
        

        public Aura(Func<Card, bool> filter, Modifiable attribute, int value, string description)
        {
            this.filter = filter;
            this.attribute = attribute;
            this.value = value;
            this.description = description;
        }
    }

    public class DynamicAura : Aura
    {
        public override int value => function();

        private Func<int> function;

        public DynamicAura(Func<Card, bool> filter, Modifiable attribute, Func<int> function, string description) : base(filter, attribute, 0, description)
        {
            this.function = function;
        }
    }
}