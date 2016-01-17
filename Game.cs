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
//x
namespace stonekart
{
    public class Game
    {
        private GameInterface gameInterface;

        private Player hero, villain, homePlayer, awayPlayer, activePlayer, inactivePlayer;
        private Pile stack;

        private Card[] attackers, defenders;
        private List<CardButton> clearMe = new List<CardButton>(); //hack 

        private bool active;
        private int step;

        public bool autoPass { get; set; }

        private GameConnection connection;
        private CardFactory cardFactory;

        private EventHandler kappa;

        private Stack<StackWrapper> stackxd;
        
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
                CardId.LightningBolt,
                CardId.LightningBolt,
                CardId.LightningBolt,
                CardId.LightningBolt, 
                CardId.FrothingGoblin,
                CardId.FrothingGoblin,
                CardId.FrothingGoblin,
                CardId.FrothingGoblin,
            };
        }

        private void gameStart()
        {
            hero.draw(4);

            loop();
        }
        
        #region eventHandlers
        private void setupEventHandlers()
        {
            kappa = new EventHandler();

            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.TOPCARD, _topcard));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.STEP, _step));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.DRAW, _draw));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.CAST, _cast));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.MOVECARD, _moveCard));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.GAINMANAORB, _gainManaOrb));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.UNTOPPLAYER, _untopPlayer));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.RESOLVE, _resolve));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.DAMAGEPLAYER, _damagePlayer));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.DAMAGECREATURE, _damageCreature));
            kappa.addBaseHandler(new stonekart.EventHandler(GameEventType.BURYCREATURE, _buryCreature));
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
            e.getPlayer().draw(e.getCards());
            e.getPlayer().notifyObserver();
        }
        private void _cast(GameEvent gevent)
        {
            CastEvent e = (CastEvent)gevent;
            StackWrapper v = e.getStackWrapper();
            moveCardTo(v.card, stack); 
            stackxd.Push(v);
            v.card.stackWrapper = v;

            v.card.owner.notifyObserver();
        }
        private void _resolve(GameEvent gevent)
        {
            ResolveEvent e = (ResolveEvent)gevent;
            var x = e.getStackWrapper();

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

            card.stackWrapper = null;
        }
        private void _moveCard(GameEvent gevent)
        {
            MoveCardEvent e = (MoveCardEvent)gevent;
            moveCardTo(e.getCard(), e.getLocation());
            e.getCard().moveReset();
            e.getCard().owner.notifyObserver();
        }
        private void _gainManaOrb(GameEvent gevent)
        {
            GainManaOrbEvent e = (GainManaOrbEvent)gevent;
            e.getPlayer().addMana(e.getColor());
        }
        private void _untopPlayer(GameEvent gevent)
        {
            //todo(seba) make this raise UNTOPCARD events
            UntopPlayerEvent e = (UntopPlayerEvent)gevent;
            e.getPlayer().untop();
        }
        private void _damagePlayer(GameEvent gevent)
        {
            DamagePlayerEvent e = (DamagePlayerEvent)gevent;
            e.getPlayer().damage(e.getDamage());
        }
        private void _buryCreature(GameEvent gevent)
        {
            BuryCreature e = (BuryCreature)gevent;
            raiseEvent(new MoveCardEvent(e.getCard(), LocationPile.GRAVEYARD));
        }
        private void _damageCreature(GameEvent gevent)
        {
            DamageCreatureEvent e = (DamageCreatureEvent)gevent;
            e.getCreature().damage(e.getDamage());
        }
        #endregion

        private void loop()
        {
            active = connection.asHomePlayer();
            gameInterface.setStep(0, active);

            while (true)
            {
                activePlayer =   active ? hero : villain;
                inactivePlayer = active ? villain : hero;

                autoPass = false;

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
                attackers = chooseMultiple("Choose attackers", c =>
                {
                    if (c.owner == hero && c.canAttack() && !(c.attacking))
                    {
                        //clearMe.Add(cb);
                        c.attacking = true;
                        return true;
                    }
                    else
                    {
                        //clearMe.Remove(c);
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
                raiseEvent(new TopEvent(a));
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

        private void combatDamageStep()
        {
            foreach (var attacker in activePlayer.getField().getCards())
            {
                if (attacker.attacking)
                {
                    if (attacker.defended)
                    {
                        Card defender = attacker.defendedBy;
                        raiseEvent(new DamageCreatureEvent(defender, attacker, attacker.currentPower));
                        raiseEvent(new DamageCreatureEvent(attacker, defender, defender.currentPower));
                    }
                    else
                    {
                        raiseEvent(new DamagePlayerEvent(inactivePlayer, attacker, attacker.currentPower));
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
                    gameInterface.getCardButton(c).setBorder(null);
                    c.defendedBy = null;
                }

                foreach (Card c in defenders)
                {
                    gameInterface.getCardButton(c).setBorder(null);
                    c.defenderOf = null;
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
            //todo(seba) make it check toggleboxes and autopass
            //todo(seba) make it a PassAction instead of a fucking null pointer just waiting to raise an exception and rain on my parade
            while (true)
            {
                checkGameState();
                CastAction action;
                if (active)
                {
                    action = castOrPass(main && stack.Count == 0);
                }
                else
                {
                    action = demandCastOrPass();
                }

                if (action.isPass())  //active player passed
                {
                    if (active)
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
                    raiseEvent(new CastEvent(action.getStackWrapper()));
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
            gameInterface.setStep(step, active);
        }

        private void checkGameState()
        {
            List<GameEvent> xd = new List<GameEvent>();

            foreach (var v in hero.getField().getCards())
            {
                if (v.currentToughness <= 0)
                {
                    xd.Add(new BuryCreature(v));
                }
            }

            foreach (var v in villain.getField().getCards())
            {
                if (v.currentToughness <= 0)
                {
                    xd.Add(new BuryCreature(v));
                }
            }

            foreach (GameEvent v in xd)
            {
                raiseEvent(v);
            }
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
                gameInterface.setMessage("You have priority");
                gameInterface.setChoiceButtons(Choice.PASS);

                a = _castOrPass(main);

                gameInterface.clear();
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
                        if (choice == Choice.PASS)
                        {
                            gameInterface.clear();
                            return new CastAction();
                        }
                        else
                        {
                            Console.WriteLine(choice);
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


                        
                        var v = a.getCost().check(c);
                        if (v == null) { continue; }

                        Target[] targets = getTargets(a); 
                        if (targets == null) { continue; }

                        gameInterface.clear();
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
            gameInterface.push();
            gameInterface.setMessage("Select target(s)");
            gameInterface.setChoiceButtons(Choice.CANCEL);

            Target[] targets = new Target[a.countTargets()];
            TargetRule[] rules = a.getTargetRules();

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
                else if (chosenGameElement.choice != null && chosenGameElement.choice.Value == Choice.CANCEL)
                {
                    targets = null;
                    break;
                }

                if (t != null && rules[i].check(t))
                {
                    targets[i++] = t;
                }
            }
            gameInterface.pop();
            return targets;
        }

        private CastAction demandCastOrPass()
        {
            gameInterface.setMessage("Opponent has priority");
            var v = connection.demandAction(typeof(CastAction)) as CastAction;
            gameInterface.setMessage("");
            return v;
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


        private Card[] chooseMultiple(string message, Func<Card, bool> xd)
        {
            gameInterface.setMessage(message);

            List<Card> cards = new List<Card>();
            while (true) 
            {
                gameInterface.setChoiceButtons(Choice.ACCEPT);
                while (true)
                {
                    GameElement chosenGameElement = gameInterface.getNextGameElementPress();
                    if (chosenGameElement.choice != null && chosenGameElement.choice.Value == Choice.ACCEPT)
                    {
                        gameInterface.clear();

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

            gameInterface.setChoiceButtons(Choice.ACCEPT);
            while (true)
            {
                Card blocker, blocked;
                
                while (true)
                {
                    blocker = blocked = null;
                    gameInterface.setMessage("Choose defenders");
                    gameInterface.setChoiceButtons(Choice.ACCEPT);

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
                            if (chosenGameElement.choice.Value == Choice.ACCEPT)
                            {
                                goto end;   // *unzips fedora*
                            }
                        }
                    }

                    gameInterface.setMessage("Blocking what?");
                    gameInterface.setChoiceButtons(Choice.CANCEL);

                    while (blocked == null)
                    {
                        GameElement e = gameInterface.getNextGameElementPress();
                        if (e.card != null)
                        {
                            blocked = e.card;
                        }
                        else if (e.choice != null)
                        {
                            if (e.choice.Value == Choice.CANCEL)
                            {
                                break;
                            }
                        }
                    }

                    if (blocked != null)
                    {
                        blocker.defenderOf = blocked;
                        blocked.defendedBy = blocker;
                        blockers.Add(blocker);
                    }
                }
            }

            end:
            gameInterface.setChoiceButtons();
            Card[] bkds = blockers.Select(@c => c.defenderOf).ToArray();
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
