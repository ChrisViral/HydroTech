#if DEBUG
#define HANDLER_SHOW_ADD_REMOVE
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    static public class HydroFlightInputManager
    {
        public class HydroFlightInputCallback
        {
            public HydroFlightInputCallback(Vessel v, String str, FlightInputCallback cb)
            {
                vessel = v;
                nameString = str;
                callback = cb;
            }

            public Vessel vessel = null;
            public String nameString = "";
            public FlightInputCallback callback = null;
        }

        private class HydroFlightInputHandler
        {
            public HydroFlightInputHandler(Vessel v)
            {
                vessel = v;
                vesselRootPart = v.rootPart;
            }

            public Vessel vessel = null;
            public Part vesselRootPart = null;
            public bool isDestroyed = false;
            public Dictionary<String, FlightInputCallback> flightInputList
                = new Dictionary<string, FlightInputCallback>();
        }

        static private List<HydroFlightInputHandler> handlerList = new List<HydroFlightInputHandler>();

        static private bool ContainsVessel(Vessel vessel)
        {
            if (vessel == null)
                throw (new Exception("HydroFlightInputManager.ContainsVessel: vessel is null"));
            foreach (HydroFlightInputHandler handler in handlerList)
                if (handler.vessel == vessel)
                    return true;
            return false;
        }

        static public bool ContainsNameString(String str)
        {
            foreach (HydroFlightInputHandler handler in handlerList)
                if (!handler.isDestroyed && handler.flightInputList.Keys.Contains(str))
                    return true;
            return false;
        }

        static private Dictionary<String, FlightInputCallback> InputList(Vessel vessel)
        {
            HydroFlightInputHandler h = null;
            try { h = Handler(vessel); }
            catch (Exception)
            {
                throw (new Exception("HydroFlightInputHandler.InputList fail: vessel not found; please check before calling"));
            }
            return h.flightInputList;
        }

        static private HydroFlightInputHandler Handler(Vessel vessel)
        {
            foreach (HydroFlightInputHandler handler in handlerList)
                if (handler.vessel == vessel)
                    return handler;
            throw (new Exception("HydroFlightInputHandler.Handler fail: vessel not found; please check before calling"));
        }

        static public void AddOnFlyByWire(HydroFlightInputCallback callback)
        {
            if (callback.vessel == null)
                throw (new Exception("HydroFlightInputManager.AddOnFlyByWire fail when adding "
                    + callback.nameString + ": vessel is null"));
            if (!ContainsVessel(callback.vessel))
                handlerList.Add(new HydroFlightInputHandler(callback.vessel));
            if (InputList(callback.vessel).Values.Contains(callback.callback))
                throw (new Exception("HydroFlightInputManager.AddOnFlyByWire fail when adding "
                    + callback.nameString + ": OnFlyByWire already added"));
            if (ContainsNameString(callback.nameString))
                throw (new Exception("HydroFlightInputManager.AddOnFlyByWire fail when adding "
                    + callback.nameString + ": duplicate nameString"));
            InputList(callback.vessel).Add(callback.nameString, callback.callback);
            callback.vessel.OnFlyByWire += callback.callback;
#if HANDLER_SHOW_ADD_REMOVE
            print("Added an OnFlyByWire, vessel " + (callback.vessel == GameStates.ActiveVessel ? "==" : "!=")
                + " ActiveVessel, name = " + callback.nameString);
            PrintCount();
#endif
        }

        static public void RemoveOnFlyByWire(HydroFlightInputCallback callback)
        {
            if (callback.vessel == null)
                throw (new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing "
                    + callback.nameString + ": vessel is null"));
            if (!ContainsVessel(callback.vessel))
                throw (new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing "
                    + callback.nameString + ": vessel does not have OnFlyByWire"));
            if (!InputList(callback.vessel).Values.Contains(callback.callback))
                throw (new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing "
                    + callback.nameString + ": OnFlyByWire not found"));
            if (!InputList(callback.vessel).Keys.Contains(callback.nameString))
                throw (new Exception("HydroFlightInputManager.RemoveOnFlyByWire fail when removing "
                    + callback.nameString + ": nameString not found"));
            InputList(callback.vessel).Remove(callback.nameString);
            callback.vessel.OnFlyByWire -= callback.callback;
            if (InputList(callback.vessel).Count == 0)
                handlerList.Remove(Handler(callback.vessel));
#if HANDLER_SHOW_ADD_REMOVE
            print("Removed an OnFlyByWire, vessel " + (callback.vessel == GameStates.ActiveVessel ? "==" : "!=")
                + " ActiveVessel, name = " + callback.nameString);
            PrintCount();
#endif
        }

        static public void AddOnFlyByWire(Vessel vessel, String nameString, FlightInputCallback callback)
        {
            AddOnFlyByWire(new HydroFlightInputCallback(vessel, nameString, callback));
        }
        static public void RemoveOnFlyByWire(Vessel vessel, String nameString, FlightInputCallback callback)
        {
            RemoveOnFlyByWire(new HydroFlightInputCallback(vessel, nameString, callback));
        }

        static public void onFlightStart()
        {
            handlerList.Clear();
        }

        static public void OnUpdate()
        {
            List<HydroFlightInputHandler> listToRemove = new List<HydroFlightInputHandler>();
            foreach (HydroFlightInputHandler handler in handlerList)
            {
                if (handler.isDestroyed)
                {
                    foreach (Vessel v in FlightGlobals.Vessels)
                    {
                        if (v.packed || ContainsVessel(v))
                            continue;
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
                        handler.vesselRootPart = handler.vessel.rootPart;
                }
            }
            foreach (HydroFlightInputHandler h in listToRemove)
            {/*
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
        static private void print(object message) { GameBehaviours.print(message); }

        static public void PrintCount()
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
            print("Total " + (vesselActive + vesselDestroyed) + " vessels, " + (apActive + apInactive) + " autopilots" +
                "\nActive: " + vesselActive + " vessels, " + apActive + " autopilots" +
                "\nInactive: " + vesselDestroyed + " vessels, " + apInactive + " autopilots");
        }

        static public String StringList()
        {
            String msgStr = "Vessel count = " + handlerList.Count;
            int count = 0;
            foreach (HydroFlightInputHandler handler in handlerList)
            {
                if (handler.isDestroyed)
                    msgStr += "\nVessel " + (count++) + " destroyed"
                        + ", count = " + handler.flightInputList.Count;
                else
                    msgStr += "\nVessel " + (count++)
                        + " isActiveVessel = " + (handler.vessel == GameStates.ActiveVessel)
                        + ", count = " + handler.flightInputList.Count;
                foreach (String str in handler.flightInputList.Keys)
                    msgStr += "\n\t" + str;
            }
            return msgStr;
        }
        static public void PrintList() { print(StringList()); }
#endif
    }
}