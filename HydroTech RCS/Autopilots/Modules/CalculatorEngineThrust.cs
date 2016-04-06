using HydroTech_FC;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.Modules
{
    public class CalculatorEngineThrust : CalculatorVesselInfoBasic
    {
        protected float maxThrust;

        protected float minThrust;
        protected Vector3 chosenDir = Vector3.down;

        public float MinThrust
        {
            get { return this.minThrust; }
            protected set { this.minThrust = value; }
        }

        public float MinAcc
        {
            get { return this.MinThrust / this.Mass; }
        }

        public float MaxThrust
        {
            get { return this.maxThrust; }
            protected set { this.maxThrust = value; }
        }

        public float MaxAcc
        {
            get { return this.MaxThrust / this.Mass; }
        }

        public void SetVector(Vector3 vec)
        {
            this.chosenDir = vec;
        }

        public void SetWorldVector(Vector3 wrldVec)
        {
            this.chosenDir = SwitchTransformCalculator.VectorTransform(wrldVec, this.transformRight, this.transformDown, this.transformForward);
        }

        protected override void Calculate()
        {
            base.Calculate();
            this.MinThrust = 0;
            this.MaxThrust = 0;
            foreach (Part p in this.partList)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleEngines)
                    {
                        ModuleEngines meng = (ModuleEngines)pm;
                        if (meng.isEnabled)
                        {
                            foreach (Transform trans in meng.thrustTransforms)
                            {
                                Vector3 thrustVector = SwitchTransformCalculator.VectorTransform(trans.up, this.transformRight, this.transformDown, this.transformForward);
                                float thrustUnit = 1; // HMaths.DotProduct(thrustVector, chosenDir);
                                if (thrustUnit > 0.0F)
                                {
                                    this.MinThrust += thrustUnit * meng.minThrust;
                                    this.MaxThrust += thrustUnit * meng.maxThrust;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OnUpdate(Vessel targetVessel, Vector3 dir)
        {
            SetVessel(targetVessel);
            SetVector(dir);
            Calculate();
        }
    }
}