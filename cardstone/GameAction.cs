using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public abstract class GameAction
    {
        public abstract string toString();

        public static GameAction fromString(string s, Game g)
        {
            string[] ss = s.Split(',');

            GameAction r;

            switch (ss[0])
            {
                case "pass":
                {
                    r = new PassAction();
                } break;

                case "select":
                {
                    r = new SelectAction(Int32.Parse(ss[1]));
                } break;

                case "attackers":
                {
                    //todo actually do this
                    r = new DeclareAttackersAction();
                    break;
                    List<Card> cs = new List<Card>();

                    for (int i = 1; i < ss.Length; i++)
                    {
                    }
                } break;

                case "cast":
                {
                    r = new CastAction(g.getCardById(Int32.Parse(ss[1])));
                } break;

                case "deck":
                {
                    CardId[] cs = new CardId[ss.Length-1];

                    for (int i = 1; i < ss.Length; i++)
                    {
                        cs[i - 1] = (CardId)Int32.Parse(ss[i]);
                    }

                    r = new DeclareDeckAction(cs);
                } break;

                default:
                {
                    throw new Exception("bad action received");
                } break;
            }

            return r;
        }
    }


    public class PassAction : GameAction
    {
        public override string toString()
        {
            return "pass";
        }
    }

    public class CastAction : GameAction
    {
        private Card card;

        public CastAction(Card c)
        {
            card = c;
        }

        public Card getCard()
        {
            return card;
        }

        public override string toString()
        {
            return "cast," + card.getId();
        }
    }

    public class DeclareAttackersAction : GameAction
    {
        private List<Card> attackers;

        public DeclareAttackersAction(List<Card> cs)
        {
            attackers = cs;
        }

        public DeclareAttackersAction()
        {
            attackers = new List<Card>();
        }

        public override string toString()
        {
            StringBuilder b = new StringBuilder();

            b.Append("attackers,");

            foreach (Card c in attackers)
            {
                b.Append(c.getId());
            }

            return b.ToString();
        }
    }

    public class SelectAction : GameAction
    {
        private int choice;

        public SelectAction(int i)
        {
            choice = i;
        }

        public override string toString()
        {
            return "select," + choice;
        }

        public int getSelection()
        {
            return choice;
        }
    }

    public class DeclareDeckAction : GameAction
    {
        private CardId[] ids;

        public DeclareDeckAction(CardId[] cs)
        {
            ids = cs;
        }

        public override string toString()
        {
            StringBuilder b = new StringBuilder("deck,");

            foreach (var v in ids)
            {
                b.Append((int)v + ",");
            }
            b.Length--;
            return b.ToString();
        }

        public CardId[] getIds()
        {
            return ids;
        }
    }
}
