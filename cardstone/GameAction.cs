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
                    r = new CastAction();
                } break;

                case "select":
                {
                    r = new SelectAction(Int32.Parse(ss[1]));
                } break;

                case "mselect":
                {
                    int[] iss = new int[ss.Length - 1];

                    for (int i = 1; i < ss.Length; i++)
                    {
                        iss[i - 1] = Int32.Parse(ss[i]);
                    }

                    r = new MultiSelectAction(iss);
                } break;

                case "cast":
                {
                    List<int[]> us = new List<int[]>();
                    List<int> ts = new List<int>();
                    int card = Int32.Parse(ss[1]);

                    for (int i = 2; i < ss.Length; i++)
                    {
                        char c = ss[i][0];
                        if (c == 'u') //u'0'0'1'
                        {
                            us.Add(ss[i].Split('\'').Select(Int32.Parse).ToArray());
                        }
                        else if (c == 't')
                        {
                            ts.Add(Int32.Parse(ss[i].Substring(1)));
                        }
                    }

                    r = new CastAction(g.getCardById(Int32.Parse(ss[1])), us.ToArray());
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


    public class CastAction : GameAction
    {
        private Card card;
        private int[][] costs;

        public CastAction()
        {
            card = null;
            costs = null;
        }

        public CastAction(Card c, int[][] cs)
        {
            card = c;
            costs = cs;
        }

        public Card getCard()
        {
            return card;
        }

        public override string toString()
        {
            if (card == null) { return "pass"; } 

            StringBuilder b = new StringBuilder();
            foreach (int[] l in costs)
            {
                b.Append('c');
                foreach (int i in l)
                {
                    b.Append('\'');
                    b.Append(i.ToString());
                }
            }
            return "cast," + card.getId() + b.ToString();
        }
    }
    /*
    public class MultiSelectEvent : GameAction
    {
        private List<Card> attackers;

        public MultiSelectEvent(List<Card> cs)
        {
            attackers = cs;
        }

        public MultiSelectEvent()
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
    */
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

        public DeclareDeckAction()
        {
            ids = new CardId[0];
        }

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
    
    public class MultiSelectAction : GameAction
    {
        private int[] ints;

        public MultiSelectAction()
        {
            ints = new int[0];
        }

        public MultiSelectAction(Card[] cs)
        {
            ints = cs.Select(@c => c.getId()).ToArray();
        }

        public MultiSelectAction(int[] iss)
        {
            ints = iss;
        }

        public int[] getSelections()
        {
            return ints;
        }

        public override string toString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("mselect,");

            foreach (int i in ints)
            {
                b.Append(i);
                b.Append(',');
            }

            b.Length--;

            return b.ToString();
        }
    }
     
}
