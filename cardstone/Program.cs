using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
            Settings.loadSettings();

            //FontLoader.init();

            Form.CheckForIllegalCrossThreadCalls = false;
            Thread t = new Thread(new ThreadStart(asd));
            t.Start();
            
            ImageLoader.init();
            FontLoader.init();
            Thread.Sleep(100);  //yeah nah
            GameController.start();
        }

        private static void asd()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrame());
        }
    }
}
