using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydroTech.Managers
{
    public static class HydroFlightInputManager
    {
        public struct HydroFlightInputCallback
        {
            #region Fields
            public FlightInputCallback callback;
            public string nameString;
            public Vessel vessel;
            #endregion

            #region Constructor
            public HydroFlightInputCallback(Vessel v, string str, FlightInputCallback cb)
            {
                this.vessel = v;
                this.nameString = str;
                this.callback = cb;
            }
            #endregion
        }

        private class HydroFlightInputHandler
        {
            #region Fields
            public readonly Dictionary<string, FlightInputCallback> flightInputList;
            public bool isDestroyed;
            public readonly Vessel vessel;
            public Part vesselRootPart;
            #endregion

            #region Constructor
            public HydroFlightInputHandler(Vessel v)
            {
                this.flightInputList = new Dictionary<string, FlightInputCallback>();
                this.vessel = v;
                this.vesselRootPart = v.rootPart;
            }
            #endregion
        }

        #region Static Fields
        private static readonly List<HydroFlightInputHandler> handlerList = new List<HydroFlightInputHandler>();
        #endregion

        #region Static methods
        private static bool ContainsVessel(Vessel vessel)
        {
            if (vessel == null) { throw new NullReferenceException("HydroFlightInputManager.ContainsVessel: vessel is null"); }
            return handlerList.Any(handler => handler.vessel == vessel);
        }

        public static bool ContainsNameString(string str)
        {
            return handlerList.Any(handler => !handler.isDestroyed && handler.flightInputList.Keys.Contains(str));
        }

        private static Dictionary<string, FlightInputCallback> InputList(Vessel vessel)
        {
            HydroFlightInputHandler h;
            try {h = Handler(vessel); }
            catch (Exception e)
            {
                throw new InvalidOperationException("HydroFlightInputHandler.InputList fail: vessel not found; please check before calling", e);
            }
            return h.flightInputList;
        }

        private static HydroFlightInputHandler Handler(Vessel vessel)
        {
            foreach (HydroFlightInputHandler handler in handlerList.Where(handler => handler.vessel == vessel)) { return handler; }
            throw new InvalidOperationException("HydroFlightInputHandler.Handler fail: vessel not found; please check before calling");
        }

        public static void AddOnFlyByWire(HydroFlightInputCallback callback)
        {
            if (callback.vessel == null) { throw new NullReferenceException("HydroFlightInputManager.AddOnFlyByWire failed when adding " + callback.nameString + ": vessel is null"); }
            if (!ContainsVessel(callback.vessel)) { handlerList.Add(new HydroFlightInputHandler(callback.vessel)); }
            if (InputList(callback.vessel).Values.Contains(callback.callback)) { throw new InvalidOperationException("HydroFlightInputManager.AddOnFlyByWire failed when adding " + callback.nameString + ": OnFlyByWire already added"); }
            if (ContainsNameString(callback.nameString)) { throw new InvalidOperationException("HydroFlightInputManager.AddOnFlyByWire fail when adding " + callback.nameString + ": duplicate nameString"); }
            InputList(callback.vessel).Add(callback.nameString, callback.callback);
            callback.vessel.OnFlyByWire += callback.callback;
#if DEBUG
            Debug.Log("Added an OnFlyByWire, vessel " + (callback.vessel.isActiveVessel ? "is" : " is not") + " ActiveVessel, name: " + callback.nameString);
            PrintCount();
#endif
        }

        public static void RemoveOnFlyByWire(HydroFlightInputCallback callback)
        {
            if (callback.vessel == null) { throw new NullReferenceException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.nameString + ": vessel is null"); }
            if (!ContainsVessel(callback.vessel)) { throw new NullReferenceException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.nameString + ": vessel does not have OnFlyByWire"); }
            if (!InputList(callback.vessel).Values.Contains(callback.callback)) { throw new InvalidOperationException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.nameString + ": OnFlyByWire not found"); }
            if (!InputList(callback.vessel).Keys.Contains(callback.nameString)) { throw new InvalidOperationException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.nameString + ": nameString not found"); }
            InputList(callback.vessel).Remove(callback.nameString);
            callback.vessel.OnFlyByWire -= callback.callback;
            if (InputList(callback.vessel).Count == 0) { handlerList.Remove(Handler(callback.vessel)); }
