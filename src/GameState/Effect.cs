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
        public TargetRule[] targetRules { get; private set; }
        private Func<bool> preResolveCheck;
        

        public Effect(params SubEffect[] subEffects)
        {
            preResolveCheck = () => true;
            this.subEffects = subEffects;

            var trs = new List<TargetRule>();
            foreach (SubEffect subEffect in subEffects)
            {
                foreach (TargetRule l in subEffect.targets)
                {
                    trs.Add(l);
                }
            }
            targetRules = trs.ToArray();
        }

        public Effect(SubEffect[] subEffects, Func<bool> preResolveCheck) : this(subEffects)
        {
            this.subEffects = subEffects;
            this.preResolveCheck = preResolveCheck;
        }

        public List<GameEvent> resolve(Card c, Target[] ts, GameInterface ginterface, GameState gameState)
        {
            List<GameEvent> r = new List<GameEvent>();
            if (!preResolveCheck()) { return r; }
            IEnumerator<Target> x = ((IEnumerable<Target>)ts).GetEnumerator();
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
        
        
        
    }

    public abstract class SubEffect
    {
        private TargetRule targetRule;
        private int targetsEaten;
        private Target[] targets;

        protected SubEffect(TargetRule t)
        {

        }

        public GameEvent[] resolveEffect(GameInterface ginterface, GameState game)
        {
            targetsEaten = 0;
            GameEvent[] r = resolve(ginterface, game);
            if (targetsEaten != targets.Length) { throw new Exception(); }
            return r;
        }

        public void resolveCastTargets(GameInterface ginterface, Card c)
        {
            
        }

        public void resolveResolveTimeTargets(GameInterface ginterface, Card c)
        {
            
        }


        abstract protected GameEvent[] resolve(GameInterface ginterface, GameState game);
        

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
            setTargets(targeted ? FilterLambda.PLAYER : FilterLambda.CONTROLLER);
        }
    }
    

    public class Timelapse : SubEffect
    {
        private int n;

        public Timelapse(int n)
        {
            this.n = n;
            setTargets(FilterLambda.CONTROLLER);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Choice shuffle;
            Player player = nextPlayer();
            if (player.isHero)
            {
                CardPanelControl p = ginterface.showCards(player.deck.cards.Reverse().Take(n).ToArray());
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

        public Draw(FilterLambda filter, int cards)
        {
            i = cards;
            setTargets(filter);
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

        public Ping(FilterLambda l, int damage)
        {
            d = damage;
            setTargets(FilterLambda.SELF, l);
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
        

        public MoveTo( l, LocationPile pile)
        {
            this.pile = pile;
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

        public GainLife(FilterLambda filter, int n)
        {
            life = n;
            setTargets(filter);
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

        public SummonTokens(FilterLambda l, int n, CardId c)
        {
            count = n;
            card = c;
            setTargets(l);
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

        public ModifyUntil(FilterLambda t, Modifiable attribute, Clojurex filter, int value)
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
            setTargets(FilterLambda.CONTROLLER, FilterLambda.PLAYER);
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

    public class Mill : SubEffect
    {
        private int cards;

        public Mill(FilterLambda filter, int cards)
        {
            this.cards = cards;
            setTargets(filter);
        }

        protected override GameEvent[] resolve(GameInterface ginterface, GameState game)
        {
            Player p = nextPlayer();
            GameEvent[] r = p.hand.cards.Take(cards).Select((c) => new MoveCardEvent(c, LocationPile.GRAVEYARD)).ToArray();
            return r;
        }
    }
}
