
namespace stonekart
{
    class Effect
    {
        private Effecter[] effecters;

        public Effect(params Effecter[] es)
        {
            effecters = es;
        }

        public void resolve()
        {
            foreach (Effecter e in effecters)
            {
                e.resolve();
            }
        }
    }

    abstract class Effecter
    {
        abstract public void resolve();
    }

    class Ping : Effecter
    {
        public override void resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
