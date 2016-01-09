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

        public static string loggedInAs { get; private set; }

        /// <summary>
        /// Connects to the game server
        /// </summary>
        /// <returns>true if connection was successful false otherwise</returns>
        public static bool connect()
        {
            bool r = true;
            try
            {
                serverConnection = new ServerConnection();
            }
            catch (Exception)
            {
                r = false;
            }

            return r;
        }

        //todo seba make it return an int representing success or cause of error
        /// <summary>
        /// Attempts to login to the game server
        /// </summary>
        /// <param name="name">The username which is to be used to log in</param>
        /// <returns>true on success false otherwise</returns>
        public static bool attemptLogin(string name)
        {
            if (serverConnection == null)
            {
                return false;
            }
            if (name.Length < 2)
            {
                return false;
            }
            if (serverConnection.handshake(name))
            {
                serverConnection.startAsync();
                loggedInAs = name;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a message to another user
        /// </summary>
        /// <param name="user">The user to send the message to</param>
        /// <param name="message">The message to send</param>
        public static void sendTell(string user, string message)
        {
            serverConnection.sendMessage(user, "tell", message);
        }

        /// <summary>
        /// Adds a friend to the currently logged in user's friendlist
        /// </summary>
        /// <param name="s">The username of the friend to add</param>
        public static void addFriend(string s)
        {
            serverConnection.sendMessage(SERVER, "friend", s);
        }
        
        /// <summary>
        /// Removes a friend from the currently logged in user's friendlist
        /// </summary>
        /// <param name="s">The username of the friend to remove</param>
        public static void removeFriend(string s)
        {
            serverConnection.sendMessage(SERVER, "unfriend", s);
        }

        /// <summary>
        /// Doesn't actually challenge the user it just forces them to play a game with you and since you can't concede yet they're going for a ride xd
        /// </summary>
        /// <param name="s">The user to 'challenge'</param>
        public static void challenge(string s)
        {
            serverConnection.sendMessage(SERVER, "requestgame", s);
        }

        /// <summary>
        /// Adds a game connection to the given user
        /// </summary>
        /// <param name="s">The user to set up the game connection with</param>
        /// <param name="c">The game connection</param>
        public static void addGameConnection(string s, GameConnection c)
        {
            gameConnections.Add(s, c);
        }

        /// <summary>
        /// The function to be called when a game message is recieved
        /// </summary>
        /// <param name="user">The name of the user the message came from</param>
        /// <param name="content">The content of the message</param>
        public static void receiveGameMessage(string user, string content)
        {
            if (!gameConnections.ContainsKey(user))
            {
                System.Console.WriteLine("game message from a dead man " + user);
                return;
            }
            gameConnections[user].receiveGameMessage(content);
        }

        /// <summary>
        /// Only to be used by experts
        /// </summary>
        public static void sendRaw(string to, string head, string message)
        {
            serverConnection.sendMessage(to, head, message);
        }
    }


    class ServerConnection : Connection
    {
        public string name { get; private set; }

        /// <summary>
        /// Creates a connection to the server
        /// </summary>
        public ServerConnection()
            : base("155.4.125.53")
        {

        }

        /// <summary>
        /// Starts asynchronously receiving data from the server
        /// </summary>
        public void startAsync()
        {
            startAsync((connection, message) => receiveMessage(message), connection => { System.Console.WriteLine("server krashed"); });
        }

        /// <summary>
        /// Initiates a handshake run with the server
        /// </summary>
        /// <param name="username">The username to use in the handshake</param>
        /// <returns>true on success false otherwise</returns>
        public bool handshake(string username)
        {
            name = username;

            sendMessage(Network.SERVER, "login", name);

            SMessage m = waitForMessage();

            switch (m.header)
            {
                case "validated":
                {
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
                        GUI.showTell(m.from, m.message);
                } break;

                case "startgame":
                {
                    var ss = m.message.Split(',');
                    GameConnection c = new GameConnection(ss[0], ss[1] == "home");
                    Network.addGameConnection(ss[0], c);
                    GameController.newGame(c);

                } break;

                case "error":
                {
                    Console.WriteLine("Error: {0}", m.message);
                } break;

                default:
                {
                    System.Console.WriteLine("borked message: " + m.ToString());
                } break;
            }
             
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

        /// <summary>
        /// Creates a GameConnection with a user
        /// </summary>
        /// <param name="s">The user to connect to</param>
        /// <param name="h">Whether to be the home player or not</param>
        public GameConnection(string s, bool h)
        {
            
            villainName = s;
            home = h;

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

        /// <summary>
        /// Gets the next game action in the buffer and asserts that it is of the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual GameAction demandAction(System.Type t)
        {
            var a = getNextGameEvent();

            if (a.GetType() == t) { return a; }

            throw new Exception("sbunh"); /* swag boys united no hate */
        }
        
    }

    class DummyConnection : GameConnection
    {
        public DummyConnection() : base("", true)
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
