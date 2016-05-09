using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TranslationDirection = HydroTech.Autopilots.APTranslation.TranslationDirection;

namespace HydroTech.Utils
{
    /// <summary>
    /// Extensions class
    /// </summary>
    internal static class Extensions
    {
        #region VesselExtensions
        private static float lastFixedTime;     //Last FixedUpdate frame, prevents recalculation multiple time per frame
        private static readonly Dictionary<Guid, HydroJebCore> coreCache = new Dictionary<Guid, HydroJebCore>();    //Module cache
        /// <summary>
        /// Obtains the controlling/active HydroJebCore of a vessel
        /// From https://github.com/MuMech/MechJeb2/blob/master/MechJeb2/VesselExtensions.cs#L47-L66
        /// </summary>
        /// <param name="vessel">Vessel to get the master core from</param>
        /// <returns>The master HydroJebCore of the vessel</returns>
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

        /// <summary>
        /// Gets the current state of the specified ActionGroup
        /// </summary>
        /// <param name="vessel">Vessel to get the state for</param>
        /// <param name="action">ActionGroup to get the state for</param>
        /// <returns>Current state of the action group</returns>
        public static bool GetState(this Vessel vessel, KSPActionGroup action) => vessel.ActionGroups[action];

        /// <summary>
        /// Sets the current state of the specified ActionGroup
        /// </summary>
        /// <param name="vessel">Vessel to set the state for</param>
        /// <param name="action">ActionGroup to set the state for</param>
        /// <param name="active">State to set the ActionGroup to</param>
        public static void SetState(this Vessel vessel, KSPActionGroup action, bool active) => vessel.ActionGroups.SetGroup(action, active);
        #endregion

        #region CelestialBodyExtensions
        private static readonly Dictionary<CelestialBody, float> syncAlts = new Dictionary<CelestialBody, float>();     //Geosync alt cache
        /// <summary>
        /// Gets the geosynchronous altitude for the specified body
        /// </summary>
        /// <param name="body">Body to get the geosync alt for</param>
        /// <returns>Geosynchronous altitude of the body</returns>
        public static float SyncAltitude(this CelestialBody body)
        {
            float alt;
            if (!syncAlts.TryGetValue(body, out alt))
            {
                alt = (float)Math.Pow((body.gravParameter * body.rotationPeriod * body.rotationPeriod) / (4 * Math.PI * Math.PI), 1 / 3d);
                syncAlts.Add(body, alt);
            }
            return alt;
        }
        #endregion

        #region EnumExtensions
        /// <summary>
        /// Gets the unit vector associated to this TranslationDirection
        /// </summary>
        /// <param name="dir">Direction to get the vector for</param>
        /// <returns>The associated unit vector to this direction</returns>
        public static Vector3 GetUnitVector(this TranslationDirection dir)
        {
            switch (dir)
            {
                case TranslationDirection.RIGHT:
                    return Vector3.right;

                case TranslationDirection.LEFT:
                    return Vector3.left;

                case TranslationDirection.DOWN:
                    return Vector3.up;

                case TranslationDirection.UP:
                    return Vector3.down;

                case TranslationDirection.FORWARD:
                    return Vector3.forward;

                case TranslationDirection.BACK:
                    return Vector3.back;
            }

            return Vector3.zero;
        }
        #endregion

        #region PartExtensions
        public static IEnumerable<T> FindModulesImplementing<T>(this IEnumerable<Part> parts) where T : PartModule
        {
            foreach (Part p in parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is T) { yield return (T)pm; }
                }
            }
        } 
        #endregion
    }
}
