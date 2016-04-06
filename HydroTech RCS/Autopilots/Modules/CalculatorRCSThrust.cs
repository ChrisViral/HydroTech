using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.Modules
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;

    public class CalculatorRCSThrust : CalculatorVesselInfoBasic
    {
        protected bool _AllRCSEnabled = true;
        public bool AllRCSEnabled
        {
            get { return _AllRCSEnabled; }
            protected set
            {
                AllRCSEnabledChanged = !(value == _AllRCSEnabled);
                _AllRCSEnabled = value;
            }
        }
        protected bool _AllRCSEnabledChanged = false;
        public bool AllRCSEnabledChanged
        {
            get { return _AllRCSEnabledChanged; }
            protected set { _AllRCSEnabledChanged = value; }
        }

        public Vector6 MaxTorque = new Vector6();
        public Vector6 MaxForce = new Vector6();
        public Vector6 MaxAngularAcc = new Vector6();
        public Vector6 MaxAcc = new Vector6();

        protected override void  Calculate()
        {
            base.Calculate();
            bool tempAllRCSEnabled = true;
            MaxTorque.Reset();
            MaxForce.Reset();
            foreach (Part p in partList)
            {
                Vector3 r = SwitchTransformCalculator.VectorTransform(
                    p.Rigidbody.worldCenterOfMass - CoM,
                    transformRight,
                    transformDown,
                    transformForward
                    );
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleRCS)
                    {
                        ModuleRCS mrcs = (ModuleRCS)pm;
                        if (mrcs.isEnabled)
                        {
                            foreach (Transform trans in mrcs.thrusterTransforms)
                            {
                                Vector3 thrust = SwitchTransformCalculator.VectorTransform(
                                    trans.up,
                                    transformRight,
                                    transformDown,
                                    transformForward
                                    ) * mrcs.thrusterPower;
                                Vector3 thrustTorque = -HMaths.CrossProduct(r, thrust);
                                MaxForce.AddX(thrust.x);
                                MaxForce.AddY(thrust.y);
                                MaxForce.AddZ(thrust.z);
                                MaxTorque.AddX(thrustTorque.x);
                                MaxTorque.AddY(thrustTorque.y);
                                MaxTorque.AddZ(thrustTorque.z);
                            }
                        }
                        else
                            tempAllRCSEnabled = false;
                    }
                }
            }
            AllRCSEnabled = tempAllRCSEnabled;
            MaxAcc = MaxForce / Mass;
            MaxAngularAcc = MaxTorque / MoI.Diagonal;
        }

        public void EnableAllRCS()
        {
            foreach (Part p in partList)
                foreach (PartModule pm in p.Modules)
                    if (pm is ModuleRCS && !((ModuleRCS)pm).isEnabled)
                        ((ModuleRCS)pm).Enable();
        }

        public override string ToString()
        {
            return "Mass = " + Mass.ToString()
                + "\nForces = " + MaxForce.ToString()
                + "\nAcc = " + MaxAcc.ToString()
                + "\nMoI = " + MoI.ToString()
                + "\nTorque = " + MaxTorque.ToString()
                + "\nAAcc = " + MaxAngularAcc.ToString();
        }
        virtual public string ToString(string format)
        {
            return "Mass = " + Mass.ToString(format)
                + "\nForces = " + MaxForce.ToString(format)
                + "\nAcc = " + MaxAcc.ToString(format)
                + "\nMoI = " + MoI.ToString(format)
                + "\nTorque = " + MaxTorque.ToString(format)
                + "\nAAcc = " + MaxAngularAcc.ToString(format);
        }

        public float GetThrustRateFromAngularAcc6(int dir, float aA)
        {
            float maxAA = 0.0F;
            switch (dir)
            {
                case 0 /* xp = pitch- */: maxAA = MaxAngularAcc.xp; break;
                case 1 /* xn = pitch+ */: maxAA = MaxAngularAcc.xn; break;
                case 2 /* yp = yaw- */: maxAA = MaxAngularAcc.yp; break;
                case 3 /* yn = yaw+ */: maxAA = MaxAngularAcc.yn; break;
                case 4 /* zp = roll- */: maxAA = MaxAngularAcc.zp; break;
                case 5 /* zn = roll+ */: maxAA = MaxAngularAcc.zn; break;
                default: maxAA = 0.0F; break;
            }
            if (maxAA == 0.0F)
                return 1.0F;
            else
                return aA / maxAA;
        }
        public float GetThrustRateFromAcc6(int dir, float a)
        {
            float maxA = 0.0F;
            switch (dir)
            {
                case 0 /* xp = right- */: maxA = MaxAcc.xp; break;
                case 1 /* xn = right+ */: maxA = MaxAcc.xn; break;
                case 2 /* yp = down- */: maxA = MaxAcc.yp; break;
                case 3 /* yn = down+ */: maxA = MaxAcc.yn; break;
                case 4 /* zp = forward- */: maxA = MaxAcc.zp; break;
                case 5 /* zn = forward+ */: maxA = MaxAcc.zn; break;
                default: maxA = 0.0F; break;
            }
            if (maxA == 0.0F)
                return 1.0F;
            else
                return a / maxA;
        }
        public void MakeRotation(FlightCtrlState ctrlState, float AngularAcc)
        {
            ctrlState.pitch *= GetThrustRateFromAngularAcc6(ctrlState.pitch >= 0 ? 1 : 0, AngularAcc);
            ctrlState.yaw *= GetThrustRateFromAngularAcc6(ctrlState.yaw >= 0 ? 3 : 2, AngularAcc);
            ctrlState.roll *= GetThrustRateFromAngularAcc6(ctrlState.roll >= 0 ? 5 : 4, AngularAcc);
        }
        public void MakeTranslation(FlightCtrlState ctrlState, float Acc)
        {
            ctrlState.X *= GetThrustRateFromAcc6(ctrlState.X >= 0 ? 0 : 1, Acc);
            ctrlState.Y *= GetThrustRateFromAcc6(ctrlState.Y >= 0 ? 2 : 3, Acc);
            ctrlState.Z *= GetThrustRateFromAcc6(ctrlState.Z >= 0 ? 4 : 5, Acc);
        }

        public float GetThrustRateFromAngularAcc3(int dir, float aA)
        {
            bool pos = aA >= 0.0F;
            return GetThrustRateFromAngularAcc6(
                dir * 2 + (pos ? 1 : 0),
                aA
                );
        }
        public float GetThrustRateFromAcc3(int dir, float a)
        {
            bool pos = a >= 0.0F;
            return GetThrustRateFromAcc6(
                dir * 2 + (pos ? 0 : 1),
                a
                );
        }
    }
}