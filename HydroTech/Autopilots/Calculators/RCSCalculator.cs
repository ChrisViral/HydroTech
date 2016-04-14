﻿using HydroTech.Panels;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class RCSCalculator : VesselInfoCalculatorBase
    {
        #region Fields
        public Vector6 maxAcc = new Vector6();
        public Vector6 maxAngularAcc = new Vector6();
        public Vector6 maxForce = new Vector6();
        public Vector6 maxTorque = new Vector6();
        #endregion

        #region Properties
        public bool AllRCSEnabled { get; private set; }
        #endregion

        #region Methods
        public void EnableAllRcs()
        {
            foreach (Part p in this.partList)
            {
                foreach (PartModule pm in p.Modules)
                {
                    ModuleRCS rcs = pm as ModuleRCS;
                    if (rcs != null && !rcs.rcsEnabled) { rcs.Enable(); }
                }
            }
        }

        public float GetThrustRateFromAngularAcc6(int dir, float aA)
        {
            switch (dir)
            {
                case 0: //xp = pitch-
                    return aA / this.maxAngularAcc.xp;

                case 1: //xn = pitch+
                    return aA / this.maxAngularAcc.xn;

                case 2: //yp = yaw-
                    return aA / this.maxAngularAcc.yp;

                case 3: //yn = yaw+
                    return aA / this.maxAngularAcc.yn;

                case 4: //zp = roll-
                    return aA / this.maxAngularAcc.zp;

                case 5: //zn = roll+
                    return aA / this.maxAngularAcc.zn;

                default:
                    return 1;
            }
        }

        public float GetThrustRateFromAcc6(int dir, float a)
        {
            switch (dir)
            {
                case 0: //xp = right
                    return a / this.maxAcc.xp;

                case 1: //xn = right
                    return a / this.maxAcc.xn;

                case 2: //yp = down-
                    return a / this.maxAcc.yp;

                case 3: //yn = down+
                    return a / this.maxAcc.yn;

                case 4: //zp = forward-
                    return a / this.maxAcc.zp;

                case 5: //zn = forward+
                    return a / this.maxAcc.zn;

                default:
                    return 1;
            }
        }

        public void MakeRotation(FlightCtrlState ctrlState, float angularAcc)
        {
            ctrlState.pitch *= GetThrustRateFromAngularAcc6(ctrlState.pitch >= 0 ? 1 : 0, angularAcc);
            ctrlState.yaw   *= GetThrustRateFromAngularAcc6(ctrlState.yaw   >= 0 ? 3 : 2, angularAcc);
            ctrlState.roll  *= GetThrustRateFromAngularAcc6(ctrlState.roll  >= 0 ? 5 : 4, angularAcc);
        }

        public void MakeTranslation(FlightCtrlState ctrlState, float acc)
        {
            ctrlState.X *= GetThrustRateFromAcc6(ctrlState.X >= 0 ? 0 : 1, acc);
            ctrlState.Y *= GetThrustRateFromAcc6(ctrlState.Y >= 0 ? 2 : 3, acc);
            ctrlState.Z *= GetThrustRateFromAcc6(ctrlState.Z >= 0 ? 4 : 5, acc);
        }

        public float GetThrustRateFromAngularAcc3(int dir, float aA)
        {
            return GetThrustRateFromAngularAcc6((dir * 2) + (aA >= 0 ? 1 : 0), aA);
        }

        public float GetThrustRateFromAcc3(int dir, float a)
        {
            return GetThrustRateFromAcc6((dir * 2) + (a >= 0 ? 0 : 1), a);
        }
        #endregion

        #region Overrides
        protected override void Calculate()
        {
            base.Calculate();
            bool tmp = true;
            this.maxTorque.Reset();
            this.maxForce.Reset();
            foreach (Part p in this.partList)
            {
                Vector3 r = SwitchTransformCalculator.VectorTransform(p.WCoM - this.CoM, this.transformRight, this.transformDown, this.transformForward);
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleRCS)
                    {
                        ModuleRCS rcs = (ModuleRCS)pm;
                        if (rcs.rcsEnabled)
                        {
                            foreach (Transform trans in rcs.thrusterTransforms)
                            {
                                Vector3 thrust = SwitchTransformCalculator.VectorTransform(trans.up, this.transformRight, this.transformDown, this.transformForward) * rcs.thrusterPower;
                                Vector3 thrustTorque = Vector3.Cross(r, thrust);
                                this.maxForce.AddX(thrust.x);
                                this.maxForce.AddY(thrust.y);
                                this.maxForce.AddZ(thrust.z);
                                this.maxTorque.AddX(thrustTorque.x);
                                this.maxTorque.AddY(thrustTorque.y);
                                this.maxTorque.AddZ(thrustTorque.z);
                            }
                        }
                        else { tmp = false; }
                    }
                }
            }
            if (this.AllRCSEnabled != tmp)
            {
                if (HighLogic.LoadedSceneIsFlight) { FlightMainPanel.Instance.RCSInfo.ResetHeight(); }
                this.AllRCSEnabled = tmp;
            }
            this.maxAcc = this.maxForce / this.Mass;
            this.maxAngularAcc = this.maxTorque / this.moI.Diagonal;
        }

        public override string ToString()
        {
            return string.Format("Mass: {0}\nForces: {1}\nAcc: {2}\nMoI: {3}\nTorque: {4}\nAAcc: {5}", this.Mass, this.maxForce, this.maxAcc, this.moI, this.maxTorque, this.maxAngularAcc);
        }

        public string ToString(string format)
        {
            return string.Format("Mass: {0}\nForces: {1}\nAcc: {2}\nMoI: {3}\nTorque: {4}\nAAcc: {5}", this.Mass.ToString(format), this.maxForce.ToString(format), this.maxAcc.ToString(format), this.moI.ToString(format), this.maxTorque.ToString(format), this.maxAngularAcc.ToString(format));
        }
        #endregion
    }
}