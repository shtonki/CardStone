namespace stonekart
{
    public class EventHandler
    {
        public delegate void eventHandler(GameEvent e);

        public int type;
        private eventHandler main, pre, post;

        public EventHandler(int type, eventHandler e)
        {
            this.type = type;
            main = e;
        }

        public void invoke(GameEvent e)
        {
            if (type == e.getType())
            {
                main(e);
            }
        }
    }
}