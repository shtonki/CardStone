using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stonekart
{
    public class Effect
    {
        private SubEffect[] subEffects;
        private TargetLambda[] targetRules;
        

        public Effect(params SubEffect[] subEffects)
        {
            this.subEffects = subEffects;

            var trs = new List<TargetLambda>();
            foreach (SubEffect subEffect in subEffects)
            {
                foreach (TargetLambda l in subEffect.targets)
                {
                    trs.Add(l);
                }
            }
            targetRules = trs.ToArray();
        }

        public List<GameEvent> resolve(Card c, Target[] ts, GameInterface ginterface, GameState gameState)
        {
            List<GameEvent> r = new List<GameEvent>();
            Target[] targets = new Target[targetRules.Length];
            int ctr = 0;
            for (int i = 0; i < targetRules.Length; i++)
            {
                Target t;
                if (targetRules[i] == TargetLambda.INTERNALCONTROLLER)
                {
                    t = new Target(c.controller);
                }
                else if (targetRules[i] == TargetLambda.INTERNALSELF)
                {
                    t = new Target(c);
                }
                else
                {
                    t = ts[ctr++];
                }
                targets[i] = t;
            }
            IEnumerator<Target> x = ((IEnumerable<Target>)targets).GetEnumerator();
            x.MoveNext();
            foreach (SubEffect e in subEffects)
            {
                foreach (var v in e.resolve(x, ginterface, gameState))
                {
                    r.Add(v);
                }
            }
            
            return r;
        }

        public TargetRule[] getTargetRules()
        {
            return targetRules.Where((l) => l != TargetLambda.INTERNALCONTROLLER && l != TargetLambda.INTERNALSELF)
                .Select((l) => new TargetRule(l))
                .ToArray();
        }
        
        
    }

    public abstract class SubEffect
    {
        public int targetCount => targets.Length;
        public TargetLambda[] targets { get; protected set; }

        private IEnumerator<Target> tgts;
        private int targetsEaten;

        protected SubEffect()
        {

        }

        public GameEvent[] resolve(IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {
            tgts = ts;
            targetsEaten = 0;
            GameEvent[] r = resolve(ginterface, game);
            if (targetsEaten != targets.Length) { throw new Exception(); }
            return r;
        }
        abstract protected GameEvent[] resolve(GameInterface ginterface, GameState game);
        protected void setTargets(params TargetLambda[] ts)
        {
            targets = ts;
        }

        protected Target nextTarget()
        {
            var r = tgts.Current;
            tgts.MoveNext();
            targetsEaten++;
            return r;
        }
        protected Player nextPlayer()
        {
            Target t = nextTarget();
            if (t.isPlayer)
            {
                return t.player;
            }
            throw new Exception();
        }
        protected Card nextCard()
        {
            Target t = nextTarget();
            if (t.isCard)
            {
                return t.card;
            }
            throw new Exception();
        }
        protected void setSelfPlayer(bool targeted)
        {
            setTargets(targeted ? TargetLambda.PLAYER : TargetLambda.INTERNALCONTROLLER);
        }
    }
    

    public class Timelapse : SubEffect
    {
        private int n;

        public Timelapse(int n)
        {
            this.n = n;
            setTargets(TargetLambda.INTERNALCONTROLLER);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Choice shuffle;
            Player player = nextPlayer();
            if (player.isHero)
            {
                CardPanelControl p = ginterface.showCards(player.deck.cards.Take(n).ToArray());
                shuffle = ginterface.getChoice("Shuffle deck?", Choice.Yes, Choice.No);
                ginterface.sendSelection((int)shuffle);
                p.closeWindow();
            }
            else
            {
                shuffle = (Choice)ginterface.demandSelection();
            }
            
            if (shuffle == Choice.Yes)
            {
                return new GameEvent[] {new ShuffleDeckEvent(player),};
            }
            return new GameEvent[]{};
        }
    }

    public class Draw : SubEffect
    {
        private int i;

        public Draw(bool targeted, int cards)
        {
            i = cards;
            setSelfPlayer(targeted);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Player p = nextPlayer();
            GameEvent e = new DrawEvent(p, i);
            return new[] {e};
        }
        
    }

    public class Ping : SubEffect
    {
        private int d;

        public Ping(int damage)
        {
            d = damage;
            setTargets(TargetLambda.INTERNALSELF, TargetLambda.ZAPPABLE);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {

            GameEvent g;

            Card source = nextCard();
            Target t = nextTarget();

            if (t.isPlayer)
            {
                g = new DamagePlayerEvent(t.player, source, d);
            }
            else
            {
                g = new DamageCreatureEvent(t.card, source, d);
            }
            

            return new []{g};
        }
        
    }

    public class MoveTo : SubEffect
    {
        private LocationPile pile;

        public MoveTo(LocationPile pile)
        {
            this.pile = pile;
            setTargets(TargetLambda.INTERNALSELF);
        }

        public MoveTo(LocationPile pile, TargetLambda l) : this(pile)
        {
            setTargets(l);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Card card = nextCard();
            var r  = new MoveCardEvent(card, pile);
            return new[] {r};
        }
    }

    public class GainLife : SubEffect
    {
        private int life;

        public GainLife(bool targetable, int n)
        {
            life = n;
            setSelfPlayer(targetable);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Player p = nextPlayer();
            return new[]{new GainLifeEvent(p, life)};
        }
        
    }

    public class SummonTokens : SubEffect
    {
        public int count { get; private set; }
        public CardId card { get; private set; }

        public SummonTokens(int n, CardId c)
        {
            count = n;
            card = c;
            setTargets(TargetLambda.INTERNALCONTROLLER);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            GameEvent[] r = new GameEvent[count];
            Player p = nextPlayer();
            for (int i = 0; i < count; i++)
            {
                r[i] = new SummonTokenEvent(p, card);
            }

            return r;
        }
    }

    public class ModifyUntil : SubEffect
    {
        public readonly Modifiable attribute;
        public readonly Clojurex filter;
        public readonly int value;

        public ModifyUntil(Modifiable attribute, Clojurex filter, int value)
        {
            this.attribute = attribute;
            this.filter = filter;
            this.value = value;
            setTargets(TargetLambda.INTERNALSELF);
        }

        public ModifyUntil(TargetLambda t, Modifiable attribute, Clojurex filter, int value)
        {
            this.attribute = attribute;
            this.filter = filter;
            this.value = value;
            setTargets(t);
        }

        

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Card card = nextCard();
            ModifyCardEvent r = new ModifyCardEvent(card, attribute, filter, value);
            return new[] {r};
        }
    }

    public class Duress : SubEffect
    {
        private Func<Card, bool> cardFilter;

        public Duress(Func<Card, bool> cardFilter)
        {
            this.cardFilter = cardFilter;
            setTargets(TargetLambda.INTERNALCONTROLLER, TargetLambda.PLAYER);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Player caster = nextPlayer();
            Player victim = nextPlayer();
            Card discard;
            if (caster.isHero)
            {
                CardPanelControl p = ginterface.showCards(victim.hand.cards.ToArray());
                ginterface.setContext("Pick a card");
                discard = p.waitForCard();
                ginterface.clearContext();
                p.closeWindow();
                ginterface.sendCard(discard);
            }
            else
            {
                discard = ginterface.demandCard(game);
            }
            var r = new MoveCardEvent(discard, LocationPile.GRAVEYARD);
            return new[] {r};

        }
    }

    
}
