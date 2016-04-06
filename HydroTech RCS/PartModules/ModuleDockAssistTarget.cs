#define BUGFIX_0_5_X3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HydroTech_FC;
using HydroTech_RCS;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants.Autopilots.Docking;
using HydroTech_RCS.Autopilots.ASAS;
using HydroTech_RCS.Panels;
using HydroTech_RCS.PartModules.Base;

public class ModuleDockAssistTarget : HydroPartModule, IPartPreview, IDAPartEditorAid
{
    public Vector3 Dir { get { return ReverseTransform_PartConfig(targetForward); } }
    public Vector3 Down { get { return ReverseTransform_PartConfig(-targetUp); } }
    public Vector3 Right { get { return HMaths.CrossProduct(Down, Dir); } }

    public Vector3 Pos { get { return part.Rigidbody.worldCenterOfMass + ReverseTransform_PartConfig(targetPos); } }

    public Vector3 RelPos()
    {
        Vector3 r = Pos - vessel.findWorldCenterOfMass();
        return SwitchTransformCalculator.VectorTransform(r, vessel.ReferenceTransform);
    }

    [KSPField(isPersistant = true)]
    public Vector3 targetPos = Vector3.zero;
    [KSPField(isPersistant = true)]
    public Vector3 targetForward = Vector3.forward;
    [KSPField(isPersistant = true)]
    public Vector3 targetUp = Vector3.up;

    [KSPField(isPersistant = true)]
    public Vector3 previewPos = -Vector3.forward;
    [KSPField(isPersistant = true)]
    public Vector3 previewForward = Vector3.forward;
    [KSPField(isPersistant = true)]
    public Vector3 previewUp = Vector3.up;
    [KSPField(isPersistant = true)]
    public float previewFoV = 90.0F;
    public void DoPreview()
    {
        HydroFlightCameraManager.SetNullTarget();
        HydroFlightCameraManager.SetTransformParent(transform);
        HydroFlightCameraManager.SetFoV(previewFoV);
        HydroFlightCameraManager.SetPosition(previewPos);
        HydroFlightCameraManager.SetRotation(previewForward, previewUp);
    }

    protected bool _IsNear = false;
    public bool IsNear()
    {
        if (vessel == GameStates.ActiveVessel
            /* || (vessel.findWorldCenterOfMass() - HydroJebCore.ActiveVessel.CoM).magnitude > Position.MaxDist */)
        {
            if (_IsNear && CurTarget == this)
                panel.ResetHeight();
            _IsNear = false;
        }
        else
            _IsNear = true;
        return _IsNear;
    }

    static protected PanelDockAssist panel { get { return PanelDockAssist.thePanel; } }
    static protected ModuleDockAssistTarget CurTarget
    {
        get { return APDockAssist.theAutopilot.Target; }
        set { APDockAssist.theAutopilot.Target = value; }
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
        if (part.name == "HydroTech.DA.2m")
            targetPos.Set(0, 0.07F, 1.375F);
#endif
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!GameStates.InFlight)
            return;
        if (this == CurTarget)
        {
            CurTarget = null;
            panel.ResetHeight();
        }
    }

    public ModulePartRename ModuleRename { get { return (ModulePartRename)part.Modules["ModulePartRename"]; } }
    public override string ToString()
    {
        if (ModuleRename.Renamed)
            return ModuleRename.nameString;
        else
            return RelPos().ToString("#0.00");
    }

    LineRenderer lineDir = null;
    LineRenderer lineUp = null;
    LineRenderer lineRight = null;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        if (state == StartState.Editor)
        {
            GameObject obj = new GameObject("DockCamLine");
            lineDir = obj.AddComponent<LineRenderer>();
            lineDir.transform.parent = transform;
            lineDir.useWorldSpace = false;
            lineDir.transform.localPosition = targetPos;
            lineDir.transform.localEulerAngles = Vector3.zero;
            lineDir.material = new Material(Shader.Find("Particles/Additive"));
            lineDir.SetColors(Color.blue, Color.blue);
            lineDir.SetVertexCount(2);
            GameObject obj2 = new GameObject("DockCamLine2");
            lineUp = obj2.AddComponent<LineRenderer>();
            lineUp.transform.parent = transform;
            lineUp.useWorldSpace = false;
            lineUp.transform.localPosition = targetPos;
            lineUp.transform.localEulerAngles = Vector3.zero;
            lineUp.material = new Material(Shader.Find("Particles/Additive"));
            lineUp.SetColors(Color.green, Color.green);
            lineUp.SetVertexCount(2);
            GameObject obj3 = new GameObject("DockCamLine3");
            lineRight = obj3.AddComponent<LineRenderer>();
            lineRight.transform.parent = transform;
            lineRight.useWorldSpace = false;
            lineRight.transform.localPosition = targetPos;
            lineRight.transform.localEulerAngles = Vector3.zero;
            lineRight.material = new Material(Shader.Find("Particles/Additive"));
            lineRight.SetColors(Color.gray, Color.gray);
            lineRight.SetVertexCount(2);
            HideEditorAid();
        }
    }

    public void ShowEditorAid()
    {
        lineDir.SetWidth(0.01F, 0.01F);
        lineDir.SetPosition(0, Vector3.zero);
        lineDir.SetPosition(1, targetForward);
        lineUp.SetWidth(0.01F, 0.01F);
        lineUp.SetPosition(0, Vector3.zero);
        lineUp.SetPosition(1, targetUp);
        lineRight.SetWidth(0.01F, 0.01F);
        lineRight.SetPosition(0, Vector3.zero);
        lineRight.SetPosition(1, HMaths.CrossProduct(targetForward, targetUp));
    }

    public void HideEditorAid()
    {
        lineDir.SetWidth(0.0F, 0.0F);
        lineDir.SetPosition(0, Vector3.zero);
        lineDir.SetPosition(1, Vector3.zero);
        lineUp.SetWidth(0.0F, 0.0F);
        lineUp.SetPosition(0, Vector3.zero);
        lineUp.SetPosition(1, Vector3.zero);
        lineRight.SetWidth(0.0F, 0.0F);
        lineRight.SetPosition(0, Vector3.zero);
        lineRight.SetPosition(1, Vector3.zero);
    }
}