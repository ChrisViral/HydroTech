using System;
using UnityEngine;

namespace HydroTech_FC
{
    public class HydroPartModule : PartModule
    {
        public virtual void OnFlightStart() { }

        public virtual void OnDestroy() { }

        protected Vector3 ReverseTransform_PartConfig(Vector3 vec)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, this.transform.right, this.transform.up, this.transform.forward);
        }

#if DEBUG
        new protected static void print(object message) { GameBehaviours.print(message); }
        protected static void warning(object message) { GameBehaviours.warning(message); }
        protected static void error(object message) { GameBehaviours.error(message); }
#else
        [Obsolete("Do not print anything on Release", true)]
        protected static void Print(object message) { }
#endif
    }
}