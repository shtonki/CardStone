using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace stonekart
{
    public class Effect
    {
        private Effecter[] effecters;
        private TargetRule[] targetRules;

        private string explanation;

        public Effect(params Effecter[] es)
        {
            effecters = es;

            targetRules = new TargetRule[countTargets()];

            int i = 0;
            StringBuilder b = new StringBuilder();

            foreach (var v in es)
            {
                foreach (var vv in v.getTargetRules())
                {
                    targetRules[i++] = vv;
                }

                b.Append(v.getExplanation());
            }

            explanation = b.ToString();
        }

        public List<GameEvent> resolve(Card c, Target[] ts)
        {
            List<GameEvent> r = new List<GameEvent>();

            Effecter.hackEnumerator = ts.GetEnumerator();

            foreach (Effecter e in effecters)
            {
                foreach (var v in e.resolve(c))
                {
                    r.Add(v);
                }
            }

            Effecter.hackEnumerator = null;

            return r;
        }

        public TargetRule[] getTargetRules()
        {
            return targetRules;
        }

        public int countTargets()
        {
            int i = 0;

            foreach (var v in effecters)
            {
                i += v.noOfTargets();
            }

            return i;
        }

        public string getExplanation()
        {
            return explanation;
        }
    }

    public abstract class Effecter
    {
        //todo hack me to the fucking moon i'm space bound baby this is ugly but jesus it feels good
        public static IEnumerator hackEnumerator; 

        protected static Target nextTarget()
        {
            hackEnumerator.MoveNext();
            return hackEnumerator.Current as Target;
        }

        protected List<TargetRule> targets; 

        abstract public GameEvent[] resolve(Card c);

        protected Effecter()
        {
            targets = new List<TargetRule>();
        }

        public List<TargetRule> getTargetRules()
        {
            return targets;
        }

        public int noOfTargets()
        {
            return targets.Count;
        }

        public abstract string getExplanation();
    }

    public class OwnerDrawsEffecter : Effecter
    {
        private int i;

        public OwnerDrawsEffecter(int cards)
        {
            i = cards;
        }

        public override GameEvent[] resolve(Card c)
        {
            GameEvent e = new DrawEvent(c.getController(), i);
            return new[] {e};
        }

        public override string getExplanation()
        {
            return "Draw " + i + " cards.";
        }
    }

    public class PingN : Effecter
    {
        private int d;

        public PingN(int targets, int damage)
        {
            for (int i = 0; i < targets; i++)
            {
                this.targets.Add(new TargetRule(TargetRulex.ZAPPABLE));                
            }
            d = damage;
        }

        public override GameEvent[] resolve(Card c)
        {

            GameEvent[] r = new GameEvent[targets.Count];

            for (int i = 0; i < r.Length; i++)
            {
                Target t = nextTarget();

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
            if (targets.Count == 1)
            {
                return "Deal " + d + " damage to target.";
            }
            else
            {
                return "Deal " + d*targets.Count + " damage split between up to " + targets.Count + " targets.";
            }
        }
    }
}