using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.Modules
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;

    public class CalculatorEngineThrust : CalculatorVesselInfoBasic
    {
        protected Vector3 chosenDir = Vector3.down;
        public void SetVector(Vector3 vec) { chosenDir = vec; }
        public void SetWorldVector(Vector3 wrldVec)
        {
            chosenDir = SwitchTransformCalculator.VectorTransform(
                wrldVec,
                transformRight,
                transformDown,
                transformForward
                );
        }

        protected float _MinThrust = 0.0F;
        public float MinThrust
        {
            get { return _MinThrust; }
            protected set { _MinThrust = value; }
        }
        public float MinAcc { get { return MinThrust / Mass; } }
        protected float _MaxThrust = 0.0F;
        public float MaxThrust
        {
            get { return _MaxThrust; }
            protected set { _MaxThrust = value; }
        }
        public float MaxAcc { get { return MaxThrust / Mass; } }

        protected override void Calculate()
        {
            base.Calculate();
            MinThrust = 0;
            MaxThrust = 0;
            foreach (Part p in partList)
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
                                Vector3 thrustVector = SwitchTransformCalculator.VectorTransform(
                                    trans.up,
                                    transformRight,
                                    transformDown,
                                    transformForward
                                    );
                                float thrustUnit = 1;// HMaths.DotProduct(thrustVector, chosenDir);
                                if (thrustUnit > 0.0F)
                                {
                                    MinThrust += thrustUnit * meng.minThrust;
                                    MaxThrust += thrustUnit * meng.maxThrust;
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