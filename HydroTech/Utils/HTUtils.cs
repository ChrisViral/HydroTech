using System;
using System.IO;
using System.Linq;
using HydroTech.Autopilots;
using HydroTech.Autopilots.Calculators;
using UnityEngine;
using TranslationDirection = HydroTech.Autopilots.APTranslation.TranslationDirection;

namespace HydroTech.Utils
{
    public static class HTUtils
    {
        #region Constants
        public const float radToDeg = 180 / Mathf.PI;
        public const string localPluginDataURL = "GameData/HydroTech/Plugins/PluginData";
        public const string localIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_icon.png";
        public const string localActiveIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_active_icon.png";
        public const string localInactiveIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_inactive_icon.png";
        #endregion

        #region Properties
        private static readonly string pluginDataURL;
        public static string PluginDataURL
        {
            get { return pluginDataURL; }
        }

        private static readonly Texture2D launcherIcon;
        public static Texture2D LauncherIcon
        {
            get { return launcherIcon; }
        }

        private static readonly Texture2D activeIcon;
        public static Texture2D ActiveIcon
        {
            get { return activeIcon; }
        }

        private static readonly Texture2D inactiveIcon;
        public static Texture2D InactiveIcon
        {
            get { return inactiveIcon; }
        }

        private static readonly int electricChargeID;
        public static int ElectricChargeID
        {
            get { return electricChargeID; }
        }
        #endregion

        #region Constructor
        static HTUtils()
        {
            pluginDataURL = Path.Combine(KSPUtil.ApplicationRootPath, localPluginDataURL);

            launcherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            launcherIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localIconURL)));

            activeIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            activeIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localActiveIconURL)));

            inactiveIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            inactiveIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localInactiveIconURL)));

            electricChargeID = PartResourceLibrary.Instance.resourceDefinitions.First(r => r.name == "ElectricCharge").id;
        }
        #endregion

        #region Methods
        public static float Clamp(float x, float min, float max)
        {
            if (x >= max) { return max; }
            return x <= min ? min : x;
        }

        public static float Clamp0(float f)
        {
            return f > 0 ? f : 0;
        }

        public static float Clamp100(float f)
        {
            return f < 100 ? f : 100;
        }

        public static bool GetState(Vessel vessel, KSPActionGroup action)
        {
            return vessel.ActionGroups[action];
        }

        public static void SetState(Vessel vessel, KSPActionGroup action, bool active)
        {
            vessel.ActionGroups.SetGroup(action, active);
        }

        public static float GetBodySyncAltitude(CelestialBody body)
        {
            return (float)Math.Pow((body.gravParameter * body.rotationPeriod * body.rotationPeriod) / (4 * Math.PI * Math.PI), 1d / 3);
        }

        public static void CamToVesselRot(FlightCtrlState ctrlState, ModuleDockAssist mcam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetRotation(ctrlState);
            sCal.ChangeTransformRotation(mcam.Right, mcam.Down, mcam.Dir, mcam.vessel.ReferenceTransform);
            sCal.SetRotation(ctrlState);
        }

        public static void CamToVesselTrans(FlightCtrlState ctrlState, ModuleDockAssist mcam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetTranslation(ctrlState);
            sCal.ChangeTransformTranslation(mcam.Right, mcam.Down, mcam.Dir, mcam.vessel.ReferenceTransform);
            sCal.SetTranslation(ctrlState);
        }

        public static Vector3 GetUnitVector(TranslationDirection dir)
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

        public static Vector3 VectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return SwitchTransformCalculator.VectorTransform(vec, x, y, z);
        }

        public static Vector3 VectorTransform(Vector3 vec, Transform trans)
        {
            return SwitchTransformCalculator.VectorTransform(vec, trans);
        }

        public static Vector3 ReverseVectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, x, y, z);
        }

        public static Vector3 ReverseVectorTransform(Vector3 vec, Transform trans)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, trans);
        }
        #endregion
    }
}