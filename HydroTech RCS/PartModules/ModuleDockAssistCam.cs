#define BUGFIX_0_3_8_1
#define BUGFIX_0_5_X3

using HydroTech_FC;
using HydroTech_RCS;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Panels;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;

public class ModuleDockAssistCam : HydroPartModule, IPartPreview, IDaPartEditorAid
{
    public static ModuleDockAssistCam activeCam;

    protected bool camActivate;

    protected bool isOnActiveVessel;

    [KSPField(isPersistant = true)]
    public float camClip = 0.01F;

    [KSPField(isPersistant = true)]
    public Vector3 camCrossPos = Vector3.zero;

    [KSPField(isPersistant = true)]
    public float camDefFoV = 60.0F;

    [KSPField(isPersistant = true)]
    public Vector3 camForward = Vector3.forward;

    // Camera settings learned from Fixed Camera
    [KSPField(isPersistant = true)]
    public Vector3 camPos = Vector3.zero;

    [KSPField(isPersistant = true)]
    public Vector3 camUp = Vector3.up;

    LineRenderer lineDir;
    LineRenderer lineRight;
    LineRenderer lineUp;
    public int mag = 1;

    [KSPField(isPersistant = true)]
    public Vector3 previewForward = -Vector3.forward;

    [KSPField(isPersistant = true)]
    public float previewFoV = 90.0F;

    [KSPField(isPersistant = true)]
    public Vector3 previewPos = Vector3.forward;

    [KSPField(isPersistant = true)]
    public Vector3 previewUp = Vector3.up;

    public Vector3 Dir
    {
        get { return ReverseTransform_PartConfig(this.camForward); }
    }

    public Vector3 Down
    {
        get { return ReverseTransform_PartConfig(-this.camUp); }
    }

    public Vector3 Right
    {
        get { return HMaths.CrossProduct(this.Down, this.Dir); }
    }

    public Vector3 Pos
    {
        get { return this.part.Rigidbody.worldCenterOfMass + ReverseTransform_PartConfig(this.camPos); }
    }

