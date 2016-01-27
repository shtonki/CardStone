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
        private TargetRule[] targetRules;

        private string Explanation;

        public Effect(params SubEffect[] es)
        {
            subEffects = es;

            targetRules = new TargetRule[targetCount];

            int i = 0;
            StringBuilder b = new StringBuilder();

            foreach (var v in es)
            {
                foreach (var vv in v.targetRules)
                {
                    targetRules[i++] = vv;
                }

                b.Append(v.explanation);
            }

            Explanation = b.ToString();
        }

        public List<GameEvent> resolve(Card c, Target[] ts)
        {
            List<GameEvent> r = new List<GameEvent>();
            IEnumerator<Target> targetEnumerator = ((IEnumerable<Target>)ts).GetEnumerator();
            targetEnumerator.MoveNext();

            foreach (SubEffect e in subEffects)
            {
                foreach (var v in e.resolve(c, targetEnumerator))
                {
                    r.Add(v);
                }
            }
            
            return r;
        }

        public TargetRule[] getTargetRules()
        {
            return targetRules;
        }

        public int targetCount => subEffects.Sum(v => v.targetCount);
        

        public string explanation => Explanation;
    }

    public abstract class SubEffect
    {
        public TargetRule[] targetRules => targets;
        public int targetCount => targets.Length;
        protected TargetRule[] targets;
        public string explanation { get; protected set; }

        protected SubEffect()
        {
            targets = new TargetRule[0];
        }

        abstract public GameEvent[] resolve(Card c, IEnumerator<Target> ts);
    }

    public class OwnerDraws : SubEffect
    {
        private int i;

        public OwnerDraws(int cards)
        {
            i = cards;
            explanation = "draw " + i + " card(s)";
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> _)
        {
            GameEvent e = new DrawEvent(c.controller, i);
            return new[] {e};
        }
        
    }

    public class PingN : SubEffect
    {
        private int d;

        public PingN(int targets, int damage)
        {
            this.targets = new TargetRule[targets];

            for (int i = 0; i < targets; i++)
            {
                this.targets[i] = new TargetRule(TargetRules.ZAPPABLE); 
            }
            d = damage;

            if (targets == 1)
            {
                explanation = "deal " + d + " damage to target";
            }
            else
            {
                explanation = "deal " + d * targets + " damage split between up to " + targets + " targets";
            }
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts)
        {

            GameEvent[] r = new GameEvent[targets.Length];

            for (int i = 0; i < r.Length; i++)
            {
                Target t = ts.Current;
                ts.MoveNext();

                if (t.isPlayer())
                {
                    r[i] = new DamagePlayerEvent(t.getPlayer(), c, d);
                }
                else
                {
                    r[i] = new DamageCreatureEvent(t.getCard(), c, d);
                }
            }

            return r;
        }
        
    }

    public class ExileTarget : SubEffect
    {
        public ExileTarget()
        {
            targets = new TargetRule[1];
            targets[0] = new TargetRule(TargetRules.CREATUREONFIELD);
            explanation = "exile target creature";
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts)
        {
            GameEvent[] r = new GameEvent[1];
            Target v = ts.Current;
            ts.MoveNext();
            r[0] = new MoveCardEvent(v.getCard(), LocationPile.EXILE);
            return r;
        }
    }

    public class GainLife : SubEffect
    {
        private int life;

        public GainLife(int n)
        {
            life = n;

            explanation = "you gain " + life + " life";
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts)
        {
            GameEvent[] l = {new GainLifeEvent(c.controller, life)};
            return l;
        }
        
    }
}