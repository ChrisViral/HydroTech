using HydroTech_FC;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.Modules
{
    public class CalculatorRcsThrust : CalculatorVesselInfoBasic
    {
        protected bool allRcsEnabled = true;
        protected bool allRcsEnabledChanged;
        public Vector6 maxAcc = new Vector6();
        public Vector6 maxAngularAcc = new Vector6();
        public Vector6 maxForce = new Vector6();

        public Vector6 maxTorque = new Vector6();

        public bool AllRcsEnabled
        {
            get { return this.allRcsEnabled; }
            protected set
            {
                this.AllRcsEnabledChanged = !(value == this.allRcsEnabled);
                this.allRcsEnabled = value;
            }
        }

        public bool AllRcsEnabledChanged
        {
            get { return this.allRcsEnabledChanged; }
            protected set { this.allRcsEnabledChanged = value; }
        }

        protected override void Calculate()
        {
            base.Calculate();
            bool tempAllRcsEnabled = true;
            this.maxTorque.Reset();
            this.maxForce.Reset();
            foreach (Part p in this.partList)
            {
                Vector3 r = SwitchTransformCalculator.VectorTransform(p.Rigidbody.worldCenterOfMass - this.coM, this.transformRight, this.transformDown, this.transformForward);
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleRCS)
                    {
                        ModuleRCS mrcs = (ModuleRCS)pm;
                        if (mrcs.isEnabled)
                        {
                            foreach (Transform trans in mrcs.thrusterTransforms)
                            {
                                Vector3 thrust = SwitchTransformCalculator.VectorTransform(trans.up, this.transformRight, this.transformDown, this.transformForward) * mrcs.thrusterPower;
                                Vector3 thrustTorque = -HMaths.CrossProduct(r, thrust);
                                this.maxForce.AddX(thrust.x);
                                this.maxForce.AddY(thrust.y);
                                this.maxForce.AddZ(thrust.z);
                                this.maxTorque.AddX(thrustTorque.x);
                                this.maxTorque.AddY(thrustTorque.y);
                                this.maxTorque.AddZ(thrustTorque.z);
                            }
                        }
                        else
                        {
                            tempAllRcsEnabled = false;
                        }
                    }
                }
            }
            this.AllRcsEnabled = tempAllRcsEnabled;
            this.maxAcc = this.maxForce / this.Mass;
            this.maxAngularAcc = this.maxTorque / this.moI.Diagonal;
        }

        public void EnableAllRcs()
        {
            foreach (Part p in this.partList) { foreach (PartModule pm in p.Modules) { if (pm is ModuleRCS && !((ModuleRCS)pm).isEnabled) { ((ModuleRCS)pm).Enable(); } } }
        }

        public override string ToString()
        {
            return "Mass = " + this.Mass + "\nForces = " + this.maxForce.ToString() + "\nAcc = " + this.maxAcc.ToString() + "\nMoI = " + this.moI.ToString() + "\nTorque = " + this.maxTorque.ToString() + "\nAAcc = " + this.maxAngularAcc.ToString();
        }

        public virtual string ToString(string format)
        {
            return "Mass = " + this.Mass.ToString(format) + "\nForces = " + this.maxForce.ToString(format) + "\nAcc = " + this.maxAcc.ToString(format) + "\nMoI = " + this.moI.ToString(format) + "\nTorque = " + this.maxTorque.ToString(format) + "\nAAcc = " + this.maxAngularAcc.ToString(format);
        }

        public float GetThrustRateFromAngularAcc6(int dir, float aA)
        {
            float maxAa = 0.0F;
            switch (dir)
            {
                case 0 /* xp = pitch- */:
                    maxAa = this.maxAngularAcc.xp;
                    break;
                case 1 /* xn = pitch+ */:
                    maxAa = this.maxAngularAcc.xn;
                    break;
                case 2 /* yp = yaw- */:
                    maxAa = this.maxAngularAcc.yp;
                    break;
                case 3 /* yn = yaw+ */:
                    maxAa = this.maxAngularAcc.yn;
                    break;
                case 4 /* zp = roll- */:
                    maxAa = this.maxAngularAcc.zp;
                    break;
                case 5 /* zn = roll+ */:
                    maxAa = this.maxAngularAcc.zn;
                    break;
                default:
                    maxAa = 0.0F;
                    break;
            }
            if (maxAa == 0.0F) { return 1.0F; }
            return aA / maxAa;
        }

        public float GetThrustRateFromAcc6(int dir, float a)
        {
            float maxA = 0.0F;
            switch (dir)
            {
                case 0 /* xp = right- */:
                    maxA = this.maxAcc.xp;
                    break;
                case 1 /* xn = right+ */:
                    maxA = this.maxAcc.xn;
                    break;
                case 2 /* yp = down- */:
                    maxA = this.maxAcc.yp;
                    break;
                case 3 /* yn = down+ */:
                    maxA = this.maxAcc.yn;
                    break;
                case 4 /* zp = forward- */:
                    maxA = this.maxAcc.zp;
                    break;
                case 5 /* zn = forward+ */:
                    maxA = this.maxAcc.zn;
                    break;
                default:
                    maxA = 0.0F;
                    break;
            }
            if (maxA == 0.0F) { return 1.0F; }
            return a / maxA;
        }

        public void MakeRotation(FlightCtrlState ctrlState, float angularAcc)
        {
            ctrlState.pitch *= GetThrustRateFromAngularAcc6(ctrlState.pitch >= 0 ? 1 : 0, angularAcc);
            ctrlState.yaw *= GetThrustRateFromAngularAcc6(ctrlState.yaw >= 0 ? 3 : 2, angularAcc);
            ctrlState.roll *= GetThrustRateFromAngularAcc6(ctrlState.roll >= 0 ? 5 : 4, angularAcc);
        }

        public void MakeTranslation(FlightCtrlState ctrlState, float acc)
        {
            ctrlState.X *= GetThrustRateFromAcc6(ctrlState.X >= 0 ? 0 : 1, acc);
            ctrlState.Y *= GetThrustRateFromAcc6(ctrlState.Y >= 0 ? 2 : 3, acc);
            ctrlState.Z *= GetThrustRateFromAcc6(ctrlState.Z >= 0 ? 4 : 5, acc);
        }

        public float GetThrustRateFromAngularAcc3(int dir, float aA)
        {
            bool pos = aA >= 0.0F;
            return GetThrustRateFromAngularAcc6((dir * 2) + (pos ? 1 : 0), aA);
        }

        public float GetThrustRateFromAcc3(int dir, float a)
        {
            bool pos = a >= 0.0F;
            return GetThrustRateFromAcc6((dir * 2) + (pos ? 0 : 1), a);
        }
    }
}