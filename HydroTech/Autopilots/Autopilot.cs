using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using UnityEngine;
using static System.Linq.Enumerable;
using static HydroTech.Extensions.VesselExtensions;

namespace HydroTech.Autopilots
{
    public abstract class Autopilot
    {
        #region Fields
        protected Vessel drivingVessel;
        #endregion

        #region Properties
        protected bool active;
        public virtual bool Active
        {
            get { return this.active; }
            set
            {
                if (this.Engaged && value != this.active)
                {
                    if (value) { AddDrive(); }
                    else
                    {
                        RemoveDrive();
                    }
                }
                this.active = value;
            }
        }

        protected bool engaged;
        public virtual bool Engaged
        {
            get { return this.Active && this.engaged; }
            set
            {
                if (!this.Active) { return; }
                if (!this.engaged && value)
                {
                    AddDrive();
                    RemoveOtherAutopilots();
                }
                else if (this.engaged && !value) { RemoveDrive(); }
                this.engaged = value;
            }
        }

        protected static Vessel ActiveVessel => FlightGlobals.ActiveVessel;

        protected static RCSCalculator ActiveRCS => HydroFlightManager.Instance.ActiveRCS;

        protected virtual string NameString => "Autopilot";

        public static Autopilot EngagedAutopilot
        {
            get { return HydroFlightManager.Instance.Autopilots.SingleOrDefault(ap => ap.Engaged); }
        }

        public static bool AutopilotEngaged => EngagedAutopilot != null;
        #endregion

        #region Destructor
        ~Autopilot()
        {
            this.Engaged = false;
            this.Active = false;
        }
        #endregion

        #region Methods
        protected void AddDrive()
        {
            HydroFlightManager.Instance.InputManager.AddOnFlyByWire(ActiveVessel, this.NameString, DriveAutopilot);
            this.drivingVessel = ActiveVessel;
        }

        protected void RemoveDrive()
        {
            HydroFlightManager.Instance.InputManager.RemoveOnFlyByWire(ActiveVessel, this.NameString, DriveAutopilot);
            this.drivingVessel = null;
        }

        protected void RemoveUserInput(FlightCtrlState ctrlState)
        {
            ctrlState.yaw = 0;
            ctrlState.roll = 0;
            ctrlState.pitch = 0;
            ctrlState.X = 0;
            ctrlState.Y = 0;
            ctrlState.Z = 0;
        }

        protected void RemoveOtherAutopilots()
        {
            foreach (Autopilot ap in HydroFlightManager.Instance.Autopilots)
            {
                if (ap != this) { ap.Engaged = false; }
            }
        }
        #endregion

        #region Virtual methods
        protected virtual void DriveAutopilot(FlightCtrlState ctrlState)
        {
            FlightGlobals.ActiveVessel.SetState(KSPActionGroup.RCS, true);
        }

        public virtual void OnFlightStart()
        {
            this.Active = true;
            if (this.Engaged && this.drivingVessel != ActiveVessel)
            {
                this.drivingVessel = null;
                this.engaged = false;
            }
            this.Engaged = false;
        }

        public virtual void OnGamePause()
        {
            this.Active = false;
        }

        public virtual void OnGameResume()
        {
            this.Active = true;
        }

        public virtual void OnDeactivate()
        {
            this.Active = false;
        }

        public virtual void OnUpdate()
        {
            if (this.Engaged && this.drivingVessel != ActiveVessel)
            {
                if (this.drivingVessel != null) { HydroFlightManager.Instance.InputManager.RemoveOnFlyByWire(this.drivingVessel, this.NameString, DriveAutopilot); }
                AddDrive();
            }
        }
        #endregion

        #region Debug
#if DEBUG
        public static string StringCtrlState(FlightCtrlState ctrlState)
        {
            return $"yaw = {ctrlState.yaw.ToString("#0.000")}, roll = {ctrlState.roll.ToString("#0.000")}, pitch = {ctrlState.pitch.ToString("#0.000")}\nX = {ctrlState.X.ToString("#0.000")}, Y = {ctrlState.Y.ToString("#0.000")}, Z = {ctrlState.Z.ToString("#0.000")}";
        }

        public static void PrintCtrlState(FlightCtrlState ctrlState)
        {
            Debug.Log(StringCtrlState(ctrlState));
        }

        public static string StringAllAPStatus()
        {
            string msg = AutopilotEngaged ? "Autopilot engaged" : "No autopilot engaged";
            foreach (Autopilot ap in HydroFlightManager.Instance.Autopilots)
            {
                msg += $"\n{ap.NameString} {ap.Engaged}";
            }
            return msg;
        }

        public static void PrintAllAPStatus()
        {
            Debug.Log(StringAllAPStatus());
        }
#endif
        #endregion
    }
}