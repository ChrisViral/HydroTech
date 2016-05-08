using System;
using System.Collections.Generic;

namespace HydroTech.Utils
{
    /// <summary>
    /// This is a helper class providing 32bit integer ID's for GUILayout window purposes, without any collisions, and with a single ID per class
    /// </summary>
    public static class IDProvider
    {
        #region Static fields
        private static readonly HashSet<int> ids = new HashSet<int>();                          //ID cache
        private static readonly Dictionary<Type, int> types = new Dictionary<Type, int>(12);    //Type cache
        #endregion

        #region Static methods
        /// <summary>
        /// Provides a unique, distinct int id, for each type, for GUI window purposes
        /// </summary>
        /// <typeparam name="T">Type to get the id for</typeparam>
        /// <returns></returns>
        public static int GetID<T>()
        {
            Type t = typeof(T);
            int id;
            if (!types.TryGetValue(t, out id))  //Only create one if none exist for this type
            {
                do
                {
                    //Trying to minimize collisions and maintain randomness
                    id = Guid.NewGuid().GetHashCode();
                }
                while (!ids.Add(id));   //Make sure id is unique
                types.Add(t, id);
            }
            return id;
        }
        #endregion
    }
}
