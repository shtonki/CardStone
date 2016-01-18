using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-us");

            

            GameController.startGame();
        }

        

        private static void xd()
        {
            while (true)
            {
                var h = _420(System.Console.ReadLine(), "0123456789");
                foreach (var v in h) { System.Console.Write("{0:x2} ", v);}
            }

            RSA.generateKeyPairs(3, "keys");

            System.Console.WriteLine("done");
            System.Console.ReadLine();
             
            Environment.Exit(0);
        }

        private static byte[] _420(string pw, string salt)
        {
            if (salt.Length != 10) { throw new Exception("salt must be of length 10");}
            if (pw.Length > 20) { throw new Exception("passwords can't exceed length 20");}

            byte[] bs = new byte[30];
            var x = SHA1.Create();

            var sbs = Encoding.ASCII.GetBytes(salt);

            byte[] xd = Encoding.ASCII.GetBytes(pw.PadLeft(20));

            for (int i = 0; i < 20; i++)
            {
                for (int a = 0; a < 10; a++)
                {
                    bs[a] = sbs[a];
                }
                for (int a = 0; a < 20; a++)
                {
                    bs[10 + a] = xd[a];
                }

                xd = x.ComputeHash(bs);
            }

            return xd;
        }
    }
}
