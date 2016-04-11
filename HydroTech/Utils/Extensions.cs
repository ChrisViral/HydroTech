using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydroTech.Utils
{
    internal static class Extensions
    {
        #region VesselExtensions
        //From https://github.com/MuMech/MechJeb2/blob/master/MechJeb2/VesselExtensions.cs#L47-L66
        private static float lastFixedTime;
        private static readonly Dictionary<Guid, HydroJebCore> coreCache = new Dictionary<Guid, HydroJebCore>();

        public static HydroJebCore GetMasterJeb(this Vessel vessel)
        {
            if (lastFixedTime != Time.fixedTime)
            {
                coreCache.Clear();
                lastFixedTime = Time.fixedTime;
            }

            Guid vesselKey = vessel.id;
            HydroJebCore jeb;
            if (!coreCache.TryGetValue(vesselKey, out jeb))
            {
                jeb = vessel.FindPartModulesImplementing<HydroJebCore>().FirstOrDefault();
                if (jeb != null) { coreCache.Add(vesselKey, jeb); }
            }
            return jeb;
        }
        #endregion
    }
}
