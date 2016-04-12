using System;
using System.Collections.Generic;

namespace HydroTech.Utils
{
    public static class GuidProvider
    {
        #region Fields
        private static readonly HashSet<int> ids = new HashSet<int>();
        #endregion

        #region Static methods
        public static int GetGuid()
        {
            int id;
            do
            {
                id = Guid.NewGuid().GetHashCode();
            }
            while (!ids.Add(id));

            return id;
        }

        public static void RemoveGuid(int id)
        {
            ids.Remove(id);
        }
        #endregion
    }
}
