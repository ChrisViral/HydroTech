using System;
using HydroTech_FC;
using HydroTech_RCS.Autopilots.Modules;
using UnityEngine;

namespace HydroTech_RCS.Autopilots
{
    public abstract class RcsAutopilot : LoadSaveFileBasic
    {
        protected bool _Active;

        protected bool _Engaged;

        protected Vessel drivingVessel;

        public virtual bool Active
        {
            get { return this._Active; }
            set
            {
                if (this.Engaged && (value != this._Active))
                {
                    if (value) { AddDrive(); }
                    else
                    { RemoveDrive(); }
                }
                this._Active = value;
            }
        }

        public virtual bool Engaged
        {
            get { return this.Active && this._Engaged; }
            set
            {
                if (!this.Active) { return; }
                if (!this._Engaged && value)
                {
                    AddDrive();
                    RemoveOtherAutopilots();
                }
                else if (this._Engaged && !value) { RemoveDrive(); }
                this._Engaged = value;
            }
        }

        protected static Vessel ActiveVessel
        {
            get { return GameStates.ActiveVessel; }
        }

        protected static CalculatorRcsThrust RcsActive
        {
            get { return HydroJebCore.activeVesselRcs; }
        }

        protected virtual string NameString
        {
            get { return "Autopilot"; }
        }

        public static RcsAutopilot EngagedAutopilot
        {
            get
            {
                foreach (RcsAutopilot ap in HydroJebCore.autopilots.Values)
                {
                    if (ap.Engaged) { return ap; }
                }
                return null;
            }
        }

        public static bool AutopilotEngaged
        {
            get { return EngagedAutopilot != null; }
        }

        ~RcsAutopilot()
        {
            this.Engaged = false;
            this.Active = false;
        }

        protected void AddDrive()
        {
            HydroFlightInputManager.AddOnFlyByWire(ActiveVessel, this.NameString, DriveAutopilot);
            this.drivingVessel = ActiveVessel;
        }

        protected void RemoveDrive()
        {
            HydroFlightInputManager.RemoveOnFlyByWire(ActiveVessel, this.NameString, DriveAutopilot);
            this.drivingVessel = null;
        }

        protected static void TurnOnRcs(Vessel vessel)
        {
            HydroActionGroupManager.SetState(vessel, KSPActionGroup.RCS, true);
        }

        protected static void TurnOffSas(Vessel vessel)
        {
            HydroActionGroupManager.SetState(vessel, KSPActionGroup.SAS, false);
        }

        protected void RemoveUserInput(FlightCtrlState ctrlState)
        {
            ctrlState.yaw = 0.0F;
            ctrlState.roll = 0.0F;
            ctrlState.pitch = 0.0F;
            ctrlState.X = 0.0F;
            ctrlState.Y = 0.0F;
            ctrlState.Z = 0.0F;
        }

        protected virtual void DriveAutopilot(FlightCtrlState ctrlState)
        {
            HydroActionGroupManager.ActiveVessel.RCS = true;
            // HydroActionGroupManager.ActiveVessel.SAS = false;
        }

        protected void RemoveOtherAutopilots()
        {
            foreach (RcsAutopilot ap in HydroJebCore.autopilots.Values) { if (ap != this) { ap.Engaged = false; } }
        }

        public virtual void OnFlightStart()
        {
            Load();
            this.Active = true;
            if (this.Engaged && (this.drivingVessel != ActiveVessel))
            {
                this.drivingVessel = null;
                this._Engaged = false;
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
            this.Active = HydroJebCore.electricity;
            if (this.Engaged)
            {
                if (this.drivingVessel != ActiveVessel)
                {
                    if (this.drivingVessel != null) { HydroFlightInputManager.RemoveOnFlyByWire(this.drivingVessel, this.NameString, DriveAutopilot); }
                    AddDrive();
                }
            }
            if (this.needSave) { Save(); }
        }

        public void MakeSaveAtNextUpdate()
        {
            this.needSave = true;
        }

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

#if DEBUG
        public static string StringCtrlState(FlightCtrlState ctrlState)
        {
            return "yaw = " + ctrlState.yaw.ToString("#0.000") + ", roll = " + ctrlState.roll.ToString("#0.000") + ", pitch = " + ctrlState.pitch.ToString("#0.000") + "\nX = " + ctrlState.X.ToString("#0.000") + ", Y = " + ctrlState.Y.ToString("#0.000") + ", Z = " + ctrlState.Z.ToString("#0.000");
        }

        public static void PrintCtrlState(FlightCtrlState ctrlState)
        {
            print(StringCtrlState(ctrlState));
        }

        public static string StringAllAPStatus()
        {
            string msg = AutopilotEngaged ? "Autopilot engaged" : "No autopilot engaged";
            foreach (RcsAutopilot ap in HydroJebCore.autopilots.Values) { msg += "\n" + ap.NameString + " " + ap.Engaged; }
            return msg;
        }

        public static void PrintAllAPStatus()
        {
            print(StringAllAPStatus());
        }
#endif
    }
}