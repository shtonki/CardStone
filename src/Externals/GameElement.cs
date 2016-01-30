using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public class GameElement
    {
        private object element;
        public GameElementType type { get; private set; }

        public Card card => element as Card;
        public Choice? choice => element as Choice?;
        public Player player => element as Player;
        public ManaColour? manaColour => element as ManaColour?;

        public GameElement(Card c)
        {
            element = c;
            type = GameElementType.CARD;
        }

        public GameElement(Choice? c)
        {
            element = c;
            type = GameElementType.CHOICE;
        }

        public GameElement(Player p)
        {
            element = p;
            type = GameElementType.PLAYER;
        }

        public GameElement(ManaColour c)
        {
            element = c;
            type = GameElementType.MANACOLOR;
        }

        public enum GameElementType
        {
            CARD,
            CHOICE,
            PLAYER,
            MANACOLOR,
        }
    }
}
