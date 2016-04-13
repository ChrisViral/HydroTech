using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Managers
{

    public class HydroCameraManager
    {
        private class Settings
        {
            #region Static Fields
            private static readonly Stack<Settings> settingsStack = new Stack<Settings>();
            #endregion

            #region Static properties
            private static HydroCameraManager CameraManager
            {
                get { return HydroFlightManager.Instance.CameraManager; }
            }
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
                this.tgt = CameraManager.Target;
                this.parent = CameraManager.TransformParent;
                this.fov = CameraManager.FoV;
                this.position = CameraManager.Position;
                this.rotation = CameraManager.Rotation;
                this.clip = CameraManager.NearClipPlane;
                this.callback = CameraManager.CamCallback;
            }

            private void Set()
            {
                if (this.vessel.isActiveVessel)
                {
                    CameraManager.Target = this.tgt;
                    CameraManager.TransformParent = this.parent;
                    CameraManager.FoV = this.fov;
                    CameraManager.Position = this.position;
                    CameraManager.Rotation = this.rotation;
                    CameraManager.NearClipPlane = this.clip;
                    CameraManager.CamCallback = this.callback;
                }
                else { CameraManager.ResetToActiveVessel(); }
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
                return string.Format("Vessel is{0} ActiveVessel\n{1} Target\n{2} Parent\nFoV = {3}\nPosition = {4}\nRotation = {5}\nClip = {6}\nIs{7} Callback", this.vessel.isActiveVessel ? string.Empty : " not", this.tgt == null ? "Null" : "Has", this.parent == null ? "Null" : "Has", this.fov, this.position, this.rotation, this.clip, this.callback == null ? " not" : string.Empty);
            }

            public string ToString(string format)
            {
                return string.Format("Vessel is{0} ActiveVessel\n{1} Target\n{2} Parent\n" + "FoV = {3}\n" + "Position = {4}\n" + "Rotation = {5}\n" + "Clip = {6}\n" + "Is{7} Callback", this.vessel.isActiveVessel ? string.Empty : " not", this.tgt == null ? "Null" : "Has", this.parent == null ? "Null" : "Has", this.fov.ToString(format), this.position.ToString(format), this.rotation.ToString(format), this.clip.ToString(format), this.callback == null ? " not" : string.Empty);
            }
            #endregion

            #region Debug
#if DEBUG
            public static int StackCount
            {
                get { return settingsStack.Count; }
            }

            public static Settings Top
            {
                get { return settingsStack.Peek(); }
            }
#endif
            #endregion
        }

        #region Constants
        private const float defaultFoV = 60;
        private const float defaultNearClip = 0.01f;
        #endregion

        #region Fields
        private FlightCamera cam;
        private Vessel origVessel;
        private Transform origParent;
        #endregion

        #region Properties
        private Transform target;
        public Transform Target
        {
            get { return this.target; }
            set
            {
                this.cam.setTarget(value);
                this.target = value;
            }
        }

        private Callback camCallback;
        public Callback CamCallback
        {
            get { return this.camCallback; }
            set { this.camCallback = value; }
        }

        public Transform TransformParent
        {
            get { return this.cam.transform.parent; }
            set { this.cam.transform.parent = value; }
        }

        public Quaternion Rotation
        {
            get { return this.cam.transform.localRotation; }
            set { this.cam.transform.localRotation = value; }
        }

        public Vector3 Position
        {
            get { return this.cam.transform.localPosition; }
            set { this.cam.transform.localPosition = value; }
        }

        public float FoV
        {
            get { return Camera.main.fieldOfView; }
            set { Camera.main.fieldOfView = value; }
        }

        public float NearClipPlane
        {
            get { return Camera.main.nearClipPlane; }
            set { Camera.main.nearClipPlane = value; }
        }
        #endregion

        #region Methods
        public void SetLookRotation(Vector3 forward, Vector3 up)
        {
            this.Rotation = Quaternion.LookRotation(forward, up);
        }

        public void ResetToActiveVessel()
        {
#if DEBUG
            Debug.Log("HydroFlightCameraManager: Setting to active vessel");
#endif
            this.TransformParent = this.origParent;
            this.FoV = defaultFoV;
            this.Target = FlightGlobals.ActiveVessel.transform;
            this.NearClipPlane = defaultNearClip;
            this.CamCallback = null;
        }

        public void SaveCurrent()
        {
            Settings.SaveCurrent();
#if DEBUG
            Debug.Log("HydroFlightCameraManager: Settings pushed into stack. Current count: " + Settings.StackCount);
#endif
        }

        public void RetrieveLast()
        {
            Settings.RetrieveLast();
#if DEBUG
            Debug.Log("HydroFlightCameraManager: Settings retrieved from stack. Current count: " + Settings.StackCount);
#endif
        }
        #endregion

        #region Functions
        internal void Start()
        {
            this.cam = Object.FindObjectOfType<FlightCamera>();
            this.origVessel = FlightGlobals.ActiveVessel;
            this.target = FlightGlobals.ActiveVessel.transform;
            this.origParent = this.TransformParent;
            this.camCallback = null;
        }

        internal void Update()
        {
            if (!this.origVessel.isActiveVessel && this.camCallback == null)
            {
                this.origVessel = FlightGlobals.ActiveVessel;
                this.origParent = this.TransformParent;
                this.target = FlightGlobals.ActiveVessel.transform;
            }
            if (this.camCallback != null) { this.camCallback(); }
        }
        #endregion

        #region Debug
#if DEBUG
        public string StringCameraState()
        {
            return new Settings().ToString("#0.00");
        }

        public void PrintCameraState()
        {
            Debug.Log(StringCameraState());
        }

        public string StringCameraStack()
        {
            return "Stack count = " + Settings.StackCount;
        }

        public void PrintCameraStack()
        {
            Debug.Log(StringCameraStack());
        }

        public string StringTopState()
        {
            return Settings.StackCount == 0 ? string.Empty : Settings.Top.ToString("#0.00");
        }

        public void PrintTopState()
        {
            Debug.Log(StringTopState());
        }
#endif
        #endregion
    }
}
