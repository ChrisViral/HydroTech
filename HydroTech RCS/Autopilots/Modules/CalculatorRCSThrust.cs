using HydroTech_FC;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.Modules
{
    public class CalculatorRcsThrust : CalculatorVesselInfoBasic
    {
        #region Fields
        public Vector6 maxAcc = new Vector6();
        public Vector6 maxAngularAcc = new Vector6();
        public Vector6 maxForce = new Vector6();
        public Vector6 maxTorque = new Vector6();
        #endregion

        #region Properties
        protected bool allRcsEnabled = true;
        public bool AllRcsEnabled
        {
            get { return this.allRcsEnabled; }
            protected set
            {
                this.AllRcsEnabledChanged = value != this.allRcsEnabled;
                this.allRcsEnabled = value;
            }
        }

        protected bool allRcsEnabledChanged;
        public bool AllRcsEnabledChanged
        {
            get { return this.allRcsEnabledChanged; }
            protected set { this.allRcsEnabledChanged = value; }
        }
        #endregion

        #region Methods
        protected override void Calculate()
        {
            base.Calculate();
            bool tempAllRcsEnabled = true;
            this.maxTorque.Reset();
            this.maxForce.Reset();
            foreach (Part p in this.partList)
            {
                Vector3 r = SwitchTransformCalculator.VectorTransform(p.GetComponent<Rigidbody>().worldCenterOfMass - this.CoM, this.transformRight, this.transformDown, this.transformForward);
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleRCS)
                    {
                        ModuleRCS rcs = (ModuleRCS)pm;
                        if (rcs.isEnabled)
                        {
                            foreach (Transform trans in rcs.thrusterTransforms)
                            {
                                Vector3 thrust = SwitchTransformCalculator.VectorTransform(trans.up, this.transformRight, this.transformDown, this.transformForward) * rcs.thrusterPower;
                                Vector3 thrustTorque = -HMaths.CrossProduct(r, thrust);
                                this.maxForce.AddX(thrust.x);
                                this.maxForce.AddY(thrust.y);
                                this.maxForce.AddZ(thrust.z);
                                this.maxTorque.AddX(thrustTorque.x);
                                this.maxTorque.AddY(thrustTorque.y);
                                this.maxTorque.AddZ(thrustTorque.z);
                            }
                        }
                        else { tempAllRcsEnabled = false; }
                    }
                }
            }
            this.AllRcsEnabled = tempAllRcsEnabled;
            this.maxAcc = this.maxForce / this.Mass;
            this.maxAngularAcc = this.maxTorque / this.moI.Diagonal;
        }

        public void EnableAllRcs()
        {
            foreach (Part p in this.partList)
            {
                foreach (PartModule pm in p.Modules)
                {
                    ModuleRCS rcs = pm as ModuleRCS;
                    if (rcs != null && !rcs.isEnabled) { rcs.Enable(); }
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Mass: {0}\nForces: {1}\nAcc: {2}\nMoI: {3}\nTorque: {4}\nAAcc: {5}", this.Mass, this.maxForce, this.maxAcc, this.moI, this.maxTorque, this.maxAngularAcc);
        }

        public virtual string ToString(string format)
        {
            return string.Format("Mass: {0}\nForces: {1}\nAcc: {2}\nMoI: {3}\nTorque: {4}\nAAcc: {5}", this.Mass.ToString(format), this.maxForce.ToString(format), this.maxAcc.ToString(format), this.moI.ToString(format), this.maxTorque.ToString(format), this.maxAngularAcc.ToString(format));
        }

        public float GetThrustRateFromAngularAcc6(int dir, float aA)
        {
            float maxAa = 0;
            switch (dir)
            {
                case 0: //xp = pitch-
                    maxAa = this.maxAngularAcc.xp;
                    break;
                case 1: //xn = pitch+
                    maxAa = this.maxAngularAcc.xn;
                    break;
                case 2: //yp = yaw-
                    maxAa = this.maxAngularAcc.yp;
                    break;
                case 3: //yn = yaw+
                    maxAa = this.maxAngularAcc.yn;
                    break;
                case 4: //zp = roll-
                    maxAa = this.maxAngularAcc.zp;
                    break;
                case 5: //zn = roll+
                    maxAa = this.maxAngularAcc.zn;
                    break;
            }
            return maxAa == 0 ? 1:  aA / maxAa;
        }

        public float GetThrustRateFromAcc6(int dir, float a)
        {
            float maxA = 0;
            switch (dir)
            {
                case 0: //xp = right
                    maxA = this.maxAcc.xp;
                    break;
                case 1: //xn = right
                    maxA = this.maxAcc.xn;
                    break;
                case 2: //yp = down-
                    maxA = this.maxAcc.yp;
                    break;
                case 3: //yn = down+
                    maxA = this.maxAcc.yn;
                    break;
                case 4: //zp = forward-
                    maxA = this.maxAcc.zp;
                    break;
                case 5: //zn = forward+
                    maxA = this.maxAcc.zn;
                    break;
            }
            return maxA == 0 ? 1 : a / maxA;
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
    }
}