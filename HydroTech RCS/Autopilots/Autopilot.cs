using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace HydroTech_RCS.Autopilots
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;
    using Modules;

    abstract public class RCSAutopilot : LoadSaveFileBasic
    {
        ~RCSAutopilot()
        {
            engaged = false;
            active = false;
        }

        protected bool _Active = false;
        public virtual bool active
        {
            get { return _Active; }
            set
            {
                if (engaged && value != _Active)
                {
                    if (value)
                        AddDrive();
                    else
                        RemoveDrive();
                }
                _Active = value;
            }
        }

        protected bool _Engaged = false;
        public virtual bool engaged
        {
            get { return active && _Engaged; }
            set
            {
                if (!active)
                    return;
                if (!_Engaged && value)
                {
                    AddDrive();
                    RemoveOtherAutopilots();
                }
                else if (_Engaged && !value)
                    RemoveDrive();
                _Engaged = value;
            }
        }

        protected static Vessel ActiveVessel { get { return GameStates.ActiveVessel; } }
        protected static CalculatorRCSThrust RCSActive { get { return HydroJebCore.activeVesselRCS; } }

        protected virtual String nameString { get { return "Autopilot"; } }

        protected Vessel drivingVessel = null;
        protected void AddDrive()
        {
            HydroFlightInputManager.AddOnFlyByWire(ActiveVessel, nameString, DriveAutopilot);
            drivingVessel = ActiveVessel;
        }
        protected void RemoveDrive()
        {
            HydroFlightInputManager.RemoveOnFlyByWire(ActiveVessel, nameString, DriveAutopilot);
            drivingVessel = null;
        }

        protected static void TurnOnRCS(Vessel vessel) { HydroActionGroupManager.SetState(vessel, KSPActionGroup.RCS, true); }
        protected static void TurnOffSAS(Vessel vessel) { HydroActionGroupManager.SetState(vessel, KSPActionGroup.SAS, false); }

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
            foreach (RCSAutopilot ap in HydroJebCore.autopilots.Values)
                if (ap != this)
                    ap.engaged = false;
        }

        public static RCSAutopilot EngagedAutopilot
        {
            get
            {
                foreach (RCSAutopilot ap in HydroJebCore.autopilots.Values)
                    if (ap.engaged)
                        return ap;
                return null;
            }
        }
        public static bool AutopilotEngaged { get { return EngagedAutopilot != null; } }

        public virtual void OnFlightStart()
        {
            Load();
            active = true;
            if (engaged && drivingVessel != ActiveVessel)
            {
                drivingVessel = null;
                _Engaged = false;
            }
            engaged = false;
        }
        public virtual void onGamePause() { active = false; }
        public virtual void onGameResume() { active = true; }
        public virtual void OnDeactivate() { active = false; }

        public virtual void OnUpdate()
        {
            active = HydroJebCore.electricity;
            if (engaged)
            {
                if (drivingVessel != ActiveVessel)
                {
                    if (drivingVessel != null)
                    {
                        HydroFlightInputManager.RemoveOnFlyByWire(drivingVessel, nameString, DriveAutopilot);
                    }
                    AddDrive();
                }
            }
            if (needSave)
                Save();
        }

        public void MakeSaveAtNextUpdate() { needSave = true; }

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
        public static String StringCtrlState(FlightCtrlState ctrlState)
        {
            return "yaw = " + ctrlState.yaw.ToString("#0.000")
                + ", roll = " + ctrlState.roll.ToString("#0.000")
                + ", pitch = " + ctrlState.pitch.ToString("#0.000")
                + "\nX = " + ctrlState.X.ToString("#0.000")
                + ", Y = " + ctrlState.Y.ToString("#0.000")
                + ", Z = " + ctrlState.Z.ToString("#0.000");
        }
        public static void PrintCtrlState(FlightCtrlState ctrlState) { print(StringCtrlState(ctrlState)); }

        public static String StringAllAPStatus()
        {
            String msg = (AutopilotEngaged ? "Autopilot engaged" : "No autopilot engaged");
            foreach (RCSAutopilot ap in HydroJebCore.autopilots.Values)
                msg += "\n" + ap.nameString + " " + ap.engaged;
            return msg;
        }
        public static void PrintAllAPStatus() { print(StringAllAPStatus()); }
#endif
    }
}