using System;

namespace stonekart
{
    public abstract class Cost
    {
        abstract public bool pay();
    }

    public class ManaCost : Cost
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

        public override bool pay()
        {
            throw new NotImplementedException();
        }

        public int[] getColors()
        {
            return colors;
        }
    }
}
