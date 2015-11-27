using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace stonekart
{
    public class Game
    {
        private Player hero, villain, homePlayer, awayPlayer, activePlayer, inactivePlayer;
        private Pile stack;

        private bool active;

        private GameConnection connection;
        private CardFactory cardFactory;

        private KappaMan kappa;

        private delegate bool xd(Card c);

        public Game(GameConnection cn)
        {
            connection = cn;
            connection.setGame(this);
            cardFactory = new CardFactory();

            setupEventHandlers();

            hero = new Player();
            villain = new Player();
            homePlayer = cn.asHomePlayer() ? hero : villain;
            awayPlayer = cn.asHomePlayer() ? villain : hero;

            stack = new Pile();

            setLocations();

            MainFrame.setObservers(hero, villain, stack);
        }

        public void start()
        {
            CardId[] myCards = loadDeck();

            bool starting = connection.asHomePlayer();

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

            hero.loadDeck(myDeck, new Location(Location.DECK, Location.HEROSIDE));
            villain.loadDeck(otherDeck, new Location(Location.DECK, Location.VILLAINSIDE));


            hero.shuffleDeck();
            villain.shuffleDeck();

            gameStart();
        }

        private CardId[] loadDeck()
        {
            return new[]
            {
                CardId.SolemnAberration, CardId.SolemnAberration, CardId.SolemnAberration, CardId.SolemnAberration,
                CardId.SolemnAberration, CardId.SolemnAberration,
            };
        }

        private void gameStart()
        {
            hero.draw(4);

            loop();
        }

        private void setupEventHandlers()
        {
            kappa = new KappaMan();

            kappa.addBaseHandler(new EventHandler(GameEventType.DRAW, @gevent =>
            {
                DrawEvent e = (DrawEvent)gevent;
                e.getPlayer().draw();
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.CAST, @gevent =>
            {
                CastEvent e = (CastEvent)gevent;
                e.getCard().moveTo(stack);
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.MOVECARD, @gevent =>
            {
                MoveCardEvent e = (MoveCardEvent)gevent;
                e.getCard().moveTo(e.getLocation());
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.GAINMANA, @gevent =>
            {
                GainManaOrbEvent e = (GainManaOrbEvent)gevent;
                e.getPlayer().addMana(e.getColor());
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.UNTOPPLAYER, @gevent =>
            {
                UntopPlayerEvent e = (UntopPlayerEvent)gevent;
                e.getPlayer().untop();
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.RESOLVE, @gevent =>
            {
                ResolveEvent e = (ResolveEvent)gevent;
                e.getCard().resolve(this);
            }));
        }

        private void setLocations()
        {
            Location.setPile(Location.HAND, Location.HEROSIDE, hero.getHand());
            Location.setPile(Location.DECK, Location.HEROSIDE, hero.getDeck());
            Location.setPile(Location.FIELD, Location.HEROSIDE, hero.getField());
            Location.setPile(Location.GRAVEYARD, Location.HEROSIDE, hero.getGraveyard());
            Location.setPile(Location.EXILE, Location.HEROSIDE, hero.getExile());

            Location.setPile(Location.HAND, Location.VILLAINSIDE, villain.getHand());
            Location.setPile(Location.DECK, Location.VILLAINSIDE, villain.getDeck());
            Location.setPile(Location.FIELD, Location.VILLAINSIDE, villain.getField());
            Location.setPile(Location.GRAVEYARD, Location.VILLAINSIDE, villain.getGraveyard());
            Location.setPile(Location.EXILE, Location.VILLAINSIDE, villain.getExile());

            Location.setPile(Location.STACK, Location.HEROSIDE, stack);
        }


        private void loop()
        {
            //Thread.CurrentThread.Name = "XDDDDDDDDDD";
            active = connection.asHomePlayer();

            while (true)
            {
                activePlayer = active ? hero : villain;

                //untop step
                untopStep();
                advanceStep();

                //draw step
                drawStep();
                advanceStep();

                //main phase 1
                mainStep(1);
                advanceStep();

                //startCombat
                startCombatStep();
                advanceStep();

                //attackers
                chooseAttackersStep();
                advanceStep();

                //defenders
                chooseDefendersStep();
                advanceStep();

                //combatDamage
                combatDamageStep();
                advanceStep();

                //endCombat
                endCombatStep();
                advanceStep();

                //main2
                mainStep(2);
                advanceStep();

                //end
                endStep();
                advanceStep();

                active = !active;
            }
        }

        private void untopStep()
        {
            raiseEvent(new UntopPlayerEvent(activePlayer));

            int s;
            if (active)
            {
                MainFrame.showAddMana(true);
                int c;
                do
                {
                    c = getManaColor();
                } while (hero.getMaxMana(c) == 6);
                MainFrame.showAddMana(false);
                s = c;
                raiseAction(new SelectAction(c));
            }
            else
            {
                s = demandSelection();
            }

            raiseEvent(new GainManaOrbEvent(activePlayer, s));
            raiseEvent(new StepEvent(StepEvent.UNTOP));
            givePriority(false);
        }

        private void drawStep()
        {
            raiseEvent(new DrawEvent(activePlayer));
            raiseEvent(new StepEvent(StepEvent.DRAW));
            givePriority(false);
        }

        private void mainStep(int i)
        {
            raiseEvent(new StepEvent(i == 1 ?StepEvent.MAIN1 : StepEvent.MAIN2));
            givePriority(true);
        }

        private void startCombatStep()
        {
            raiseEvent(new StepEvent(StepEvent.BEGINCOMBAT));
            givePriority(false);
        }

        private void chooseAttackersStep()
        {
            Card[] attackers;

            if (active)
            {
                attackers = chooseMultiple("Choose attackers", Color.Red, Location.FIELD, @c => { return c.getOwner() == hero && c.canAttack(); });
                raiseAction(new MultiSelectAction(attackers));
            }
            else
            {
                attackers = demandMultiSelection().Select(@i => cardFactory.getCardById(i)).ToArray();
            }

            foreach (var a in attackers)
            {
                a.setAttacking(true);
            }

            raiseEvent(new StepEvent(StepEvent.ATTACKERS));
            givePriority(false);
        }

        private void chooseDefendersStep()
        {
            raiseEvent(new StepEvent(StepEvent.DEFENDERS));
            givePriority(false);
        }

        private void combatDamageStep()
        {
            raiseEvent(new StepEvent(StepEvent.DAMAGE));
            givePriority(false);
        }

        private void endCombatStep()
        {
            raiseEvent(new StepEvent(StepEvent.ENDCOMBAT));
            givePriority(false);
        }

        private void endStep()
        {
            raiseEvent(new StepEvent(StepEvent.END));
            givePriority(false);
        }


        private void givePriority(bool main)
        {
            //todo make it check toggleboxes and autopass
            while (true)
            {
                Card c;
                if (active)
                {
                    c = castOrPass(main);
                }
                else
                {
                    c = demandCastOrPass();
                }

                if (c == null)  //active player passed
                {
                    if (active)
                    {
                        c = demandCastOrPass();
                    }
                    else
                    {
                        if (stack.Count == 0) //todo make this respect stop options
                        {
                            c = null;
                            raiseAction(new CastAction());
                        }
                        else
                        {
                            c = castOrPass(main);
                        }
                    }
                }

                if (c != null)
                {
                    raiseEvent(new CastEvent(c));
                }
                else //both passed
                {
                    if (stack.Count > 0)
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

        private void advanceStep()
        {
            MainFrame.advanceStep();
        }

        /// <summary>
        /// Makes the user either pick a card or pass priority, then calls raiseAction on the resulting action which is either a PassAction or a CastAction
        /// </summary>
        /// <param name="main"></param>
        /// <returns>A Card if a card was selected, null otherwise</returns>
        private Card castOrPass(bool main)
        {
            MainFrame.setMessage("You have priority");
            while (true)
            {
                MainFrame.showButtons(ACCEPT);
                while (true)
                {
                    Foo f = getFoo();
                    if (f is FooButton)
                    {
                        var b = (FooButton)f;
                        if (b.getType() == ButtonPanel.ACCEPT)
                        {
                            MainFrame.clear();
                            raiseAction(new CastAction());
                            return null;
                        }
                    }
                    else if (f is CardButton)
                    {
                        CardButton b = (CardButton)f;
                        Card c = b.getCard();
                        if (((main && stack.Count == 0) || c.isInstant()) && c.isCastable())
                        {
                            var v = c.getCastingCost().check(c);
                            if (v != null)
                            {
                                c.getCastingCost().pay(c, v);
                                MainFrame.clear();
                                raiseAction(new CastAction(c, v));
                                return c;
                            }
                        }
                    }
                }
            }
        }


        private Card demandCastOrPass()
        {
            var v = connection.demandAction(typeof(CastAction)) as CastAction;
            return v.getCard();
        }

        private int demandSelection()
        {
            var v = connection.demandAction(typeof (SelectAction)) as SelectAction;
            return v.getSelection();
        }

        private CardId[] demandDeck()
        {
            var v = connection.demandAction(typeof(DeclareDeckAction)) as DeclareDeckAction;
            return v.getIds();
        }

        private int[] demandMultiSelection()
        {
            var v = connection.demandAction(typeof(MultiSelectAction)) as MultiSelectAction;
            return v.getSelections();
        }


        private Card[] chooseMultiple(string message, Color c, int location, xd xd)
        {
            MainFrame.setMessage(message);

            List<CardButton> bs = new List<CardButton>();
            while (true) 
            {
                MainFrame.showButtons(ACCEPT);
                while (true)
                {
                    Foo f = getFoo();
                    if (f is FooButton)
                    {
                        var b = (FooButton)f;
                        if (b.getType() == ButtonPanel.ACCEPT)
                        {
                            MainFrame.clear();

                            Card[] r = new Card[bs.Count];

                            for (int i = 0; i < bs.Count; i++)
                            {
                                r[i] = bs[i].getCard();
                                bs[i].setBorder(null);
                            }

                            return r;
                        }
                    }
                    else if (f is CardButton)
                    {
                        var cb = f as CardButton;

                        Card crd = cb.getCard();

                        if (!xd(crd)) { continue; }

                        if (bs.Contains(cb))
                        {
                            bs.Remove(cb);
                            cb.setBorder(null);
                        }
                        else
                        {
                            bs.Add(cb);
                            cb.setBorder(c);
                        }
                    }
                }
            }
        }

        private void choseBlockers()
        {
            
        }

        private void combatDamage()
        {
            
        }

        private void resolveTop()
        {
            raiseEvent(new ResolveEvent(stack.peek()));
            //c.ToOwners(Location.FIELD);
            //raiseEvent(new ResolveCardEvent(c));
        }

        public Player getHero()
        {
            return hero;
        }

        public void raiseEvent(GameEvent e)
        {
            kappa.handle(e);
        }

        private void raiseAction(GameAction a)
        {
            connection.sendGameAction(a);
        }


        public Card getCardById(int i)
        {
            return cardFactory.getCardById(i);
        }



        private Foo f;
        private AutoResetEvent e = new AutoResetEvent(false);

        private Foo getFoo()
        {
            e.WaitOne();
            if (f == null) { throw new Exception("this should never happen kappa"); }
            Foo r = f;
            f = null;

            return r;
        }

        private Card getCard()
        {
            while (true)
            {
                Foo f = getFoo();
                if (f is CardButton)
                {
                    return ((CardButton)f).getCard();
                }
            }
        }

        private FooButton getButton(int i)
        {
            MainFrame.showButtons(i);
            while (true)
            {
                Foo f = getFoo();
                if (f is FooButton)
                {
                    MainFrame.showButtons(NONE);
                    return (FooButton)f;
                }
            }
        }

        private int getManaColor()
        {
            while (true)
            {
                Foo f = getFoo();
                if (f is PlayerPanel.ManaButton)
                {
                    return ((PlayerPanel.ManaButton)f).getColor();
                }
            }
        }

        public void fooPressed(Foo foo)
        {
            f = foo;
            e.Set();
        }

        private const int
            NONE = ButtonPanel.NOTHING,
            ACCEPT = ButtonPanel.ACCEPT,
            ACCEPTCANCEL = ButtonPanel.ACCEPT | ButtonPanel.CANCEL;


        private class KappaMan
        {
            private EventHandler[] xds;

            public KappaMan()
            {
                xds = new EventHandler[100]; //todo nope
            }

            public void addBaseHandler(EventHandler e)
            {
                int i = (int)e.type;
                if (xds[i] != null) { throw new Exception("event already handled"); }
                xds[i] = e;
            }

            public void handle(GameEvent e)
            {
                if (xds[(int)e.getType()] == null)
                {
                    System.Console.WriteLine("No handler for " + e.GetType());
                    return;
                }

                xds[(int)e.getType()].invoke(e);
            }
        }

        private class CardFactory
        {
            private Dictionary<int, Card> cards = new Dictionary<int, Card>();
            private int ctr = 0;

            public Card makeCard(Player owner, CardId id)
            {
                int i = ctr++;
                Card c = new Card(id);
                c.setId(i);
                c.setOwner(owner);
                cards.Add(i, c);
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


}
