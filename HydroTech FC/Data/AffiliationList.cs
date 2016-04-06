using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    public class AffiliationList<T, U> : List<U>
    {
        public delegate U GetItemFunction_Single(T t);
        public delegate List<U> GetItemFunction_Multi(T t);
        public delegate bool Requirement(T t);

        public AffiliationList() { }
        public AffiliationList(List<T> parent, GetItemFunction_Single get, Requirement req = null)
        {
            parentList = parent;
            single = true;
            getItem_s = get;
            requirement = req;
        }
        public AffiliationList(List<T> parent, GetItemFunction_Multi get, Requirement req = null)
        {
            parentList = parent;
            single = false;
            getItem_m = get;
            requirement = req;
        }

        protected List<T> parentList = null;
        protected bool single = true;
        protected GetItemFunction_Single getItem_s = null;
        protected GetItemFunction_Multi getItem_m = null;
        protected Requirement requirement = null;

        public void SetParent(List<T> parent) { parentList = parent; }

        public void SetGetFunction(GetItemFunction_Single get)
        {
            single = true;
            getItem_s = get;
        }
        public void SetGetFunction(GetItemFunction_Multi get)
        {
            single = false;
            getItem_m = get;
        }

        public virtual void OnUpdate()
        {
            Clear();
            if (parentList == null)
                throw new Exception("AffiliationList cannot update: parent list is null.");
            if ((single && getItem_s == null) || (!single && getItem_m == null))
                throw new Exception("AffiliationList cannot update: get function is null.");
            foreach (T t in parentList)
            {
                if (requirement != null && !requirement(t))
                    continue;
                if (single)
                {
                    U u = getItem_s(t);
                    if (!Contains(u))
                        Add(u);
                }
                else
                {
                    foreach (U u in getItem_m(t))
                        if (!Contains(u))
                            Add(u);
                }
            }
        }
    }
}