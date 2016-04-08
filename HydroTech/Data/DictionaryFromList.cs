using System.Collections.Generic;

namespace HydroTech.Data
{
    public class DictionaryFromList<T, TU> : Dictionary<T, TU> where TU : struct
    {
        #region Fields
        protected TU defaultValue;
        protected List<T> keyList;
        #endregion

        #region Constructors
        public DictionaryFromList(List<T> keys, TU defVal = default(TU))
        {
            this.keyList = keys;
            this.defaultValue = defVal;
        }
        #endregion

        #region Virtual methods
        public virtual void Update()
        {
            foreach (T item in this.keyList)
            {
                if (!ContainsKey(item)) { Add(item, this.defaultValue); }
            }
            foreach (T item in this.Keys)
            {
                if (!this.keyList.Contains(item)) { Remove(item);}
            }
        }
        #endregion
    }
}