using System.Collections.Generic;
using System.Linq;

namespace HydroTech_FC
{
    public class HydroPartListBase<T>
    {
        public List<T> listActiveVessel = new List<T>();
        public List<T> listInactiveVessel = new List<T>();

        protected static Vessel ActiveVessel
        {
            get { return GameStates.ActiveVessel; }
        }

        public virtual int CountActive
        {
            get { return CountList(this.listActiveVessel); }
        }

        public virtual int CountInactive
        {
            get { return CountList(this.listInactiveVessel); }
        }

        public int Count
        {
            get { return this.CountActive + this.CountInactive; }
        }

        public virtual T FirstActive
        {
            get { return FirstOrDefault(this.listActiveVessel); }
        }

        public virtual T FirstInactive
        {
            get { return FirstOrDefault(this.listInactiveVessel); }
        }

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

        protected virtual void AddItemActive(T item)
        {
            AddItem(this.listActiveVessel, item);
        }

        protected virtual void AddItemInactive(T item)
        {
            AddItem(this.listInactiveVessel, item);
        }

        public void Add(T item)
        {
            if (GetVessel(item) == ActiveVessel) { AddItemActive(item); }
            else
            { AddItemInactive(item); }
        }

        protected virtual void RemoveItem(List<T> list, T item)
        {
            list.Remove(item);
        }

        protected virtual void RemoveItemActive(T item)
        {
            RemoveItem(this.listActiveVessel, item);
        }

        protected virtual void RemoveItemInactive(T item)
        {
            RemoveItem(this.listInactiveVessel, item);
        }

        public void Remove(T item)
        {
            if (ContainsItem(this.listActiveVessel, item)) { RemoveItemActive(item); }
            else if (ContainsItem(this.listInactiveVessel, item)) { RemoveItemInactive(item); }
        }

        protected virtual bool ContainsItem(List<T> list, T item)
        {
            return list.Contains(item);
        }

        protected virtual bool ContainsItemActive(T item)
        {
            return ContainsItem(this.listActiveVessel, item);
        }

        protected virtual bool ContainsItemInactive(T item)
        {
            return ContainsItem(this.listInactiveVessel, item);
        }

        public bool Contains(T item)
        {
            return ContainsItemActive(item) || ContainsItemInactive(item);
        }

        protected virtual int CountList(List<T> list)
        {
            return list.Count;
        }

        protected virtual T FirstOrDefault(List<T> list)
        {
            return list.FirstOrDefault();
        }

        public bool HasItemOnVessel(Vessel vessel)
        {
            if (vessel == ActiveVessel) { return CountList(this.listActiveVessel) != 0; }
            foreach (T item in this.listInactiveVessel) { if (GetVessel(item) == vessel) { return true; } }
            return false;
        }

        public void OnStart()
        {
            bool clearAll = false;
            foreach (T item in this.listActiveVessel)
            {
                if (IsNull(item))
                {
                    clearAll = true;
                    break;
                }
            }
            if (clearAll) { goto CLEARALL; }
            foreach (T item in this.listInactiveVessel)
            {
                if (IsNull(item))
                {
                    clearAll = true;
                    break;
                }
            }
            if (!clearAll) { return; }
            CLEARALL:
            this.listActiveVessel.Clear();
            this.listInactiveVessel.Clear();
        }

        protected virtual bool NeedsToRemove(T item)
        {
            return item == null;
        }

        protected virtual bool NeedsToRemoveActive(T item)
        {
            return NeedsToRemove(item);
        }

        protected virtual bool NeedsToRemoveInactive(T item)
        {
            return NeedsToRemove(item);
        }

        protected virtual bool NeedsToMoveActive(T item)
        {
            return GetVessel(item) == ActiveVessel;
        }

        protected virtual bool NeedsToMoveInactive(T item)
        {
            return GetVessel(item) != ActiveVessel;
        }

        protected virtual void MoveItemToActive(T item)
        {
            this.listInactiveVessel.Remove(item);
            this.listActiveVessel.Add(item);
        }

        protected virtual void MoveItemToInactive(T item)
        {
            this.listActiveVessel.Remove(item);
            this.listInactiveVessel.Add(item);
        }

        public void OnUpdate()
        {
            List<T> partsToRemoveActive = new List<T>();
            List<T> partsToRemoveInactive = new List<T>();
            List<T> partsToMoveInactive = new List<T>();
            List<T> partsToMoveActive = new List<T>();
            foreach (T item in this.listActiveVessel)
            {
                if (NeedsToRemoveActive(item))
                {
                    partsToRemoveActive.Add(item);
                    continue;
                }
                if (NeedsToMoveInactive(item)) { partsToMoveInactive.Add(item); }
            }
            foreach (T item in this.listInactiveVessel)
            {
                if (NeedsToRemoveInactive(item))
                {
                    partsToRemoveInactive.Add(item);
                    continue;
                }
                if (NeedsToMoveActive(item)) { partsToMoveActive.Add(item); }
            }
            foreach (T item in partsToRemoveActive) { RemoveItem(this.listActiveVessel, item); }
            foreach (T item in partsToRemoveInactive) { RemoveItem(this.listInactiveVessel, item); }
            foreach (T item in partsToMoveActive) { MoveItemToActive(item); }
            foreach (T item in partsToMoveInactive) { MoveItemToInactive(item); }
        }
    }

    public class HydroPartList : HydroPartListBase<Part>
    {
        protected override Vessel GetVessel(Part item)
        {
            return item.vessel;
        }
    }

    public class HydroPartModuleList : HydroPartListBase<HydroPartModule>
    {
        protected override bool IsNull(HydroPartModule item)
        {
            return item.part == null;
        }

        protected override Vessel GetVessel(HydroPartModule item)
        {
            return item.vessel;
        }

        protected override void AddItem(List<HydroPartModule> list, HydroPartModule item)
        {
            base.AddItem(list, item);
            item.OnFlightStart();
        }

        protected override void RemoveItem(List<HydroPartModule> list, HydroPartModule item)
        {
            base.RemoveItem(list, item);
            item.OnDestroy();
        }

        protected override bool NeedsToRemove(HydroPartModule item)
        {
            return item.part == null;
        }
    }
}