using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public const string localIconURL = "GameData/HydroTech/Plugins/PluginData/default.png";
        public const string localActiveIconURL = "GameData/HydroTech/Plugins/PluginData/active.png";
        public const string localInactiveIconURL = "GameData/HydroTech/Plugins/PluginData/inactive.png";
        private static readonly Vector2 anchor = new Vector2(0.5f, 0.5f);
        #endregion

        #region Properties
        public static string PluginDataURL { get; }

        public static Texture2D LauncherIcon { get; }

        public static Texture2D ActiveIcon { get; }

        public static Texture2D InactiveIcon { get; }

        public static int ElectricChargeID { get; }

        public static PartResourceDefinition Electricity { get; }

        public static List<PartResourceDefinition> ElectrictyList { get; }
        #endregion

        #region Constructor
        static HTUtils()
        {
            PluginDataURL = Path.Combine(KSPUtil.ApplicationRootPath, localPluginDataURL);

            LauncherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            LauncherIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localIconURL)));

            ActiveIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            ActiveIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localActiveIconURL)));

            InactiveIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            InactiveIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localInactiveIconURL)));

            Electricity = PartResourceLibrary.Instance.resourceDefinitions.First(r => r.name == "ElectricCharge");
            ElectricChargeID = Electricity.id;
            ElectrictyList = new List<PartResourceDefinition>(1) { Electricity };
        }
        #endregion

        #region Methods
        public static float Clamp(float x, float min, float max)
        {
            if (x >= max) { return max; }
            return x <= min ? min : x;
        }

        public static float Clamp0(float f) => f > 0 ? f : 0;

        public static float Clamp100(float f) => f < 100 ? f : 100;

        public static void SpawnPopupDialog(string title, string message, string button)
        {
            PopupDialog.SpawnPopupDialog(anchor, anchor, title, message, button, false, HighLogic.UISkin);
        }

        public static bool GetState(Vessel vessel, KSPActionGroup action) => vessel.ActionGroups[action];

        public static void SetState(Vessel vessel, KSPActionGroup action, bool active) => vessel.ActionGroups.SetGroup(action, active);

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