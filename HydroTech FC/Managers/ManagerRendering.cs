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

    static public class HydroRenderingManager
    {
        static private Dictionary<int, Callback> CallbackList = new Dictionary<int, Callback>();

        static public void AddToPostDrawQueue(int queueSpot, Callback drawFunction)
        {
            if (CallbackList.ContainsKey(queueSpot) || CallbackList.ContainsValue(drawFunction))
                throw (new Exception("AddToPostDrawQueue fail: draw function (" + queueSpot + ") already added."));
            CallbackList.Add(queueSpot, drawFunction);
            RenderingManager.AddToPostDrawQueue(queueSpot, drawFunction);
#if SHOW_ADD_REMOVE
            print("Added a draw function (" + queueSpot + ")");
#endif
        }

        static public void RemoveFromPostDrawQueue(int queueSpot, Callback drawFunction)
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

        static public bool Contains(int queueSpot)
        {
            return CallbackList.ContainsKey(queueSpot);
        }

#if DEBUG
        static private void print(object message) { GameBehaviours.print(message); }
#endif
    }
}