using HydroTech_RCS;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Panels;
using UnityEngine;

public class HydroDockAssistCam : Strut
{
    public static HydroDockAssistCam activeCam;
    protected static Transform origParent;
    protected static float origFoV;
    protected static float origClip;

    protected bool camActivate;
    public float camClip = 0.01F;
    public float camDefFoV = 60.0F;
    public Vector3 camForward = Vector3.forward;

    // Camera settings learned from Fixed Camera
    public Vector3 camPos = Vector3.zero;
    public Vector3 camUp = Vector3.up;

    protected bool isOnActiveVessel;
    public int mag = 1;
    public Vector3 previewForward = -Vector3.forward;
    public float previewFoV = 90.0F;

    public Vector3 previewPos = Vector3.forward;
    public Vector3 previewUp = Vector3.up;

    public Vector3 Dir
    {
        get { return (this.transform.right * this.camForward.x) + (this.transform.forward * this.camForward.z) + (this.transform.up * -this.camForward.y); }
    }

    public Vector3 Down
    {
        get { return (this.transform.right * this.camUp.x) + (this.transform.forward * this.camUp.z) + (this.transform.up * -this.camUp.y); }
    }

    public Vector3 Right
    {
        get { return -Vector3.Cross(this.Down, this.Dir); }
    }

    public Vector3 CoM
    {
        get { return this.Rigidbody.worldCenterOfMass; }
    }

    public Transform VesselTransform
    {
        get { return this.vessel.ReferenceTransform; }
    }

    public bool CamActivate
    {
        get { return this.camActivate; }
        set
        {
            if (!this.camActivate && value)
            {
                if (activeCam == null)
                {
                    origParent = HydroFlightCameraManager.GetTransformParent();
                    origFoV = HydroFlightCameraManager.GetFoV();
                    origClip = HydroFlightCameraManager.GetNearClipPlane();
                }
                activeCam = this;
            }
            else if (this.camActivate && !value)
            {
                HydroFlightCameraManager.SetTransformParent(origParent);
                HydroFlightCameraManager.SetFoV(origFoV);
                HydroFlightCameraManager.SetNearClipPlane(origClip);
                HydroFlightCameraManager.SetTarget(this.vessel);
                this.mag = 1;
                activeCam = null;
            }
            this.camActivate = value;
        }
    }

    protected static APDockingAssist Da
    {
        get { return APDockingAssist.theAutopilot; }
    }

    protected static PanelDockAssist Panel
    {
        get { return PanelDockAssist.ThePanel; }
    }

    protected static HydroDockAssistCam CurCam
    {
        get { return Da.Cam; }
        set { Da.Cam = value; }
    }

    ModulePartRename ModuleRename
    {
        get { return (ModulePartRename)this.Modules["ModulePartRename"]; }
    }

    public Vector3 VectorTransform(Vector3 vec)
    {
        return HydroSwitchTransformCalculator.VectorTransform(vec, this.Right, this.Down, this.Dir);
    }

    public static Vector3 VectorTransform(Vector3 vec, HydroDockAssistCam cam)
    {
        return cam.VectorTransform(vec);
    }

    public bool IsOnActiveVessel()
    {
        if (this.vessel != FlightGlobals.ActiveVessel)
        {
            if (this.isOnActiveVessel && CurCam == this)
            {
                Panel.ResetHeight();
                if (this.CamActivate) { this.CamActivate = false; }
            }
            this.isOnActiveVessel = false;
        }
        else
        {
            this.isOnActiveVessel = true;
        }
        return this.isOnActiveVessel;
    }

    public Vector3 RelPos()
    {
        Vector3 r = this.CoM - this.vessel.findWorldCenterOfMass();
        return HydroSwitchTransformCalculator.VectorTransform(r, this.vessel.ReferenceTransform);
    }

    protected virtual void SetIcon()
    {
        this.stackIcon.SetIconColor(XKCDColors.SkyBlue);
    }

    protected override void onFlightStart()
    {
        base.onFlightStart();
        SetIcon();
        HydroJebCore.onFlightStart(this);
    }

    protected override void onGamePause()
    {
        base.onGamePause();
        HydroJebCore.onGamePause(this);
    }

    protected override void onGameResume()
    {
        base.onGameResume();
        HydroJebCore.onGameResume(this);
    }

    protected override void onPartDestroy()
    {
        base.onPartDestroy();
        if (!HydroJebCore.InFlight) { return; }
        if (this == CurCam)
        {
            CurCam = null;
            Panel.ResetHeight();
            if (this.CamActivate) { this.CamActivate = false; }
        }
        HydroJebCore.onPartDestroy(this);
    }

    protected override void onPartStart()
    {
        base.onPartStart();
        SetIcon();
    }

    protected override void onPartUpdate()
    {
        base.onPartUpdate();
        HydroJebCore.onPartUpdate(this);
        if (this.CamActivate)
        {
            HydroFlightCameraManager.SetNullTarget();
            HydroFlightCameraManager.SetTransformParent(this.transform);
            HydroFlightCameraManager.SetPosition(this.camPos);
            HydroFlightCameraManager.SetRotation(this.camForward, this.camUp);
            HydroFlightCameraManager.SetFoV(this.camDefFoV / this.mag);
            HydroFlightCameraManager.SetNearClipPlane(this.camClip);
            RequestResource("ElectricCharge", Behaviours.electricConsumptionCamera * TimeWarp.deltaTime);
        }
    }

    public override string ToString()
    {
        if (this.ModuleRename.Renamed) { return this.ModuleRename.nameString; }
        return RelPos().ToString("#0.00");
    }
}