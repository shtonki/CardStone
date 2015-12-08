using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public class ActivatedAbility
    {
        private Cost cost;
        private Effect effect;
        private List<int> from;
        private bool moveToStack;
        private bool instant;
        private Card card;

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

        public bool isInstant()
        {
            return instant;
        }

        public Card getCard()
        {
            return card;
        }
    }
}
