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

        //todo(seba) just rename these ToString and Parse because we aren't actually imbeciles we just pretend for the sake of irony
        /// <summary>
        /// Translates the GameAction to a string which can be sent to the other player
        /// </summary>
        /// <returns>The string representing this action</returns>
        public abstract string toString();

        /// <summary>
        /// Translates a string into a game action within a given game
        /// </summary>
        /// <param name="s">The string to translate</param>
        /// <param name="g">The game in which to translate it</param>
        /// <returns></returns>
        public static GameAction fromString(string s, GameController g)
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
                    string thepuddn = ss[1];
                    string[] puddns = thepuddn.Split(';');

                    Card c = g.getCardById(Int32.Parse(puddns[0]));
                    Ability a = c.getAbilityByIndex(Int32.Parse(puddns[1]));
                    c = c.isCast(a) ? c : Card.createDummy(a);

                    string[] ts = puddns[2].Split('\'');

                    List<Target> targets = new List<Target>(ts.Length);
                    Target ta;

                    foreach (var t in ts)
                    {
                        if (t.Length == 0) { continue; }
                        int x = Int32.Parse(t.Substring(1));
                        if (t[0] == 'p')
                        {
                            ta = new Target(g.getPlayerById((LocationPlayer)x));
                        }
                        else if (t[0] == 'c')
                        {
                            ta = new Target(g.getCardById(x));
                        }
                        else
                        {
                            throw new InvalidOleVariantTypeException("yup");
                        }
                        targets.Add(ta);
                    }



                    string[] cs = puddns[3].Split('\'');

                    int[][] csts;
                    if (cs[0].Length == 0)
                    {
                        csts = new int[0][];
                    }
                    else
                    {
                         csts = new int[cs.Length][];
                    }


                    for (int i = 0; i < csts.Length; i++)
                    {
                        string[] xds = cs[i].Split('*');
                        int[] xx = new int[xds.Length];

                        for (int j = 0; j < xx.Length; j++)
                        {
                            xx[j] = Int32.Parse(xds[j]);
                        }

                        csts[i] = xx;
                    }
                    a.setCastTargets(targets.ToArray());
                    var sw = new StackWrapper(c, a, targets.ToArray());
                    r = new CastAction(sw, csts);

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
                    throw new Exception("bad action received:\n{0}" + s);
                } break;
            }

            return r;
        }
    }
    

    public class CastAction : GameAction
    {
        private StackWrapper sw;
        private int[][] costs;

        public CastAction()
        {
            sw = null;
            costs = null;
        }

        public CastAction(StackWrapper s, int[][] cs)
        {
            sw = s;
            costs = cs;
        }

        public bool isPass()
        {
            return sw == null;
        }

        public int[][] getCosts()
        {
            return costs;
        }

        public StackWrapper getStackWrapper()
        {
            return sw;
        }

        public override string toString()
        {
            if (sw == null) { return "pass"; }
            
            Card c = sw.card.isDummy ? sw.ability.card : sw.card;
            StackWrapper wrapper = new StackWrapper(c, sw.ability, sw.targets);
            StringBuilder ts = new StringBuilder(), cs = new StringBuilder();
            foreach (var t in wrapper.targets)
            {
                if (t.isPlayer)
                {
                    ts.Append("p");
                    ts.Append(1-(int)t.player.side); //hack dangerouze
                }
                else if (t.isCard)
                {
                    ts.Append("c");
                    ts.Append(t.card.getId());
                }
                ts.Append("'");
            }
            if (ts.Length > 0) { ts.Length--; }
            foreach (var v in costs)
            {
                foreach (var i in v)
                {
                    cs.Append(i);
                    cs.Append("*");
                }
                if (v.Length == 0) { continue; } //
                cs.Length--;
                cs.Append("'");
            }
            if (cs.Length > 0) { cs.Length--; }
            return "cast," + wrapper.card.getId() + ';' + wrapper.card.getAbilityIndex(wrapper.ability) + ';' + ts + ';' + cs;
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
