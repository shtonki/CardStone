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
            CAST = 2,
            MOVE = 4;

        private int type;
        protected Card card;

        public GameEvent(int type)
        {
            this.type = type;
        }

        public int getType()
        {
            return type;
        }

        public Card getCard()
        {
            return card;
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
        public CastEvent(Card c) : base(CAST)
        {
            card = c;
        }

        public Card getCard()
        {
            return card;
        }
    }

    class MoveEvent : GameEvent
    {
        public MoveEvent() : base(MOVE)
        {
        }
    }
}
