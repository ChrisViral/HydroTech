using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class HydroFlightCameraManager : MonoBehaviour
    {
        private class Settings
        {
            #region Static Fields
            private static readonly Stack<Settings> settingsStack = new Stack<Settings>();
            #endregion

            #region Fields
            private Vessel vessel;
            private Transform tgt;
            private Transform parent;
            private float fov = 60;
            private Vector3 position;
            private Quaternion rotation;
            private float clip = 0.01f;
            private Callback callback;
            #endregion

            #region Constructor
            public Settings()
            {
                Get();
            }
            #endregion

            #region Methods
            private void Get()
            {
                this.vessel = FlightGlobals.ActiveVessel;
                this.tgt = Target;
                this.parent = TransformParent;
                this.fov = FoV;
                this.position = Position;
                this.rotation = Rotation;
                this.clip = NearClipPlane;
                this.callback = CamCallback;
            }

            private void Set()
            {
                if (this.vessel == FlightGlobals.ActiveVessel)
                {
                    Target = this.tgt;
                    TransformParent = this.parent;
                    FoV = this.fov;
                    Position = this.position;
                    Rotation = this.rotation;
                    NearClipPlane = this.clip;
                    CamCallback = this.callback;
                }
                else { ResetToActiveVessel(); }
            }
            #endregion

            #region Static methods
            public static void SaveCurrent()
            {
                settingsStack.Push(new Settings());
            }

            public static void RetrieveLast()
            {
                settingsStack.Pop().Set();
            }
            #endregion

            #region Overrides
            public override string ToString()
            {
                return string.Format("Vessel is{0} ActiveVessel\n{1} Target\n{2} Parent\nFoV = {3}\nPosition = {4}\nRotation = {5}\nClip = {6}\nIs{7} Callback", (this.vessel.isActiveVessel ? string.Empty : " not"), (this.tgt == null ? "Null" : "Has"), (this.parent == null ? "Null" : "Has"), this.fov, this.position, this.rotation, this.clip, (this.callback == null ? " not" : string.Empty));
            }

            public string ToString(string format)
            {
                return string.Format("Vessel is{0} ActiveVessel\n{1} Target\n{2} Parent\n" + "FoV = {3}\n" + "Position = {4}\n" + "Rotation = {5}\n" + "Clip = {6}\n" + "Is{7} Callback", (this.vessel.isActiveVessel ? string.Empty : " not"), (this.tgt == null ? "Null" : "Has"), (this.parent == null ? "Null" : "Has"), this.fov.ToString(format), this.position.ToString(format), this.rotation.ToString(format), this.clip.ToString(format), (this.callback == null ? " not" : string.Empty));
            }
            #endregion

            #region Debug
#if DEBUG
            public static int StackCount
            {
                get { return settingsStack.Count;}
            }

            public static Settings Top
            {
                get { return settingsStack.Peek();}
            }
#endif
            #endregion
        }

        #region Constants
        private const float defaultFoV = 60;
        private const float defaultNearClip = 0.01f;
        #endregion

        #region Static fields
        private static FlightCamera cam;
        private static Vessel origVessel;
        private static Transform origParent;
        #endregion

        #region Static Properties
        private static Transform target;
        public static Transform Target
        {
            get { return target; }
            set
            {
                cam.setTarget(value);
                target = value;
            }
        }

        private static Callback camCallback;
        public static Callback CamCallback
        {
            get { return camCallback; }
            set { camCallback = value; }
        }

        public static Transform TransformParent
        {
            get { return cam.transform.parent; }
            set { cam.transform.parent = value; }
        }

        public static Quaternion Rotation
        {
            get { return cam.transform.localRotation; }
            set { cam.transform.localRotation = value; }
        }

        public static Vector3 Position
        {
            get { return cam.transform.localPosition; }
            set { cam.transform.localPosition = value; }
        }

        public static float FoV
        {
            get { return Camera.main.fieldOfView; }
            set { Camera.main.fieldOfView = value; }
        }

        public static float NearClipPlane
        {
            get { return Camera.main.nearClipPlane; }
            set { Camera.main.nearClipPlane = value; }
        }
        #endregion

        #region Static methods
        public static void SetLookRotation(Vector3 forward, Vector3 up)
        {
            Rotation = Quaternion.LookRotation(forward, up);
        }

        public static void ResetToActiveVessel()
        {
#if DEBUG
            Debug.Log("HydroFlightCameraManager: Setting to active vessel");
#endif
            TransformParent = origParent;
            FoV = defaultFoV;
            Target = FlightGlobals.ActiveVessel.transform;
            NearClipPlane = defaultNearClip;
            CamCallback = null;
        }

        public static void SaveCurrent()
        {
            Settings.SaveCurrent();
#if DEBUG
            Debug.Log("HydroFlightCameraManager: Settings pushed into stack. Current count: " + Settings.StackCount);
#endif
        }

        public static void RetrieveLast()
        {
            Settings.RetrieveLast();
#if DEBUG
            Debug.Log("HydroFlightCameraManager: Settings retrieved from stack. Current count: " + Settings.StackCount);
#endif
        }     
        #endregion

        #region Functions
        private void Start()
        {
            cam = FindObjectOfType<FlightCamera>();
            origVessel = FlightGlobals.ActiveVessel;
            target = FlightGlobals.ActiveVessel.transform;
            origParent = TransformParent;
            camCallback = null;
        }

        public void Update()
        {
            if (!origVessel.isActiveVessel && camCallback == null)
            {
                origVessel = FlightGlobals.ActiveVessel;
                origParent = TransformParent;
                target = FlightGlobals.ActiveVessel.transform;
            }
            if (camCallback != null) { camCallback(); }
        }
        #endregion

        #region Debug
#if DEBUG
        public static string StringCameraState()
        {
            return new Settings().ToString("#0.00");
        }

        public static void PrintCameraState()
        {
            Debug.Log(StringCameraState());
        }

        public static string StringCameraStack()
        {
            return "Stack count = " + Settings.StackCount;
        }

        public static void PrintCameraStack()
        {
            Debug.Log(StringCameraStack());
        }

        public static string StringTopState()
        {
            return Settings.StackCount == 0 ? string.Empty : Settings.Top.ToString("#0.00");
        }

        public static void PrintTopState()
        {
            Debug.Log(StringTopState());
        }
#endif
        #endregion
    }
}