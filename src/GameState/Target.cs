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
        protected Target[] targets;
        public Target[] getTargets()
        {
            return targets;
        }
        public abstract Target[] resolveCastTargets(GameInterface ginterface, GameState gstate);
        public abstract void resolveResolveTargets(GameInterface ginterface, Card resolving, Target[] last);
        public abstract bool check(Target[] ts);
    }

    public class FilterTargetRule : TargetRule
    {
        private int targetCount;
        private List<Func<Target, bool>> checks = new List<Func<Target, bool>>();

        public FilterTargetRule(int targetCount, params FilterLambda[] ls)
        {
            targets = new Target[targetCount];
            checks = new List<Func<Target, bool>>(ls.Length);
            this.targetCount = targetCount;

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

        public override Target[] resolveCastTargets(GameInterface ginterface, GameState gstate)
        {
            int i = 0;
            ginterface.setContext("choose targetx");
            while (i < targetCount)
            {
                GameElement ge = ginterface.getNextGameElementPress();

                if (ge.player != null)
                {
                    Target t = new Target(ge.player);
                    if (checks.All(f => f(t)))
                    {
                        targets[i++] = t;
                    }
                }
                if (ge.card != null)
                {
                    Target t = new Target(ge.card);
                    if (checks.All(f => f(t)))
                    {
                        targets[i++] = t;
                    }
                }
                if (ge.choice != null && ge.choice.Value == Choice.Cancel)
                {
                    targets = null;
                    break;
                }
            }
            ginterface.clearContext();
            return targets;
        }
        public override void resolveResolveTargets(GameInterface ginterface, Card resolving, Target[] last)
        {
            
        }
        public override bool check(Target[] ts)
        {
            return targets.All(t => checks.All(f => f(t)));
        }

        public bool check(Target t)
        {
            return checks.All(v => v(t));
        }

        public void forceTargets(Target[] ts)
        {
            targets = ts;
        }
    }

    public class ResolveTargetRule : TargetRule
    {
        private ResolveTarget www;

        public ResolveTargetRule(ResolveTarget www)
        {
            this.www = www;
            targets = new Target[1];
        }


        public override Target[] resolveCastTargets(GameInterface ginterface, GameState gstate)
        {
            return new Target[] {};
        }

        public override void resolveResolveTargets(GameInterface ginterface, Card resolving, Target[] last)
        {
            switch (www)
            {
                case ResolveTarget.CONTROLLER:
                {
                    targets[1] = new Target(resolving.owner);
                } break;

                case ResolveTarget.SELF:
                {
                    targets[1] = new Target(resolving);
                } break;

                case ResolveTarget.LAST:
                {
                    targets = last;
                } break;

                default:
                {
                    throw new Exception("xd");
                } break;
            }
        }

        public override bool check(Target[] ts)
        {
            return true;
        }
    }

    public class SelectFromTargetRule : TargetRule
    {
        private Func<Card[]> cards;

        public SelectFromTargetRule(Func<Card[]> cards, int count)
        {
            this.cards = cards;
            targets = new Target[count];
        }


        public override Target[] resolveCastTargets(GameInterface ginterface, GameState gstate)
        {
            return new Target[] { };
        }

        public override void resolveResolveTargets(GameInterface ginterface, Card resolving, Target[] last)
        {
            CardPanelControl p = ginterface.showCards(cards());

            int i = 0;
            while (i < targets.Length)
            {
                targets[i++] = new Target(p.waitForCard());
            }
            p.closeWindow();
        }

        public override bool check(Target[] ts)
        {
            throw new NotImplementedException();
        }
    }

    public enum FilterLambda
    {
        ANY,
        PLAYER,
        CREATURE,
        ZAPPABLE,
        //ZAPPABLECREATURE, 
    }

    public enum ResolveTarget
    {
        SELF,
        CONTROLLER,
        LAST,
    }
}
