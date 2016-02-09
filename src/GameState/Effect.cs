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

        public void aquireTargets(GameInterface ginterface, GameState gstate)
        {
            foreach (SubEffect e in subEffects)
            {
                e.resolveCastTargets(ginterface, gstate);
            }
        }

        public List<GameEvent> resolve(Card c, Target[] ts, GameInterface ginterface, GameState gameState)
        {
            List<GameEvent> r = new List<GameEvent>();
            if (!preResolveCheck()) { return r; }
            Card baseCard = c.isDummy ? c.dummyFor.card : c;
            Target[] pts = null;
            for (int i = 0; i < subEffects.Length; i++)
            {
                subEffects[i].resolveResolveTargets(ginterface, baseCard, pts);
                pts = subEffects[i].targets;
            }
            foreach (SubEffect e in subEffects)
            {
                foreach (GameEvent ge in e.resolveEffect(ginterface, gameState, baseCard))
                {
                    r.Add(ge);
                }
            }
            
            return r;
        }
    }

    public abstract class SubEffect
    {
        private TargetRule targetRule;

        protected SubEffect(TargetRule t)
        {
            targetRule = t;
        }

        public GameEvent[] resolveEffect(GameInterface ginterface, GameState game, Card baseCard)
        {
            List<GameEvent> r = new List<GameEvent>();

            if (!targetRule.check(targetRule.getTargets()))
            {
                return r.ToArray();
            }
            foreach (Target t in targetRule.getTargets())
            {
                foreach (GameEvent e in resolve(ginterface, t, baseCard))
                {
                    r.Add(e);
                }
            }
            return r.ToArray();
        }
        public bool resolveCastTargets(GameInterface ginterface, GameState gstate)
        {
            return targetRule.resolveCastTargets(ginterface, gstate);
        }
        public void resolveResolveTargets(GameInterface gi, Card resolving, Target[] last)
        {
            targetRule.resolveResolveTargets(gi, resolving, last);
        }
        public Target[] targets => targetRule.getTargets();


        abstract protected GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard);
    }
    

    public class ShuffleOption : SubEffect
    {
        public ShuffleOption(TargetRule t) : base(t)
        {

        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
        {
            Choice shuffle;
            Player player = t.player;
            if (player.isHero)
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
                return new GameEvent[] {new ShuffleDeckEvent(player),};
            }
            return new GameEvent[]{};
        }
    }

    public class Draw : SubEffect
    {
        private int cardCount;

        public Draw(TargetRule t, int cards) : base(t)
        {
            cardCount = cards;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
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

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
        {

            GameEvent g;

            Card source = baseCard;

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

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
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

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
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

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
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

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
        {
            Card card = t.card;
            ModifyCardEvent r = new ModifyCardEvent(card, attribute, filter, value);
            return new[] {r};
        }
    }

    /*
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
    */

    public class Mill : SubEffect
    {
        private int cards;

        public Mill(TargetRule t, int cards) : base(t)
        {
            this.cards = cards;
        }

        protected override GameEvent[] resolve(GameInterface ginterface, Target t, Card baseCard)
        {
            Player p = t.player;
            GameEvent[] r = p.hand.cards.Take(cards).Select((c) => new MoveCardEvent(c, LocationPile.GRAVEYARD)).ToArray();
            return r;
        }
    }
}
