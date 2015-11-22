﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
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
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-us");

            Settings.loadSettings();

            //FontLoader.init();

            //Form.CheckForIllegalCrossThreadCalls = false;
            Thread t = new Thread(new ThreadStart(asd));
            t.Start();
            
            ImageLoader.init();
            FontLoader.init();

            MainFrame.x.WaitOne();

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
