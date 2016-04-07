using System;
using System.Collections.Generic;

namespace HydroTech_FC
{
    public class AffiliationList<T, TU> : List<TU>
    {
        protected GetItemFunctionMulti getItemM;
        protected GetItemFunctionSingle getItemS;

        protected List<T> parentList;
        protected Requirement requirement;
        protected bool single = true;

        public delegate List<TU> GetItemFunctionMulti(T t);

        public delegate TU GetItemFunctionSingle(T t);

        public delegate bool Requirement(T t);

        public AffiliationList() { }

        public AffiliationList(List<T> parent, GetItemFunctionSingle get, Requirement req = null)
        {
            this.parentList = parent;
            this.single = true;
            this.getItemS = get;
            this.requirement = req;
        }

        public AffiliationList(List<T> parent, GetItemFunctionMulti get, Requirement req = null)
        {
            this.parentList = parent;
            this.single = false;
            this.getItemM = get;
            this.requirement = req;
        }

        public void SetParent(List<T> parent)
        {
            this.parentList = parent;
        }

        public void SetGetFunction(GetItemFunctionSingle get)
        {
            this.single = true;
            this.getItemS = get;
        }

        public void SetGetFunction(GetItemFunctionMulti get)
        {
            this.single = false;
            this.getItemM = get;
        }

        public virtual void OnUpdate()
        {
            Clear();
            if (this.parentList == null) { throw new Exception("AffiliationList cannot update: parent list is null."); }
            if (this.single && this.getItemS == null || !this.single && this.getItemM == null) { throw new Exception("AffiliationList cannot update: get function is null."); }
            foreach (T t in this.parentList)
            {
                if (this.requirement != null && !this.requirement(t)) { continue; }
                if (this.single)
                {
                    TU u = this.getItemS(t);
                    if (!Contains(u)) { Add(u); }
                }
                else
                {
                    foreach (TU u in this.getItemM(t)) { if (!Contains(u)) { Add(u); } }
                }
            }
        }
    }
}