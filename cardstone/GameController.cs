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

        public static void newGame()
        {
            Thread t = new Thread(newGameT);
            t.Start();
        }

        private static void newGameT()
        {
            currentGame = new Game(null);
            MainFrame.memesx();
            currentGame.start();
            
        }
    }
}
