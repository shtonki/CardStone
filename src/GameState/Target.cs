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
        public abstract void resolveResolveTargets(GameInterface ginterface, GameState gstate, Card resolving, Target[] last);
        public abstract bool check(Target[] ts);

        protected static Func<Target, bool> resolveLambda(FilterLambda l)
        {
            switch (l)
            {
                case FilterLambda.ANY:
                    {
                        return (@t => true);
                    } break;
                case FilterLambda.PLAYER:
                    {
                        return (@t => t.isPlayer);
                    } break;
                case FilterLambda.ZAPPABLE:
                    {
                        return (@t => t.isPlayer ||
                                     t.card.location.pile == LocationPile.FIELD);
                    } break;
                case FilterLambda.CREATURE:
                    {
                        return (@t => t.isCard && t.card.getType() == CardType.Creature);
                    } break;
                case FilterLambda.ONFIELD:
                    {
                        return (@t => t.isCard && !t.isPlayer && t.card.location.pile == LocationPile.FIELD);
                    } break;
                case FilterLambda.INHAND:
                    {
                        return (@t => t.isCard && t.card.location.pile == LocationPile.HAND && t.card.controller.isHero);
                    } break;
                default:
                    throw new Exception();
            }
        }
    }

    interface Forcable
    {
        void forceTargets(Target[] ts);
    }

    public class FilterTargetRule : TargetRule, Forcable
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
                checks.Add(resolveLambda(l));
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
        public override void resolveResolveTargets(GameInterface ginterface, GameState gstate, Card resolving, Target[] last)
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
        private Func<Target, bool> filter;

        public ResolveTargetRule(ResolveTarget www)
        {
            this.www = www;
            targets = new Target[1];
            filter = (_) => true;
        }

        public ResolveTargetRule(ResolveTarget www, FilterLambda l) : this(www)
        {
            filter = resolveLambda(l);
        }

        public override Target[] resolveCastTargets(GameInterface ginterface, GameState gstate)
        {
            return new Target[] {};
        }
        public override void resolveResolveTargets(GameInterface ginterface, GameState gstate, Card resolving, Target[] last)
        {
            switch (www)
            {
                case ResolveTarget.CONTROLLER:
                {
                    targets[0] = new Target(resolving.owner);
                } break;

                case ResolveTarget.SELF:
                {
                    targets[0] = new Target(resolving.isDummy ? resolving.dummyFor.card : resolving);
                } break;

                case ResolveTarget.LAST:
                {
                    targets = last;
                } break;

                case ResolveTarget.OPPONENT:
                {
                    targets[0] = new Target(resolving.owner.opponent);
                } break;

                case ResolveTarget.FIELDCREATURES:
                {
                    targets = gstate.allCards.Where(card => card.isCreature && card.location.pile == LocationPile.FIELD).Select(card => new Target(card)).ToArray();
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

    public class SelectFromTargetRule : TargetRule, Forcable
    {
        private TargetRule showCardsTo;
        private TargetRule takePileFrom;
        private Func<Player, Card[]> selectCards;
        private int cardCount;

        public SelectFromTargetRule(TargetRule showCardsTo, TargetRule takePileFrom, Func<Player, Card[]> selectCards, int cardCount = 1)
        {
            this.showCardsTo = showCardsTo;
            this.takePileFrom = takePileFrom;
            this.selectCards = selectCards;
            this.cardCount = cardCount;
            if (showCardsTo is Forcable) throw new Exception();
        }

        public void forceTargets(Target[] ts)
        {
            (takePileFrom as Forcable)?.forceTargets(ts);
        }

        public override Target[] resolveCastTargets(GameInterface ginterface, GameState gstate)
        {
            return takePileFrom.resolveCastTargets(ginterface, gstate);
        }

        public override void resolveResolveTargets(GameInterface ginterface, GameState gstate, Card resolving, Target[] last)
        {
            targets = new Target[cardCount];
            showCardsTo.resolveResolveTargets(ginterface, gstate, resolving, last);
            takePileFrom.resolveResolveTargets(ginterface, gstate, resolving, last);
            Target[] showPlayerx = showCardsTo.getTargets();
            Target[] takePlayerx = takePileFrom.getTargets();

            if (showPlayerx.Length != 1 || !showPlayerx[0].isPlayer ||
                takePlayerx.Length != 1 || !takePlayerx[0].isPlayer) throw new Exception();
            Player showTo = showPlayerx[0].player;
            Card[] cards = selectCards(takePlayerx[0].player);

            if (showTo.isHero)
            {
                CardPanelControl p = ginterface.showCards(cards);
                for (int i = 0; i < targets.Length; i++)
                {
                    Card c = p.waitForCard();
                    ginterface.sendCard(c);
                    targets[i] = new Target(c);
                }
                p.closeWindow();
            }
            else
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    Card c = ginterface.demandCard(gstate);
                    targets[i] = new Target(c);
                }
            }
        }

        public override bool check(Target[] ts)
        {
            return takePileFrom.check(ts);
        }
    }

    public enum FilterLambda
    {
        ANY,
        PLAYER,
        CREATURE,
        ZAPPABLE,
        ONFIELD,
        INHAND,
        //ZAPPABLECREATURE, 
    }

    public enum ResolveTarget
    {
        SELF,
        CONTROLLER,
        LAST,
        OPPONENT,
        FIELDCREATURES,
    }
}