    public Vector3 CrossPos
    {
        get { return this.part.Rigidbody.worldCenterOfMass + ReverseTransform_PartConfig(this.camCrossPos); }
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
                if (activeCam == null) { HydroFlightCameraManager.SaveCurrent(); }
                HydroFlightCameraManager.SetCallback(DoCamera);
                activeCam = this;
            }
            else if (this.camActivate && !value)
            {
                HydroFlightCameraManager.RetrieveLast();
                this.mag = 1;
                activeCam = null;
            }
            this.camActivate = value;
        }
    }

    protected static APDockAssist Da
    {
        get { return APDockAssist.TheAutopilot; }
    }

    protected static PanelDockAssist Panel
    {
        get { return PanelDockAssist.ThePanel; }
    }

    protected static ModuleDockAssistCam CurCam
    {
        get { return Da.Cam; }
        set { Da.Cam = value; }
    }

    public ModulePartRename ModuleRename
    {
        get { return (ModulePartRename)this.part.Modules["ModulePartRename"]; }
    }

    public void ShowEditorAid()
    {
        this.lineDir.SetWidth(0.01F, 0.01F);
        this.lineDir.SetPosition(0, Vector3.zero);
        this.lineDir.SetPosition(1, this.camForward);
        this.lineUp.SetWidth(0.01F, 0.01F);
        this.lineUp.SetPosition(0, Vector3.zero);
        this.lineUp.SetPosition(1, this.camUp);
        this.lineRight.SetWidth(0.01F, 0.01F);
        this.lineRight.SetPosition(0, Vector3.zero);
        this.lineRight.SetPosition(1, HMaths.CrossProduct(this.camForward, this.camUp));
    }

    public void HideEditorAid()
    {
        this.lineDir.SetWidth(0.0F, 0.0F);
        this.lineDir.SetPosition(0, Vector3.zero);
        this.lineDir.SetPosition(1, Vector3.zero);
        this.lineUp.SetWidth(0.0F, 0.0F);
        this.lineUp.SetPosition(0, Vector3.zero);
        this.lineUp.SetPosition(1, Vector3.zero);
        this.lineRight.SetWidth(0.0F, 0.0F);
        this.lineRight.SetPosition(0, Vector3.zero);
        this.lineRight.SetPosition(1, Vector3.zero);
    }

    public void DoPreview()
    {
        HydroFlightCameraManager.SetNullTarget();
        HydroFlightCameraManager.SetTransformParent(this.transform);
        HydroFlightCameraManager.SetFoV(this.previewFoV);
        HydroFlightCameraManager.SetPosition(this.previewPos);
        HydroFlightCameraManager.SetRotation(this.previewForward, this.previewUp);
    }

    public Vector3 VectorTransform(Vector3 vec)
    {
        return SwitchTransformCalculator.VectorTransform(vec, this.Right, this.Down, this.Dir);
    }

    public static Vector3 VectorTransform(Vector3 vec, ModuleDockAssistCam mcam)
    {
        return mcam.VectorTransform(vec);
    }

    public void DoCamera()
    {
        HydroFlightCameraManager.SetNullTarget();
        HydroFlightCameraManager.SetTransformParent(this.transform);
        HydroFlightCameraManager.SetPosition(this.camPos);
        HydroFlightCameraManager.SetRotation(this.camForward, this.camUp);
        HydroFlightCameraManager.SetFoV(this.camDefFoV / this.mag);
        HydroFlightCameraManager.SetNearClipPlane(this.camClip);
    }

    public bool IsOnActiveVessel()
    {
        if (this.vessel != FlightGlobals.ActiveVessel)
        {
            if (this.isOnActiveVessel && (CurCam == this))
            {
                Panel.ResetHeight();
                if (this.CamActivate) { this.CamActivate = false; }
            }
            this.isOnActiveVessel = false;
        }
        else
        { this.isOnActiveVessel = true; }
        return this.isOnActiveVessel;
    }

    public Vector3 RelPos()
    {
        Vector3 r = this.Pos - this.vessel.findWorldCenterOfMass();
        return SwitchTransformCalculator.VectorTransform(r, this.vessel.ReferenceTransform);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        HydroJebCore.OnUpdate(this);
        if (this.CamActivate) { this.part.RequestResource("ElectricCharge", Behaviours.electricConsumptionCamera * TimeWarp.deltaTime); }
    }

    public override void OnFlightStart()
    {
        base.OnFlightStart();
#if BUGFIX_0_3_8_1
        if (this.part.name == "HydroTech.DA.1m") { this.camCrossPos.Set(0, 0.065F, -0.75F); }
#endif
#if BUGFIX_0_5_X3
        if (this.part.name == "HydroTech.DA.2m")
        {
            this.camPos.Set(0, -0.025F, -1.375F);
            this.camCrossPos.Set(0, 0.065F, -1.375F);
        }
#endif
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!GameStates.InFlight) { return; }
        if (this == CurCam)
        {
            CurCam = null;
            Panel.ResetHeight();
            if (this.CamActivate) { this.CamActivate = false; }
        }
    }

    public override string ToString()
    {
        if (this.ModuleRename.Renamed) { return this.ModuleRename.nameString; }
        return RelPos().ToString("#0.00");
    }

    public override void OnStart(StartState state)
    {
        base.OnStart(state);
        if (state == StartState.Editor)
        {
            GameObject obj = new GameObject("DockCamLine");
            this.lineDir = obj.AddComponent<LineRenderer>();
            this.lineDir.transform.parent = this.transform;
            this.lineDir.useWorldSpace = false;
            this.lineDir.transform.localPosition = this.camPos;
            this.lineDir.transform.localEulerAngles = Vector3.zero;
            this.lineDir.material = new Material(Shader.Find("Particles/Additive"));
            this.lineDir.SetColors(Color.blue, Color.blue);
            this.lineDir.SetVertexCount(2);
            GameObject obj2 = new GameObject("DockCamLine2");
            this.lineUp = obj2.AddComponent<LineRenderer>();
            this.lineUp.transform.parent = this.transform;
            this.lineUp.useWorldSpace = false;
            this.lineUp.transform.localPosition = this.camPos;
            this.lineUp.transform.localEulerAngles = Vector3.zero;
            this.lineUp.material = new Material(Shader.Find("Particles/Additive"));
            this.lineUp.SetColors(Color.green, Color.green);
            this.lineUp.SetVertexCount(2);
            GameObject obj3 = new GameObject("DockCamLine3");
            this.lineRight = obj3.AddComponent<LineRenderer>();
            this.lineRight.transform.parent = this.transform;
            this.lineRight.useWorldSpace = false;
            this.lineRight.transform.localPosition = this.camPos;
            this.lineRight.transform.localEulerAngles = Vector3.zero;
            this.lineRight.material = new Material(Shader.Find("Particles/Additive"));
            this.lineRight.SetColors(Color.gray, Color.gray);
            this.lineRight.SetVertexCount(2);
            HideEditorAid();
        }
    }
}