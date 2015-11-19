using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;
//
namespace stonekart
{
    class BoomBox
    {
        public const int
            MACARENA = 0,
            WITCHDOCTOR = 1;

        public static string[] playList = new string[2];

        static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();

        public static void init()
        {
            player = new WMPLib.WindowsMediaPlayer();
            loadAudio();
        }

        private static void loadAudio()
        {
            playList[MACARENA] = "res/AUDIO/Macarena.mp3";
            playList[WITCHDOCTOR] = "res/AUDIO/Witch Doctor.mp3";
        }

        public static void play(int song)
        {
            player.URL = playList[song];
        }
    }
}
