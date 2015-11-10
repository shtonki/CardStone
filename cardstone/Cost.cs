using System;
using System.Collections.Generic;

namespace stonekart
{
    public class Cost
    {
        private ManaCost manaCost;
        private List<Coster> costs;

        public bool tryPay()
        {
            if (checkAll())
            {
                payAll();
                return true;
            }
            return false;
        }

        private bool checkAll()
        {
            foreach (Coster c in costs)
            {
                if (!c.check())
                {
                    return false;
                }
            }
            return true;
        }

        private void payAll()
        {
            foreach (Coster c in costs)
            {
                c.pay();
            }
        }

        public Cost(ManaCost c)
        {
            manaCost = c;
            costs = new List<Coster>();
            costs.Add(manaCost);
        }

        public ManaCost getManaCost()
        {
            return manaCost;
        }
    }

    public abstract class Coster
    {
        public abstract bool check();
        abstract public void pay();
    }

    public class ManaCost : Coster
    {
        public const int
            WHITE = 0,
            BLUE = 1,
            BLACK = 2,
            RED = 3,
            GREEN = 4;

        private int[] colors;

        public ManaCost(params int[] cs)
        {
            colors = cs;
        }

        public ManaCost(int white, int blue, int black, int red, int green)
        {
            int m = white + blue + black + red + green;
            colors = new int[m];
            int c = 0;
            for (int i = 0; i < white; i++)
            {
                colors[c] = WHITE;
                c++;
            }
            for (int i = 0; i < blue; i++)
            {
                colors[c] = BLUE;
                c++;
            }
            for (int i = 0; i < black; i++)
            {
                colors[c] = BLACK;
                c++;
            }
            for (int i = 0; i < red; i++)
            {
                colors[c] = RED;
                c++;
            }
            for (int i = 0; i < green; i++)
            {
                colors[c] = GREEN;
                c++;
            }
        }

        public override bool check()
        {
            for (int i = 0; i < 5; i++)
            {
                if (GameController.getHero().getCurrentMana(i) < colors[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override void pay()
        {
            throw new NotImplementedException();
        }

        public int[] getColors()
        {
            return colors;
        }
    }
}
