using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace stonekart
{
    static class CentralShitterModule
    {
        public static void startGame()
        {
            setup();

            start();
        }

        private static void setup()
        {
            Settings.loadSettings();

            FontLoader.init();

            GUI.createFrame();
        }

        private static void start()
        {
            bool connected = Network.connect();
            
            GUI.transitionToMainMenu();

            if (connected)
            {
                Console.WriteLine("Connected to server");
                if (GUI.login())
                {
                    Console.WriteLine("Logged in as {0}", Network.loggedInAs);
                }
                else
                {
                    Console.WriteLine("Not logged in");
                }
            }
            else
            {
                Console.WriteLine("Couldn't connect to server");
            }
            
            GUI.showPlayPanel();
        }

        public static void newGame(GameConnection c)
        {
            Thread t = new Thread(newGameT);
            t.Start(c);
        }

        private static void newGameT(object o)
        {
            GameConnection c = (GameConnection)o;
            GameInterface gi = new GameInterface();
            GUI.addGameWindow(gi);
            gi.connection = c;
            GameController g = new GameController(c, gi);
            gi.setGame(g);
            GUI.transitionToGame(gi);
            g.start(c.asHomePlayer());
        }
    }
}
