using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydroTech.Managers
{
    public class HydroInputManager
    {
        public struct InputCallback
        {
            #region Fields
            public FlightInputCallback callback;
            public string name;
            public Vessel vessel;
            #endregion

            #region Constructor
            public InputCallback(Vessel v, string str, FlightInputCallback cb)
            {
                this.vessel = v;
                this.name = str;
                this.callback = cb;
            }
            #endregion
        }

        private class InputHandler
        {
            #region Fields
            public readonly Dictionary<string, FlightInputCallback> flightInputList;
            public bool isDestroyed;
            public readonly Vessel vessel;
            public Part vesselRootPart;
            #endregion

            #region Constructor
            public InputHandler(Vessel v)
            {
                this.flightInputList = new Dictionary<string, FlightInputCallback>();
                this.vessel = v;
                this.vesselRootPart = v.rootPart;
            }
            #endregion
        }

        #region Fields
        private readonly List<InputHandler> handlerList = new List<InputHandler>();
        #endregion

        #region Methods
        private bool ContainsVessel(Vessel vessel)
        {
            if (vessel == null) { throw new NullReferenceException("HydroFlightInputManager.ContainsVessel: vessel is null"); }
            return this.handlerList.Any(handler => handler.vessel == vessel);
        }

        public bool ContainsNameString(string str)
        {
            return this.handlerList.Any(handler => !handler.isDestroyed && handler.flightInputList.Keys.Contains(str));
        }

        private Dictionary<string, FlightInputCallback> InputList(Vessel vessel)
        {
            InputHandler h;
            try {h = Handler(vessel); }
            catch (Exception e)
            {
                throw new InvalidOperationException("HydroFlightInputHandler.InputList fail: vessel not found; please check before calling", e);
            }
            return h.flightInputList;
        }

        private InputHandler Handler(Vessel vessel)
        {
            InputHandler handler = this.handlerList.FirstOrDefault(h => h.vessel == vessel);
            if (handler == null) { throw new InvalidOperationException("HydroFlightInputHandler.Handler fail: vessel not found; please check before calling"); }
            return handler;
        }

        public void AddOnFlyByWire(InputCallback callback)
        {
            if (callback.vessel == null) { throw new NullReferenceException("HydroFlightInputManager.AddOnFlyByWire failed when adding " + callback.name + ": vessel is null"); }
            if (!ContainsVessel(callback.vessel)) { this.handlerList.Add(new InputHandler(callback.vessel)); }
            if (InputList(callback.vessel).Values.Contains(callback.callback)) { throw new InvalidOperationException("HydroFlightInputManager.AddOnFlyByWire failed when adding " + callback.name + ": OnFlyByWire already added"); }
            if (ContainsNameString(callback.name)) { throw new InvalidOperationException("HydroFlightInputManager.AddOnFlyByWire fail when adding " + callback.name + ": duplicate nameString"); }
            InputList(callback.vessel).Add(callback.name, callback.callback);
            callback.vessel.OnFlyByWire += callback.callback;
#if DEBUG
            Debug.Log("Added an OnFlyByWire, vessel " + (callback.vessel.isActiveVessel ? "is" : " is not") + " ActiveVessel, name: " + callback.name);
            PrintCount();
#endif
        }

        public void RemoveOnFlyByWire(InputCallback callback)
        {
            if (callback.vessel == null) { throw new NullReferenceException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.name + ": vessel is null"); }
            if (!ContainsVessel(callback.vessel)) { throw new NullReferenceException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.name + ": vessel does not have OnFlyByWire"); }
            if (!InputList(callback.vessel).Values.Contains(callback.callback)) { throw new InvalidOperationException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.name + ": OnFlyByWire not found"); }
            if (!InputList(callback.vessel).Keys.Contains(callback.name)) { throw new InvalidOperationException("HydroFlightInputManager.RemoveOnFlyByWire failed when removing " + callback.name + ": nameString not found"); }
            InputList(callback.vessel).Remove(callback.name);
            callback.vessel.OnFlyByWire -= callback.callback;
            if (InputList(callback.vessel).Count == 0) { this.handlerList.Remove(Handler(callback.vessel)); }
#if DEBUG
            Debug.Log("Removed an OnFlyByWire, vessel " + (callback.vessel.isActiveVessel ? "is" : " is not") + " ActiveVessel, name: " + callback.name);
            PrintCount();
#endif
        }

        public void AddOnFlyByWire(Vessel vessel, string nameString, FlightInputCallback callback)
        {
            AddOnFlyByWire(new InputCallback(vessel, nameString, callback));
        }

        public void RemoveOnFlyByWire(Vessel vessel, string nameString, FlightInputCallback callback)
        {
            RemoveOnFlyByWire(new InputCallback(vessel, nameString, callback));
        }
        #endregion

        #region Functions
        public void Start()
        {
            this.handlerList.Clear();
        }

        public void Update()
        {
            List<InputHandler> listToRemove = new List<InputHandler>();
            foreach (InputHandler handler in this.handlerList)
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
            foreach (InputHandler h in listToRemove)
            {
                this.handlerList.Remove(h);
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
        public void PrintCount()
        {
            int vesselActive = 0, vesselDestroyed = 0;
            int apActive = 0, apInactive = 0;
            foreach (InputHandler handler in this.handlerList)
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

        public string StringList()
        {
            string msgStr = "Vessel count: " + this.handlerList.Count;
            int count = 0;
            foreach (InputHandler handler in this.handlerList)
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

        public void PrintList()
        {
            Debug.Log(StringList());
        }
#endif
        #endregion
    }
}