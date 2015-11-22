using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    static class Network
    {
        private static ServerConnection serverConnection;

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
    }


    class ServerConnection : Connection
    {
        private string name;

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
                        System.Console.WriteLine("Connected");
                        start();
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
            string[] ss = Encoding.UTF8.GetString(bs).Split(':');

            switch (ss[0])
            {
                case "told":
                    {
                        MainFrame.getTell(ss[1], ss[2]);
                } break;

                default:
                {
                    System.Console.WriteLine('\'' + ss[0] + '\'');
                } break;
            }
        }

        public string requestFriends()
        {
            sendString("friend");
            return waitForString();
        }
    }

    class GameConnection : Connection
    {
        public GameConnection(Socket s) : base(s)
        {
        }

        public GameConnection()
        {
            //setCallback((connection, bytes) => gotString(bytes), connection => { System.Console.WriteLine("other player disconnected"); });
            //start();
        }

        public virtual bool asHomePlayer()
        {
            return false;
        }

        private void gotString(byte[] bs)
        {
            System.Console.WriteLine(bs);
        }

        public void sendGameEvent(GameEvent e)
        {
            sendString(e.toNetworkString());
        }

        public new virtual void sendString(string s) 
        {
            base.sendString(s);
        }

    }

    class DummyConnection : GameConnection
    {
        public DummyConnection()
        {

        }

        public override bool asHomePlayer()
        {
            return true;
        }


        public override void sendString(String s)
        {
            System.Console.WriteLine(s);
        }

    }
}
