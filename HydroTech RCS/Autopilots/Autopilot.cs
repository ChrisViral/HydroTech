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
            Engaged = false;
            Active = false;
        }

        protected bool _Active = false;
        virtual public bool Active
        {
            get { return _Active; }
            set
            {
                if (Engaged && value != _Active)
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
        virtual public bool Engaged
        {
            get { return Active && _Engaged; }
            set
            {
                if (!Active)
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

        static protected Vessel ActiveVessel { get { return GameStates.ActiveVessel; } }
        static protected CalculatorRCSThrust RCSActive { get { return HydroJebCore.activeVesselRCS; } }

        virtual protected String nameString { get { return "Autopilot"; } }

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

        static protected void TurnOnRCS(Vessel vessel) { HydroActionGroupManager.SetState(vessel, KSPActionGroup.RCS, true); }
        static protected void TurnOffSAS(Vessel vessel) { HydroActionGroupManager.SetState(vessel, KSPActionGroup.SAS, false); }

        protected void RemoveUserInput(FlightCtrlState ctrlState)
        {
            ctrlState.yaw = 0.0F;
            ctrlState.roll = 0.0F;
            ctrlState.pitch = 0.0F;
            ctrlState.X = 0.0F;
            ctrlState.Y = 0.0F;
            ctrlState.Z = 0.0F;
        }

        virtual protected void DriveAutopilot(FlightCtrlState ctrlState)
        {
            HydroActionGroupManager.ActiveVessel.RCS = true;
            // HydroActionGroupManager.ActiveVessel.SAS = false;
        }

        protected void RemoveOtherAutopilots()
        {
            foreach (RCSAutopilot ap in HydroJebCore.autopilots.Values)
                if (ap != this)
                    ap.Engaged = false;
        }

        static public RCSAutopilot EngagedAutopilot
        {
            get
            {
                foreach (RCSAutopilot ap in HydroJebCore.autopilots.Values)
                    if (ap.Engaged)
                        return ap;
                return null;
            }
        }
        static public bool AutopilotEngaged { get { return EngagedAutopilot != null; } }

        virtual public void onFlightStart()
        {
            Load();
            Active = true;
            if (Engaged && drivingVessel != ActiveVessel)
            {
                drivingVessel = null;
                _Engaged = false;
            }
            Engaged = false;
        }
        virtual public void onGamePause() { Active = false; }
        virtual public void onGameResume() { Active = true; }
        virtual public void OnDeactivate() { Active = false; }

        virtual public void OnUpdate()
        {
            Active = HydroJebCore.electricity;
            if (Engaged)
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

        static protected Vector3 VectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return SwitchTransformCalculator.VectorTransform(vec, x, y, z);
        }
        static protected Vector3 VectorTransform(Vector3 vec, Transform trans)
        {
            return SwitchTransformCalculator.VectorTransform(vec, trans);
        }

        static protected Vector3 ReverseVectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, x, y, z);
        }
        static protected Vector3 ReverseVectorTransform(Vector3 vec, Transform trans)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, trans);
        }

#if DEBUG
        static public String StringCtrlState(FlightCtrlState ctrlState)
        {
            return "yaw = " + ctrlState.yaw.ToString("#0.000")
                + ", roll = " + ctrlState.roll.ToString("#0.000")
                + ", pitch = " + ctrlState.pitch.ToString("#0.000")
                + "\nX = " + ctrlState.X.ToString("#0.000")
                + ", Y = " + ctrlState.Y.ToString("#0.000")
                + ", Z = " + ctrlState.Z.ToString("#0.000");
        }
        static public void PrintCtrlState(FlightCtrlState ctrlState) { print(StringCtrlState(ctrlState)); }

        static public String StringAllAPStatus()
        {
            String msg = (AutopilotEngaged ? "Autopilot engaged" : "No autopilot engaged");
            foreach (RCSAutopilot ap in HydroJebCore.autopilots.Values)
                msg += "\n" + ap.nameString + " " + ap.Engaged;
            return msg;
        }
        static public void PrintAllAPStatus() { print(StringAllAPStatus()); }
#endif
    }
}