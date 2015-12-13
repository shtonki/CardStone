using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public abstract class Ability
    {
        protected Effect effect;
        protected Card card;

        public Card getCard()
        {
            return card;
        }

        public Effect getEffect()
        {
            return effect;
        }

        public int countTargets()
        {
            return effect.countTargets();
        }

        public TargetRule[] getTargetRules()
        {
            return effect.getTargetRules();
        }
    }

    public class ActivatedAbility : Ability
    {
        private Cost cost;
        private List<int> from;
        private bool instant;

        public ActivatedAbility(Card ca, Cost c) : this(ca, c, new Effect())
        {
            
        }

        public ActivatedAbility(Card ca, Cost c, Effect e)
        {
            from = new List<int>();
            from.Add(Location.HAND);

            card = ca;
            cost = c;
            effect = e;
        }


        public Cost getCost()
        {
            return cost;
        }

        public bool castableFrom(int i)
        {
            foreach (var v in from)
            {
                if (v == i) { return true;}
            }
            return false;
        }

        public void setInstant(bool b)
        {
            instant = b;
        }

        public bool isInstant()
        {
            return instant;
        }

    }
}
