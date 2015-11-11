using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;


namespace stonekart
{
    class GameController
    {
        private static Player hero, villain;

        public static void start()
        {
            Thread.Sleep(100);

            setupEventHandlers();

            hero = new Player();
            villain = new Player();

            setLocations();

            hero.loadDeck(new[] { CardId.KAPPA, CardId.KAPPA, CardId.KEEPO, CardId.KEEPO, }, new Location(Location.DECK, Location.HEROSIDE));
            villain.loadDeck(new[] { CardId.KAPPA, CardId.KAPPA, CardId.KEEPO, CardId.KEEPO, }, new Location(Location.DECK, Location.VILLAINSIDE));

            MainFrame.setObservers(hero, villain);

            hero.shuffleDeck();
            villain.shuffleDeck();

            loop();
        }

        private static void setupEventHandlers()
        {
            drawEventHandler = new EventHander(GameEvent.DRAW, delegate(GameEvent gevent) { hero.draw(); });
        }

        private static void setLocations()
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
        }

        private static void loop()
        {
            while (true)
            {
                hero.resetMana();
                addMana();
                handleEvent(new DrawEvent(true));
                mainPhase();
            }
        }

        private static void mainPhase()
        {
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
                            return;
                        }
                    }
                    else if (f is CardButton)
                    {
                        CardButton b = (CardButton)f;
                        Card c = b.getCard();
                        if (c.getCost().tryPay())
                        {
                            cast(c);
                        }
                    }
                }
            }
        }

        private static void cast(Card c)
        {
            c.moveTo(hero.getGraveyard());
        }

        private static void addMana()
        {
            MainFrame.showAddMana();
            int c;
            do
            {
                c = getManaColor();
            } while (hero.getMaxMana(c) == 6);
            hero.addMana(c);
        }

        public static Player getHero()
        {
            return hero;
        }

        public static void handleEvent(GameEvent e)
        {
            drawEventHandler.invoke(e);
        }

        private static Foo f;
        private static AutoResetEvent e = new AutoResetEvent(false);

        private static Foo getFoo()
        {
            e.WaitOne();
            if (f == null) { throw new Exception("this should never happen kappa"); }
            Foo r = f;
            f = null;

            return r;
        }

        private static Card getCard()
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

        private static FooButton getButton(int i)
        {
            MainFrame.showButtons(i);
            while (true)
            {
                Foo f = getFoo();
                if (f is FooButton)
                {
                    MainFrame.showButtons(NOTHING);
                    return (FooButton)f;
                }
            }
        }

        private static int getManaColor()
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

        public static void handleCommand(string s)
        {
            
        }

        public static void fooPressed(Foo foo)
        {
            f = foo;
            e.Set();
        }

        private const int
            NOTHING = ButtonPanel.NOTHING,
            ACCEPT = ButtonPanel.ACCEPT,
            ACCEPTCANCEL = ButtonPanel.ACCEPT | ButtonPanel.CANCEL;

        private static EventHander drawEventHandler;

        private class EventHander
        {
            public delegate void eventHandler(GameEvent e);
            private int types;
            private eventHandler func;

            public EventHander(int types, eventHandler e)
            {
                this.types = types;
                func = e;
            }

            public void invoke(GameEvent e)
            {
                if ((types & e.getType()) != 0)
                {
                    func(e);
                }
            }
        }
    }

}
