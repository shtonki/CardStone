﻿using System;
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
                    r = new CastAction(null);
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
            return card == null ? "pass" : "cast," + card.getId();
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
