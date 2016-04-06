using System.Collections.Generic;

namespace HydroTech_FC
{
    public class SubList<T> : List<T>
    {
        protected List<T> parentList;
        protected Requirement requirement;

        public delegate bool Requirement(T item);

        public SubList(List<T> parent, Requirement req)
        {
            this.parentList = parent;
            this.requirement = req;
        }

        public virtual void OnUpdate()
        {
            Clear();
            foreach (T item in this.parentList) { if (this.requirement(item)) { Add(item); } }
        }
    }
}