#if DEBUG
            Debug.Log("Removed an OnFlyByWire, vessel " + (callback.vessel.isActiveVessel ? "is" : " is not") + " ActiveVessel, name: " + callback.nameString);
            PrintCount();
#endif
        }

        public static void AddOnFlyByWire(Vessel vessel, string nameString, FlightInputCallback callback)
        {
            AddOnFlyByWire(new HydroFlightInputCallback(vessel, nameString, callback));
        }

        public static void RemoveOnFlyByWire(Vessel vessel, string nameString, FlightInputCallback callback)
        {
            RemoveOnFlyByWire(new HydroFlightInputCallback(vessel, nameString, callback));
        }

        public static void OnFlightStart()
        {
            handlerList.Clear();
        }

        public static void OnUpdate()
        {
            List<HydroFlightInputHandler> listToRemove = new List<HydroFlightInputHandler>();
            foreach (HydroFlightInputHandler handler in handlerList)
            {
                if (handler.isDestroyed)
                {
                    foreach (Vessel v in FlightGlobals.Vessels)
                    {
                        if (v.packed || ContainsVessel(v)) { continue; }
                        if (v.rootPart == handler.vesselRootPart)
                        {
#if DEBUG
                            Debug.Log("A previously destroyed vessel has been detected.");
#endif
                            listToRemove.Add(handler);
                        }
                    }
                }
                else if(handler.vessel == null)
                {
#if DEBUG
                    Debug.Log("A vessel has been detected destroyed.");
#endif
                    handler.isDestroyed = true;
                }
                else { handler.vesselRootPart = handler.vessel.rootPart; }
            }
            foreach (HydroFlightInputHandler h in listToRemove)
            {
                handlerList.Remove(h);
            }
#if DEBUG
            if (listToRemove.Count != 0)
            {
                Debug.Log("OnFlyByWire's on destroyed vessels has been removed.");
                PrintCount();
            }
#endif
        }
        #endregion

        #region Debug
#if DEBUG
        public static void PrintCount()
        {
            int vesselActive = 0, vesselDestroyed = 0;
            int apActive = 0, apInactive = 0;
            foreach (HydroFlightInputHandler handler in handlerList)
            {
                if (handler.isDestroyed)
                {
                    vesselDestroyed++;
                    apInactive += handler.flightInputList.Count;
                }
                else
                {
                    vesselActive++;
                    apActive += handler.flightInputList.Count;
                }
            }
            Debug.Log(string.Format("Total {0} vessels, {1} autopilots\nActive: {2} vessels, {3} autopilots\nInactive: {4} vessels, {5} autopilots", vesselActive + vesselDestroyed, apActive + apInactive, vesselActive, apActive, vesselDestroyed, apInactive));
        }

        public static string StringList()
        {
            string msgStr = "Vessel count: " + handlerList.Count;
            int count = 0;
            foreach (HydroFlightInputHandler handler in handlerList)
            {
                if (handler.isDestroyed)
                {
                    msgStr += string.Format("\nVessel {0} destroyed, count: {1}", count++, handler.flightInputList.Count);
                }
                else
                {
                    msgStr += string.Format("\nVessel {0} isActiveVessel = {1}, count = {2}", count++, handler.vessel.isActiveVessel, handler.flightInputList.Count);
                }
                msgStr = handler.flightInputList.Keys.Aggregate(msgStr, (current, str) => current + "\n\t" + str);
            }
            return msgStr;
        }

        public static void PrintList()
        {
            Debug.Log(StringList());
        }
#endif
        #endregion
    }
}