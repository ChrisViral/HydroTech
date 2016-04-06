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

    static public class HydroFlightCameraManager
    {
        private class Settings
        {
            static private Stack<Settings> settingsStack = new Stack<Settings>();
            static public void SaveCurrent() { settingsStack.Push(new Settings()); }
            static public void RetrieveLast() { settingsStack.Pop().Set(); }
            static public void AbandonLast() { settingsStack.Pop(); }
            static public void UseLast() { settingsStack.Peek().Set(); }
            static public void ResetAll() { settingsStack.Clear(); }

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
            static public int StackCount() { return settingsStack.Count; }
            static public Settings Top() { return settingsStack.Peek(); }
#endif
        }

        static private FlightCamera cam = null;

        static private Vessel ActiveVessel { get { return GameStates.ActiveVessel; } }
        static private Vessel origVessel = null;
        static private Transform origParent = null;
        const float DefaultFoV = 60.0F;
        const float DefaultNearClip = 0.01F;
        static public void ResetToActiveVessel()
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

        static public void SaveCurrent()
        {
            Settings.SaveCurrent();
#if SHOW_MANAGER_OPERATIONS
            print("HydroFlightCameraManager: Settings pushed into stack. Current count = " + Settings.StackCount());
#endif
        }
        static public void RetrieveLast()
        {
            Settings.RetrieveLast();
#if SHOW_MANAGER_OPERATIONS
            print("HydroFlightCameraManager: Settings retrieved from stack. Current count = " + Settings.StackCount());
#endif
        }

        static public void onFlightStart()
        {
            cam = (FlightCamera)GameObject.FindObjectOfType(typeof(FlightCamera));
            origVessel = ActiveVessel;
            _Target = ActiveVessel.transform;
            origParent = GetTransformParent();
            camCallback = null;
        }

        static public void OnUpdate()
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

        static private Transform _Target = null;
        static private Transform Target
        {
            get { return _Target; }
            set
            {
                cam.setTarget(value);
                _Target = value;
            }
        }
        static public Transform GetTarget() { return Target; }
        static public void SetNullTarget() { Target = null; }
        static public void SetTarget(Transform tgt) { Target = tgt; }
        static public void SetTarget(Vessel v) { Target = v.transform; }
        static public void SetTarget(Part p) { Target = p.transform; }

        static public Transform GetTransformParent() { return cam.transform.parent; }
        static public void SetTransformParent(Transform parentTrans) { cam.transform.parent = parentTrans; }

        static public Vector3 GetPostition() { return cam.transform.localPosition; }
        static public void SetPosition(Vector3 r) { cam.transform.localPosition = r; }
        static public void SetPosition(float x, float y, float z) { cam.transform.localPosition.Set(x, y, z); }

        static public Quaternion GetRotation() { return cam.transform.localRotation; }
        static public void SetRotation(Quaternion quat) { cam.transform.localRotation = quat; }
        static public void SetRotation(Vector3 forward, Vector3 up)
        {
            SetRotation(Quaternion.LookRotation(forward, up));
        }

        static public float GetFoV() { return Camera.mainCamera.fov; }
        static public void SetFoV(float fov) { Camera.mainCamera.fov = fov; }

        static public float GetNearClipPlane() { return Camera.mainCamera.nearClipPlane; }
        static public void SetNearClipPlane(float clip) { Camera.mainCamera.nearClipPlane = clip; }

        static private Callback camCallback = null;
        static public Callback GetCallback() { return camCallback; }
        static public void SetCallback(Callback callback) { camCallback = callback; }

#if DEBUG
        static private void print(object message) { GameBehaviours.print(message); }

        static public String StringCameraState() { return new Settings().ToString("#0.00"); }
        static public void PrintCameraState() { print(StringCameraState()); }

        static public String StringCameraStack()
        {
            return "Stack count = " + Settings.StackCount().ToString();
        }
        static public void PrintCameraStack() { print(StringCameraStack()); }

        static public String StringTopState()
        {
            if (Settings.StackCount() == 0)
                return "";
            else
                return Settings.Top().ToString("#0.00");
        }
        static public void PrintTopState() { print(StringTopState()); }
#endif
    }
}