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
        private Func<bool> preResolveCheck;
        

        public Effect(params SubEffect[] subEffects)
        {
            preResolveCheck = () => true;
            this.subEffects = subEffects;
        }

        public Effect(SubEffect[] subEffects, Func<bool> preResolveCheck) : this(subEffects)
        {
            this.subEffects = subEffects;
            this.preResolveCheck = preResolveCheck;
        }

        public Target[] aquireTargets(GameInterface ginterface, GameState gstate, bool cancellable)
        {
            List<Target> l = new List<Target>();
            foreach (SubEffect e in subEffects)
            {
                Target[] ts = e.resolveCastTargets(ginterface, gstate, cancellable);
                if (ts == null) return null;
                l.AddRange(ts);
            }
            return l.ToArray();
        }

        public void setTargets(Target[] ts)
        {
            if (subEffects.Length == 0) return;
            subEffects[0].forceCastTargets(ts);
        }

        public List<GameEvent> resolve(Card c, Target[] ts, GameInterface ginterface, GameState gameState)
        {
            List<GameEvent> r = new List<GameEvent>();
            if (!preResolveCheck()) { return r; }
            Target[] pts = null;
            for (int i = 0; i < subEffects.Length; i++)
            {
                subEffects[i].resolveResolveTargets(ginterface, gameState, c, pts);
                pts = subEffects[i].targets;
            }
            foreach (SubEffect e in subEffects)
            {
                foreach (GameEvent ge in e.resolveEffect(ginterface, gameState, c))
                {
                    r.Add(ge);
                }
            }
            
            return r;
        }
    }

    public abstract class SubEffect
    {
        public Target[] targets => targetRule.getTargets();
        private TargetRule targetRule;

        protected SubEffect(TargetRule t)
        {
            targetRule = t;
        }

        public GameEvent[] resolveEffect(GameInterface ginterface, GameState game, Card resolvingCard)
        {
            List<GameEvent> r = new List<GameEvent>();

            if (!targetRule.check(targetRule.getTargets()))
            {
                return r.ToArray();
            }
            foreach (Target t in targetRule.getTargets())
            {
                foreach (GameEvent e in resolve(ginterface, t, resolvingCard))
                {
                    r.Add(e);
                }
            }
            return r.ToArray();
        }
        public Target[] resolveCastTargets(GameInterface ginterface, GameState gstate, bool cancellable)
        {
            return targetRule.resolveCastTargets(ginterface, gstate, cancellable);
        }
        public void resolveResolveTargets(GameInterface gi, GameState gstate, Card resolving, Target[] last)
        {
            targetRule.resolveResolveTargets(gi, gstate, resolving, last);
        }

        public void forceCastTargets(Target[] ts)
        {
            (targetRule as Forcable)?.forceTargets(ts);
        }

        abstract protected GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard);
    }

    public class Exhaust : SubEffect
    {
        public Exhaust(TargetRule t) : base(t)
        {
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Card c = t.card;
            return new[] {new ExhaustEvent(c)};
        }
    }

    public class Timelapse : SubEffect
    {
        private int cardCount;
        public Timelapse(int cardCount) : base(new ResolveTargetRule(ResolveTarget.CONTROLLER))
        {
            this.cardCount = cardCount;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Choice shuffle;
            Player player = t.player;
            if (player.isHero)
            {
                var p = ginterface.showCards(player.deck.cards.Reverse().Take(cardCount).ToArray());
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

    public class Shuffle : SubEffect
    {
        private bool optional;

        public Shuffle(TargetRule t, bool optional) : base(t)
        {
            this.optional = optional;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Choice shuffle = Choice.No;
            Player player = t.player;
            if (player.isHero && optional)
            {
                shuffle = ginterface.getChoice("Shuffle deck?", Choice.Yes, Choice.No);
                ginterface.sendSelection((int)shuffle);
            }
            else
            {
                shuffle = (Choice)ginterface.demandSelection();
            }

            if (shuffle == Choice.Yes)
            {
                return new GameEvent[] { new ShuffleDeckEvent(player), };
            }
            return new GameEvent[] { };
        }
    }

    public class Draw : SubEffect
    {
        private int cardCount;

        public Draw(TargetRule t, int cards) : base(t)
        {
            cardCount = cards;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Player p = t.player;
            GameEvent e = new DrawEvent(p, cardCount);
            return new[] {e};
        }
        
    }

    public class Ping : SubEffect
    {
        private int damage;

        public Ping(TargetRule t, int damage) : base(t)
        {
            this.damage = damage;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {

            GameEvent g;

            Card source = resolvingCard;

            if (t.isPlayer)
            {
                g = new DamagePlayerEvent(t.player, source, damage);
            }
            else
            {
                g = new DamageCreatureEvent(t.card, source, damage);
            }
            

            return new []{g};
        }
        
    }

    public class MoveTo : SubEffect
    {
        private LocationPile pile;
        

        public MoveTo(TargetRule t, LocationPile pile) : base(t)
        {
            this.pile = pile;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Card card = t.card;
            var r  = new MoveCardEvent(card, pile);
            return new[] {r};
        }
    }

    public class GainLife : SubEffect
    {
        private int life;

        public GainLife(TargetRule t, int n) : base(t)
        {
            life = n;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Player p = t.player;
            return new[]{new GainLifeEvent(p, life)};
        }
        
    }

    public class SummonTokens : SubEffect
    {
        private CardId[] cards;

        public SummonTokens(TargetRule t, params CardId[] cards) : base(t)
        {
            this.cards = cards;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            GameEvent[] r = new GameEvent[cards.Length];
            Player p = t.player;
            for (int i = 0; i < cards.Length; i++)
            {
                r[i] = new SummonTokenEvent(p, cards[i]);
            }
            return r;
        }
    }

    public class ModifyUntil : SubEffect
    {
        public readonly Modifiable attribute;
        public readonly Clojurex filter;
        public readonly int value;

        public ModifyUntil(TargetRule t, Modifiable attribute, Clojurex filter, int value) : base(t)
        {
            this.attribute = attribute;
            this.filter = filter;
            this.value = value;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Card card = t.card;
            ModifyCardEvent r = new ModifyCardEvent(card, attribute, filter, value);
            return new[] {r};
        }
    }

    public class CounterSpell : SubEffect
    {
        public CounterSpell(TargetRule t) : base(t)
        {
        }
        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            return new[] {new CounterSpellEvent(t.card) };
        }
    }

    public class Mill : SubEffect
    {
        private int cards;

        public Mill(TargetRule t, int cards) : base(t)
        {
            this.cards = cards;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Player p = t.player;
            GameEvent[] r = p.deck.cards.Take(cards).Select((c) => new MoveCardEvent(c, LocationPile.GRAVEYARD)).ToArray();;
            return r;
        }
    }

    public class Pyro : SubEffect
    {
        private int damage;
        private Func<Card, bool> cardFilter;

        public Pyro(TargetRule t, int damage, Func<Card, bool> cardFilter) : base(t)
        {
            this.damage = damage;
            this.cardFilter = cardFilter;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card resolvingCard)
        {
            Player p = t.player;
            List<GameEvent> r = new List<GameEvent>(p.field.count);

            foreach (Card c in p.field.cards)
            {
                if (c.hasPT() && cardFilter(c))
                {
                    r.Add(new DamageCreatureEvent(c, resolvingCard, damage));
                }
            }
            return r.ToArray();
        }
    }
}
