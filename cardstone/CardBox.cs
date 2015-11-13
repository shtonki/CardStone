using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    class CardBox : Panel, Foo, Observer
    {
        private CardButton[] buttons;

        public CardBox()
        {
        }

        public void notifyObserver(Observable o)
        {
            throw new NotImplementedException();
        }
    }
}
