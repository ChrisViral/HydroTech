using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public class HydroPartModule : PartModule
    {
        virtual public void OnFlightStart() { }
        virtual public void OnDestroy() { }

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
        new static protected void print(object message) { GameBehaviours.print(message); }
        static protected void warning(object message) { GameBehaviours.warning(message); }
        static protected void error(object message) { GameBehaviours.error(message); }
#else
        [Obsolete("Do not print anything on Release", true)]
        new static protected void print(object message) { }
#endif
    }
}