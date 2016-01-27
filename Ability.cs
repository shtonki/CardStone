using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public abstract class Ability
    {
        public Card card { get; protected set; }
        
        public Effect effect { get; protected set; }

        public int targetCount => effect.targetCount;


        public TargetRule[] targetRules => effect.getTargetRules();

        public virtual string getExplanation()
        {
            return effect.explanation;
        }
    }

    public class ActivatedAbility : Ability
    {
        private Cost cost;
        private LocationPile from;
        private bool instant;
        
        public ActivatedAbility(Card ca, Cost c, Effect e, LocationPile pile)
        {
            from = pile;

            card = ca;
            cost = c;
            effect = e;
        }


        public Cost getCost()
        {
            return cost;
        }

        public bool castableFrom(LocationPile p)
        {
            return from == p;
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

    public class TriggeredAbility : Ability
    {
        public EventFilter filter { get; private set; }
        public LocationPile pile { get; private set; }
        public EventTiming timing { get; private set; }

        public TriggeredAbility(EventFilter f, LocationPile p, EventTiming t, Effect e)
        {
            filter = f;
            pile = p;
            effect = e;
            timing = t;
        }
    }
}
