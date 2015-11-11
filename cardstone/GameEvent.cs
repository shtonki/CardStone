using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{

    abstract class GameEvent
    {
        public const int
            DRAW = 1,
            CAST = 2;

        private int type;

        public GameEvent(int type)
        {
            this.type = type;
        }

        public int getType()
        {
            return type;
        }
    }

    class DrawEvent : GameEvent
    {
        private bool h;

        public DrawEvent(bool hero) : base(DRAW)
        {
            h = hero;
        }

        public bool isHeroDraw()
        {
            return h;
        }
    }

    class CastEvent : GameEvent
    {
        private Card card;

        public CastEvent(Card c) : base(CAST)
        {
            card = c;
        }

        public Card getCard()
        {
            return card;
        }
    }
}
