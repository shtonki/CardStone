using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    /// <summary>
    /// kill me now this is so fucking broken it's a joke at this point
    /// it just werks though
    /// </summary>
    public class Location
    {

        private const int PILES = 11;
        private static Pile[] piles = new Pile[PILES];

        public const byte
            HAND = 0,
            DECK = 1,
            FIELD = 2,
            GRAVEYARD = 3,
            EXILE = 4,
            STACK = 5,
            NOWHERE = 6,

            HEROSIDE = 0,
            VILLAINSIDE = 1,
            NOONE = 2;

        private byte location;
        private byte side;

        /// <summary>
        /// This isn't the contructor you're looking for
        /// </summary>
        /// <param name="location"></param>
        public Location(byte location)
        {
            this.location = location;
            this.side = NOONE;
        }

        public Location(byte location, byte side)
        {
            this.location = location;
            this.side = side;
        }

        public static void setPile(byte location, byte side, Pile p)
        {
            piles[location*2 + side] = p;
        }

        public Pile getPile()
        {
            return getPile(location, side);
        }

        public byte getSide()
        {
            return side;
        }

        public byte getLocation()
        {
            return location;
        }

        public static Pile getPile(int location, int side)
        {
            if (location == NOWHERE || side == NOONE) { return null; }
            return piles[location * 2 + side];
        }

        public static Location getLocation(Pile p)
        {
            for (int i = 0; i < PILES; i++)
            {
                if (piles[i] == p)
                {
                    return new Location((byte)(i/2), (byte)(i%2));
                }
            }
            throw new Exception("I really hope the never happens");
        }

    }
}
