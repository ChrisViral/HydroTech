#define BUGFIX_0_5_X3

using HydroTech_FC;
using HydroTech_RCS;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Panels;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;
using HydroPartModule = HydroTech_FC.HydroPartModule;

public class ModuleDockAssistTarget : HydroPartModule, IPartPreview, IDAPartEditorAid
{
    protected bool isNear;

    LineRenderer lineDir;
    LineRenderer lineRight;
    LineRenderer lineUp;

    [KSPField(isPersistant = true)]
    public Vector3 previewForward = Vector3.forward;

    [KSPField(isPersistant = true)]
    public float previewFoV = 90.0F;

    [KSPField(isPersistant = true)]
    public Vector3 previewPos = -Vector3.forward;

    [KSPField(isPersistant = true)]
    public Vector3 previewUp = Vector3.up;

    [KSPField(isPersistant = true)]
    public Vector3 targetForward = Vector3.forward;

    [KSPField(isPersistant = true)]
    public Vector3 targetPos = Vector3.zero;

    [KSPField(isPersistant = true)]
    public Vector3 targetUp = Vector3.up;

    public Vector3 Dir
    {
        get { return ReverseTransform_PartConfig(this.targetForward); }
    }

    public Vector3 Down
    {
        get { return ReverseTransform_PartConfig(-this.targetUp); }
    }

    public Vector3 Right
    {
        get { return HMaths.CrossProduct(this.Down, this.Dir); }
    }

    public Vector3 Pos
    {
        get { return this.part.Rigidbody.worldCenterOfMass + ReverseTransform_PartConfig(this.targetPos); }
    }

    protected static PanelDockAssist Panel
    {
        get { return PanelDockAssist.ThePanel; }
    }

    protected static ModuleDockAssistTarget CurTarget
    {
        get { return APDockAssist.TheAutopilot.target; }
        set { APDockAssist.TheAutopilot.target = value; }
    }

    public ModulePartRename ModuleRename
    {
        get { return (ModulePartRename)this.part.Modules["ModulePartRename"]; }
    }

    public void ShowEditorAid()
    {
        this.lineDir.SetWidth(0.01F, 0.01F);
        this.lineDir.SetPosition(0, Vector3.zero);
        this.lineDir.SetPosition(1, this.targetForward);
        this.lineUp.SetWidth(0.01F, 0.01F);
        this.lineUp.SetPosition(0, Vector3.zero);
        this.lineUp.SetPosition(1, this.targetUp);
        this.lineRight.SetWidth(0.01F, 0.01F);
        this.lineRight.SetPosition(0, Vector3.zero);
        this.lineRight.SetPosition(1, HMaths.CrossProduct(this.targetForward, this.targetUp));
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

    public Vector3 RelPos()
    {
        Vector3 r = this.Pos - this.vessel.findWorldCenterOfMass();
        return SwitchTransformCalculator.VectorTransform(r, this.vessel.ReferenceTransform);
    }

    public bool IsNear()
    {
        if (this.vessel == GameStates.ActiveVessel
            /* || (vessel.findWorldCenterOfMass() - HydroJebCore.ActiveVessel.CoM).magnitude > Position.MaxDist */)
        {
            if (this.isNear && CurTarget == this) { Panel.ResetHeight(); }
            this.isNear = false;
        }
        else
        {
            this.isNear = true;
        }
        return this.isNear;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        HydroJebCore.OnUpdate(this);
    }

    public override void OnFlightStart()
    {
        base.OnFlightStart();
#if BUGFIX_0_5_X3
        if (this.part.name == "HydroTech.DA.2m") { this.targetPos.Set(0, 0.07F, 1.375F); }
#endif
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!GameStates.InFlight) { return; }
        if (this == CurTarget)
        {
            CurTarget = null;
            Panel.ResetHeight();
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
            this.lineDir.transform.localPosition = this.targetPos;
            this.lineDir.transform.localEulerAngles = Vector3.zero;
            this.lineDir.material = new Material(Shader.Find("Particles/Additive"));
            this.lineDir.SetColors(Color.blue, Color.blue);
            this.lineDir.SetVertexCount(2);
            GameObject obj2 = new GameObject("DockCamLine2");
            this.lineUp = obj2.AddComponent<LineRenderer>();
            this.lineUp.transform.parent = this.transform;
            this.lineUp.useWorldSpace = false;
            this.lineUp.transform.localPosition = this.targetPos;
            this.lineUp.transform.localEulerAngles = Vector3.zero;
            this.lineUp.material = new Material(Shader.Find("Particles/Additive"));
            this.lineUp.SetColors(Color.green, Color.green);
            this.lineUp.SetVertexCount(2);
            GameObject obj3 = new GameObject("DockCamLine3");
            this.lineRight = obj3.AddComponent<LineRenderer>();
            this.lineRight.transform.parent = this.transform;
            this.lineRight.useWorldSpace = false;
            this.lineRight.transform.localPosition = this.targetPos;
            this.lineRight.transform.localEulerAngles = Vector3.zero;
            this.lineRight.material = new Material(Shader.Find("Particles/Additive"));
            this.lineRight.SetColors(Color.gray, Color.gray);
            this.lineRight.SetVertexCount(2);
            HideEditorAid();
        }
    }
}