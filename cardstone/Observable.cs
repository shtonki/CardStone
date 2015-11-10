using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public abstract class Observable
    {
        private Observer observer;

        public void setObserver(Observer o)
        {
            observer = o;
            notifyObserver();
        }

        public void clearObserver()
        {
            observer = null;
            observer.notifyObserver(null);
        }

        public void notifyObserver()
        {
            if (observer == null) { return; }
            observer.notifyObserver(this);
        }
    }
}
