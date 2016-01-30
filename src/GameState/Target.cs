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
        private Player p;
        private Card c;

        public Target(Player player)
        {
            p = player;
        }

        public Target(Card card)
        {
            c = card;
        }

        public bool isPlayer()
        {
            return p != null;
        }

        public bool isCard()
        {
            return c != null;
        }

        public Card getCard()
        {
            return c;
        }

        public Player getPlayer()
        {
            return p;
        }
    }

    public class TargetRule
    {
        private List<Func<Target, bool>> checks; 

        public TargetRule(TargetRules r) : this(howDoIInlineFunctionsxd(r).ToArray())
        {
            
        }

        public TargetRule(params Func<Target, bool>[] fs)
        {
            checks = new List<Func<Target, bool>>(fs);
        }

        public bool check(Target t)
        {
            return checks.All(v => v(t));
        }

        private static List<Func<Target, bool>> howDoIInlineFunctionsxd(TargetRules r)
        {
            List<Func<Target, bool>> rt = new List<Func<Target, bool>>();

            switch (r)
            {
                case TargetRules.ANY:
                {
                    rt.Add(@t => true);
                } break;

                case TargetRules.PLAYER:
                {
                    rt.Add(@t => t.isPlayer());
                } break;

                case TargetRules.ZAPPABLE:
                {
                    rt.Add(@t => t.isPlayer() ||
                                 t.getCard().location.pile == LocationPile.FIELD);
                } break;
                case TargetRules.CREATUREONFIELD:
                {
                    rt.Add(@t => t.isCard() && t.getCard().location.pile == LocationPile.FIELD);
                } break;
            }

            return rt;
        }
    }

    public enum TargetRules
    {
        ANY,
        PLAYER,
        ZAPPABLE,
        CREATUREONFIELD,
    }
}
