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
