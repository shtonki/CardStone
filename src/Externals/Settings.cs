

using System;
using System.IO;

namespace stonekart
{
    static class Settings
    {
        public static string resFont => @"RES/FONT/";
        public static string resButton => @"RES/IMG/BUTTON/";
        public static string resCard => @"RES/IMG/CARD/";
        public static string resFrame => @"RES/IMG/FRAME/";

        public static string username { get; private set; }


        public static void loadSettings()
        {
            string s;
            if (!File.Exists("settings"))
            {
                var f = File.Create("settings");
                f.Close();
                f.Dispose();
            }
            using (var r = new StreamReader("settings"))
            {
                while (!r.EndOfStream)
                {
                    s = r.ReadLine();
                    string[] ss = s.Split('=');
                    var ls = ss[0].Trim();
                    var rs = ss[1].Trim();

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
}
