using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public class HydroPartModule : PartModule
    {
        public virtual void OnFlightStart() { }
        public virtual void OnDestroy() { }

        protected Vector3 ReverseTransform_PartConfig(Vector3 vec)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(
                vec,
                transform.right,
                transform.up,
                transform.forward
                );
        }

#if DEBUG
        new protected static void print(object message) { GameBehaviours.print(message); }
        protected static void warning(object message) { GameBehaviours.warning(message); }
        protected static void error(object message) { GameBehaviours.error(message); }
#else
        [Obsolete("Do not print anything on Release", true)]
        new protected static void print(object message) { }
#endif
    }
}