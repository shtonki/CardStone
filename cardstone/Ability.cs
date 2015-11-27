using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    class Ability
    {
        public Cost cost;
        public Effect effect;

        public Ability(Cost c, Effect e)
        {
            cost = c;
            effect = e;
        }
    }
}
