using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Managers
{
    public static class HydroRenderingManager
    {
        #region Static Fields
        private static readonly Dictionary<int, Callback> callbackList = new Dictionary<int, Callback>();
        #endregion

        #region Static methods
        public static void AddToPostDrawQueue(int queueSpot, Callback drawFunction)
        {
            if (callbackList.ContainsKey(queueSpot) || callbackList.ContainsValue(drawFunction)) { throw new InvalidOperationException("AddToPostDrawQueue fail: draw function (" + queueSpot + ") already added."); }
            callbackList.Add(queueSpot, drawFunction);
            RenderingManager.AddToPostDrawQueue(queueSpot, drawFunction);
#if DEBUG
            Debug.Log("Added a draw function (" + queueSpot + ")");
#endif
        }

        public static void RemoveFromPostDrawQueue(int queueSpot, Callback drawFunction)
        {
            if (!callbackList.ContainsKey(queueSpot)) { throw new InvalidOperationException("RemoveFromPostDrawQueue fail: queue spot (" + queueSpot + ") not found"); }
            if (callbackList[queueSpot] != drawFunction) { throw new InvalidOperationException("RemoveFromPostDrawQueue fail: draw function not matching with queue spot (" + queueSpot + ")"); }
            callbackList.Remove(queueSpot);
            RenderingManager.RemoveFromPostDrawQueue(queueSpot, drawFunction);
#if DEBUG
            Debug.Log("Removed a draw function (" + queueSpot + ")");
#endif
        }

        public static bool Contains(int queueSpot)
        {
            return callbackList.ContainsKey(queueSpot);
        }
        #endregion
    }
}