using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace stonekart
{
    static class Network
    {
        private static ServerConnection serverConnection;
        private static Dictionary<string, GameConnection> gameConnections = new Dictionary<string, GameConnection>();

        private static string[] friends;

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
                string s = serverConnection.requestFriends();
                string[] ss = s.Split(':');
                if (ss[0] != "friend") { throw new Exception("v bad"); }
                friends =  ss[1].Split(',').TakeWhile(s1 => s1.Length != 0).ToArray();
                serverConnection.start();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string[] getFriends()
        {
            return friends;
        }

        public static void sendTell(string user, string message)
        {
            serverConnection.sendString("tell:" + user + ':' + message);
        }

        public static void addFriend(string s)
        {
            serverConnection.sendString("friend:" + s);
        }

        public static void removeFriend(string s)
        {
            serverConnection.sendString("unfriend:" + s);
        }

        public static void challenge(string s)
        {
            serverConnection.sendString("challenge:" + s);
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

        public static void sendRaw(string s)
        {
            serverConnection.sendString(s);
        }
    }


    class ServerConnection : Connection
    {
        public string name { get; private set; }

        public ServerConnection()
            : base("46.239.124.155")
        {
            setCallback((connection, bytes) => gotString(bytes), connection => { System.Console.WriteLine("server krashed"); });
        }

        public bool handshake(string n)
        {
            name = n;

            sendString(name);

            String s = waitForString();
            string[] ss = s.Split(':');

            switch (ss[0])
            {
                case "handshakeok":
                    {
                        System.Console.WriteLine("Connected as " + n);
                        return true;
                    }

                case "error":
                    {
                        System.Console.WriteLine(ss[1]);
                        return false;
                    } break;

                default:
                    {
                        System.Console.WriteLine("Uknown return {0}", s);
                    } break;
            }

            return true;
        }

        public void gotString(byte[] bs)
        {
            string s = Encoding.UTF8.GetString(bs);
            string[] ss = s.Split(':');

            switch (ss[0])
            {
                case "told":
                {
                        MainFrame.getTell(ss[1], ss[2]);
                } break;

                case "startgame":
                {
                    GameConnection c = new GameConnection(ss[1], ss[2]);
                    Network.addGameConnection(ss[1], c);
                    GameController.newGame(c);
                } break;

                case "game":
                {
                    Network.receiveGameMessage(ss[1], s.Substring(ss[0].Length + ss[1].Length + 2));
                } break;

                default:
                {
                    System.Console.WriteLine('\'' + s + '\'');
                } break;
            }
        }

        public string requestFriends()
        {
            sendString("friend");
            return waitForString();
        }
    }

    public class GameConnection
    {
        public string villainName;
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

        public bool asHomePlayer()
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
            Network.sendRaw(head + ':' + villainName + ':' + content);
        }

        public virtual void sendGameAction(GameAction a)
        {
            send("game", a.toString());
        }

        private GameAction getNextGameEvent()
        {
            if (eventQueue.Count == 0)
            {
                mre.WaitOne();
            }

            smf.WaitOne();
            string r = eventQueue.Dequeue();
            smf.Release();

            return GameAction.fromString(r, game);
        }

        public virtual GameAction demandCastOrPass()
        {
            GameAction a = getNextGameEvent();

            if (a is PassAction || a is CastAction)
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

        public override GameAction demandCastOrPass()
        {
            return new PassAction();
        }

        public override int demandSelection()
        {
            return 0;
        }

        public override CardId[] demandDeck()
        {
            var v = new DeclareDeckAction(new[] {CardId.Kappa, CardId.BearCavalary, CardId.LightningBolt,});
            return ((DeclareDeckAction)GameAction.fromString(v.toString(), null)).getIds();
        }
    }
}
