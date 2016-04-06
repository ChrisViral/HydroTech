#if DEBUG
#define SHOW_MANAGER_OPERATIONS
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public static class HydroFlightCameraManager
    {
        private class Settings
        {
            private static Stack<Settings> settingsStack = new Stack<Settings>();
            public static void SaveCurrent() { settingsStack.Push(new Settings()); }
            public static void RetrieveLast() { settingsStack.Pop().Set(); }
            public static void AbandonLast() { settingsStack.Pop(); }
            public static void UseLast() { settingsStack.Peek().Set(); }
            public static void ResetAll() { settingsStack.Clear(); }

            public Settings() { Get(); }

            public Vessel vessel = null;
            public Transform tgt = null;
            public Transform parent = null;
            public float fov = 60.0F;
            public Vector3 position = new Vector3();
            public Quaternion rotation = new Quaternion();
            public float clip = 0.01F;
            public Callback callback = null;

            public void Get()
            {
                vessel = ActiveVessel;
                tgt = GetTarget();
                parent = GetTransformParent();
                fov = GetFoV();
                position = GetPostition();
                rotation = GetRotation();
                clip = GetNearClipPlane();
                callback = GetCallback();
            }

            public void Set()
            {
                if (vessel == ActiveVessel)
                {
                    SetTarget(tgt);
                    SetTransformParent(parent);
                    SetFoV(fov);
                    SetPosition(position);
                    SetRotation(rotation);
                    SetNearClipPlane(clip);
                    SetCallback(callback);
                }
                else
                    ResetToActiveVessel();
            }

            public override string ToString()
            {
                return "Vessel is" + (vessel == ActiveVessel ? "" : " not") + " ActiveVessel\n"
                    + (tgt == null ? "Null" : "Has") + " Target\n"
                    + (parent == null ? "Null" : "Has") + " Parent\n"
                    + "FoV = " + fov.ToString() + "\n"
                    + "Position = " + position.ToString() + "\n"
                    + "Rotation = " + rotation.ToString() + "\n"
                    + "Clip = " + clip.ToString() + "\n"
                    + "Is" + (callback == null ? " Not" : "") + " Callback";
            }
            public string ToString(string format)
            {
                return "Vessel is" + (vessel == ActiveVessel ? "" : " not") + " ActiveVessel\n"
                    + (tgt == null ? "Null" : "Has") + " Target\n"
                    + (parent == null ? "Null" : "Has") + " Parent\n"
                    + "FoV = " + fov.ToString(format) + "\n"
                    + "Position = " + position.ToString(format) + "\n"
                    + "Rotation = " + rotation.ToString(format) + "\n"
                    + "Clip = " + clip.ToString(format) + "\n"
                    + "Is" + (callback == null ? " Not" : "") + " Callback";
            }

#if DEBUG
            public static int StackCount() { return settingsStack.Count; }
            public static Settings Top() { return settingsStack.Peek(); }
#endif
        }

        private static FlightCamera cam = null;

        private static Vessel ActiveVessel { get { return GameStates.ActiveVessel; } }
        private static Vessel origVessel = null;
        private static Transform origParent = null;
        const float DefaultFoV = 60.0F;
        const float DefaultNearClip = 0.01F;
        public static void ResetToActiveVessel()
        {
#if SHOW_MANAGER_OPERATIONS
            print("HydroFlightCameraManager: Setting to active vessel");
#endif
            SetTransformParent(origParent);
            SetFoV(DefaultFoV);
            SetTarget(ActiveVessel);
            SetNearClipPlane(DefaultNearClip);
            SetCallback(null);
        }

        public static void SaveCurrent()
        {
            Settings.SaveCurrent();
#if SHOW_MANAGER_OPERATIONS
            print("HydroFlightCameraManager: Settings pushed into stack. Current count = " + Settings.StackCount());
#endif
        }
        public static void RetrieveLast()
        {
            Settings.RetrieveLast();
#if SHOW_MANAGER_OPERATIONS
            print("HydroFlightCameraManager: Settings retrieved from stack. Current count = " + Settings.StackCount());
#endif
        }

        public static void onFlightStart()
        {
            cam = (FlightCamera)GameObject.FindObjectOfType(typeof(FlightCamera));
            origVessel = ActiveVessel;
            _Target = ActiveVessel.transform;
            origParent = GetTransformParent();
            camCallback = null;
        }

        public static void OnUpdate()
        {
            if (origVessel != ActiveVessel && camCallback == null)
            {
                origVessel = ActiveVessel;
                origParent = GetTransformParent();
                _Target = ActiveVessel.transform;
            }
            if (camCallback != null)
                camCallback();
        }

        private static Transform _Target = null;
        private static Transform Target
        {
            get { return _Target; }
            set
            {
                cam.setTarget(value);
                _Target = value;
            }
        }
        public static Transform GetTarget() { return Target; }
        public static void SetNullTarget() { Target = null; }
        public static void SetTarget(Transform tgt) { Target = tgt; }
        public static void SetTarget(Vessel v) { Target = v.transform; }
        public static void SetTarget(Part p) { Target = p.transform; }

        public static Transform GetTransformParent() { return cam.transform.parent; }
        public static void SetTransformParent(Transform parentTrans) { cam.transform.parent = parentTrans; }

        public static Vector3 GetPostition() { return cam.transform.localPosition; }
        public static void SetPosition(Vector3 r) { cam.transform.localPosition = r; }
        public static void SetPosition(float x, float y, float z) { cam.transform.localPosition.Set(x, y, z); }

        public static Quaternion GetRotation() { return cam.transform.localRotation; }
        public static void SetRotation(Quaternion quat) { cam.transform.localRotation = quat; }
        public static void SetRotation(Vector3 forward, Vector3 up)
        {
            SetRotation(Quaternion.LookRotation(forward, up));
        }

        public static float GetFoV() { return Camera.mainCamera.fov; }
        public static void SetFoV(float fov) { Camera.mainCamera.fov = fov; }

        public static float GetNearClipPlane() { return Camera.mainCamera.nearClipPlane; }
        public static void SetNearClipPlane(float clip) { Camera.mainCamera.nearClipPlane = clip; }

        private static Callback camCallback = null;
        public static Callback GetCallback() { return camCallback; }
        public static void SetCallback(Callback callback) { camCallback = callback; }

#if DEBUG
        private static void print(object message) { GameBehaviours.print(message); }

        public static String StringCameraState() { return new Settings().ToString("#0.00"); }
        public static void PrintCameraState() { print(StringCameraState()); }

        public static String StringCameraStack()
        {
            return "Stack count = " + Settings.StackCount().ToString();
        }
        public static void PrintCameraStack() { print(StringCameraStack()); }

        public static String StringTopState()
        {
            if (Settings.StackCount() == 0)
                return "";
            else
                return Settings.Top().ToString("#0.00");
        }
        public static void PrintTopState() { print(StringTopState()); }
#endif
    }
}