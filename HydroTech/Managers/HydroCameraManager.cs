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
            private static HydroCameraManager CameraManager => HydroFlightManager.Instance.CameraManager;
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
                return $"Vessel is{(this.vessel.isActiveVessel ? string.Empty : " not")} ActiveVessel\n{(this.tgt == null ? "Null" : "Has")} Target\n{(this.parent == null ? "Null" : "Has")} Parent\nFoV = {this.fov}\nPosition = {this.position}\nRotation = {this.rotation}\nClip = {this.clip}\nIs{(this.callback == null ? " not" : string.Empty)} Callback";
            }

            public string ToString(string format)
            {
                return $"Vessel is{(this.vessel.isActiveVessel ? string.Empty : " not")} ActiveVessel\n{(this.tgt == null ? "Null" : "Has")} Target\n{(this.parent == null ? "Null" : "Has")} Parent\n" + $"FoV = {this.fov.ToString(format)}\n" + $"Position = {this.position.ToString(format)}\n" + $"Rotation = {this.rotation.ToString(format)}\n" + $"Clip = {this.clip.ToString(format)}\n" + $"Is{(this.callback == null ? " not" : string.Empty)} Callback";
            }
            #endregion

            #region Debug
#if DEBUG
            public static int StackCount => settingsStack.Count;

            public static Settings Top => settingsStack.Peek();
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

        public Callback CamCallback { get; set; }

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
            this.CamCallback = null;
        }

        internal void Update()
        {
            if (!this.origVessel.isActiveVessel && this.CamCallback == null)
            {
                this.origVessel = FlightGlobals.ActiveVessel;
                this.origParent = this.TransformParent;
                this.target = FlightGlobals.ActiveVessel.transform;
            }
            this.CamCallback?.Invoke();
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
