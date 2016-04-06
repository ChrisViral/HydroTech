using System.Collections.Generic;

namespace HydroTech_FC
{
    public class DictionaryFromList<T, TU> : Dictionary<T, TU> where TU : struct
    {
        protected TU defaultValue;

        protected List<T> keyList;

        public DictionaryFromList(List<T> keys, TU defVal = default(TU))
        {
            this.keyList = keys;
            this.defaultValue = defVal;
        }

        public virtual void OnUpdate()
        {
            foreach (T item in this.keyList) { if (!ContainsKey(item)) { Add(item, this.defaultValue); } }
            List<T> keysToRemove = new List<T>();
            foreach (T item in this.Keys) { if (!this.keyList.Contains(item)) { keysToRemove.Add(item); } }
            foreach (T item in keysToRemove) { Remove(item); }
        }
    }
}