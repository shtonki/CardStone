using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace stonekart
{
    public class Game
    {
        private Player hero, villain, homePlayer, awayPlayer, activePlayer, inactivePlayer;
        private Pile stack;

        private bool active;
        private int step;

        private GameConnection connection;
        private CardFactory cardFactory;

        private KappaMan kappa;

        private Stack<StackWrapperFuckHopeGasTheKikes> stackxd;

        private delegate bool xd(Card c);

        public Game(GameConnection cn)
        {
            connection = cn;
            connection.setGame(this);
            cardFactory = new CardFactory();

            setupEventHandlers();

            hero = new Player(Location.HEROSIDE);
            villain = new Player(Location.VILLAINSIDE);
            homePlayer = cn.asHomePlayer() ? hero : villain;
            awayPlayer = cn.asHomePlayer() ? villain : hero;

            stack = new Pile();
            stackxd = new Stack<StackWrapperFuckHopeGasTheKikes>();

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
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
                CardId.SolemnAberration,  
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

            kappa.addBaseHandler(new EventHandler(GameEventType.STEP, @g =>
            {
                
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.DRAW, @gevent =>
            {
                DrawEvent e = (DrawEvent)gevent;
                e.getPlayer().draw(e.getCards());
                e.getPlayer().notifyObserver();
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.CAST, @gevent =>
            {
                CastEvent e = (CastEvent)gevent;
                var v = e.getStuff();
                v.card.moveTo(stack);
                stackxd.Push(v);

                v.card.getOwner().notifyObserver();
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.MOVECARD, @gevent =>
            {
                MoveCardEvent e = (MoveCardEvent)gevent;
                e.getCard().moveTo(e.getLocation());
                e.getCard().getOwner().notifyObserver();
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
                var x = e.getStuff();

                if (!stackxd.Pop().Equals(x) || stack.peek() != x.card)
                {
                    throw new CannotUnloadAppDomainException("we don't need to deal with the immigration \"problem\" that's not politically correct xddd");
                }

                Ability a = x.ability;
                Card card = x.card;

                List<GameEvent> es = a.getEffect().resolve(card, x.targets);

                foreach (var v in es)
                {
                    kappa.handle(v);
                }

                if (card.isDummy())
                {
                    throw new NotImplementedException();
                }

                if (card.getType() == Type.Instant || card.getType() == Type.Sorcery)
                {
                    raiseEvent(new MoveCardEvent(card, Location.GRAVEYARD));
                }
                else
                {
                    raiseEvent(new MoveCardEvent(card, Location.FIELD));
                }

            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.DAMAGEPLAYER, @gevent =>
            {
                DamagePlayerEvent e = (DamagePlayerEvent)gevent;
                e.getPlayer().damage(e.getDamage());
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.DAMAGECREATURE, @gevent =>
            {
                DamageCreatureEvent e = (DamageCreatureEvent)gevent;
                e.getCreature().damage(e.getDamage());
            }));

            kappa.addBaseHandler(new EventHandler(GameEventType.BURYCREATURE, @gevent =>
            {
                BuryCreature e = (BuryCreature)gevent;
                raiseEvent(new MoveCardEvent(e.getCard(), Location.GRAVEYARD));
            }));
        }

        /// <summary>
        ///  Sets the piles in Location
        /// </summary>
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
            active = connection.asHomePlayer();
            MainFrame.setStep(0, active);

            while (true)
            {
                activePlayer =   active ? hero : villain;
                inactivePlayer = active ? villain : hero;

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
                bool b = chooseAttackersStep();
                advanceStep();

                //defenders
                if (b) { chooseDefendersStep(); }
                advanceStep();

                //combatDamage
                if (b) { combatDamageStep(); }
                advanceStep();

                //endCombat
                endCombatStep();
                advanceStep();

                //main2
                mainStep(2);
                advanceStep();

                //end
                endStep();
                active = !active;
                advanceStep();
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

        private bool chooseAttackersStep()
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

            if (attackers.Length == 0) { return false; }

            foreach (var a in attackers)
            {
                a.setAttacking(true);
                //todo attackers event
            }

            //raiseEvent(new StepEvent(StepEvent.ATTACKERS));
            givePriority(false);
            return true;
        }

        private void chooseDefendersStep()
        {
            //raiseEvent(new StepEvent(StepEvent.DEFENDERS));
            givePriority(false);
        }

        private void combatDamageStep()
        {
            foreach (var v in activePlayer.getField().getCards())
            {
                if (v.isAttacking())
                {
                    raiseEvent(new DamagePlayerEvent(inactivePlayer, v, v.getCurrentPower()));
                }
            }

            //raiseEvent(new StepEvent(StepEvent.DAMAGE));
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
                checkGameState();
                StackWrapperFuckHopeGasTheKikes a;
                if (active)
                {
                    a = castOrPass(main && stack.Count == 0);
                }
                else
                {
                    a = demandCastOrPass();
                }

                if (a == null)  //active player passed
                {
                    if (active)
                    {
                        a = demandCastOrPass();
                    }
                    else
                    {
                        a = castOrPass(false);
                    }
                }
                if (a != null)
                {
                    raiseEvent(new CastEvent(a));
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
            step = (step + 1)%10;
            MainFrame.setStep(step, active);
        }

        private void checkGameState()
        {
            List<GameEvent> xd = new List<GameEvent>();

            foreach (var v in hero.getField().getCards())
            {
                if (v.getCurrentToughness() <= 0)
                {
                    xd.Add(new BuryCreature(v));
                }
            }

            foreach (var v in villain.getField().getCards())
            {
                if (v.getCurrentToughness() <= 0)
                {
                    xd.Add(new BuryCreature(v));
                }
            }

            foreach (var v in xd)
            {
                raiseEvent(v);
            }
        }

        /// <summary>
        /// Makes the user either pick a card or pass priority, then calls raiseAction on the resulting action which is either a PassAction or a CastAction
        /// </summary>
        /// <param name="main"></param>
        /// <returns>A Card if a card was selected, null otherwise</returns>
        private StackWrapperFuckHopeGasTheKikes castOrPass(bool main)
        {
            if (checkAutoPass()) { return null; }
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
                        var abilities = c.getAvailableActivatedAbilities(main);

                        ActivatedAbility a;
                        if (abilities.Count == 0)
                        {
                            a = null;
                        }
                        else if (abilities.Count == 1)
                        {
                            a = abilities[0];
                        }
                        else
                        {
                            throw new Exception("we don't support these things yet");
                        }

                        

                        if (a != null)
                        {
                            var v = a.getCost().check(c);
                            if (v != null)
                            {
                                var targets = getTargets(a);    //todo allow canceling xd
                                a.getCost().pay(c, v);
                                MainFrame.clear();
                                var sw = new StackWrapperFuckHopeGasTheKikes(c, a, targets);
                                CastAction ca = new CastAction(sw, v);
                                //CastAction cb = GameAction.fromString("cast," + ca.toString(), this) as CastAction;
                                raiseAction(new CastAction(sw, v)); //todo (seba) not even an anti pattern just horrible
                                return sw;
                            }
                        }
                    }
                }
            }
        }

        private bool cmp(CastAction a, CastAction b)
        {
            if (a.isPass() != b.isPass()) { return false; }

            StackWrapperFuckHopeGasTheKikes sa = a.getStackWrapper(), sb = b.getStackWrapper();

            if (sa.ability != sb.ability ||
                sa.card != sb.card ||
                sa.targets.Length != sb.targets.Length) { return false; }

            for (int i = 0; i < sa.targets.Length; i++)
            {
                Target ta = sa.targets[i], tb = sb.targets[i];
                if (ta.getPlayer() != tb.getPlayer() || ta.getCard() != tb.getCard()) { return false; }
            }

            return true;
        }

        private bool checkAutoPass()
        {
            return false;
        }

        private Target[] getTargets(Ability a)
        {
            MainFrame.setMessage("Select target(s)");

            Target[] targets = new Target[a.countTargets()];
            TargetRule[] rules = a.getTargetRules();

            int i = 0;
            while (i < targets.Length)
            {
                Target t = null;
                Foo f = getFoo();
                if (f is PlayerButton)
                {
                    t = new Target(((PlayerButton)f).getPlayer());
                }
                else if (f is CardButton)
                {
                    t = new Target(((CardButton)f).getCard());
                }

                //add option to cancel this shit

                if (t != null && rules[i].check(t))
                {
                    targets[i++] = t;
                }
            }
            return targets;
        }

        private StackWrapperFuckHopeGasTheKikes demandCastOrPass()
        {
            var v = connection.demandAction(typeof(CastAction)) as CastAction;
            if (v.isPass()) { return null; }
            if (v.getStackWrapper().ability is ActivatedAbility)
            {
                ActivatedAbility aa = v.getStackWrapper().ability as ActivatedAbility;
                aa.getCost().pay(v.getStackWrapper().card, v.getCosts());
            }
            else
            {
                throw new NotImplementedException();
            }
            return v.getStackWrapper();
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
            raiseEvent(new ResolveEvent(stackxd.Peek()));
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

        /*todo 0 - home 1 - away; meaning one has to flip it when getting a 
         * message from the other player which is anything but practical */
        public Player getPlayerById(int i)
        {
            return i == 0 ? homePlayer : awayPlayer;
        }

        //todo seba this really shouldn't be here
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
                xds = new EventHandler[100]; //1todo nope
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

    //the fact that making this a struct creates 20 lines of code shows what a fucking joke this language is
    public class StackWrapperFuckHopeGasTheKikes
    {
        public Card card;
        public Ability ability;
        public Target[] targets;

        public StackWrapperFuckHopeGasTheKikes(Card c, Ability a, Target[] cs)
        {
            card = c;
            ability = a;
            targets = cs;
        }
    }
}
