using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    //todo(seba) make it work more like a union
    public class Target
    {
        public Player player { get; private set; }
        public Card card { get; private set; }
        public bool isPlayer => player != null;
        public bool isCard => card != null;


        public Target(Player player)
        {
            this.player = player;
        }

        public Target(Card card)
        {
            this.card = card;
        }
    }

    abstract public class TargetRule
    {
    }

    public enum TargetLambda
    {
        SELF,
        CONTROLLER,
        LAST,

        ANY,
        PLAYER,
        CREATURE,
        ZAPPABLE,
        //ZAPPABLECREATURE, 
    }
}
