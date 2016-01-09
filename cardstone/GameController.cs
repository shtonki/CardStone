using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace stonekart
{
    static class GameController
    {
        public static Game currentGame { get; private set; }

        public static void startGame()
        {
            setup();

            start();
        }

        private static void setup()
        {
            Settings.loadSettings();

            ImageLoader.init();
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
            GUI.transitionToGame();
            currentGame = new Game(c);
            currentGame.start();
            
        }
    }
}
