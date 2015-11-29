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

        public static void newGame(GameConnection c)
        {
            Thread t = new Thread(new ParameterizedThreadStart(newGameT));
            t.Start(c);
        }

        private static void newGameT(object o)
        {
            GameConnection c = (GameConnection)o;
            MainFrame.transitionToGame();
            currentGame = new Game(c);
            currentGame.start();
            
        }
    }
}
