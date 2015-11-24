using System;
using System.Collections.Generic;
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
            CardId[] myDeck = loadDeck();

            bool starting;

            //flip 'fair' coin xd
            if (connection.asHomePlayer())
            {
                int z = new Random().Next(2);
                raiseAction(new SelectAction(z));
                starting = z == 0;
            }
            else
            {
                starting = demandSelection() == 1;
            }

            raiseAction(new DeclareDeckAction(myDeck));
            CardId[] otherDeck = demandDeck();

            

            //raiseAction();

            if (connection.asHomePlayer())
            {
                
            }

            var v =
                cardFactory.makeList(hero, new[]
                {
                    CardId.SolemnAberration, CardId.SolemnAberration, CardId.SolemnAberration, CardId.SolemnAberration,
                    CardId.SolemnAberration, CardId.SolemnAberration,
                });

            var vx = cardFactory.makeList(villain, new[]
                {
                    CardId.SolemnAberration, CardId.SolemnAberration, CardId.SolemnAberration, CardId.SolemnAberration,
                    CardId.SolemnAberration, CardId.SolemnAberration,
                });

            hero.loadDeck(v, new Location(Location.DECK, Location.HEROSIDE));
            villain.loadDeck(vx, new Location(Location.DECK, Location.VILLAINSIDE));


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
            active = true;

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
            raiseEvent(new StepEvent(StepEvent.MAIN1));
            givePriority(true);
        }

        private void startCombatStep()
        {

        }

        private void chooseAttackersStep()
        {

        }

        private void chooseDefendersStep()
        {

        }

        private void combatDamageStep()
        {

        }

        private void endCombatStep()
        {

        }

        private void endStep()
        {

        }


        private void givePriority(bool main)
        {
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
                        c = castOrPass(main);
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
                            raiseAction(new PassAction());
                            return null;
                        }
                    }
                    else if (f is CardButton)
                    {
                        CardButton b = (CardButton)f;
                        Card c = b.getCard();
                        if (((main && stack.Count == 0) || c.isInstant()) && c.isCastable() && c.getCost().tryPay(hero))
                        {
                            MainFrame.clear();
                            raiseAction(new CastAction(c));
                            return c;
                        }
                    }
                }
            }
        }


        private Card demandCastOrPass()
        {
            var v = connection.demandCastOrPass();
            if (v is PassAction) { return null; }
            if (v is CastAction) { return ((CastAction)v).getCard(); }
            throw new Exception("really shouldn't happen");
        }

        private int demandSelection()
        {
            return connection.demandSelection();
        }

        private CardId[] demandDeck()
        {
            return connection.demandDeck();
        }


        private void chooseAttackers()
        {
            List<Card> cs = null;

            MainFrame.setMessage("Choose attackers");
            while (cs == null) 
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

                            cs = new List<Card>();
                            foreach (Card c in hero.getField().getCards())
                            {
                                if (c.isAttacking()) { cs.Add(c); }
                            }

                            break;
                        }
                    }
                    else if (f is CardButton)
                    {

                        CardButton b = (CardButton)f;
                        Card c = b.getCard();
                        if (c.canAttack()) { c.toggleAttacking(); }
                    }
                }
            }

            raiseAction(new DeclareAttackersAction(cs));
            raiseEvent(new AttackingEvent(cs));
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
