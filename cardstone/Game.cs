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
    public class Game
    {
        private Player hero, villain, homePlayer, awayPlayer, activePlayer, inactivePlayer;
        private Pile stack;

        private Card[] attackers, defenders;
        private List<CardButton> clearMe = new List<CardButton>(); //hack 

        private bool active;
        private int step;

        private GameConnection connection;
        private CardFactory cardFactory;

        private EventHandler kappa;

        private Stack<StackWrapperFuckHopeGasTheKikes> stackxd;
        
        public Game(GameConnection cn)
        {
            connection = cn;
            connection.setGame(this);
            cardFactory = new CardFactory();

            setupEventHandlers();

            hero = new Player(this, LocationPlayer.HERO);
            villain = new Player(this, LocationPlayer.VILLAIN);
            homePlayer = cn.asHomePlayer() ? hero : villain;
            awayPlayer = cn.asHomePlayer() ? villain : hero;

            stack = new Pile(new Location(LocationPile.STACK, LocationPlayer.NOONE));
            stackxd = new Stack<StackWrapperFuckHopeGasTheKikes>();
            

            GUI.setObservers(hero, villain, stack);
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

            hero.loadDeck(myDeck);
            villain.loadDeck(otherDeck);


            hero.shuffleDeck();
            villain.shuffleDeck();

            gameStart();
        }

        private CardId[] loadDeck()
        {
            return new[]
            {
                CardId.FrothingGoblin,  
                CardId.FrothingGoblin,  
                CardId.FrothingGoblin,  
                CardId.FrothingGoblin,  
                CardId.FrothingGoblin,  
                CardId.LightningBolt,
                CardId.LightningBolt,
                CardId.LightningBolt,
                CardId.LightningBolt,
                CardId.LightningBolt,
            };
        }

        private void gameStart()
        {
            hero.draw(4);

            loop();
        }

        private void setupEventHandlers()
        {
            kappa = new EventHandler();

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.STEP, @g =>
            {
                
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.DRAW, @gevent =>
            {
                DrawEvent e = (DrawEvent)gevent;
                e.getPlayer().draw(e.getCards());
                e.getPlayer().notifyObserver();
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.CAST, @gevent =>
            {
                CastEvent e = (CastEvent)gevent;
                var v = e.getStuff();
                moveCardTo(v.card, stack);  //v.card.moveTo(stack);
                stackxd.Push(v);

                v.card.owner.notifyObserver();
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.MOVECARD, @gevent =>
            {
                MoveCardEvent e = (MoveCardEvent)gevent;
                moveCardTo(e.getCard(), e.getLocation());//e.getCard().moveTo(e.getLocation());
                e.getCard().owner.notifyObserver();
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.GAINMANA, @gevent =>
            {
                GainManaOrbEvent e = (GainManaOrbEvent)gevent;
                e.getPlayer().addMana(e.getColor());
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.UNTOPPLAYER, @gevent =>
            {
                UntopPlayerEvent e = (UntopPlayerEvent)gevent;
                e.getPlayer().untop();
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.RESOLVE, @gevent =>
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
                    raiseEvent(new MoveCardEvent(card, LocationPile.GRAVEYARD));
                }
                else
                {
                    raiseEvent(new MoveCardEvent(card, LocationPile.FIELD));
                }

            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.DAMAGEPLAYER, @gevent =>
            {
                DamagePlayerEvent e = (DamagePlayerEvent)gevent;
                e.getPlayer().damage(e.getDamage());
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.DAMAGECREATURE, @gevent =>
            {
                DamageCreatureEvent e = (DamageCreatureEvent)gevent;
                e.getCreature().damage(e.getDamage());
            }));

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.BURYCREATURE, @gevent =>
            {
                BuryCreature e = (BuryCreature)gevent;
                raiseEvent(new MoveCardEvent(e.getCard(), LocationPile.GRAVEYARD));
            }));
        }
        


        private void loop()
        {
            active = connection.asHomePlayer();
            GUI.setStep(0, active);

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
                GUI.showAddMana(true);
                int c;
                do
                {
                    c = getManaColor();
                } while (hero.getMaxMana(c) == 6);
                GUI.showAddMana(false);
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
            if (active)
            {
                attackers = chooseMultiple("Choose attackers", cb =>
                {
                    Card c = cb.getCard();
                    if (c.owner == hero && c.canAttack() && !(c.attacking))
                    {
                        cb.setBorder(Color.Red);
                        clearMe.Add(cb);
                        c.attacking = true;
                        return true;
                    }
                    else
                    {
                        cb.setBorder(null);
                        clearMe.Remove(cb);
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
                    GUI.getCardButtonByCard(c).setBorder(Color.Red);
                }
            }

            if (attackers.Length == 0)
            {
                attackers = null;
                return false;
            }

            foreach (var a in attackers)
            {
                a.setTopped(true);  //todo(seba) use event and check for "vigilance"
                //todo attackers event
            }
            
            givePriority(false);
            return true;
        }

        private void chooseDefendersStep()
        {
            if (active)
            {
                defenders = demandMultiSelection().Select(@i => cardFactory.getCardById(i)).ToArray();
                Card[] v = demandMultiSelection().Select(@i => cardFactory.getCardById(i)).ToArray();

                if (defenders.Length != v.Length) { throw new NetworkInformationException();}

                for (int i = 0; i < defenders.Length; i++)
                {
                    defenders[i].defending = v[i];
                }
            }
            else
            {
                Tuple<Card[], Card[]> ls = chooseDefenders();
                defenders = ls.Item1;
                raiseAction(new MultiSelectAction(defenders));
                raiseAction(new MultiSelectAction(ls.Item2));
            }

            givePriority(false);
        }

        private void combatDamageStep()
        {
            foreach (var v in activePlayer.getField().getCards())
            {
                if (v.attacking)
                {
                    raiseEvent(new DamagePlayerEvent(inactivePlayer, v, v.getCurrentPower()));
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
                    GUI.getCardButtonByCard(c).setBorder(null);
                }

                foreach (Card c in defenders)
                {
                    GUI.getCardButtonByCard(c).setBorder(null);
                }
                attackers = defenders = null;
            }

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
            GUI.setStep(step, active);
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
            GUI.setMessage("You have priority");
            while (true)
            {
                GUI.showButtons(ACCEPT);
                while (true)
                {
                    GameElement f = getGameElement();
                    if (f is ChoiceButton)
                    {
                        var b = (ChoiceButton)f;
                        if (b.getType() == GUI.ACCEPT)
                        {
                            GUI.clear();
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
                                GUI.clear();
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

        private bool checkAutoPass()
        {
            return false;
        }

        private Target[] getTargets(Ability a)
        {
            GUI.setMessage("Select target(s)");

            Target[] targets = new Target[a.countTargets()];
            TargetRule[] rules = a.getTargetRules();

            int i = 0;
            while (i < targets.Length)
            {
                Target t = null;
                GameElement f = getGameElement();
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
            GUI.setMessage("Opponent has priority");
            var v = connection.demandAction(typeof(CastAction)) as CastAction;
            GUI.setMessage("");
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


        private Card[] chooseMultiple(string message, Func<CardButton, bool> xd)
        {
            GUI.setMessage(message);

            List<CardButton> bs = new List<CardButton>();
            while (true) 
            {
                GUI.showButtons(ACCEPT);
                while (true)
                {
                    GameElement f = getGameElement();
                    if (f is ChoiceButton)
                    {
                        var b = (ChoiceButton)f;
                        if (b.getType() == GUI.ACCEPT)
                        {
                            GUI.clear();

                            return bs.Select(bt => bt.getCard()).ToArray();
                        }
                    }
                    else if (f is CardButton)
                    {
                        var cb = f as CardButton;

                        Card crd = cb.getCard();

                        if (!xd(cb))
                        {
                            bs.Remove(cb);
                            continue;
                        }

                        if (bs.Contains(cb))
                        {
                            bs.Remove(cb);
                        }
                        else
                        {
                            bs.Add(cb);
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

            GUI.showButtons(GUI.ACCEPT);
            while (true)
            {
                CardButton blocker, blocked;
                
                while (true)
                {
                    blocker = blocked = null;
                    GUI.setMessage("Choose defenders");
                    GUI.showButtons(GUI.ACCEPT);

                    while (blocker == null)
                    {
                        GameElement e = getGameElement();
                        if (e is CardButton)
                        {
                            CardButton b = (CardButton)e;
                            Card c = b.getCard();

                            if (c.owner == hero && !c.canDefend()) { continue; }

                            if (c.defending == null)
                            {
                                blocker = b;
                                b.setBorder(Color.Blue);
                                clearMe.Add(b);
                            }
                            else
                            {
                                blockers.Remove(c);
                                c.defending = null;
                                b.setBorder(null);
                                clearMe.Remove(b);
                            }
                        }
                        else if (e is ChoiceButton)
                        {
                            ChoiceButton c = (ChoiceButton)e;
                            if (c.getType() == GUI.ACCEPT)
                            {
                                goto end;   // *unzips fedora*
                            }
                        }
                    }

                    GUI.setMessage("Blocking what?");
                    GUI.showButtons(GUI.CANCEL);

                    while (blocked == null)
                    {
                        GameElement e = getGameElement();
                        if (e is CardButton)
                        {
                            blocked = (CardButton)e;
                        }
                        else if (e is ChoiceButton)
                        {
                            ChoiceButton c = (ChoiceButton)e;
                            if (c.getType() == GUI.CANCEL)
                            {
                                break;
                            }
                        }
                    }

                    if (blocked != null)
                    {
                        Card bkr = blocker.getCard();

                        bkr.defending = bkr;
                        blockers.Add(bkr);
                    }
                }
            }

            end:
            Card[] bkds = blockers.Select(@c => c.defending).ToArray();
            return new Tuple<Card[], Card[]>(blockers.ToArray(), bkds);
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

        public void moveCardTo(Card c, Pile to)
        {
            Pile from = pileFromLocation(c.location);
            from.remove(c);
            to.add(c);
            c.location = to.location;
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

        //todo seba this really shouldn't be here
        private GameElement f;
        private AutoResetEvent e = new AutoResetEvent(false);

        private GameElement getGameElement()
        {
            e.WaitOne();
            if (f == null) { throw new Exception("this should never happen kappa"); }
            GameElement r = f;
            f = null;

            return r;
        }

        private Card getCard()
        {
            while (true)
            {
                GameElement f = getGameElement();
                if (f is CardButton)
                {
                    return ((CardButton)f).getCard();
                }
            }
        }

        private ChoiceButton getButton(int i)
        {
            GUI.showButtons(i);
            while (true)
            {
                GameElement f = getGameElement();
                if (f is ChoiceButton)
                {
                    GUI.showButtons(NONE);
                    return (ChoiceButton)f;
                }
            }
        }

        private int getManaColor()
        {
            while (true)
            {
                GameElement f = getGameElement();
                if (f is PlayerPanel.ManaButton)
                {
                    return ((PlayerPanel.ManaButton)f).getColor();
                }
            }
        }

        public void fooPressed(GameElement gameElement)
        {
            f = gameElement;
            e.Set();
        }

        private const int
            NONE = GUI.NOTHING,
            ACCEPT = GUI.ACCEPT,
            ACCEPTCANCEL = GUI.ACCEPT | GUI.CANCEL;


        private class EventHandler
        {
            private stonekart.EventHandler[] xds;

            public EventHandler()
            {
                xds = new stonekart.EventHandler[100]; //1todo nope
            }

            public void addBaseHandler(stonekart.EventHandler e)
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
