using System;
using System.Collections.Generic;
using System.Linq;
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
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string[] getFriends()
        {
            if (friends != null) { return friends; }

            string s = serverConnection.requestFriends();
            string[] ss = s.Split(':');
            if (ss[0] != "friend") { throw new Exception("v bad"); }
            return ss[1].Split(',').TakeWhile(s1 => s1.Length != 0).ToArray();
            
        }
    }


    class ServerConnection : Connection
    {
        private string name;

        public ServerConnection()
            : base("46.239.124.155")
        {

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

        public string requestFriends()
        {
            sendString("friend:" + name);
            return waitForString();
        }
    }


}
