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

        //todo(seba) drink bleach
        public Observer getObserver(ThankGodThereIsNoFriendKeywordInThisLanguageOrIWouldntNeedToDoThisNonsense g)
        {
            if (g == null) { throw new Exception("even naughty naughtier"); }
            return observer;
        }


        public void notifyObserver()
        {
            if (observer == null) { return; }
            observer.notifyObserver(this);
        }
    }
}
