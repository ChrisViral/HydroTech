using System.Collections.Generic;
using System.Linq;

namespace HydroTech.Data
{
    public class HydroPartListBase<T>
    {
        #region Fields
        protected readonly List<T> listActiveVessel = new List<T>();
        public List<T> ListActiveVessel
        {
            get { return this.listActiveVessel; }
        }

        protected readonly List<T> listInactiveVessel = new List<T>();
        public List<T> ListInactiveVessel
        {
            get { return this.listInactiveVessel; }
        } 
        #endregion

        #region Properties
        public int Count
        {
            get { return this.ListActiveVessel.Count + this.ListInactiveVessel.Count; }
        }

        public virtual int CountActive
        {
            get { return this.ListActiveVessel.Count; }
        }

        public virtual int CountInactive
        {
            get { return this.ListInactiveVessel.Count; }
        }

        public virtual T FirstActive
        {
            get { return this.ListActiveVessel.Count > 0 ? this.ListActiveVessel[0] : default(T); }
        }

        public virtual T FirstInactive
        {
            get { return this.ListInactiveVessel.Count > 0 ? this.ListInactiveVessel[0] : default(T); }
        }
        #endregion

        #region Methods
        public void Add(T item)
        {
            AddItem(GetVessel(item) == FlightGlobals.ActiveVessel ? this.ListActiveVessel : this.ListInactiveVessel, item);
        }

        public void Remove(T item)
        {
            if (this.ListActiveVessel.Contains(item)) { RemoveItem(this.ListActiveVessel, item); }
            else if (this.ListInactiveVessel.Contains(item)) { RemoveItem(this.ListInactiveVessel, item); }
        }

        public bool Contains(T item)
        {
            return this.ListActiveVessel.Contains(item) || this.ListInactiveVessel.Contains(item);
        }     

        protected void MoveItemToActive(T item)
        {
            this.ListInactiveVessel.Remove(item);
            this.ListActiveVessel.Add(item);
        }

        protected void MoveItemToInactive(T item)
        {
            this.ListActiveVessel.Remove(item);
            this.ListInactiveVessel.Add(item);
        }

        public void Initialize()
        {
            if (this.ListActiveVessel.Any(IsNull) || this.ListInactiveVessel.Any(IsNull))
            {
                this.ListActiveVessel.Clear();
                this.ListInactiveVessel.Clear();
            }
        }

        public void Update()
        {
            List<T> partsToRemoveActive = new List<T>(), partsToRemoveInactive = new List<T>();
            List<T> partsToMoveActive = new List<T>(), partsToMoveInactive = new List<T>();
            foreach (T item in this.ListActiveVessel)
            {
                if (NeedsToRemove(item)) { partsToRemoveActive.Add(item); }
                else if (GetVessel(item) != FlightGlobals.ActiveVessel) { partsToMoveInactive.Add(item); }
            }
            foreach (T item in this.ListInactiveVessel)
            {
                if (NeedsToRemove(item)) { partsToRemoveInactive.Add(item); }
                else if (GetVessel(item) != FlightGlobals.ActiveVessel) { partsToMoveActive.Add(item); }
            }

            partsToRemoveActive.ForEach(i => RemoveItem(this.ListActiveVessel, i));
            partsToRemoveInactive.ForEach(i => RemoveItem(this.ListInactiveVessel, i));
            partsToMoveActive.ForEach(MoveItemToActive);
            partsToMoveInactive.ForEach(MoveItemToInactive);
        }
        #endregion

        #region Virtual Methods
        protected virtual bool IsNull(T item)
        {
            return item == null;
        }

        protected virtual Vessel GetVessel(T item)
        {
            return null;
        }

        protected virtual void AddItem(List<T> list, T item)
        {
            list.Add(item);
        }

        protected virtual void RemoveItem(List<T> list, T item)
        {
            list.Remove(item);
        }

        protected virtual bool NeedsToRemove(T item)
        {
            return item == null;
        }
        #endregion
    }
}