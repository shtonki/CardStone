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
        public LocationPile pile { get; private set; }
        public LocationPlayer side { get; private set; }

        public Location(LocationPile p, LocationPlayer plr)
        {
            pile = p;
            side = plr;
        }
        
        public static bool operator ==(Location a, Location b)
        {
            if (ReferenceEquals(a, b)) { return true; }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) { return false; }
            return a.pile == b.pile && a.side == b.side;
        }

        public static bool operator !=(Location a, Location b)
        {
            return !(a == b);
        }
        
    }

    public enum LocationPile
    {
        GRAVEYARD,
        HAND,
        DECK,
        EXILE,
        FIELD,
        STACK,
    }

    public enum LocationPlayer
    {
        HERO,
        VILLAIN,
        NOONE,
    }
}
