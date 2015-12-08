using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace stonekart
{
    static class Network
    {
        public const string SERVER = "server";

        private static ServerConnection serverConnection;
        private static Dictionary<string, GameConnection> gameConnections = new Dictionary<string, GameConnection>();

        public static void connect()
        {
            serverConnection = new ServerConnection();
        }

        public static bool login(string name)
        {
            if (name.Length < 2)
            {
                return false;
            }
            if (serverConnection.handshake(name))
            {
                serverConnection.startAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void sendTell(string user, string message)
        {
            serverConnection.sendMessage(user, "tell", message);
        }

        public static void addFriend(string s)
        {
            serverConnection.sendMessage(SERVER, "friend", s);
        }

        public static void removeFriend(string s)
        {
            serverConnection.sendMessage(SERVER, "unfriend", s);
        }

        public static void challenge(string s)
        {
            serverConnection.sendMessage(s, "challenge", "");
        }

        public static void addGameConnection(string s, GameConnection c)
        {
            gameConnections.Add(s, c);
        }

        public static void receiveGameMessage(string user, string content)
        {
            if (!gameConnections.ContainsKey(user))
            {
                System.Console.WriteLine("game message from a dead man " + user);
                return;
            }
            gameConnections[user].receiveGameMessage(content);
        }

        public static void sendRaw(string to, string head, string message)
        {
            serverConnection.sendMessage(to, head, message);
        }
    }


    class ServerConnection : Connection
    {
        public string name { get; private set; }

        public ServerConnection()
            : base("46.239.124.155")
        {

        }

        public void startAsync()
        {
            startAsync((connection, message) => receiveMessage(message), connection => { System.Console.WriteLine("server krashed"); });
        }

        public bool handshake(string n)
        {
            name = n;

            sendMessage(Network.SERVER, "login", name);

            SMessage m = waitForMessage();

            switch (m.header)
            {
                case "validated":
                {
                    System.Console.WriteLine("Connected as " + n);
                    return true;
                } break;

                case "error":
                {
                    System.Console.WriteLine(m.message);
                    return false;
                } break;

                default:
                {
                    System.Console.WriteLine("Uknown return {0}", m.header);
                } break;
            }

            return true;
        }

        public void receiveMessage(SMessage m)
        {
            System.Console.WriteLine(m.ToString());
            
            switch (m.header)
            {
                case "game":
                {
                    Network.receiveGameMessage(m.from, m.message);
                } break;

                case "tell":
                {
                        MainFrame.getTell(m.from, m.message);
                } break;

                case "startgame":
                {
                    var ss = m.message.Split(',');
                    GameConnection c = new GameConnection(ss[0], ss[1]);
                    Network.addGameConnection(ss[0], c);
                    GameController.newGame(c);

                } break;

                default:
                {
                    System.Console.WriteLine("borked message: " + m.ToString());
                } break;
            }
             
        }

        public string requestFriends()
        {
            sendMessage(Network.SERVER, "friend", "");
            var v = waitForMessage();
            return v.message;
        }

        public void sendMessage(string user, string header, string message)
        {
            base.sendMessage(new SMessage(user, name, header, message));
        }
    }

    public class GameConnection
    {
        public readonly string villainName;
        private bool home;

        protected Queue<string> eventQueue;

        private Semaphore smf;
        private AutoResetEvent mre;
        private Game game;

        public GameConnection(string s, string h)
        {
            
            villainName = s;
            home = h == "home";

            eventQueue = new Queue<string>();
            mre = new AutoResetEvent(false);
            smf = new Semaphore(1, 1);
        }

        public void setGame(Game g)
        {
            //todo this really isn't pretty
            game = g;
        }

        public virtual bool asHomePlayer()
        {
            return home;
        }

        public void receiveGameMessage(string message)
        {
            smf.WaitOne();
            eventQueue.Enqueue(message);
            mre.Set();
            
            smf.Release();
        }

        private void send(string head, string content)
        {
            Network.sendRaw(villainName, head, content);
        }

        public virtual void sendGameAction(GameAction a)
        {
            send("game", a.toString());
        }

        private GameAction getNextGameEvent()
        {
            while (eventQueue.Count == 0)
            {
                mre.WaitOne();
            }

            smf.WaitOne();
            string r = eventQueue.Dequeue();
            smf.Release();

            return GameAction.fromString(r, game);
        }

        public virtual GameAction demandAction(System.Type t)
        {
            var a = getNextGameEvent();

            if (a.GetType() == t) { return a; }

            throw new Exception("sbunh");
        }
        /*
        public virtual GameAction demandCastOrPass()
        {
            GameAction a = getNextGameEvent();

            if (a is CastAction)
            {
                return a;
            }
            else
            {
                throw new Exception("demanded cast or pass and got " + a.GetType());
            }
        }

        public virtual int demandSelection()
        {

            var v = getNextGameEvent();
            if (v is SelectAction) { return ((SelectAction)v).getSelection(); }
            throw new Exception("really bad");
        }

        public virtual CardId[] demandDeck()
        {
            return ((DeclareDeckAction)getNextGameEvent()).getIds();
        }
         */
    }

    class DummyConnection : GameConnection
    {
        public DummyConnection() : base("", "")
        {
        }

        public override void sendGameAction(GameAction e)
        {
            System.Console.WriteLine(">" + e.toString());
        }

        public override GameAction demandAction(System.Type t)
        {
            if (t == typeof(CastAction))
            {
                return new CastAction();
            }

            if (t == typeof(MultiSelectAction))
            {
                return new MultiSelectAction();
            }

            if (t == typeof(DeclareDeckAction))
            {
                return new DeclareDeckAction();
            }

            if (t == typeof(SelectAction))
            {
                return new SelectAction(0);
            }

            throw new NotImplementedException("oops xdd");
        }

        public override bool asHomePlayer()
        {
            return true;
        }
    }
}
