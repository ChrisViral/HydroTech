using System.Linq;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Autopilots
{
    public abstract class Autopilot : LoadSaveFileBasic
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

        protected static Vessel ActiveVessel
        {
            get { return FlightGlobals.ActiveVessel; }
        }

        protected static RCSCalculator ActiveRCS
        {
            get { return HydroFlightManager.Instance.ActiveRCS; }
        }

        protected virtual string NameString
        {
            get { return "Autopilot"; }
        }

        public static Autopilot EngagedAutopilot
        {
            get { return HydroFlightManager.Instance.Autopilots.SingleOrDefault(ap => ap.Engaged); }
        }

        public static bool AutopilotEngaged
        {
            get { return EngagedAutopilot != null; }
        }
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

        public void MakeSaveAtNextUpdate()
        {
            this.needSave = true;
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
       
        #region Static Methods
        protected static Vector3 VectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return SwitchTransformCalculator.VectorTransform(vec, x, y, z);
        }

        protected static Vector3 VectorTransform(Vector3 vec, Transform trans)
        {
            return SwitchTransformCalculator.VectorTransform(vec, trans);
        }

        protected static Vector3 ReverseVectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, x, y, z);
        }

        protected static Vector3 ReverseVectorTransform(Vector3 vec, Transform trans)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, trans);
        }
        #endregion

        #region Virtual methods
        protected virtual void DriveAutopilot(FlightCtrlState ctrlState)
        {
            HTUtils.SetState(FlightGlobals.ActiveVessel, KSPActionGroup.RCS, true);
        }

        public virtual void OnFlightStart()
        {
            Load();
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
            if (this.Engaged)
            {
                if (this.drivingVessel != ActiveVessel)
                {
                    if (this.drivingVessel != null) { HydroFlightManager.Instance.InputManager.RemoveOnFlyByWire(this.drivingVessel, this.NameString, DriveAutopilot); }
                    AddDrive();
                }
            }
            if (this.needSave) { Save(); }
        }
        #endregion

        #region Debug
#if DEBUG
        public static string StringCtrlState(FlightCtrlState ctrlState)
        {
            return string.Format("yaw = {0}, roll = {1}, pitch = {2}\nX = {3}, Y = {4}, Z = {5}",
                ctrlState.yaw.ToString("#0.000"), ctrlState.roll.ToString("#0.000"), ctrlState.pitch.ToString("#0.000"),
                ctrlState.X.ToString("#0.000"), ctrlState.Y.ToString("#0.000"), ctrlState.Z.ToString("#0.000"));
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
                msg += string.Format("\n{0} {1}", ap.NameString, ap.Engaged);
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