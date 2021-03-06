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
        public static int CARDCOUNT = Enum.GetNames(typeof(CardId)).Count();

        private int id;
        public CardId cardId { get; private set; }
        public Location location { get; set; }
        public Player owner { get; set; }
        public Player controller { get; set; }

        private bool Attacking;
        private Card DefenderOf;
        private Card DefendedBy;

        private bool _topped;
        public bool exhausted
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
        public Rarity rarity => rarities[(int)cardId];

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
        private bool _summoningSick { get; set; }

        public bool summoningSick
        {
            get { return isCreature && _summoningSick; }
            set { _summoningSick = value; }
        }

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
        public bool canDefend => location.pile == LocationPile.FIELD && !exhausted;
        public bool isCreature => cardType == CardType.Creature;
        public bool isDummy => dummyFor != null;
        public bool canExhaust => !exhausted && !summoningSick;
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
                        blueCost = 2;
                        greyCost = 2;
                        basePower = 1;
                        baseToughness = 3;
                        cardType = CardType.Creature;
                        race = Race.Salamander;
                        activatedAbilities.Add(new ActivatedAbility(this,
                        new Cost(new ExhaustCost(this)),
                            new Effect(new Mill(new ResolveTargetRule(ResolveTarget.CONTROLLER), 4)),
                            true,
                            LocationPile.FIELD,
                        "E: Target player mills 4 cards."));
                    }
                    break;
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
                    }
                    break;
                #endregion
                #region LightningBolt
                case CardId.LightningBolt:
                    {
                        redCost = 1;
                        greyCost = 1;
                        cardType = CardType.Instant;
                        fx.Add(new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 3));
                        castDescription = "Deal 3 damage to target player or creature.";
                    }
                    break;
                #endregion
                #region ForkedLightning
                case CardId.ForkedLightning:
                    {
                        redCost = 1;
                        greyCost = 1;
                        cardType = CardType.Sorcery;
                        fx.Add(new Ping(new FilterTargetRule(2, FilterLambda.ZAPPABLE), 1));
                        castDescription = "Deal 1 damage to 2 target players or creatures.";
                    }
                    break;
                #endregion
                #region SolemnAberration
                case CardId.SolemnAberration:
                    {
                        blackCost = 1;
                        cardType = CardType.Creature;
                        race = Race.Zombie;
                        basePower = 2;
                        baseToughness = 1;
                    }
                    break;
                #endregion
                #region PropheticVision
                case CardId.PropheticVision:
                    {
                        blueCost = 2;
                        cardType = CardType.Sorcery;
                        fx.Add(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 2));
                        castDescription = "Draw 2 cards";
                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
                #endregion
                #region Rapture
                case CardId.Rapture:
                    {
                        whiteCost = 2;
                        greyCost = 1;
                        cardType = CardType.Instant;
                        fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.EXILE));
                        castDescription = "Exile target creature";
                    }
                    break;
                #endregion
                #region CallToArms
                case CardId.CallToArms:
                    {
                        whiteCost = 1;
                        cardType = CardType.Sorcery;
                        fx.Add(new SummonTokens(new ResolveTargetRule(ResolveTarget.CONTROLLER), CardId.Squire, CardId.Squire));
                        castDescription = "Summon two Squires.";
                    }
                    break;
                #endregion
                #region Squire
                case CardId.Squire:
                    {
                        isToken = true;
                        race = Race.Human;
                        baseToughness = 1;
                        basePower = 1;
                        forceColour = Colour.WHITE;

                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
                #endregion
                #region AlterTime
                case CardId.AlterTime:
                    {
                        blueCost = 1;
                        cardType = CardType.Instant;
                        fx.Add(new Timelapse(2));
                        fx.Add(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1));
                        castDescription = "Timelapse 2 " + timelapseReminder2 + "\nDraw a card.";
                    }
                    break;
                #endregion
                #region GrizzlyCub
                case CardId.GrizzlyCub:
                    {
                        greenCost = 1;
                        cardType = CardType.Creature;
                        race = Race.Bear;
                        basePower = 2;
                        baseToughness = 2;
                    }
                    break;
                #endregion
                #region EvolveFangs
                case CardId.EvolveFangs:
                    {
                        greenCost = 1;
                        cardType = CardType.Instant;
                        fx.Add(new ModifyUntil(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), Modifiable.Power, never, 2));
                        castDescription = "Target creature gets +2/+0.";
                    }
                    break;
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
                            LocationPile.GRAVEYARD));
                        castDescription =
                            "As an additional cost to casting this card pay 3 life.\nLook at target players hand and choose 1 card from it. The chosen card is discarded.";
                    }
                    break;
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
                    }
                    break;
                #endregion
                #region Unmake
                case CardId.Unmake:
                    {
                        blueCost = 1;
                        cardType = CardType.Instant;
                        fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.HAND));
                        castDescription = "Return target creature to its owners hand";
                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
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
                            MoveCardEvent mevent = (MoveCardEvent)e;
                            return mevent.from?.pile == LocationPile.FIELD && mevent.to?.pile == LocationPile.GRAVEYARD &&
                                   mevent.card.owner == owner && mevent.card.isCreature && mevent.card != this;
                        };

                    triggeredAbilities.Add(new TriggeredAbility(this, f, "Whenever a friendly creature dies this creature gets +1/+1.", LocationPile.FIELD, EventTiming.Post,
                            new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Power, () => false, 1),
                            new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Toughness, () => false, 1)));
                    }
                    break;
                #endregion
                #region Infiltrator
                case CardId.Infiltrator:
                    {
                        blueCost = 3;
                    basePower = 3;
                    baseToughness = 3;
                        cardType = CardType.Creature;
                        EventFilter f = (e) =>
                        {
                            if (e.type != GameEventType.DAMAGEPLAYER) { return false; }
                            DamagePlayerEvent devent = (DamagePlayerEvent)e;
                            return devent.source == this;
                        };
                    triggeredAbilities.Add(new TriggeredAbility(this, f, "Whenever this creature deals damage to a player that player mills 3", LocationPile.FIELD, EventTiming.Post,
                        new Mill(new ResolveTargetRule(ResolveTarget.OPPONENT), 3)));
                    }
                    break;
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
                            LocationPile.FIELD, EventTiming.Post, () => true, new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.GRAVEYARD)));
                    }
                    break;
                #endregion
                #region IlatianWineMerchant
                case CardId.IlatianWineMerchant:
                    {
                        blackCost = 1;
                        greyCost = 2;
                        cardType = CardType.Creature;
                        basePower = 1;
                        baseToughness = 2;

                    activatedAbilities.Add(new ActivatedAbility(this, new Cost(new MoveToCost(LocationPile.HAND, LocationPile.GRAVEYARD, 1)), new Effect(new GainLife(new ResolveTargetRule(ResolveTarget.CONTROLLER), 3)), true, LocationPile.FIELD, "Discard a card: Gain 3 life."));
                        //triggeredAbilities.Add(new ActivatedAbility(this, new Cost(), ));
                        /*
                        Card c = fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.INHAND), LocationPile.GRAVEYARD)); //todo jasin: take cost of creature and put it in gainlife
                        fx.Add(new GainLife(new ResolveTargetRule(ResolveTarget.CONTROLLER), c.manacost));
                        */
                    }
                    break;
                #endregion
                #region MeteorRain
                case CardId.MeteorRain: //todo: seba review
                    {
                        redCost = 2;
                        greyCost = 1;
                        cardType = CardType.Sorcery;
                        castDescription = "Deal 3 damage to all creatures.";
                    fx.Add(new Ping(new ResolveTargetRule(ResolveTarget.FIELDCREATURES), 3));
                    }
                    break;
                #endregion
                #region FuryOfTheRighteous
                case CardId.FuryOfTheRighteous: //todo: seba review
                    {
                        name = "Fury of the Righteous";
                        whiteCost = 2;
                        greyCost = 2;
                        cardType = CardType.Sorcery;
                        castDescription = "Deal 2 damage to all non-white creatures";
                    fx.Add(new Ping(new ResolveTargetRule(ResolveTarget.FIELDCREATURES, FilterLambda.NONWHITE), 2));
                    }
                    break;
                #endregion
                #region Extinguish
                case CardId.Extinguish: //todo: seba review
                    {
                        blackCost = 2;
                        cardType = CardType.Instant;
                        castDescription = "Kill target creature.";
                        flavourText = "Be gone!";
                        fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.ZAPPABLE, FilterLambda.CREATURE), LocationPile.GRAVEYARD));
                    }
                    break;
                #endregion
                #region ElderTreeant
                case CardId.ElderTreeant: //todo serious balance and flavor issues
                    {
                    greenCost = 2;
                    greyCost = 1;
                        basePower = 1;
                        baseToughness = 2;
                        cardType = CardType.Creature;
                    activatedAbilities.Add(new ActivatedAbility(this, new Cost(new ManaCost(0,0,0,0,2,1)),
                            new Effect(new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Power, never, 1),
                            new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Toughness, never, 1)), true,
                        LocationPile.FIELD, "1GG: gain +1/+1"));
                    }
                    break;
                #endregion
                #region EssenceOfDemise
                //todo: each time player casts spell deal one damage to face
                case CardId.EssenceOfDemise:
                    {
                        name = "Essence of Demise";
                        blackCost = 3;
                        greyCost = 1;
                        cardType = CardType.Relic;
                        auras.Add(new Aura((crd) => crd.isCreature, Modifiable.Power, -1, "All creatures get -1/-1"));
                        auras.Add(new Aura((crd) => crd.isCreature, Modifiable.Toughness, -1, ""));
                    }
                    break;
                #endregion
                #region Counterspell
                case CardId.Counterspell:
                    {
                        blueCost = 2;
                        greyCost = 1;
                        cardType = CardType.Instant;
                        castDescription = "Counter target spell.";
                        fx.Add(new CounterSpell(new FilterTargetRule(1, FilterLambda.ONSTACK)));
                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
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
                #region StampedingDragon
                case CardId.StampedingDragon:
                    {
                    redCost = 3;
                        baseToughness = 1;
                    basePower = 6;
                        cardType = CardType.Creature; ;

                        keyAbilities.Add(KeyAbility.Fervor);
                    triggeredAbilities.Add(new TriggeredAbility(this, stepFilter(Step.END), "At the end of turn sacrifice this creature.",
                            LocationPile.FIELD, EventTiming.Post, new MoveTo(new ResolveTargetRule(ResolveTarget.SELF), LocationPile.GRAVEYARD)));
                    }
                    break;
                #endregion
                #region MorenianMedic
                case CardId.MorenianMedic:
                    {
                        whiteCost = 2;
                        basePower = 2;
                        baseToughness = 2;
                        activatedAbilities.Add(new ActivatedAbility(this,
                        new Cost(new ExhaustCost(this), new ManaCost(1, 0, 0, 0, 0, 1)), 
                            new Effect(new GainLife(new ResolveTargetRule(ResolveTarget.CONTROLLER), 2)),
                            true,
                            LocationPile.FIELD,
                        "E, 1W: Gain 2 life."));
                    }
                    break;
                #endregion
                #region MattysGambit
                case CardId.MattysGambit:
                    {
                        name = "Matty's Gambit";
                        redCost = 1;
                        castingCosts.Add(new PayLifeCost(3));
                        castDescription =
                            "As an additional cost to casting this card pay 3 life.\nDeal 4 damage to target creature or player.";
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
                        castingCosts.Add(new PayLifeCost(4));
                        castDescription =
                            "As an additional cost to casting this card pay 4 life.\nTarget creature gets +3/+3.";
                        cardType = CardType.Instant;
                        fx.Add(new ModifyUntil(new FilterTargetRule(1, FilterLambda.CREATURE, FilterLambda.ONFIELD), Modifiable.Power, never, 3));
                        fx.Add(new ModifyUntil(new ResolveTargetRule(ResolveTarget.LAST), Modifiable.Toughness, never, 3));
                    }
                    break;
                #endregion
                #region GrazingBison
                case CardId.GrazingBison:
                    {
                        cardType = CardType.Creature;
                        race = Race.Bison;
                        greenCost = 2;
                        greyCost = 2;
                        basePower = 4;
                        baseToughness = 5;
                    }
                    break;
                #endregion
                #region RockhandOgre
                case CardId.RockhandOgre:
                    {
                        cardType = CardType.Creature;
                        race = Race.Ogre;
                        greenCost = 3;
                        greyCost = 3;
                        basePower = 6;
                        baseToughness = 7;
                    }
                    break;
                #endregion
                #region Figment
                case CardId.Figment:
                    {
                        blackCost = 2;
                        greyCost = 1;
                        cardType = CardType.Sorcery;
                        castDescription = "Search your deck for a card and move it to your hand. Shuffle your deck.";
                        fx.Add(new MoveTo(
                            new SelectFromTargetRule(new ResolveTargetRule(ResolveTarget.CONTROLLER), new ResolveTargetRule(ResolveTarget.LAST),
                        (p) => p.deck.cards.ToArray()), 
                            LocationPile.HAND));
                        fx.Add(new Shuffle(new ResolveTargetRule(ResolveTarget.CONTROLLER), false));

                    }
                    break;
                #endregion
                #region SebasGambit
                case CardId.SebasGambit:
                    {
                        name = "Seba's Gambit";
                        blueCost = 1;
                        castingCosts.Add(new PayLifeCost(4));
                        castDescription =
                            "As an additional cost to casting this card pay 4 life.\nCounter target spell.";
                        cardType = CardType.Instant;
                        fx.Add(new CounterSpell(new FilterTargetRule(1, FilterLambda.ONSTACK)));
                    }
                    break;
                #endregion
                #region AlterFate
                case CardId.AlterFate:
                {
                    blueCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new Mill(new ResolveTargetRule(ResolveTarget.OPPONENT), 6));
                    fx.Add(new Draw(new ResolveTargetRule(ResolveTarget.OPPONENT), 2));
                    castDescription = "Every opponent mills 6 cards then draws 2 cards.";
                } break;
                #endregion       
                #region IlatianFlutePlayer
                case CardId.IlatianFlutePlayer:
                {
                    blackCost = 4;
                    cardType = CardType.Creature;
                    baseToughness = 1;
                    basePower = 2;
                    EventFilter f = (gevent) =>
                    {
                        if (gevent.type != GameEventType.MOVECARD) return false;
                        MoveCardEvent mevent = (MoveCardEvent)gevent;
                        return mevent.from?.pile == LocationPile.FIELD && mevent.to.pile == LocationPile.GRAVEYARD
                                && mevent.card.owner == controller && mevent.card.hasPT() && mevent.card.isToken == false;
                    };
                    triggeredAbilities.Add(new TriggeredAbility(this, f, "When a friendly non-token creature dies spawn a 1/1 skeltal.",
                        LocationPile.FIELD, EventTiming.Post, new SummonTokens(new ResolveTargetRule(ResolveTarget.CONTROLLER), CardId.Skeltal)));
                } break;
                #endregion
                #region Skeltal
                case CardId.Skeltal:
                {
                    //should be exiled when dies
                    cardType = CardType.Creature;
                    isToken = true;
                    baseToughness = 1;
                    basePower = 1;
                    forceColour = Colour.BLACK;
                } break;
                #endregion
                #region AberrantSacrifice
                case CardId.AberrantSacrifice:
                {
                    blackCost = 2;
                    castingCosts.Add(new MoveToCost(LocationPile.FIELD, LocationPile.GRAVEYARD, 1));
                    cardType = CardType.Sorcery;
                    fx.Add(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 2));
                    castDescription = "As an additional cost to cast this card sacrifice a creature.\nDraw 2 cards.";
                } break;
                #endregion
                #region Spark
                case CardId.Spark:
                {
                    redCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 2));
                    castDescription = "Deal 2 damage to target creature or player.";
                } break;
                #endregion
                #region MaleficentSpirit
                case CardId.MaleficentSpirit:
                {
                    blackCost = 2;
                    greyCost = 2;
                    basePower = 3;
                    baseToughness = 2;
                    cardType = CardType.Creature;
                    triggeredAbilities.Add(new TriggeredAbility(this,
                        thisETB(this),
                        thisETBDescription + "target player discards a card",
                        LocationPile.FIELD, 
                        EventTiming.Post,
                        new Effect(new MoveTo(new SelectFromTargetRule(
                            new FilterTargetRule(1, FilterLambda.PLAYER),
                            new ResolveTargetRule(ResolveTarget.LAST), 
                            p => p.hand.cards.ToArray()), LocationPile.GRAVEYARD)) 
                        ));
                } break;
                #endregion
                #region Bubastis
                case CardId.Bubastis:
                {
                    blueCost = 4;
                    greyCost = 3;
                    basePower = 5;
                    baseToughness = 5;
                    cardType = CardType.Creature;
                    activatedAbilities.Add(new ActivatedAbility(this, 
                        new Cost(new ManaCost(0, 2, 0, 0, 0, 1)),
                        new Effect(new Exhaust(new FilterTargetRule(1, FilterLambda.CREATURE, FilterLambda.ONFIELD))),
                        true,
                        LocationPile.FIELD, 
                        "1BB: Exhaust target creature."
                        ));
                } break;
                #endregion
                #region HauntedChapel
                case CardId.HauntedChapel:
                {
                    blackCost = 2;
                    whiteCost = 2;
                    cardType = CardType.Relic;
                    activatedAbilities.Add(new ActivatedAbility(this,
                        new Cost(new ManaCost(1, 0, 1, 0, 0, 0), new MoveToCost(LocationPile.GRAVEYARD, LocationPile.EXILE, 1), new ExhaustCost(this)),
                        new Effect(new SummonTokens(new ResolveTargetRule(ResolveTarget.CONTROLLER), CardId.Spirit)),
                        true,
                        LocationPile.FIELD, 
                        "E, BW, Exile a card from your graveyard: Summon a Spirit token."
                        ));
                } break;
                #endregion
                #region Spirit
                case CardId.Spirit:
                {
                    isToken = true;
                    keyAbilities.Add(KeyAbility.Flying);
                    forceColour = Colour.WHITE;
                    basePower = 1;
                    baseToughness = 1;
                } break;
                #endregion
                #region OneWithNature
                case CardId.OneWithNature:
                {
                    greenCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new GainBonusMana(new ResolveTargetRule(ResolveTarget.CONTROLLER), Colour.GREEN, Colour.GREEN, Colour.GREEN));
                    castDescription = "Add GGG until end of step.";
                } break;
                #endregion
                #region MysteriousLilac
                case CardId.MysteriousLilac:
                {
                    blueCost = 1;
                    cardType = CardType.Relic;
                        triggeredAbilities.Add(new TriggeredAbility(this,
                            thisETB(this),
                            thisETBDescription + "draw 1 card.",
                            LocationPile.FIELD, 
                            EventTiming.Post,
                            new Effect(new Draw(new ResolveTargetRule(ResolveTarget.CONTROLLER), 1))
                            ));
                    activatedAbilities.Add(new ActivatedAbility(this,
                        new Cost(new ManaCost(0, 0, 0, 0, 0, 2)),
                        new Effect(new GainBonusMana(new ResolveTargetRule(ResolveTarget.CONTROLLER), Colour.BLUE)),
                        true,
                        LocationPile.FIELD,
                        "2: Gain U until end of step."
                        ));
                } break;
                #endregion
                #region Overgrow
                case CardId.Overgrow:
                {
                    greenCost = 2;
                    cardType = CardType.Instant;
                    fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.RELIC, FilterLambda.ONFIELD), LocationPile.GRAVEYARD));
                    castDescription = "Destroy target relic";
                } break;
                #endregion
                #region Abolish
                case CardId.Abolish:
                    {
                        whiteCost = 2;
                        cardType = CardType.Instant;
                        fx.Add(new MoveTo(new FilterTargetRule(1, FilterLambda.RELIC, FilterLambda.ONFIELD), LocationPile.GRAVEYARD));
                        castDescription = "Destroy target relic";
                    }
                    break;
                #endregion
                #region ElvenDruid
                case CardId.ElvenDruid:
                {
                    greenCost = 1;
                    cardType = CardType.Creature;
                    basePower = 1;
                    baseToughness = 1;
                    activatedAbilities.Add(new ActivatedAbility(this,
                        new Cost(new ExhaustCost(this)),
                        new Effect(new GainBonusMana(new ResolveTargetRule(ResolveTarget.CONTROLLER), Colour.GREEN)),
                        true,
                        LocationPile.FIELD, 
                        "E: Gain G until end of step." 
                        ));
                } break;
                #endregion
                #region ChromaticUnicorn
                case CardId.ChromaticUnicorn:
                {
                    greenCost = 1;
                    cardType = CardType.Creature;
                    basePower = 1;
                    baseToughness = 1;
                    auras.Add(new DynamicAura(crd => this == crd,
                        Modifiable.Power, 
                        () => owner.getMaxMana((int)Colour.RED) > 0 ? 2 : 0,
                        "This creature gets +2/+0 as long as you have a red mana orb." 
                        ));
                    auras.Add(new DynamicAura(crd => this == crd,
                        Modifiable.Toughness, 
                        () => owner.getMaxMana((int)Colour.WHITE) > 0 ? 2 : 0,
                        "This creature gets +0/+2 as long as you have a white mana orb."
                        ));
                    } break;
                #endregion
                #region Flamemane
                case CardId.Flamemane:
                {
                    redCost = 3;
                    greyCost = 1;
                    cardType = CardType.Creature;
                    basePower = 4;
                    baseToughness = 4;
                    keyAbilities.Add(KeyAbility.Flying);
                    activatedAbilities.Add(new ActivatedAbility(this,
                        new Cost(new ManaCost(0, 0, 0, 2, 0, 0)),
                        new Effect(new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 1)),
                        true,
                        LocationPile.FIELD, 
                        "RR: Deal 1 damage to target creature or player."
                        ));
                } break;
                #endregion
                #region CoupDeGrace
                case CardId.CoupDeGrace:
                {
                    whiteCost = 1;
                    cardType = CardType.Instant;
                    fx.Add(
                        new MoveTo(
                            new FilterTargetRule(1, FilterLambda.CREATURE, FilterLambda.ONFIELD, FilterLambda.EXHAUSTED),
                            LocationPile.GRAVEYARD));
                    castDescription = "Destroy target exhausted creature.";
                }
                    break;

                    #endregion
                #region LoneRanger
                case CardId.LoneRanger:
                {
                    //better if it isnt when summoned, instead it should be dynamic when no other creatures are on board?
                    //filter doesn't account for relics on board
                    greenCost = 2;
                    cardType = CardType.Creature;
                    basePower = 2;
                    baseToughness = 2;
                    auras.Add(new DynamicAura(crd => crd == this,
                        Modifiable.Power,
                        () => owner.field.cards.Count() == 1 ? 1 : 0,
                        "This creature gains +1/+2 as long as you control no other permanents."));
                    auras.Add(new DynamicAura(crd => crd == this,
                        Modifiable.Toughness,
                        () => owner.field.cards.Count() == 1 ? 2 : 0,
                        ""));
                    } break;
                #endregion
                #region SoothingRhapsode
                case CardId.SoothingRhapsode:
                {
                    cardType = CardType.Creature;
                    blueCost = 1;
                    basePower = 2;
                    baseToughness = 1;
                    triggeredAbilities.Add(new TriggeredAbility(this, thisETB(this), 
                        "When Soothing Rhapsod enters the battlefield all creatures gets exhausted.",
                        LocationPile.FIELD, EventTiming.Post, 
                        new Exhaust(new ResolveTargetRule(ResolveTarget.FIELDCREATURES))));
                } break;
                #endregion
                #region Hypnotist
                case CardId.Hypnotist:
                {
                    blueCost = 2;
                    greyCost = 1;
                    cardType = CardType.Creature;
                    baseToughness = 1;
                    basePower = 2;
                    activatedAbilities.Add(new ActivatedAbility(this, 
                        new Cost(new ManaCost(0, 1, 0, 0, 0, 0), new ExhaustCost(this)), 
                        new Effect(new Exhaust(new FilterTargetRule(1, FilterLambda.ONFIELD, FilterLambda.CREATURE))),
                        true, LocationPile.FIELD, "E, U: Exhaust target creature."));
                } break;
                #endregion
                #region NerosDisciple
                case CardId.NerosDisciple:
                {
                    name = "Nero's Disciple";
                    cardType = CardType.Creature;
                    baseToughness = 1;
                    basePower = 1;
                    redCost = 2;
                    
                    activatedAbilities.Add(new ActivatedAbility(this, new Cost(new ExhaustCost(this)),
                        new Effect(new Ping(new FilterTargetRule(1, FilterLambda.ZAPPABLE), 2)),
                        true, LocationPile.FIELD, "E: deal 2 damage to target creature or player"));
                } break;
                #endregion
                #region Nero
                case CardId.Nero:
                {
                    cardType = CardType.Creature;
                    redCost = 6;
                    baseToughness = 5;
                    basePower = 2;

                    triggeredAbilities.Add(new TriggeredAbility(this, thisETB(this),
                        "When Nero enters the battlefield: deal 3 damage to all creatures and destroy all relics.",
                        LocationPile.FIELD, EventTiming.Post, 
                        new Ping(new ResolveTargetRule(ResolveTarget.FIELDCREATURES), 3),
                        new MoveTo(new ResolveTargetRule(ResolveTarget.FIELDRELICS), LocationPile.GRAVEYARD)));
                    
                    activatedAbilities.Add(new ActivatedAbility(this, new Cost(new ExhaustCost(this)),
                        new Effect(new Ping(new FilterTargetRule(1, FilterLambda.PLAYER), 4)), true,
                        LocationPile.FIELD, "E: deal 4 damage to a player."));
                } break;
                #endregion
                #region DecayingZombie
                case CardId.DecayingZombie:
                {
                    cardType = CardType.Creature;
                    blackCost = 2;
                    basePower = 4;
                    baseToughness = 5;
                    triggeredAbilities.Add(new TriggeredAbility(this, stepFilter(Step.END, true), "At the beginning of your endstep this creature gains -1/-1.",
                        LocationPile.FIELD, EventTiming.Post, 
                        new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Power, never, -1),
                        new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Toughness, never, -1)));
                    race = Race.Zombie;
                } break;
                #endregion
                #region HourOfTheWolf
                case CardId.HourOfTheWolf:
                {
                    name = "Hour of the Wolf";
                    greenCost = 3;
                    greyCost = 1;
                    cardType = CardType.Sorcery;
                    fx.Add(new SummonTokens(new ResolveTargetRule(ResolveTarget.CONTROLLER), CardId.Wolf, CardId.Wolf));
                    castDescription = "Summon two wolves. Wolves grant +1/+1 to all other wolves.";
                } break;
                #endregion
                #region Wolf
                case CardId.Wolf:
                {
                        //todo make wolf a subtype of creatures, so that all wolves gain buff not only identical cards
                    cardType = CardType.Creature;
                    isToken = true;
                    baseToughness = 2;
                    basePower = 2;
                    forceColour = Colour.GREEN;
                    Aura a = new Aura(
                        (crd) => crd.cardId == CardId.Wolf && crd != this,
                        Modifiable.Power, 1,
                        "Other wolves get +1/+1");
                    Aura aa = new Aura(
                        (crd) => crd.cardId == CardId.Wolf && crd != this,
                        Modifiable.Toughness, 1,
                        "");
                    auras.Add(a);
                    auras.Add(aa);
                } break;
                #endregion
                #region LoneWolf
                case CardId.LoneWolf:
                {
                    name = "Lone Wolf";
                    greenCost = 3;
                    baseToughness = 2;
                    basePower = 2;
                    cardType = CardType.Creature;
                    EventFilter f = (gevent) =>
                    {
                        if (gevent.type != GameEventType.MOVECARD) return false;
                        MoveCardEvent mevent = (MoveCardEvent)gevent;
                        return mevent.from?.pile == LocationPile.FIELD && mevent.to.pile == LocationPile.GRAVEYARD
                                && mevent.card.cardId == CardId.Wolf && mevent.card.owner == controller;
                    };
                    triggeredAbilities.Add(new TriggeredAbility(this, f, "When a friendly wolf dies this creature gains +1/+1",
                        LocationPile.FIELD, EventTiming.Post, new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Power, never, 1),
                        new ModifyUntil(new ResolveTargetRule(ResolveTarget.SELF), Modifiable.Toughness, never, 1)));
                }
                break;
                #endregion
                #region NaturesAttendant
                //todo jasin: heal isnt really heal as much as its target gains hp. creature can get more than maxhp
                case CardId.NaturesAttendant:
                    {
                        greenCost = 1;
                        baseToughness = 3;
                        basePower = 1;
                        cardType = CardType.Creature;
                        activatedAbilities.Add(new ActivatedAbility(this, new Cost(new ExhaustCost(this)),
                            new Effect(new Ping(new FilterTargetRule(1, FilterLambda.CREATURE), -2)), true,
                            LocationPile.FIELD, "E: Heal target creature for 2 health."));
                    }
                    break;
                #endregion
                #region default
                default:
                    {
                        throw new Exception("pls no" + c.ToString());
                    }
                #endregion
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

        //todo enable specifying which players step (not just a bool)
        private static EventFilter stepFilter(Step step, bool onlyOwnersTurn = false)
        {
            return @e =>
            {
                if (e.type != GameEventType.STEP) return false;
                StepEvent stepEvent = (StepEvent)e;
                if (onlyOwnersTurn) return stepEvent.step == step && stepEvent.activePlayer.isHero;
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
            return exhausted;
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
            return canExhaust;
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
            _summoningSick = true;
            exhausted = false;
            if (defenderOf != null) defenderOf.defendedBy = null;
            if (defendedBy != null) defendedBy.defenderOf = null;
            defendedBy = defenderOf = null;
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

        public static Rarity rarityOf(CardId id)
        {
            return rarities[(int)id];
        }
        private static Rarity[] rarities = new Rarity[Enum.GetNames(typeof(CardId)).Count()];

        static Card()
        {
            rarities[(int)CardId.Kappa] = Rarity.Common;
            rarities[(int)CardId.GrizzlyBear] = Rarity.Uncommon;
            rarities[(int)CardId.LightningBolt] = Rarity.Uncommon;
            rarities[(int)CardId.SolemnAberration] = Rarity.Common;
            rarities[(int)CardId.PropheticVision] = Rarity.Common;
            rarities[(int)CardId.ForkedLightning] = Rarity.Common;
            rarities[(int)CardId.DragonHatchling] = Rarity.Common;
            rarities[(int)CardId.TempleHealer] = Rarity.Ebin;
            rarities[(int)CardId.Rapture] = Rarity.Uncommon;
            rarities[(int)CardId.Squire] = Rarity.Token;
            rarities[(int)CardId.CallToArms] = Rarity.Common;
            rarities[(int)CardId.ShimmeringKoi] = Rarity.Common;
            rarities[(int)CardId.Belwas] = Rarity.Legendair;
            rarities[(int)CardId.AlterTime] = Rarity.Common;
            rarities[(int)CardId.EvolveFangs] = Rarity.Common;
            rarities[(int)CardId.GrizzlyCub] = Rarity.Common;
            rarities[(int)CardId.YungLich] = Rarity.Ebin;
            rarities[(int)CardId.Unmake] = Rarity.Common;
            rarities[(int)CardId.EnragedDragon] = Rarity.Uncommon;
            rarities[(int)CardId.SteamBolt] = Rarity.Uncommon;
            rarities[(int)CardId.IlasGravekeeper] = Rarity.Uncommon;
            rarities[(int)CardId.FuryOfTheRighteous] = Rarity.Uncommon;
            rarities[(int)CardId.MeteorRain] = Rarity.Uncommon;
            rarities[(int)CardId.RiderOfDeath] = Rarity.Legendair;
            rarities[(int)CardId.Extinguish] = Rarity.Ebin;
            rarities[(int)CardId.RottingZombie] = Rarity.Uncommon;
            rarities[(int)CardId.EssenceOfDemise] = Rarity.Ebin;
            rarities[(int)CardId.EssenceOfRage] = Rarity.Ebin;
            rarities[(int)CardId.EssenceOfClarity] = Rarity.Ebin;
            rarities[(int)CardId.MorenianMedic] = Rarity.Uncommon;
            rarities[(int)CardId.MattysGambit] = Rarity.Ebin;
            rarities[(int)CardId.IlasGambit] = Rarity.Ebin;
            rarities[(int)CardId.BelwasGambit] = Rarity.Ebin;
            rarities[(int)CardId.Figment] = Rarity.Ebin;
            rarities[(int)CardId.StampedingDragon] = Rarity.Ebin;
            rarities[(int)CardId.ElderTreeant] = Rarity.Uncommon;
            rarities[(int)CardId.Counterspell] = Rarity.Common;
            rarities[(int)CardId.Infiltrator] = Rarity.Uncommon;
            rarities[(int)CardId.IlatianWineMerchant] = Rarity.Common;
            rarities[(int)CardId.RockhandOgre] = Rarity.Common;
            rarities[(int)CardId.GrazingBison] = Rarity.Uncommon;
            rarities[(int)CardId.SebasGambit] = Rarity.Ebin;
            rarities[(int)CardId.Spark] = Rarity.Common;
            rarities[(int)CardId.AberrantSacrifice] = Rarity.Uncommon;
            rarities[(int)CardId.MaleficentSpirit] = Rarity.Common;
            rarities[(int)CardId.Bubastis] = Rarity.Legendair;
            rarities[(int)CardId.HauntedChapel] = Rarity.Ebin;
            rarities[(int)CardId.Spirit] = Rarity.Token;
            rarities[(int)CardId.OneWithNature] = Rarity.Ebin;
            rarities[(int)CardId.MysteriousLilac] = Rarity.Uncommon;
            rarities[(int)CardId.Overgrow] = Rarity.Common;
            rarities[(int)CardId.ElvenDruid] = Rarity.Common;
            rarities[(int)CardId.ChromaticUnicorn] = Rarity.Uncommon;
            rarities[(int)CardId.Flamemane] = Rarity.Uncommon;
            rarities[(int)CardId.Abolish] = Rarity.Common;
            rarities[(int)CardId.CoupDeGrace] = Rarity.Ebin;
            rarities[(int)CardId.AlterFate] = Rarity.Uncommon;
            rarities[(int)CardId.DecayingZombie] = Rarity.Ebin;
            rarities[(int)CardId.Hypnotist] = Rarity.Common;
            rarities[(int)CardId.LoneRanger] = Rarity.Uncommon;
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
        Figment,
        StampedingDragon,
        ElderTreeant,
        Counterspell,
        Infiltrator,
        IlatianWineMerchant,
        BelwasGambit,
        RockhandOgre,
        GrazingBison,
        SebasGambit,
        AberrantSacrifice,
        Spark,
        MaleficentSpirit,
        Bubastis,
        HauntedChapel,
        Spirit,
        IlatianFlutePlayer,
        Skeltal,
        OneWithNature,
        MysteriousLilac,
        Overgrow,
        Abolish,
        ElvenDruid,
        ChromaticUnicorn,
        Flamemane,
        CoupDeGrace,
        LoneRanger,
        SoothingRhapsode,
        Hypnotist,
        NerosDisciple,
        DecayingZombie,
        Nero,
        AlterFate,
        LoneWolf,
        Wolf,
        HourOfTheWolf,
        NaturesAttendant,

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
        Ogre,
        Bison,
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
        Flying,
    }

    public enum Modifiable
    {
        Power,
        Toughness,
    }

    public enum Rarity
    {
        Xperimental,
        Token,
        Common,
        Uncommon,
        Ebin,
        Legendair,
    }

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