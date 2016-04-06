using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    public class HydroPartListBase<T>
    {
        public List<T> listActiveVessel = new List<T>();
        public List<T> listInactiveVessel = new List<T>();

        virtual protected bool IsNull(T item) { return item == null; }

        static protected Vessel ActiveVessel { get { return GameStates.ActiveVessel; } }
        virtual protected Vessel GetVessel(T item) { return null; }

        virtual protected void AddItem(List<T> list, T item) { list.Add(item); }
        virtual protected void AddItemActive(T item) { AddItem(listActiveVessel, item); }
        virtual protected void AddItemInactive(T item) { AddItem(listInactiveVessel, item); }
        public void Add(T item)
        {
            if (GetVessel(item) == ActiveVessel)
                AddItemActive(item);
            else
                AddItemInactive(item);
        }

        virtual protected void RemoveItem(List<T> list, T item) { list.Remove(item); }
        virtual protected void RemoveItemActive(T item) { RemoveItem(listActiveVessel, item); }
        virtual protected void RemoveItemInactive(T item) { RemoveItem(listInactiveVessel, item); }
        public void Remove(T item)
        {
            if (ContainsItem(listActiveVessel, item))
                RemoveItemActive(item);
            else if (ContainsItem(listInactiveVessel, item))
                RemoveItemInactive(item);
        }

        virtual protected bool ContainsItem(List<T> list, T item) { return list.Contains(item); }
        virtual protected bool ContainsItemActive(T item) { return ContainsItem(listActiveVessel, item); }
        virtual protected bool ContainsItemInactive(T item) { return ContainsItem(listInactiveVessel, item); }
        public bool Contains(T item) { return ContainsItemActive(item) || ContainsItemInactive(item); }

        virtual protected int CountList(List<T> list) { return list.Count; }
        virtual public int CountActive { get { return CountList(listActiveVessel); } }
        virtual public int CountInactive { get { return CountList(listInactiveVessel); } }
        public int Count { get { return CountActive + CountInactive; } }

        virtual protected T FirstOrDefault(List<T> list) { return list.FirstOrDefault(); }
        virtual public T FirstActive { get { return FirstOrDefault(listActiveVessel); } }
        virtual public T FirstInactive { get { return FirstOrDefault(listInactiveVessel); } }

        public bool HasItemOnVessel(Vessel vessel)
        {
            if (vessel == ActiveVessel)
                return CountList(listActiveVessel) != 0;
            else
            {
                foreach (T item in listInactiveVessel)
                    if (GetVessel(item) == vessel)
                        return true;
                return false;
            }
        }

        public void OnStart()
        {
            bool clearAll = false;
            foreach (T item in listActiveVessel)
                if (IsNull(item))
                {
                    clearAll = true;
                    break;
                }
            if (clearAll)
                goto CLEARALL;
            foreach(T item in listInactiveVessel)
                if (IsNull(item))
                {
                    clearAll = true;
                    break;
                }
            if (!clearAll)
                return;
        CLEARALL:
            listActiveVessel.Clear();
            listInactiveVessel.Clear();
        }

        virtual protected bool NeedsToRemove(T item) { return item == null; }
        virtual protected bool NeedsToRemoveActive(T item) { return NeedsToRemove(item); }
        virtual protected bool NeedsToRemoveInactive(T item) { return NeedsToRemove(item); }
        virtual protected bool NeedsToMoveActive(T item) { return GetVessel(item) == ActiveVessel; }
        virtual protected bool NeedsToMoveInactive(T item) { return GetVessel(item) != ActiveVessel; }
        virtual protected void MoveItemToActive(T item)
        {
            listInactiveVessel.Remove(item);
            listActiveVessel.Add(item);
        }
        virtual protected void MoveItemToInactive(T item)
        {
            listActiveVessel.Remove(item);
            listInactiveVessel.Add(item);
        }
        public void OnUpdate()
        {
            List<T> partsToRemoveActive = new List<T>();
            List<T> partsToRemoveInactive = new List<T>();
            List<T> partsToMoveInactive = new List<T>();
            List<T> partsToMoveActive = new List<T>();
            foreach (T item in listActiveVessel)
            {
                if (NeedsToRemoveActive(item))
                {
                    partsToRemoveActive.Add(item);
                    continue;
                }
                if (NeedsToMoveInactive(item))
                    partsToMoveInactive.Add(item);
            }
            foreach (T item in listInactiveVessel)
            {
                if (NeedsToRemoveInactive(item))
                {
                    partsToRemoveInactive.Add(item);
                    continue;
                }
                if (NeedsToMoveActive(item))
                    partsToMoveActive.Add(item);
            }
            foreach (T item in partsToRemoveActive)
                RemoveItem(listActiveVessel, item);
            foreach (T item in partsToRemoveInactive)
                RemoveItem(listInactiveVessel, item);
            foreach (T item in partsToMoveActive)
                MoveItemToActive(item);
            foreach (T item in partsToMoveInactive)
                MoveItemToInactive(item);
        }
    }

    public class HydroPartList : HydroPartListBase<Part>
    {
        protected override Vessel GetVessel(Part item) { return item.vessel; }
    }

    public class HydroPartModuleList : HydroPartListBase<HydroPartModule>
    {
        protected override bool IsNull(HydroPartModule item) { return item.part == null; }
        protected override Vessel GetVessel(HydroPartModule item) { return item.vessel; }

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
        protected override bool NeedsToRemove(HydroPartModule item) { return item.part == null; }
    }
}