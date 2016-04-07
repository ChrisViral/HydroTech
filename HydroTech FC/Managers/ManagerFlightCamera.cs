#if DEBUG
#define SHOW_MANAGER_OPERATIONS
#endif

using System.Collections.Generic;
using UnityEngine;

namespace HydroTech_FC
{
    public static class HydroFlightCameraManager
    {
        private class Settings
        {
            private static Stack<Settings> settingsStack = new Stack<Settings>();

            public static void SaveCurrent()
            {
                settingsStack.Push(new Settings());
            }

            public static void RetrieveLast()
            {
                settingsStack.Pop().Set();
            }

            public static void AbandonLast()
            {
                settingsStack.Pop();
            }

            public static void UseLast()
            {
                settingsStack.Peek().Set();
            }

            public static void ResetAll()
            {
                settingsStack.Clear();
            }

            public Settings()
            {
                Get();
            }

            public Vessel vessel;
            public Transform tgt;
            public Transform parent;
            public float fov = 60.0F;
            public Vector3 position;
            public Quaternion rotation;
            public float clip = 0.01F;
            public Callback callback;

            public void Get()
            {
                this.vessel = ActiveVessel;
                this.tgt = GetTarget();
                this.parent = GetTransformParent();
                this.fov = GetFoV();
                this.position = GetPostition();
                this.rotation = GetRotation();
                this.clip = GetNearClipPlane();
                this.callback = GetCallback();
            }

            public void Set()
            {
                if (this.vessel == ActiveVessel)
                {
                    SetTarget(this.tgt);
                    SetTransformParent(this.parent);
                    SetFoV(this.fov);
                    SetPosition(this.position);
                    SetRotation(this.rotation);
                    SetNearClipPlane(this.clip);
                    SetCallback(this.callback);
                }
                else
                {
                    ResetToActiveVessel();
                }
            }

            public override string ToString()
            {
                return "Vessel is" + (this.vessel == ActiveVessel ? "" : " not") + " ActiveVessel\n" + (this.tgt == null ? "Null" : "Has") + " Target\n" + (this.parent == null ? "Null" : "Has") + " Parent\n" + "FoV = " + this.fov + "\n" + "Position = " + this.position + "\n" + "Rotation = " + this.rotation + "\n" + "Clip = " + this.clip + "\n" + "Is" + (this.callback == null ? " Not" : "") + " Callback";
            }

            public string ToString(string format)
            {
                return "Vessel is" + (this.vessel == ActiveVessel ? "" : " not") + " ActiveVessel\n" + (this.tgt == null ? "Null" : "Has") + " Target\n" + (this.parent == null ? "Null" : "Has") + " Parent\n" + "FoV = " + this.fov.ToString(format) + "\n" + "Position = " + this.position.ToString(format) + "\n" + "Rotation = " + this.rotation.ToString(format) + "\n" + "Clip = " + this.clip.ToString(format) + "\n" + "Is" + (this.callback == null ? " Not" : "") + " Callback";
            }

#if DEBUG
            public static int StackCount() { return settingsStack.Count; }
            public static Settings Top() { return settingsStack.Peek(); }
#endif
        }

        private static FlightCamera cam;

        private static Vessel ActiveVessel
        {
            get { return FlightGlobals.ActiveVessel; }
        }

        private static Vessel origVessel;
        private static Transform origParent;
        const float defaultFoV = 60.0F;
        const float defaultNearClip = 0.01F;

        public static void ResetToActiveVessel()
        {
#if SHOW_MANAGER_OPERATIONS
            Debug.Log("HydroFlightCameraManager: Setting to active vessel");
#endif
            SetTransformParent(origParent);
            SetFoV(defaultFoV);
            SetTarget(ActiveVessel);
            SetNearClipPlane(defaultNearClip);
            SetCallback(null);
        }

        public static void SaveCurrent()
        {
            Settings.SaveCurrent();
#if SHOW_MANAGER_OPERATIONS
            Debug.Log("HydroFlightCameraManager: Settings pushed into stack. Current count = " + Settings.StackCount());
#endif
        }

        public static void RetrieveLast()
        {
            Settings.RetrieveLast();
#if SHOW_MANAGER_OPERATIONS
            Debug.Log("HydroFlightCameraManager: Settings retrieved from stack. Current count = " + Settings.StackCount());
#endif
        }

        public static void OnFlightStart()
        {
            cam = (FlightCamera)Object.FindObjectOfType(typeof(FlightCamera));
            origVessel = ActiveVessel;
            target = ActiveVessel.transform;
            origParent = GetTransformParent();
            camCallback = null;
        }

        public static void OnUpdate()
        {
            if (origVessel != ActiveVessel && camCallback == null)
            {
                origVessel = ActiveVessel;
                origParent = GetTransformParent();
                target = ActiveVessel.transform;
            }
            if (camCallback != null) { camCallback(); }
        }

        private static Transform target;

        private static Transform Target
        {
            get { return target; }
            set
            {
                cam.setTarget(value);
                target = value;
            }
        }

        public static Transform GetTarget()
        {
            return Target;
        }

        public static void SetNullTarget()
        {
            Target = null;
        }

        public static void SetTarget(Transform tgt)
        {
            Target = tgt;
        }

        public static void SetTarget(Vessel v)
        {
            Target = v.transform;
        }

        public static void SetTarget(Part p)
        {
            Target = p.transform;
        }

        public static Transform GetTransformParent()
        {
            return cam.transform.parent;
        }

        public static void SetTransformParent(Transform parentTrans)
        {
            cam.transform.parent = parentTrans;
        }

        public static Vector3 GetPostition()
        {
            return cam.transform.localPosition;
        }

        public static void SetPosition(Vector3 r)
        {
            cam.transform.localPosition = r;
        }

        public static void SetPosition(float x, float y, float z)
        {
            cam.transform.localPosition.Set(x, y, z);
        }

        public static Quaternion GetRotation()
        {
            return cam.transform.localRotation;
        }

        public static void SetRotation(Quaternion quat)
        {
            cam.transform.localRotation = quat;
        }

        public static void SetRotation(Vector3 forward, Vector3 up)
        {
            SetRotation(Quaternion.LookRotation(forward, up));
        }

        public static float GetFoV()
        {
            return Camera.mainCamera.fov;
        }

        public static void SetFoV(float fov)
        {
            Camera.mainCamera.fov = fov;
        }

        public static float GetNearClipPlane()
        {
            return Camera.mainCamera.nearClipPlane;
        }

        public static void SetNearClipPlane(float clip)
        {
            Camera.mainCamera.nearClipPlane = clip;
        }

        private static Callback camCallback;

        public static Callback GetCallback()
        {
            return camCallback;
        }

        public static void SetCallback(Callback callback)
        {
            camCallback = callback;
        }

#if DEBUG
        public static string StringCameraState() { return new Settings().ToString("#0.00"); }
        public static void PrintCameraState() { Debug.Log(StringCameraState()); }

        public static string StringCameraStack()
        {
            return "Stack count = " + Settings.StackCount();
        }
        public static void PrintCameraStack() { Debug.Log(StringCameraStack()); }

        public static string StringTopState()
        {
            return Settings.StackCount() == 0 ? "" : Settings.Top().ToString("#0.00");
        }

        public static void PrintTopState() { Debug.Log(StringTopState()); }
#endif
    }
}