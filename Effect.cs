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

                b.Append(v.getExplanation());
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

        public int targetCount
        {
            get { return subEffects.Sum(v => v.targetCount); }
        }

        public string explanation => Explanation;
    }

    public abstract class SubEffect
    {
        public TargetRule[] targetRules => targets;
        public int targetCount => targets.Length;
        protected TargetRule[] targets; 


        protected SubEffect()
        {
        }

        abstract public GameEvent[] resolve(Card c, IEnumerator<Target> ts);

        public abstract string getExplanation();
    }

    public class OwnerDrawsSubEffect : SubEffect
    {
        private int i;

        public OwnerDrawsSubEffect(int cards)
        {
            i = cards;
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> _)
        {
            GameEvent e = new DrawEvent(c.getController(), i);
            return new[] {e};
        }

        public override string getExplanation()
        {
            return "Draw " + i + " cards.";
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

        public override string getExplanation()
        {
            if (targets.Length == 1)
            {
                return "Deal " + d + " damage to target.";
            }
            else
            {
                return "Deal " + d*targets.Length + " damage split between up to " + targets.Length + " targets.";
            }
        }
    }
}