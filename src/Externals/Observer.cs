﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public interface Observer
    {
        void notifyObserver(Observable observed, object args);
    }
}
