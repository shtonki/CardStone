using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public delegate bool Clojurex();

    public class Modifiable<T>
    {
        public delegate T Operator(T l, T r);

        
        private T baseValue;
        private T modifier;
        private List<Housekeeping> hks = new List<Housekeeping>();
        private Operator add;
        private Operator subtract;


        public Modifiable(Operator addition, Operator subtraction)
        {
            this.add = addition;
            this.subtract = subtraction;
        }

        public void setBaseValue(T value)
        {
            baseValue = value;
        }

        public T getValue()
        {
            return add(baseValue, modifier);
        }

        public void addModifier(T modifyBy, Clojurex removeClojure)
        {
            modifier = add(modifier, modifyBy);
            hks.Add(new Housekeeping(modifier, removeClojure));
        }

        public void check()
        {
            for (int i = hks.Count - 1; i >= 0; i--)
            {
                if (hks[i].clojure())
                {
                    modifier = subtract(modifier, hks[i].value);
                    hks.RemoveAt(i);
                }
            }
        }

        public void clear()
        {
            modifier = default(T);
            hks.Clear();
        }

        private struct Housekeeping
        {
            public readonly T value;
            public readonly Clojurex clojure;

            public Housekeeping(T value, Clojurex clojure)
            {
                this.value = value;
                this.clojure = clojure;
            }
        }
    }
}
