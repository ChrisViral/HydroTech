#if DEBUG
#define SHOW_ADD_REMOVE
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public static class HydroRenderingManager
    {
        private static Dictionary<int, Callback> CallbackList = new Dictionary<int, Callback>();

        public static void AddToPostDrawQueue(int queueSpot, Callback drawFunction)
        {
            if (CallbackList.ContainsKey(queueSpot) || CallbackList.ContainsValue(drawFunction))
                throw (new Exception("AddToPostDrawQueue fail: draw function (" + queueSpot + ") already added."));
            CallbackList.Add(queueSpot, drawFunction);
            RenderingManager.AddToPostDrawQueue(queueSpot, drawFunction);
#if SHOW_ADD_REMOVE
            print("Added a draw function (" + queueSpot + ")");
#endif
        }

        public static void RemoveFromPostDrawQueue(int queueSpot, Callback drawFunction)
        {
            if (!CallbackList.ContainsKey(queueSpot))
                throw (new Exception("RemoveFromPostDrawQueue fail: queue spot (" + queueSpot + ") not found"));
            if (CallbackList[queueSpot] != drawFunction)
                throw (new Exception("RemoveFromPostDrawQueue fail: draw function not matching with queue spot (" + queueSpot + ")"));
            CallbackList.Remove(queueSpot);
            RenderingManager.RemoveFromPostDrawQueue(queueSpot, drawFunction);
#if SHOW_ADD_REMOVE
            print("Removed a draw function (" + queueSpot + ")");
#endif
        }

        public static bool Contains(int queueSpot)
        {
            return CallbackList.ContainsKey(queueSpot);
        }

#if DEBUG
        private static void print(object message) { GameBehaviours.print(message); }
#endif
    }
}