using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    public class SubList<T> : List<T>
    {
        public delegate bool Requirement(T item);

        public SubList(List<T> parent, Requirement req) { parentList = parent; requirement = req; }

        protected List<T> parentList = null;
        protected Requirement requirement = null;

        virtual public void OnUpdate()
        {
            Clear();
            foreach (T item in parentList)
                if (requirement(item))
                    Add(item);
        }
    }
}
