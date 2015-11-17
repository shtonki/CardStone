using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;


namespace stonekart
{
    class Game
    {
        private Player hero, villain;
        private Pile stack;

        public Game(Connection cn)
        {
            Thread.Sleep(100);

            setupEventHandlers();

            hero = new Player();
            villain = new Player();

            stack = new Pile();

            setLocations();

            MainFrame.setObservers(hero, villain, stack);
        }

        public void start()
        {

            hero.loadDeck(new[] { CardId.BearCavalary, CardId.BearCavalary, CardId.BearCavalary, CardId.Kappa, CardId.Kappa, }, new Location(Location.DECK, Location.HEROSIDE));
            villain.loadDeck(new CardId[] {}, new Location(Location.DECK, Location.VILLAINSIDE));


            hero.shuffleDeck();
            villain.shuffleDeck();


            loop();
        }

        private void setupEventHandlers()
        {
            drawEventHandler = new EventHander(GameEvent.DRAW, delegate(GameEvent gevent)
            {
                hero.draw();
            });

            castEventHandler = new EventHander(GameEvent.CAST, delegate(GameEvent gevent)
            {
                CastEvent e = (CastEvent)gevent;
                e.getCard().moveTo(new Location(Location.STACK, Location.HEROSIDE));
            });
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
            while (true)
            {
                //refill step
                hero.resetMana();
                unTop(hero);
                addMana();
                givePriority(false);
                advanceStep();

                //draw step
                draw();
                givePriority(false);
                advanceStep();

                //main phase 1
                givePriority(true);
                advanceStep();

                //startCombat
                givePriority(false);
                advanceStep();

                //attackers
                choseAttackers();
                givePriority(false);
                advanceStep();

                //blockers
                choseBlockers();
                givePriority(false);
                advanceStep();

                //combatDamage
                combatDamage();
                givePriority(false);
                advanceStep();

                //endCombat
                givePriority(false);
                advanceStep();

                //main2
                givePriority(true);
                advanceStep();

                //end
                givePriority(false);
                advanceStep();
            }
        }

        private void givePriority(bool main)
        {
            while (true)
            {
                if (castOrPass(main))
                {
                    
                }
                else
                {
                    if (stack.Count > 0)
                    {
                        resolve(stack.peek());                       
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

        private bool castOrPass(bool main)
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
                            return false;
                        }
                    }
                    else if (f is CardButton)
                    {
                        CardButton b = (CardButton)f;
                        Card c = b.getCard();
                        if (((main && stack.Count == 0) || c.isInstant()) && c.isCastable() && c.getCost().tryPay(hero))
                        {
                            MainFrame.clear();
                            cast(c);
                            return true;
                        }
                    }
                }
            }
        }

        private void choseAttackers()
        {
            MainFrame.setMessage("Choose attackers");
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
                            return;
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
        }

        private void choseBlockers()
        {
            
        }

        private void combatDamage()
        {
            
        }

        private void unTop(Player p)
        {
            foreach (Card c in p.getField().getCards())
            {
                c.unTop();
            }
        }

        private void draw()
        {
            handleEvent(new DrawEvent(true));
        }

        private void cast(Card c)
        {
            handleEvent(new CastEvent(c));
        }

        private void resolve(Card c)
        {
            c.moveToOwners(Location.FIELD);
        }

        private void addMana()
        {
            MainFrame.showAddMana();
            int c;
            do
            {
                c = getManaColor();
            } while (hero.getMaxMana(c) == 6);
            hero.addMana(c);
        }

        public Player getHero()
        {
            return hero;
        }

        public void handleEvent(GameEvent e)
        {
            drawEventHandler.invoke(e);
            castEventHandler.invoke(e);
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

        private static EventHander 
            drawEventHandler, 
            castEventHandler;

        private class EventHander
        {
            public delegate void eventHandler(GameEvent e);
            private int types;
            private eventHandler main, pre, post;

            public EventHander(int types, eventHandler e)
            {
                this.types = types;
                main = e;
            }

            public void invoke(GameEvent e)
            {
                if ((types & e.getType()) != 0)
                {
                    main(e);
                }
            }
        }
    }

}
