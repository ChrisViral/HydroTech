using HydroTech_FC;
using UnityEngine;

namespace HydroTech_RCS.PartModules.Base
{
    public class HydroPartModule : PartModule
    {
        #region Methods
        protected Vector3 ReverseTransform_PartConfig(Vector3 vec)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, this.transform.right, this.transform.up, this.transform.forward);
        }
        #endregion

        #region Virtual methods
        public virtual void OnFlightStart() { }

        public virtual void OnDestroy() { }
        #endregion        
    }
}