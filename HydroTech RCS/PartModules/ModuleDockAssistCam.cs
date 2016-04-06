#define BUGFIX_0_3_8_1
#define BUGFIX_0_5_X3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HydroTech_FC;
using HydroTech_RCS;
using HydroTech_RCS.PartModules.Base;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Autopilots.ASAS;
using HydroTech_RCS.Panels;
using HydroTech_RCS.Constants.Core;

public class ModuleDockAssistCam : HydroPartModule, IPartPreview, IDAPartEditorAid
{
    public Vector3 Dir { get { return ReverseTransform_PartConfig(camForward); } }
    public Vector3 Down { get { return ReverseTransform_PartConfig(-camUp); } }
    public Vector3 Right { get { return HMaths.CrossProduct(Down, Dir); } }

    public Vector3 VectorTransform(Vector3 vec)
    {
        return SwitchTransformCalculator.VectorTransform(vec, Right, Down, Dir);
    }
    public static Vector3 VectorTransform(Vector3 vec, ModuleDockAssistCam mcam)
    {
        return mcam.VectorTransform(vec);
    }

    public Vector3 Pos { get { return part.Rigidbody.worldCenterOfMass + ReverseTransform_PartConfig(camPos); } }
    public Vector3 CrossPos { get { return part.Rigidbody.worldCenterOfMass + ReverseTransform_PartConfig(camCrossPos); } }
    public Transform VesselTransform { get { return vessel.ReferenceTransform; } }

    // Camera settings learned from Fixed Camera
    [KSPField(isPersistant = true)]
    public Vector3 camPos = Vector3.zero;
    [KSPField(isPersistant = true)]
    public Vector3 camForward = Vector3.forward;
    [KSPField(isPersistant = true)]
    public Vector3 camUp = Vector3.up;
    [KSPField(isPersistant = true)]
    public float camDefFoV = 60.0F;
    public int mag = 1;
    [KSPField(isPersistant = true)]
    public float camClip = 0.01F;
    [KSPField(isPersistant = true)]
    public Vector3 camCrossPos = Vector3.zero;
    public void DoCamera()
    {
        HydroFlightCameraManager.SetNullTarget();
        HydroFlightCameraManager.SetTransformParent(transform);
        HydroFlightCameraManager.SetPosition(camPos);
        HydroFlightCameraManager.SetRotation(camForward, camUp);
        HydroFlightCameraManager.SetFoV(camDefFoV / mag);
        HydroFlightCameraManager.SetNearClipPlane(camClip);
    }

    [KSPField(isPersistant = true)]
    public Vector3 previewPos = Vector3.forward;
    [KSPField(isPersistant = true)]
    public Vector3 previewForward = -Vector3.forward;
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

    public static ModuleDockAssistCam ActiveCam = null;

    protected bool _CamActivate = false;
    public bool CamActivate
    {
        get { return _CamActivate; }
        set
        {
            if (!_CamActivate && value)
            {
                if (ActiveCam == null)
                    HydroFlightCameraManager.SaveCurrent();
                HydroFlightCameraManager.SetCallback(DoCamera);
                ActiveCam = this;
            }
            else if (_CamActivate && !value)
            {
                HydroFlightCameraManager.RetrieveLast();
                mag = 1;
                ActiveCam = null;
            }
            _CamActivate = value;
        }
    }

    protected static APDockAssist DA { get { return APDockAssist.theAutopilot; } }
    protected static PanelDockAssist panel { get { return PanelDockAssist.thePanel; } }
    protected static ModuleDockAssistCam CurCam
    {
        get { return DA.cam; }
        set { DA.cam = value; }
    }

    protected bool _IsOnActiveVessel = false;
    public bool IsOnActiveVessel()
    {
        if (vessel != FlightGlobals.ActiveVessel)
        {
            if (_IsOnActiveVessel && CurCam == this)
            {
                panel.ResetHeight();
                if (CamActivate)
                    CamActivate = false;
            }
            _IsOnActiveVessel = false;
        }
        else
            _IsOnActiveVessel = true;
        return _IsOnActiveVessel;
    }

    public Vector3 RelPos()
    {
        Vector3 r = Pos - vessel.findWorldCenterOfMass();
        return SwitchTransformCalculator.VectorTransform(r, vessel.ReferenceTransform);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        HydroJebCore.OnUpdate(this);
        if (CamActivate)
            part.RequestResource("ElectricCharge", Behaviours.Electric_Consumption_Camera * TimeWarp.deltaTime);
    }

    public override void OnFlightStart()
    {
        base.OnFlightStart();
#if BUGFIX_0_3_8_1
        if (part.name == "HydroTech.DA.1m")
            camCrossPos.Set(0, 0.065F, -0.75F);
#endif
#if BUGFIX_0_5_X3
        if (part.name == "HydroTech.DA.2m")
        {
            camPos.Set(0, -0.025F, -1.375F);
            camCrossPos.Set(0, 0.065F, -1.375F);
        }
#endif
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!GameStates.InFlight)
            return;
        if (this == CurCam)
        {
            CurCam = null;
            panel.ResetHeight();
            if (CamActivate)
                CamActivate = false;
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
            lineDir.transform.localPosition = camPos;
            lineDir.transform.localEulerAngles = Vector3.zero;
            lineDir.material = new Material(Shader.Find("Particles/Additive"));
            lineDir.SetColors(Color.blue, Color.blue);
            lineDir.SetVertexCount(2);
            GameObject obj2 = new GameObject("DockCamLine2");
            lineUp = obj2.AddComponent<LineRenderer>();
            lineUp.transform.parent = transform;
            lineUp.useWorldSpace = false;
            lineUp.transform.localPosition = camPos;
            lineUp.transform.localEulerAngles = Vector3.zero;
            lineUp.material = new Material(Shader.Find("Particles/Additive"));
            lineUp.SetColors(Color.green, Color.green);
            lineUp.SetVertexCount(2);
            GameObject obj3 = new GameObject("DockCamLine3");
            lineRight = obj3.AddComponent<LineRenderer>();
            lineRight.transform.parent = transform;
            lineRight.useWorldSpace = false;
            lineRight.transform.localPosition = camPos;
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
        lineDir.SetPosition(1, camForward);
        lineUp.SetWidth(0.01F, 0.01F);
        lineUp.SetPosition(0, Vector3.zero);
        lineUp.SetPosition(1, camUp);
        lineRight.SetWidth(0.01F, 0.01F);
        lineRight.SetPosition(0, Vector3.zero);
        lineRight.SetPosition(1, HMaths.CrossProduct(camForward, camUp));
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