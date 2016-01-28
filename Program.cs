using System;
using System.Collections.Generic;
using System.Drawing;
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

            while (true)
            {
                string s = Console.ReadLine();
                string[] ss = s.Split(' ').Select(l => l.Trim()).ToArray();
                if (ss[0] == "res")
                {
                    if (ss[1] == "du")
                    {
                        Dictionary<string, bool> d = new Dictionary<string, bool>();

                        foreach (Control v in GUI.getAll())
                        {
                            string[] xx = v.GetType().ToString().Split('.');
                            string x = xx[xx.Length - 1];
                            if (!d.ContainsKey(x)) { d[x] = v is Resolutionable; }
                        }

                        foreach (KeyValuePair<string, bool> v in d)
                        {
                            Console.WriteLine(v.Key.PadRight(30) + v.Value);
                        }
                    }

                    if (ss[1] == "s")
                    {
                        Resolution.set(ss[2], Int32.Parse(ss[3]));

                        ImageLoader.updateResolution();
                        GUI.updateAll();
                    }

                    if (ss[1] == "u")
                    {
                        ImageLoader.updateResolution();
                        GUI.updateAll();
                    }

                    if (ss[1] == "l")
                    {
                        Console.WriteLine(Resolution.currentResolutionx);
                        int i = 0;
                        foreach (var v in Resolution.getAllPairs())
                        {
                            Console.WriteLine("{0}\t{1}{2}", i++, v.Item1.ToString().PadRight(35), v.Item2);
                        }
                    }

                    if (ss[1] == "n")
                    {
                        Resolution.set((ElementDimensions)Int32.Parse(ss[2]), Int32.Parse(ss[3]));

                        ImageLoader.updateResolution();
                        GUI.updateAll();
                    }

                    if (ss[1] == "v")
                    {
                        Resolution.save();
                    }

                    if (ss[1] == "h")
                    {
                        Resolution.currentResolution.scale(3, 4);
                        ImageLoader.updateResolution();
                        GUI.updateAll();
                    }

                    if (ss[1] == "d")
                    {
                        Resolution.currentResolution.scale(4, 3);
                        ImageLoader.updateResolution();
                        GUI.updateAll();
                    }
                }
            }
        }

        

        private static void xd()
        {
            while (true)
            {
                for (int i = 0; i < 10000; i++)
                {
                    Brush b = new SolidBrush(Color.Red);
                    b.Dispose();
                }
                Console.ReadKey();
            }
            /*
            while (true)
            {
                var h = _420(System.Console.ReadLine(), "0123456789");
                foreach (var v in h) { System.Console.Write("{0:x2} ", v);}
            }

            RSA.generateKeyPairs(3, "keys");

            System.Console.WriteLine("done");
            System.Console.ReadLine();
             
            */
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
