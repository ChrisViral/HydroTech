using System;
using System.Collections.Generic;

namespace HydroTech.Utils
{
    public static class GuidProvider
    {
        #region Static fields
        private static readonly HashSet<int> ids = new HashSet<int>();
        private static readonly Dictionary<Type, int> types = new Dictionary<Type, int>(10); 
        #endregion

        #region Static methods
        public static int GetGuid<T>()
        {
            Type t = typeof(T);
            int id;
            if (!types.TryGetValue(t, out id))
            {
                do
                {
                    id = Guid.NewGuid().GetHashCode();
                }
                while (!ids.Add(id));
                types.Add(t, id);
            }
            return id;
        }
        #endregion
    }
}
