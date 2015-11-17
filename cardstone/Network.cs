using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cardstone
{
    class Network
    {
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
                        Console.WriteLine("Connected");
                        return true;
                    }

                case "error":
                    {
                        Console.WriteLine(ss[1]);
                        return false;
                    } break;

                default:
                    {
                        Console.WriteLine("Uknown return {0}", s);
                    } break;
            }

            return true;
        }
    }


}
