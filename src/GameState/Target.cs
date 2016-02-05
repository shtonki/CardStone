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

    public class TargetRule
    {
        private List<Func<Target, bool>> checks; 

        public TargetRule(TargetLambda r) : this(howDoIInlineFunctionsxd(r).ToArray())
        {
            
        }

        private TargetRule(params Func<Target, bool>[] fs)
        {
            checks = new List<Func<Target, bool>>(fs);
        }

        public bool check(Target t)
        {
            return checks.All(v => v(t));
        }

        private static List<Func<Target, bool>> howDoIInlineFunctionsxd(TargetLambda r)
        {
            List<Func<Target, bool>> rt = new List<Func<Target, bool>>();

            switch (r)
            {
                case TargetLambda.ANY:
                {
                    rt.Add(@t => true);
                } break;

                case TargetLambda.PLAYER:
                {
                    rt.Add(@t => t.isPlayer);
                } break;

                case TargetLambda.ZAPPABLE:
                {
                    rt.Add(@t => t.isPlayer ||
                                 t.card.location.pile == LocationPile.FIELD);
                } break;
                case TargetLambda.ZAPPABLECREATURE:
                {
                    rt.Add(@t => t.isCard && t.card.location.pile == LocationPile.FIELD);
                } break;

                default:
                    throw new Exception();
            }

            return rt;
        }
    }

    public enum TargetLambda
    {
        SELF,
        CONTROLLER,

        ANY,
        PLAYER,
        ZAPPABLE,
        ZAPPABLECREATURE,
    }
}
