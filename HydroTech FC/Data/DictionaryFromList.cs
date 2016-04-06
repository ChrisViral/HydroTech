using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    public class DictionaryFromList<T, U> : Dictionary<T, U>
        where U : struct
    {
        public DictionaryFromList(List<T> keys, U defVal = default(U)) { keyList = keys; defaultValue = defVal; }

        protected List<T> keyList = null;
        protected U defaultValue = new U();

        virtual public void OnUpdate()
        {
            foreach (T item in keyList)
                if (!ContainsKey(item))
                    Add(item, defaultValue);
            List<T> keysToRemove = new List<T>();
            foreach (T item in Keys)
                if (!keyList.Contains(item))
                    keysToRemove.Add(item);
            foreach (T item in keysToRemove)
                Remove(item);
        }
    }
}