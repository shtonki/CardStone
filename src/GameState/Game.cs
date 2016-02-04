using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace stonekart
{
    public class TurnTracker
    {
        public Step step { get; private set; }
        public bool heroTurn;

        public TurnTracker()
        {
        }

        public void advanceStep()
        {
            step = (Step)(((int)step + 1)%10);
            heroTurn = step == 0 ? !heroTurn : heroTurn;
        }
    }

    public enum Step
    {
        UNTOP,
        DRAW,
        MAIN1,
        STARTCOMBAT,
        ATTACKERS,
        DEFENDERS,
        DAMAGE,
        ENDCOMBAT,
        MAIN2,
        END,
    }

    public class Game
    {
        public Step currentStep => turn.step;
        public GameInterface gameInterface { get; private set; }

        private Player hero, villain, homePlayer, awayPlayer, activePlayer, inactivePlayer;
        private Pile stack;

        private Card[] attackers, defenders;

        private IEnumerable<Card> allCards => cardFactory.allCards;
        private IEnumerable<Card> heroCards => cardFactory.heroCards;
        private IEnumerable<Card> villainCards => cardFactory.villainCards;


        private TurnTracker turn = new TurnTracker();

        public bool autoPass { get; set; }

        public GameConnection connection { get; private set; }
        private CardFactory cardFactory;

        private EventHandler[] baseEventHandlers = new EventHandler[Enum.GetNames(typeof(GameEventType)).Length];

        private Stack<StackWrapper> stackxd;

        private List<TriggeredAbility> waitingTriggeredAbilities = new List<TriggeredAbility>();

        private Random deckShuffler;

        public Game(GameConnection cn, GameInterface g)
        {
            gameInterface = g;
            connection = cn;
            connection.setGame(this);
            cardFactory = new CardFactory();
               
            setupEventHandlers();

            hero = new Player(this, LocationPlayer.HERO);
            villain = new Player(this, LocationPlayer.VILLAIN);
            homePlayer = cn.asHomePlayer() ? hero : villain;
            awayPlayer = cn.asHomePlayer() ? villain : hero;

            stack = new Pile(new Location(LocationPile.STACK, LocationPlayer.NOONE));
            stackxd = new Stack<StackWrapper>();
            

            gameInterface.setObservers(hero, villain, stack);

            CardId[] myCards = loadDeck();
            raiseAction(new DeclareDeckAction(myCards));
            CardId[] otherCards = demandDeck();

            List<Card> myDeck, otherDeck;

            if (connection.asHomePlayer())
            {
                myDeck = cardFactory.makeList(hero, myCards);
                otherDeck = cardFactory.makeList(villain, otherCards);
            }
            else
            {
                otherDeck = cardFactory.makeList(villain, otherCards);
                myDeck = cardFactory.makeList(hero, myCards);
            }

            hero.loadDeck(myDeck);
            villain.loadDeck(otherDeck);
        }

        public void start()
        {
            bool home = connection.asHomePlayer();
            int seed;
            bool goingFirst;
            if (home)
            {
                Random r = new Random();
                seed = r.Next();

                Choice c = gameInterface.getChoice("Do you want to go first?", Choice.Yes, Choice.No);
                goingFirst = c == Choice.Yes;
                raiseAction(new MultiSelectAction(new int[]{seed, (int)c}));
            }
            else
            {
                int[] fml = demandMultiSelection();
                seed = fml[0];
                goingFirst = fml[1] == (int)Choice.No;
            }
            deckShuffler = new Random(seed);
            turn.heroTurn = goingFirst;
            shuffleDeck(homePlayer);
            shuffleDeck(awayPlayer);
            handleEvent(new DrawEvent(hero, 4));
            handleEvent(new DrawEvent(villain, 4));
            loop();
        }

        private CardId[] loadDeck()
        {
            return new[]
            {
                CardId.GrizzlyCub,
                CardId.GrizzlyCub,
                CardId.GrizzlyCub,
                CardId.GrizzlyCub,
                CardId.EvolveFangs,
                CardId.EvolveFangs,
                CardId.EvolveFangs,
                CardId.EvolveFangs,
                CardId.GrizzlyBear,
                CardId.GrizzlyBear,
                CardId.GrizzlyBear,
                CardId.GrizzlyBear,
                CardId.CallToArms,
                CardId.CallToArms,
                CardId.CallToArms,
                CardId.CallToArms,
                CardId.TempleHealer,
                CardId.TempleHealer,
                CardId.TempleHealer,
                CardId.TempleHealer,
                CardId.Belwas,
                CardId.Belwas,
                CardId.Belwas,
                CardId.Belwas,
                CardId.Rapture,
                CardId.Rapture,
                CardId.LightningBolt,
                CardId.LightningBolt,
                CardId.LightningBolt,
                CardId.LightningBolt,
            };
        }

        public void winGame(bool wonnered)
        {
            //GameController.
        }

        #region eventHandlers
        private void setupEventHandlers()
        {

            addBaseHandler(GameEventType.TOPCARD, _topcard);
            addBaseHandler(GameEventType.STEP, _step);
            addBaseHandler(GameEventType.DRAW, _draw);
            addBaseHandler(GameEventType.CAST, _cast);
            addBaseHandler(GameEventType.MOVECARD, _movecard);
            addBaseHandler(GameEventType.GAINMANAORB, _gainmanaorb);
            addBaseHandler(GameEventType.UNTOPPLAYER, _untopplayer);
            addBaseHandler(GameEventType.DAMAGEPLAYER, _damageplayer);
            addBaseHandler(GameEventType.DAMAGECREATURE, _damagecreature);
            addBaseHandler(GameEventType.BURYCREATURE, _burycreature);
            addBaseHandler(GameEventType.GAINLIFE, _gainlife);
            addBaseHandler(GameEventType.SUMMONTOKEN, _summontoken);
            addBaseHandler(GameEventType.MODIFYCARD, _modifycard);
        }

        private void addBaseHandler(GameEventType t, EventAction a)
        {
            baseEventHandlers[(int)t] = new EventHandler(t, a);
        }

        private void _modifycard(GameEvent gevent)
        {
            ModifyCardEvent e = (ModifyCardEvent)gevent;
            e.card.modify(e.modifiable, e.value, e.clojure);
        }
        private void _summontoken(GameEvent gevent)
        {
            SummonTokenEvent e = (SummonTokenEvent)gevent;
            Card card = cardFactory.makeCard(e.player, e.id);
            handleEvent(new MoveCardEvent(card, LocationPile.FIELD));
        }
        private void _gainlife(GameEvent gevent)
        {
            GainLifeEvent e = (GainLifeEvent)gevent;
            e.player.setLifeRelative(e.life);
        }
        private void _topcard(GameEvent gevent)
        {
            TopEvent e = (TopEvent)gevent;
            e.getCard().topped = true;
        }
        private void _step(GameEvent gevent)
        {
            
        }
        private void _draw(GameEvent gevent)
        {
            DrawEvent e = (DrawEvent)gevent;
            
            int i = 0;
            while (i++ < e.cardCount)
            {
                handleEvent(new MoveCardEvent(e.player.deck.peek(), e.player.hand.location));
            }
            e.player.notifyObservers();
            
        }
        private void _cast(GameEvent gevent)
        {
            CastEvent e = (CastEvent)gevent;
            StackWrapper v = e.getStackWrapper();
            //v.card = v.card.createDummy();
            Card card = v.card;

            

            moveCardTo(card, stack); 
            stackxd.Push(v);
            v.card.stackWrapper = v;

            v.card.owner?.notifyObservers();
        }
        private void _movecard(GameEvent gevent)
        {
            MoveCardEvent e = (MoveCardEvent)gevent;
            moveCardTo(e.card, e.to);
            e.card.moveReset();
            e.card.owner.notifyObservers();
        }
        private void _gainmanaorb(GameEvent gevent)
        {
            GainManaOrbEvent e = (GainManaOrbEvent)gevent;
            e.player.addMana(e.getColor());
        }
        private void _untopplayer(GameEvent gevent)
        {
            //todo(seba) make this raise UNTOPCARD events
            UntopPlayerEvent e = (UntopPlayerEvent)gevent;
            e.player.untop();
        }
        private void _damageplayer(GameEvent gevent)
        {
            DamagePlayerEvent e = (DamagePlayerEvent)gevent;
            e.player.setLifeRelative(-e.damage);
        }
        private void _burycreature(GameEvent gevent)
        {
            BuryCreatureEvent e = (BuryCreatureEvent)gevent;
            handleEvent(new MoveCardEvent(e.getCard(), LocationPile.GRAVEYARD));
        }
        private void _damagecreature(GameEvent gevent)
        {
            DamageCreatureEvent e = (DamageCreatureEvent)gevent;
            e.creature.damage(e.damage);
        }
        #endregion
        
        private void loop()
        {
            while (true)
            {
                gameInterface.setStep(turn);

                switch (turn.step)
                {
                    case Step.UNTOP:
                    {
                        activePlayer = turn.heroTurn ? hero : villain;
                        inactivePlayer = turn.heroTurn ? villain : hero;
                        autoPass = false;

                        untopStep();
                    } break;

                    case Step.DRAW:
                    {
                        drawStep();
                    } break;
                    case Step.MAIN1:
                    {
                        mainStep(1);
                    } break;
                    case Step.STARTCOMBAT:
                    {
                        startCombatStep();
                    } break;
                    case Step.ATTACKERS:
                    {
                        if (!attackersStep()) //no attackers were declared
                        {
                            turn.advanceStep(); //skip defenders
                            turn.advanceStep(); //skip damage
                        }
                    } break;
                    case Step.DEFENDERS:
                    {
                        defendersStep();
                    } break;
                    case Step.DAMAGE:
                    {
                        damageStep();
                    } break;
                    case Step.ENDCOMBAT:
                    {
                        endCombatStep();
                    } break;
                    case Step.MAIN2:
                    {
                        mainStep(2);
                    } break;
                    case Step.END:
                    {
                        endStep();
                    } break;
                }

                turn.advanceStep();
            }
        }

        private void untopStep()
        {
            handleEvent(new UntopPlayerEvent(activePlayer));

            int s;
            if (turn.heroTurn)
            {
                gameInterface.showAddMana(true);
                int c;
                do
                {

                    c = (int)gameInterface.getManaColour();
                } while (hero.getMaxMana(c) == 6);
                gameInterface.showAddMana(false);
                s = c;
                raiseAction(new SelectAction(c));
            }
            else
            {
                s = demandSelection();
            }

            handleEvent(new GainManaOrbEvent(activePlayer, s));
            handleEvent(new StepEvent(StepEvent.UNTOP));
            givePriority(false);
        }

        private void drawStep()
        {
            handleEvent(new DrawEvent(activePlayer));
            handleEvent(new StepEvent(StepEvent.DRAW));
            givePriority(false);
        }

        private void mainStep(int i)
        {
            handleEvent(new StepEvent(i == 1 ?StepEvent.MAIN1 : StepEvent.MAIN2));
            givePriority(true);
        }

        private void startCombatStep()
        {
            handleEvent(new StepEvent(StepEvent.BEGINCOMBAT));
            givePriority(false);
        }

        private bool attackersStep()
        {
            if (turn.heroTurn && !autoPass)
            {
                attackers = chooseMultiple("Choose attackers", c =>
                {
                    if (c.owner == hero && c.canAttack() && !(c.attacking))
                    {
                        c.attacking = true;
                        return true;
                    }
                    else
                    {
                        c.attacking = false;
                        return false;
                    }
                });
                raiseAction(new MultiSelectAction(attackers));
            }
            else
            {
                attackers = demandMultiSelection().Select(@i => cardFactory.getCardById(i)).ToArray();
                foreach (Card c in attackers)
                {
                    c.attacking = true;
                }
            }

            if (attackers.Length == 0)
            {
                attackers = null;
                return false;
            }

            foreach (var a in attackers)
            {
                handleEvent(new TopEvent(a));
                //todo attackers event
            }
            
            givePriority(false);
            return true;
        }

        private void defendersStep()
        {
            if (turn.heroTurn)
            {
                defenders = demandMultiSelection().Select(@i => cardFactory.getCardById(i)).ToArray();
                Card[] v = demandMultiSelection().Select(@i => cardFactory.getCardById(i)).ToArray();

                if (defenders.Length != v.Length) { throw new NetworkInformationException();}

                for (int i = 0; i < defenders.Length; i++)
                {
                    defenders[i].defenderOf = v[i];
                    v[i].defendedBy = defenders[i];
                }
            }
            else
            {
                Tuple<Card[], Card[]> ls = chooseDefenders();
                defenders = ls.Item1;
                raiseAction(new MultiSelectAction(ls.Item1));
                raiseAction(new MultiSelectAction(ls.Item2));
            }

            givePriority(false);
        }

        private void damageStep()
        {
            foreach (var attacker in activePlayer.field.cards)
            {
                if (attacker.attacking)
                {
                    if (attacker.defended)
                    {
                        Card defender = attacker.defendedBy;
                        handleEvent(new DamageCreatureEvent(defender, attacker, attacker.currentPower));
                        handleEvent(new DamageCreatureEvent(attacker, defender, defender.currentPower));
                    }
                    else
                    {
                        handleEvent(new DamagePlayerEvent(inactivePlayer, attacker, attacker.currentPower));
                    }
                }
            }
            
            givePriority(false);
        }

        private void endCombatStep()
        {
            if (attackers != null)
            {

                foreach (Card c in attackers)
                {
                    //gameInterface.getCardButton(c).setBorder(null);
                    c.attacking = false;
                    c.defendedBy = null;
                }

                foreach (Card c in defenders)
                {
                    //gameInterface.getCardButton(c).setBorder(null);
                    c.defenderOf = null;
                }
                attackers = defenders = null;
            }

            handleEvent(new StepEvent(StepEvent.ENDCOMBAT));
            givePriority(false);
        }

        private void endStep()
        {
            handleEvent(new StepEvent(StepEvent.END));
            givePriority(false);
        }


        private void givePriority(bool main)
        {
            //todo(seba) make it check toggleboxes and autopass
            //todo(seba) make it a PassAction instead of a fucking null pointer just waiting to raise an exception and rain on my parade
            while (true)
            {
                checkGameState();
                CastAction action;
                if (turn.heroTurn)
                {
                    action = castOrPass(main && stack.count == 0);
                }
                else
                {
                    action = demandCastOrPass();
                }

                if (action.isPass())  //turn.heroTurn player passed
                {
                    if (turn.heroTurn)
                    {
                        action = demandCastOrPass();
                    }
                    else
                    {
                        action = castOrPass(false);
                    }
                }
                if (!action.isPass())
                {
                    StackWrapper stackWrapper = action.getStackWrapper();
                    Ability a = stackWrapper.ability;
                    if (a is ActivatedAbility)
                    {
                        ((ActivatedAbility)a).getCost().pay(stackWrapper.card, action.getCosts());
                    }
                    handleEvent(new CastEvent(action.getStackWrapper()));
                }
                else //both passed
                {
                    if (stack.count > 0)
                    {
                        resolveTop();                        
                    }
                    else
                    {
                        break;
                    }
                }

            }
        }

        public void shuffleDeck(Player p)
        {
            p.deck.shuffle(deckShuffler);
        }
        
        

        private void checkGameState()
        {
            List<BuryCreatureEvent> xd = new List<BuryCreatureEvent>();
            do
            {
                xd.Clear();
                foreach (var v in allCards)
                {
                    v.checkModifiers();
                }

                var fld = hero.field.cards.Concat(villain.field.cards);
                var field = fld as Card[] ?? fld.ToArray();

                foreach (var v in field)
                {
                    foreach (var a in v.auras)
                    {
                        foreach (var c in field)
                        {
                            if (a.filter(c))
                            {
                                c.modify(a.attribute, a.value, () => true);
                            }
                        }
                    }
                }

                var vs = allCards;

                foreach (var v in field)
                {
                    if (v.currentToughness <= 0)
                    {
                        xd.Add(new BuryCreatureEvent(v));
                    }
                }


                foreach (GameEvent v in xd)
                {
                    handleEvent(v);
                }

            } while (xd.Count > 0);
            /*
            foreach (var v in hero.field.cards)
            {
                if (v.currentToughness <= 0)
                {
                    xd.Add(new BuryCreatureEvent(v));
                }
            }

            foreach (var v in villain.field.cards)
            {
                if (v.currentToughness <= 0)
                {
                    xd.Add(new BuryCreatureEvent(v));
                }
            }
            */

            foreach (TriggeredAbility v in waitingTriggeredAbilities)
            {
                if (v.targetCount != 0)
                {
                    throw new NotImplementedException();
                }
                StackWrapper w = new StackWrapper(Card.createDummy(v), v, emptyTargetList);
                handleEvent(new CastEvent(w));
            }

            waitingTriggeredAbilities.Clear();
        }

        /// <summary>
        /// Makes the user either pick a card or pass priority, then calls raiseAction on the resulting action which is either a PassAction or a CastAction
        /// </summary>
        /// <param name="main"></param>
        /// <returns>A Card if a card was selected, null otherwise</returns>
        private CastAction castOrPass(bool main)
        {
            CastAction a;
            if (autoPass)
            {
                a = new CastAction();
            }
            else
            {
                gameInterface.setContext("Your turn to act.", Choice.Pass);
                a = _castOrPass(main);
                gameInterface.clearContext();
            }

            raiseAction(a);
            
            return a;
        }

        private CastAction _castOrPass(bool main)
        {
            while (true)
            {
                while (true)
                {
                    GameElement chosenGameElement = gameInterface.getNextGameElementPress();
                    if (chosenGameElement.choice != null)
                    {
                        Choice choice = chosenGameElement.choice.Value;
                        if (choice == Choice.Pass)
                        {
                            return new CastAction();
                        }
                        else
                        {
                            throw new Exception(); //paranoid
                        }
                    }
                    else if (chosenGameElement.card != null)
                    {
                        Card c = chosenGameElement.card;
                        var abilities = c.getAvailableActivatedAbilities(main);
                        ActivatedAbility a;
                        if (abilities.Count == 0)
                        {
                            continue;
                        }
                        else if (abilities.Count == 1)
                        {
                            a = abilities[0];
                        }
                        else
                        {
                            throw new Exception("we don't support these things yet");
                        }


                        
                        var v = a.getCost().check(c, gameInterface);
                        if (v == null) { continue; }

                        Target[] targets = getTargets(a); 
                        if (targets == null) { continue; }
                        
                        var sw = new StackWrapper(c, a, targets);
                        return new CastAction(sw, v);
                    }
                }
            }
        }

        private bool checkAutoPass()
        {
            return false;
        }

        //todo(seba) allow canceling
        private Target[] getTargets(Ability a)
        {
            gameInterface.setContext("Select target(s)", Choice.Cancel);
            Target[] targets = new Target[a.targetCount];
            TargetRule[] rules = a.targetRules;

            int i = 0;
            while (i < targets.Length)
            {
                Target t = null;
                GameElement chosenGameElement = gameInterface.getNextGameElementPress();
                if (chosenGameElement.player != null)
                {
                    t = new Target(chosenGameElement.player);
                }
                else if (chosenGameElement.card != null)
                {
                    t = new Target(chosenGameElement.card);
                }
                else if (chosenGameElement.choice != null && chosenGameElement.choice.Value == Choice.Cancel)
                {
                    targets = null;
                    break;
                }

                if (t != null && rules[i].check(t))
                {
                    targets[i++] = t;
                }
            }
            gameInterface.clearContext();
            return targets;
        }

        private CastAction demandCastOrPass()
        {
            gameInterface.setContext("Opponents turn to act.");
            var v = connection.demandAction(typeof(CastAction)) as CastAction;
            gameInterface.clearContext();
            return v;
        }

        public int demandSelection()
        {
            var v = connection.demandAction(typeof (SelectAction)) as SelectAction;
            return v.getSelection();
        }

        private CardId[] demandDeck()
        {
            var v = connection.demandAction(typeof(DeclareDeckAction)) as DeclareDeckAction;
            return v.getIds();
        }

        public int[] demandMultiSelection()
        {
            var v = connection.demandAction(typeof(MultiSelectAction)) as MultiSelectAction;
            return v.getSelections();
        }

        public Card[] demandMultiSelectionAsCards()
        {
            int[] ids = demandMultiSelection();
            Card[] r = new Card[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                r[i] = cardFactory.getCardById(ids[i]);
            }
            return r;
        }

        public void sendSelection(int i)
        {
            raiseAction(new SelectAction(i));
        }

        public void sendMultiSelection(params int[] ns)
        {
            raiseAction(new MultiSelectAction(ns));
        }

        public void sendMultiSelection(params Card[] ns)
        {
            raiseAction(new MultiSelectAction(ns));
        }

        private Card[] chooseMultiple(string message, Func<Card, bool> xd)
        {
            List<Card> cards = new List<Card>();
            gameInterface.setContext(message, Choice.PADDING, Choice.Accept);
            while (true)
            {
                while (true)
                {
                    GameElement chosenGameElement = gameInterface.getNextGameElementPress();
                    if (chosenGameElement.choice != null && chosenGameElement.choice.Value == Choice.Accept)
                    {
                        gameInterface.clearContext();
                        return cards.ToArray();
                    }
                    else if (chosenGameElement.card != null)
                    {
                        Card card = chosenGameElement.card;

                        if (!xd(card))
                        {
                            cards.Remove(card);
                            continue;
                        }

                        if (cards.Contains(card))
                        {
                            cards.Remove(card);
                        }
                        else
                        {
                            cards.Add(card);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Defenders as the first list, defended as the second</returns>
        private Tuple<Card[], Card[]> chooseDefenders()
        {
            //todo(seba) could keep track of undefended attackers
            List<Card> blockers = new List<Card>();

            while (true)
            {
                Card blocker, blocked;
                
                while (true)
                {
                    blocker = blocked = null;
                    gameInterface.setContext("Choose a defender", Choice.PADDING, Choice.Accept);
                    while (blocker == null)
                    {
                        GameElement chosenGameElement = gameInterface.getNextGameElementPress();
                        if (chosenGameElement.card != null)
                        {
                            Card c = chosenGameElement.card;

                            if (c.owner == hero && !c.canDefend()) { continue; }

                            if (c.defenderOf == null)
                            {
                                blocker = c;
                            }
                            else
                            {
                                blockers.Remove(c);
                                c.defenderOf.defendedBy = null;
                                c.defenderOf = null;
                            }
                        }
                        else if (chosenGameElement.choice != null)
                        {
                            if (chosenGameElement.choice.Value == Choice.Accept)
                            {
                                goto end;   // *unzips fedora*
                            }
                        }
                    }
                    gameInterface.clearContext();
                    gameInterface.setContext("Defending what?", Choice.Cancel);

                    while (blocked == null)
                    {
                        GameElement e = gameInterface.getNextGameElementPress();
                        if (e.card != null)
                        {
                            blocked = e.card;
                        }
                        else if (e.choice != null)
                        {
                            if (e.choice.Value == Choice.Cancel)
                            {
                                break;
                            }
                        }
                    }
                    gameInterface.clearContext();
                    if (blocked != null)
                    {
                        blocker.defenderOf = blocked;
                        blocked.defendedBy = blocker;
                        blockers.Add(blocker);
                    }
                }
            }

            end:
            gameInterface.clearContext();
            Card[] bkds = blockers.Select(@c => c.defenderOf).ToArray();
            return new Tuple<Card[], Card[]>(blockers.ToArray(), bkds);
        }

        private void combatDamage()
        {
            
        }

        private void resolveTop()
        {
            //handleEvent(new ResolveEvent(stackxd.Peek()));


            StackWrapper x = stackxd.Peek();

            if (!stackxd.Pop().Equals(x) || stack.peek() != x.card)
            {
                throw new CannotUnloadAppDomainException("we don't need to deal with the immigration \"problem\" that's not politically correct xddd");
            }

            Ability a = x.ability;
            Card card = x.card;

            List<GameEvent> es = a.resolve(card, x.targets);

            foreach (GameEvent e in es)
            {
                handleEvent(e);
            }

            if (card.isDummy)
            {
                //throw new NotImplementedException();
                stack.remove(card);
            }
            else if (card.getType() == Type.Instant || card.getType() == Type.Sorcery)
            {
                handleEvent(new MoveCardEvent(card, LocationPile.GRAVEYARD));
            }
            else
            {
                handleEvent(new MoveCardEvent(card, LocationPile.FIELD));
            }

            card.stackWrapper = null;
        }

        public Player getHero()
        {
            return hero;
        }

        
        private readonly Target[] emptyTargetList = {};
        public void handleEvent(GameEvent e)
        {
            LinkedList<TriggeredAbility>[] timingLists =
            {
                new LinkedList<TriggeredAbility>(),
                new LinkedList<TriggeredAbility>(),
                new LinkedList<TriggeredAbility>(),
            };

            foreach (var l in timingLists)
            {
                l.Clear();
            }

            foreach (Card c in allCards)
            {
                foreach (TriggeredAbility a in c.triggeredAbilities)
                {
                    if (a.filter(e))
                    {
                        timingLists[(int)a.timing].AddLast(a);
                    }
                }
            }

            timingListLambda(timingLists[(int)EventTiming.Pre]);

            int i = (int)e.type;

            //todo(seba) implement instead of
            if (baseEventHandlers[i] != null)
            {
                baseEventHandlers[i].handle(e, EventTiming.Main);
            }

            timingListLambda(timingLists[(int)EventTiming.Post]);
        }

        public void raiseTriggeredAbility(TriggeredAbility a)
        {
            waitingTriggeredAbilities.Add(a);
        }

        private void timingListLambda(LinkedList<TriggeredAbility> l)
        {
            foreach (TriggeredAbility ability in l)
            {
                if (ability.card.location.pile != ability.pile) { continue; } 
                
                if (ability.targetCount != 0) { throw new Exception("nopers2222"); }
                
                raiseTriggeredAbility(ability);
                
            }
        }

        private void raiseAction(GameAction a)
        {
            connection.sendGameAction(a);
        }

        public void moveCardTo(Card card, Pile to)
        {
            if (card.location != null)
            {
                pileFromLocation(card.location).remove(card);
            }
            else
            {
                Console.WriteLine("xddd");
            }


            to.add(card);
            card.location = to.location;
        }

        public void moveCardTo(Card c, Location l)
        {
            moveCardTo(c, pileFromLocation(l));
        }
        
        private Pile pileFromLocation(Location l)
        {
            if (l.side == LocationPlayer.NOONE)
            {
                if (l.pile == LocationPile.STACK) { return stack; }
                throw new ArgumentException();
            }

            Player p = l.side == LocationPlayer.HERO ? hero : villain;
            return p.getPile(l.pile);
        }

        public Card getCardById(int i)
        {
            return cardFactory.getCardById(i);
        }

        /*todo 0 - home 1 - away; meaning one has to flip it when getting a 
         * message from the other player which is anything but practical */
        public Player getPlayerById(int i)
        {
            return i == 0 ? homePlayer : awayPlayer;
        }
        
        /*
        private int getManaColor()
        {
            while (true)
            {
                GameUIElement f = gameInterface.getNextGameElementPress();
                if (f is PlayerPanel.ManaButton)
                {
                    return ((PlayerPanel.ManaButton)f).getColor();
                }
            }
        }
        */
        


        private class CardFactory
        {
            private Dictionary<int, Card> cards = new Dictionary<int, Card>();
            private int ctr = 0;

            private List<Card> hero = new List<Card>(40), villain = new List<Card>(40);

            public IEnumerable<Card> heroCards => hero;
            public IEnumerable<Card> villainCards => villain;
            public IEnumerable<Card> allCards => hero.Concat(villain);

            public Card makeCard(Player owner, CardId id)
            {
                int i = ctr++;
                Card c = new Card(id);
                c.setId(i);
                c.owner = owner;
                c.controller = owner;
                cards.Add(i, c);
                if (owner.getSide() == LocationPlayer.HERO)
                {
                    hero.Add(c);
                }
                else if (owner.getSide() == LocationPlayer.VILLAIN)
                {
                    villain.Add(c);
                }
                else
                {
                    throw new Exception();
                }

                return c;
            }

            public List<Card> makeList( Player p, params CardId[] ids)
            {
                return ids.Select((a) => makeCard(p, a)).ToList(); //LINQ: pretty and readable
            }

            public Card getCardById(int i)
            {
                return cards[i];
            }
        }
    }

    //the fact that making this a struct creates 20 lines of code shows what a fucking joke this language is
    //the artiste formerly known as StackWrapperFuckHopeGasTheKikes
    public class StackWrapper
    {
        public Card card;
        public Ability ability;
        public Target[] targets;

        public StackWrapper(Card c, Ability a, Target[] cs)
        {
            card = c;
            ability = a;
            targets = cs;
        }
    }
}
