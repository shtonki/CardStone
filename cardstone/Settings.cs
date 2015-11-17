

using System;
using System.IO;

namespace stonekart
{
    static class Settings
    {
        public static string username { get; private set; }


        public static void loadSettings()
        {
            var f = File.Open("settings", FileMode.OpenOrCreate);
            var r = new StreamReader(f);

            string s;

            while ((s = r.ReadLine()) != null)
            {
                string ls, rs;
                string[] ss = s.Split('=');
                ls = ss[0].Trim();
                rs = ss[1].Trim();

                switch (ls)
                {
                    case "username":
                    {
                        username = rs;
                    } break;

                    default:
                    {
                        System.Console.WriteLine("bad setting: {0}", s);
                    } break;
                }
            }
        }
    }
}
