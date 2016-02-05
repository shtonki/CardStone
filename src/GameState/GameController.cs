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
    

    public class GameController
    {
        public GameInterface gameInterface { get; private set; } // hack public
        public bool autoPass { get; set; }

        private GameState game;
        private Random deckShuffler;
        private Player homePlayer, awayPlayer;
        private Card[] attackers, defenders;

        public GameController(GameConnection cn, GameInterface g)
        {
            gameInterface = g;
            cn.setGame(this);
            game = new GameState();
            gameInterface.setObservers(game.hero, game.villain, game.stack);
            setupEventHandlers();
            stackxd = new Stack<StackWrapper>();
        }

        public void start(bool home)
        {
            CardId[] heroCards = loadDeck();
            gameInterface.sendDeck(heroCards);
            CardId[] villainCards = gameInterface.demandDeck();
            homePlayer = home ? game.hero : game.villain;
            awayPlayer = home ? game.villain : game.hero;
            game.loadDeck(homePlayer, home ? heroCards : villainCards);
            game.loadDeck(awayPlayer, !home ? heroCards : villainCards);

            int seed;
            bool goingFirst;
            if (home)
            {
                Random r = new Random();
                seed = r.Next();
                gameInterface.sendSelection(seed);
                Choice c = gameInterface.getChoice("Do you want to go first?", Choice.Yes, Choice.No);
                gameInterface.sendSelection((int)c);
                goingFirst = c == Choice.Yes;
            }
            else
            {
                seed = gameInterface.demandSelection();
                Choice c = (Choice)gameInterface.demandSelection();
                goingFirst = c == Choice.No;
            }
            deckShuffler = new Random(seed);
            shuffleDeck(homePlayer);
            shuffleDeck(awayPlayer);
            game.setHeroStarting(goingFirst);
            handleEvent(new DrawEvent(game.hero, 4));
            handleEvent(new DrawEvent(game.villain, 4));
            loop();
        }

        private CardId[] loadDeck()
        {
            return new[]
            {
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
                CardId.Haunt,
            };
        }

        public void winGame(bool wonnered)
        {
            //GameController.
        }

        private EventHandler[] baseEventHandlers = new EventHandler[Enum.GetNames(typeof(GameEventType)).Length];
        private Stack<StackWrapper> stackxd;
        private List<TriggeredAbility> waitingTriggeredAbilities = new List<TriggeredAbility>();

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
            Card card = game.makeCard(e.player, e.id);
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

            

            moveCardTo(card, game.stack); 
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
                gameInterface.setStep(game.currentStep, game.herosTurn);

                switch (game.currentStep)
                {
                    case Step.UNTOP:
                    {
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
                            game.advanceStep(); //skip defenders
                            game.advanceStep(); //skip damage
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

                game.advanceStep();
            }
        }

        private void untopStep()
        {
            handleEvent(new UntopPlayerEvent(game.activePlayer));

            int s;
            if (game.herosTurn)
            {
                gameInterface.showAddMana(true);
                int c;
                do
                {
                    c = (int)gameInterface.getManaColour();
                } while (game.hero.getMaxMana(c) == 6);
                gameInterface.showAddMana(false);
                s = c;
                gameInterface.sendSelection(c);
            }
            else
            {
                s = gameInterface.demandSelection();
            }

            handleEvent(new GainManaOrbEvent(game.activePlayer, s));
            handleEvent(new StepEvent(StepEvent.UNTOP));
            givePriority(false);
        }

        private void drawStep()
        {
            handleEvent(new DrawEvent(game.activePlayer));
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
            if (game.herosTurn && !autoPass)
            {
                attackers = chooseMultiple("Choose attackers", c =>
                {
                    if (c.owner.side == LocationPlayer.HERO && c.canAttack() && !(c.attacking))
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
                gameInterface.sendMultiSelection(attackers);
            }
            else
            {
                attackers = gameInterface.demandMultiSelection().Select(@i => game.getCardById(i)).ToArray();
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
            if (game.herosTurn)
            {
                defenders = gameInterface.demandMultiSelection().Select(@i => game.getCardById(i)).ToArray();
                Card[] v = gameInterface.demandMultiSelection().Select(@i => game.getCardById(i)).ToArray();

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
                gameInterface.sendMultiSelection(ls.Item1);
                gameInterface.sendMultiSelection(ls.Item2);
            }

            givePriority(false);
        }

        private void damageStep()
        {
            foreach (var attacker in game.activePlayer.field.cards)
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
                        handleEvent(new DamagePlayerEvent(game.inactivePlayer, attacker, attacker.currentPower));
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
            while (true)
            {
                CastAction action;
                checkGameState();

                action = getCastAction(game.herosTurn);

                if (action.isPass())  //turn.heroTurn player passed
                {
                    action = getCastAction(!game.herosTurn);
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
                    if (game.stack.count > 0)
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
                foreach (var v in game.allCards)
                {
                    v.checkModifiers();
                }
                

                foreach (var v in game.fieldCards)
                {
                    foreach (var a in v.auras)
                    {
                        foreach (var c in game.fieldCards)
                        {
                            if (a.filter(c))
                            {
                                c.modify(a.attribute, a.value, () => true);
                            }
                        }
                    }
                }
                

                foreach (var v in game.fieldCards)
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
        
        private CastAction getCastAction(bool actingPlayer)
        {
            CastAction action;

            if (actingPlayer)
            {
                if (autoPass)
                {
                    action = new CastAction();
                }
                else
                {
                    gameInterface.setContext("Your turn to act.", Choice.Pass);
                    action = gainPriority();
                    gameInterface.sendCastAction(action);
                    gameInterface.clearContext();
                }
            }
            else
            {
                action = gameInterface.demandCastAction();
            }

            return action;
        }

        private CastAction gainPriority()
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
                        var abilities = c.getAvailableActivatedAbilities(game.heroCanSorc);
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

                            if (c.owner == game.hero && !c.canDefend()) { continue; }

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

            if (!stackxd.Pop().Equals(x) || game.stack.peek() != x.card)
            {
                throw new CannotUnloadAppDomainException("we don't need to deal with the immigration \"problem\" that's not politically correct xddd");
            }

            Ability a = x.ability;
            Card card = x.card;

            List<GameEvent> es = a.resolve(card, x.targets, gameInterface, game);

            foreach (GameEvent e in es)
            {
                handleEvent(e);
            }

            if (card.isDummy)
            {
                //throw new NotImplementedException();
                game.stack.remove(card);
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

            foreach (Card c in game.allCards)
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
                if (l.pile == LocationPile.STACK) { return game.stack; }
                throw new ArgumentException();
            }

            Player p = l.side == LocationPlayer.HERO ? game.hero : game.villain;
            return p.getPile(l.pile);
        }

        public Card getCardById(int i)
        {
            return game.getCardById(i);
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
