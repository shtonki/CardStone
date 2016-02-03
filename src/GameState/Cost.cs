using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace stonekart
{
    public class Cost
    {
        //private ManaCoster manaCost;
        private List<SubCost> costs;
        /*
        public bool tryPay(Player p)
        {
            if (checkAll(p))
            {
                payAll(p);
                return true;
            }
            return false;
        }
        */
        public int[][] check(Card card, GameInterface gi)
        {
            int[][] r = new int[costs.Count][];
            for (int i = 0; i < costs.Count; i++)
            {
                int[] c = costs[i].check(card, gi);
                if (c == null) { return null; }
                r[i] = c;
            }
            return r;
        }

        public void pay(Card card, int[][] i)
        {
            if (i.Length != costs.Count) { throw new Exception("can't"); }
            for (int j = 0; j < i.Length; j++)
            {
                costs[j].pay(card, i[j]);
            }
        }

        public Cost(params SubCost[] cs)
        {
            costs = new List<SubCost>(cs);
        }

        public Cost(params SubCost[][] cs)
        {
            int c = 0;
            //foreach (Coster[] l in cs) { c += l.Length; }
            costs = new List<SubCost>(c);
            foreach (SubCost[] xs in cs)
            {
                foreach (SubCost t in xs)
                {
                    costs.Add(t);
                }
            }
        }
    }
    /*
    public class CastingCost : Cost
    {
        private ManaCoster manaCost;

        public CastingCost(ManaCoster c) : base(c)
        {
            manaCost = c;
        }

        public CastingCost(ManaCoster c, params Coster[] cs) : base(new Coster[]{c}, cs)
        {
            manaCost = c;
        }

        public ManaCoster getManaCost()
        {
            return manaCost;
        }
    }
    */
    public abstract class SubCost
    {
        public abstract int[] check(Card c, GameInterface gi);
        abstract public void pay(Card c, int[] i);
    }

    public class ManaCost : SubCost
    {
        //todo(seba) reconsider how we store this information yet again, no it's fine xd
        private readonly int[] Costs = new int[6];
        public int[] costs => cloneLambda(Costs);

        public IEnumerable<int> costsEnumerable => Costs;

        private int[] cloneLambda(int[] a)
        {
            int[] r = new int[a.Length];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = a[i];
            }
            return r;
        }

        public int CMC { get; private set; }

        public ManaCost(int white, int blue, int black, int red, int green, int grey)
        {
            Costs[(int)Colour.WHITE] = white;
            Costs[(int)Colour.BLUE] = blue;
            Costs[(int)Colour.BLACK] = black;
            Costs[(int)Colour.RED] = red;
            Costs[(int)Colour.GREEN] = green;
            Costs[(int)Colour.GREY] = grey;

            CMC = white + blue + black + red + green + grey;
        }

        public override int[] check(Card card, GameInterface gi)
        {
            Player owner = card.owner;

            if (CMC > owner.totalMana) { return null; }

            int[] r = new int[CMC];
            int[] cz = costs;
            int c = 0;

            for (int i = 0; i < 5; i++)
            {
                if (cz[i] > owner.getCurrentMana(i))
                {
                    return null;
                }
                while (cz[i]-- > 0)
                {
                    r[c++] = i;
                }
            }

            if (c == CMC) { return r; }

            if (CMC == owner.totalMana)
            {
                for (int i = 0; i < 5; i++)
                {
                    int v = owner.getCurrentMana(i) - Costs[i];
                    while (v-- > 0)
                    {
                        r[c++] = i;
                    }
                }
                return r;
            }
            else
            {
                var paid = costs;

                gi.push();
                gi.setFakeManas(Costs);
                gi.setChoiceButtons(Choice.Cancel);

                while (c != CMC)
                {
                    gi.setMessage("Pay " + cz[5]);

                    GameElement element = gi.getNextGameElementPress();

                    if (element.choice != null && element.choice == Choice.Cancel)
                    {
                        gi.resetFakeMana();
                        gi.pop();
                        return null;
                    }

                    else if (element.manaColour != null)
                    {
                        int v = (int)element.manaColour;

                        if (owner.getCurrentMana(v) - paid[v] > 0)
                        {
                            r[c++] = v;
                            gi.decrementFakeMana(v);
                        }
                    }
                    
                }

                gi.resetFakeMana();
                gi.pop();
                return r;
            }
        }

        public override void pay(Card card, int[] i)
        {
            card.owner.spendMana(i);
        }
    }
}
