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
        private List<Observer> observers;

        public Observable()
        {
            observers = new List<Observer>();
        }

        public void addObserver(Observer o)
        {
            observers.Add(o);
            o.notifyObserver(this, null);
        }

        public void removeObserver(Observer o)
        {
            observers.Remove(o);
        }

        public List<Observer> getObservers()
        {
            return observers;
        }


        public void notifyObservers(object arg = null)
        {
            if (observers == null) { return; }
            var inpt = arg ?? this;
            try
            {
                foreach (Observer o in observers)
                {
                    o.notifyObserver(this, inpt);
                }
            }
            catch (Exception)
            {
                notifyObservers(arg);
            }
        }
    }
}
