using System;
using System.Collections.Generic;
using UnityEngine;
using static System.Linq.Enumerable;
using static HydroTech.Autopilots.APTranslation.TranslationDirection;
using TranslationDirection = HydroTech.Autopilots.APTranslation.TranslationDirection;

namespace HydroTech.Extensions
{
    /// <summary>
    /// CelestialBody extension methods
    /// </summary>
    internal static class CelestialBodyExtensions
    {
        #region Extension methods
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
    }

    /// <summary>
    /// Enum extension methods
    /// </summary>
    internal static class EnumExtensions
    {
        #region Extension methods
        /// <summary>
        /// Gets the unit vector associated to this TranslationDirection
        /// </summary>
        /// <param name="dir">Direction to get the vector for</param>
        /// <returns>The associated unit vector to this direction</returns>
        public static Vector3 GetUnitVector(this TranslationDirection dir)
        {
            switch (dir)
            {
                case RIGHT:
                    return Vector3.right;

                case LEFT:
                    return Vector3.left;

                case DOWN:
                    return Vector3.up;

                case UP:
                    return Vector3.down;

                case FORWARD:
                    return Vector3.forward;

                case BACK:
                    return Vector3.back;
            }

            return Vector3.zero;
        }
        #endregion
    }

    /// <summary>
    /// Part extension methods
    /// </summary>
    internal static class PartExtensions
    {
        #region Extension methods
        /// <summary>
        /// Returns all the PartModules implementing <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Implementation type to look for</typeparam>
        /// <param name="parts">Parts list to search</param>
        /// <returns>Streaming enumerable of the found <typeparamref name="T"/></returns>
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

        /// <summary>
        /// Returns true only when the part is not fully physically insignificant
        /// </summary>
        /// <param name="part">Part to get physical significance for</param>
        /// <returns>If the part is physically significant</returns>
        public static bool IsPhysicallySignificant(this Part part) => part.physicalSignificance != Part.PhysicalSignificance.NONE;

        /// <summary>
        /// Returns the total mass of the part, including resources, and it's physical significance
        /// </summary>
        /// <param name="part">Part to get the mass from</param>
        /// <returns>Total mass of the part. Zero if the part is physically insignifiant</returns>
        public static float TotalMass(this Part part) => part.IsPhysicallySignificant() ? 0 : part.mass + part.GetResourceMass();
        #endregion
    }

    /// <summary>
    /// Vessel extension methods
    /// </summary>
    internal static class VesselExtensions
    {
        #region Extension methods
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

            HydroJebCore jeb;
            if (!coreCache.TryGetValue(vessel.id, out jeb))
            {
                jeb = vessel.FindModulesImplementing<HydroJebCore>().FirstOrDefault();
                if (jeb != null) { coreCache.Add(vessel.id, jeb); }
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

        /// <summary>
        /// Returns all the PartModules implementing <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Implementation type to look for</typeparam>
        /// <param name="vessel">Vessel to search</param>
        /// <returns>Streaming enumerable of the found <typeparamref name="T"/></returns>
        public static IEnumerable<T> FindModulesImplementing<T>(this Vessel vessel) where T : PartModule => vessel.parts.FindModulesImplementing<T>();
        #endregion
    }
}