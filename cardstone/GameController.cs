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

        public static void start()
        {
            Network.connect();
            MainFrame.login();
        }

        public static void newGame()
        {
            Thread t = new Thread(newGameT);
            t.Start();
        }

        private static void newGameT()
        {
            MainFrame.transitionToGame();
            currentGame = new Game(null);
            currentGame.start();
            
        }
    }
}
