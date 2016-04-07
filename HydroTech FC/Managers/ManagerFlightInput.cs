#if DEBUG
#define HANDLER_SHOW_ADD_REMOVE
#endif

using System;
using System.Collections.Generic;
using System.Linq;

namespace HydroTech_FC
{
    public static class HydroFlightInputManager
    {
        public class HydroFlightInputCallback
        {
            public FlightInputCallback callback;
            public string nameString = "";

            public Vessel vessel;

            public HydroFlightInputCallback(Vessel v, string str, FlightInputCallback cb)
            {
                this.vessel = v;
                this.nameString = str;
                this.callback = cb;
            }
        }

        private class HydroFlightInputHandler
        {
            public Dictionary<string, FlightInputCallback> flightInputList = new Dictionary<string, FlightInputCallback>();

            public bool isDestroyed;

            public Vessel vessel;
            public Part vesselRootPart;

            public HydroFlightInputHandler(Vessel v)
            {
                this.vessel = v;
                this.vesselRootPart = v.rootPart;
            }
        }

        private static List<HydroFlightInputHandler> handlerList = new List<HydroFlightInputHandler>();

        private static bool ContainsVessel(Vessel vessel)
        {
            if (vessel == null) { throw new Exception("HydroFlightInputManager.ContainsVessel: vessel is null"); }
            return handlerList.Any(handler => handler.vessel == vessel);
        }

        public static bool ContainsNameString(string str)
        {
            foreach (HydroFlightInputHandler handler in handlerList) { if (!handler.isDestroyed && handler.flightInputList.Keys.Contains(str)) { return true; } }
            return false;
        }

        private static Dictionary<string, FlightInputCallback> InputList(Vessel vessel)
        {
            HydroFlightInputHandler h = null;
            try { h = Handler(vessel); }
            catch (Exception)
            {
                throw new Exception("HydroFlightInputHandler.InputList fail: vessel not found; please check before calling");
            }
            return h.flightInputList;
        }

        private static HydroFlightInputHandler Handler(Vessel vessel)
        {
            foreach (HydroFlightInputHandler handler in handlerList) { if (handler.vessel == vessel) { return handler; } }
            throw new Exception("HydroFlightInputHandler.Handler fail: vessel not found; please check before calling");
        }

        public static void AddOnFlyByWire(HydroFlightInputCallback callback)
        {
            if (callback.vessel == null) { throw new Exception("HydroFlightInputManager.AddOnFlyByWire fail when adding " + callback.nameString + ": vessel is null"); }
            if (!ContainsVessel(callback.vessel)) { handlerList.Add(new HydroFlightInputHandler(callback.vessel)); }
            if (InputList(callback.vessel).Values.Contains(callback.callback)) { throw new Exception("HydroFlightInputManager.AddOnFlyByWire fail when adding " + callback.nameString + ": OnFlyByWire already added"); }
            if (ContainsNameString(callback.nameString)) { throw new Exception("HydroFlightInputManager.AddOnFlyByWire fail when adding " + callback.nameString + ": duplicate nameString"); }
            InputList(callback.vessel).Add(callback.nameString, callback.callback);
            callback.vessel.OnFlyByWire += callback.callback;
#if HANDLER_SHOW_ADD_REMOVE
            print("Added an OnFlyByWire, vessel " + (callback.vessel == GameStates.ActiveVessel ? "==" : "!=") + " ActiveVessel, name = " + callback.nameString);
            PrintCount();
#endif
        }

        public static void RemoveOnFlyByWire(HydroFlightInputCallback callback)
        {
            if (callback.vessel == null) { throw new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing " + callback.nameString + ": vessel is null"); }
            if (!ContainsVessel(callback.vessel)) { throw new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing " + callback.nameString + ": vessel does not have OnFlyByWire"); }
            if (!InputList(callback.vessel).Values.Contains(callback.callback)) { throw new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing " + callback.nameString + ": OnFlyByWire not found"); }
            if (!InputList(callback.vessel).Keys.Contains(callback.nameString)) { throw new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing " + callback.nameString + ": nameString not found"); }
            InputList(callback.vessel).Remove(callback.nameString);
            callback.vessel.OnFlyByWire -= callback.callback;
            if (InputList(callback.vessel).Count == 0) { handlerList.Remove(Handler(callback.vessel)); }
#if HANDLER_SHOW_ADD_REMOVE
            print("Removed an OnFlyByWire, vessel " + (callback.vessel == GameStates.ActiveVessel ? "==" : "!=") + " ActiveVessel, name = " + callback.nameString);
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
#if HANDLER_SHOW_ADD_REMOVE
                            print("A previously destroyed vessel has been detected.");
#endif
                            listToRemove.Add(handler);
                        }
                    }
                }
                else
                {
                    if (handler.vessel == null)
                    {
#if HANDLER_SHOW_ADD_REMOVE
                        print("A vessel has been detected destroyed.");
#endif
                        handler.isDestroyed = true;
                    }
                    else
                    {
                        handler.vesselRootPart = handler.vessel.rootPart;
                    }
                }
            }
            foreach (HydroFlightInputHandler h in listToRemove)
            {
                /*
                foreach (FlightInputCallback callback in h.flightInputList.Values)
                    h.vesselRootPart.vessel.OnFlyByWire -= callback;*/
                handlerList.Remove(h);
            }
#if HANDLER_SHOW_ADD_REMOVE
            if (listToRemove.Count != 0)
            {
                print("OnFlyByWire's on destroyed vessels has been removed.");
                PrintCount();
            }
#endif
        }

#if DEBUG
        private static void print(object message)
        {
            GameBehaviours.print(message);
        }

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
            print("Total " + (vesselActive + vesselDestroyed) + " vessels, " + (apActive + apInactive) + " autopilots" + "\nActive: " + vesselActive + " vessels, " + apActive + " autopilots" + "\nInactive: " + vesselDestroyed + " vessels, " + apInactive + " autopilots");
        }

        public static string StringList()
        {
            string msgStr = "Vessel count = " + handlerList.Count;
            int count = 0;
            foreach (HydroFlightInputHandler handler in handlerList)
            {
                if (handler.isDestroyed)
                {
                    msgStr += "\nVessel " + count++ + " destroyed" + ", count = " + handler.flightInputList.Count;
                }
                else
                {
                    msgStr += "\nVessel " + count++ + " isActiveVessel = " + (handler.vessel == GameStates.ActiveVessel) + ", count = " + handler.flightInputList.Count;
                }
                foreach (string str in handler.flightInputList.Keys)
                {
                    msgStr += "\n\t" + str;
                }
            }
            return msgStr;
        }

        public static void PrintList()
        {
            print(StringList());
        }
#endif
    }
}