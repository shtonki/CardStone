namespace stonekart
{
    class Effect
    {
        private Effecter[] effecters;

        public Effect(params Effecter[] es)
        {
            effecters = es;
        }

        public void resolve(Card c)
        {
            foreach (Effecter e in effecters)
            {
                e.resolve(c);
            }
        }
    }

    abstract class Effecter
    {
        abstract public void resolve(Card c);
    }

    class StackResolve : Effecter
    {
        public override void resolve(Card c)
        {
            if (c.getType() == Type.Creature || c.getType() == Type.Relic)
            {
                c.moveTo(c.getController().getField());
            }
            else
            {
                c.moveTo(c.getOwner().getGraveyard());
            }
        }
    }
}