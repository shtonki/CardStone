﻿using System;
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

        public abstract string getExplanation();
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


        public override string getExplanation()
        {
            return effect.getExplanation();
        }
    }

    
}