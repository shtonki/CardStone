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
        protected Effect effect;
        public List<GameEvent> resolve(Card c, Target[] ts, GameInterface ginterface, GameState gameState)
        {
            return effect.resolve(c, ts, ginterface, gameState);
        }
        public string description { get; protected set; }
        protected Ability(Card c)
        {
            card = c;
        }

        public Target[] aquireTargets(GameInterface gi, GameState gs)
        {
            return effect.aquireTargets(gi, gs, this is ActivatedAbility);
        }

        public void setCastTargets(Target[] ts)
        {
            effect.setTargets(ts);
        }
    }

    public class ActivatedAbility : Ability
    {
        private Cost cost;
        private LocationPile from;
        public bool instant;
        
        public ActivatedAbility(Card ca, Cost c, Effect e, bool instantSpeed, LocationPile pile, string desc) : base(ca)
        {
            from = pile;
            instant = instantSpeed;
            card = ca;
            cost = c;
            effect = e;

            description = desc;
        }


        public Cost getCost()
        {
            return cost;
        }

        public bool castableFrom(LocationPile p)
        {
            return from == p;
        }
    }

    public class TriggeredAbility : Ability
    {
        public EventFilter filter { get; private set; }
        public LocationPile pile { get; private set; }
        public EventTiming timing { get; private set; }

        public TriggeredAbility(Card c, EventFilter f, string desc, LocationPile p, EventTiming t, Effect e) : base(c)
        {
            filter = f;
            pile = p;
            effect = e;
            timing = t;
            description = desc;
        }

        public TriggeredAbility(Card c, EventFilter f, string desc, LocationPile p, EventTiming t, params SubEffect[] es) 
            :this(c, f, desc, p, t, new Effect(es))
        {
            
        }

        public TriggeredAbility(Card c, EventFilter f, string desc, LocationPile p, EventTiming t, Func<bool> preResolveCheck, params SubEffect[] es)
            : this(c, f, desc, p, t, new Effect(es, preResolveCheck))
        {
        }
    }
    
}
