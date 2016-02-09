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
        public abstract Target[] getTargets();
        public abstract void resolveCastTargets(GameInterface ginterface, GameState gstate);
        public abstract void resolveResolveTargets(Card resolving, Target[] last);
        public abstract bool check(Target[] ts);
    }

    public class FilterTargetRule : TargetRule
    {
        private List<Func<Target, bool>> checks = new List<Func<Target, bool>>();

        public FilterTargetRule(params FilterLambda[] ls)
        {
            foreach (FilterLambda l in ls)
            {
                switch (l)
                {
                    case FilterLambda.ANY:
                        {
                            checks.Add(@t => true);
                        }
                        break;

                    case FilterLambda.PLAYER:
                        {
                            checks.Add(@t => t.isPlayer);
                        }
                        break;

                    case FilterLambda.ZAPPABLE:
                        {
                            checks.Add(@t => t.isPlayer ||
                                         t.card.location.pile == LocationPile.FIELD);
                        }
                        break;
                    case FilterLambda.CREATURE:
                        {
                            checks.Add(@t => t.isCard && t.card.getType() == CardType.Creature);
                        }
                        break;

                    default:
                        throw new Exception();
                }
            }

        }
        
        public bool check(Target t)
        {
            return checks.All(v => v(t));
        }
    }

    public enum FilterLambda
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
