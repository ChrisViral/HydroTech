using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class EngineCalculator : VesselInfoCalculatorBase
    {
        #region Fields
        protected Vector3 chosenDir = Vector3.down;
        #endregion

        #region Properties
        protected float minThrust;
        public float MinThrust
        {
            get { return this.minThrust; }
            protected set { this.minThrust = value; }
        }

        protected float maxThrust;
        public float MaxThrust
        {
            get { return this.maxThrust; }
            protected set { this.maxThrust = value; }
        }

        public float MinAcc => this.MinThrust / this.Mass;

        public float MaxAcc => this.MaxThrust / this.Mass;
        #endregion

        #region Methods
        public void SetVector(Vector3 vec)
        {
            this.chosenDir = vec;
        }

        public void SetWorldVector(Vector3 wrldVec)
        {
            this.chosenDir = SwitchTransformCalculator.VectorTransform(wrldVec, this.transformRight, this.transformDown, this.transformForward);
        }

        public void OnUpdate(Vessel targetVessel, Vector3 dir)
        {
            SetVessel(targetVessel);
            SetVector(dir);
            Calculate();
        }
        #endregion

        #region Overrides
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
                        ModuleEngines eng = (ModuleEngines)pm;
                        if (eng.EngineIgnited)
                        {
                            int transforms = eng.thrustTransforms.Count;
                            this.MinThrust += transforms * eng.minThrust;
                            this.MaxThrust += transforms * eng.maxThrust;

                            //This is weird.
                            /* foreach (Transform trans in eng.thrustTransforms)
                             * {
                             *     Vector3 thrustVector = SwitchTransformCalculator.VectorTransform(trans.up, this.transformRight, this.transformDown, this.transformForward);
                             *     float thrustUnit = 1; // Vector3.Dot(thrustVector, chosenDir);
                             *     if (thrustUnit > 0)
                             *     {
                             *         this.MinThrust += thrustUnit * eng.minThrust;
                             *         this.MaxThrust += thrustUnit * eng.maxThrust;
                             *     }
                             * }*/
                        }
                    }
                }
            }
        }
        #endregion
    }
}