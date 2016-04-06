using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HydroTech_RCS;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Autopilots.ASAS;
using HydroTech_RCS.Managers;
using HydroTech_RCS.Panels;
using HydroTech_RCS.Constants.Core;

public class HydroDockAssistCam : Strut
{
    public Vector3 Dir 
    {
        get
        {
            return transform.right * CamForward.x
                + transform.forward * CamForward.z
                + transform.up * -CamForward.y;
        }
    }
    public Vector3 Down
    {
        get
        {
            return transform.right * CamUp.x
                + transform.forward * CamUp.z
                + transform.up * -CamUp.y;
        }
    }
    public Vector3 Right { get { return -Vector3.Cross(Down, Dir); } }

    public Vector3 VectorTransform(Vector3 vec)
    {
        return HydroSwitchTransformCalculator.VectorTransform(vec, Right, Down, Dir);
    }
    static public Vector3 VectorTransform(Vector3 vec, HydroDockAssistCam Cam)
    {
        return Cam.VectorTransform(vec);
    }

    public Vector3 CoM { get { return Rigidbody.worldCenterOfMass; } }
    public Transform VesselTransform { get { return vessel.ReferenceTransform; } }

    // Camera settings learned from Fixed Camera
    public Vector3 CamPos = Vector3.zero;
    public Vector3 CamForward = Vector3.forward;
    public Vector3 CamUp = Vector3.up;
    public float CamDefFoV = 60.0F;
    public int Mag = 1;
    public float CamClip = 0.01F;

    public Vector3 PreviewPos = Vector3.forward;
    public Vector3 PreviewForward = -Vector3.forward;
    public Vector3 PreviewUp = Vector3.up;
    public float PreviewFoV = 90.0F;

    public static HydroDockAssistCam ActiveCam = null;
    protected static Transform origParent;
    protected static float origFoV;
    protected static float origClip;

    protected bool _CamActivate = false;
    public bool CamActivate
    {
        get { return _CamActivate; }
        set
        {
            if (!_CamActivate && value)
            {
                if (ActiveCam == null)
                {
                    origParent = HydroFlightCameraManager.GetTransformParent();
                    origFoV = HydroFlightCameraManager.GetFoV();
                    origClip = HydroFlightCameraManager.GetNearClipPlane();
                }
                ActiveCam = this;
            }
            else if (_CamActivate && !value)
            {
                HydroFlightCameraManager.SetTransformParent(origParent);
                HydroFlightCameraManager.SetFoV(origFoV);
                HydroFlightCameraManager.SetNearClipPlane(origClip);
                HydroFlightCameraManager.SetTarget(vessel);
                Mag = 1;
                ActiveCam = null;
            }
            _CamActivate = value;
        }
    }

    static protected APDockingAssist DA { get { return APDockingAssist.theAutopilot; } }
    static protected PanelDockAssist panel { get { return PanelDockAssist.thePanel; } }
    static protected HydroDockAssistCam CurCam
    {
        get { return DA.Cam; }
        set { DA.Cam = value; }
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
        Vector3 r = CoM - vessel.findWorldCenterOfMass();
        return HydroSwitchTransformCalculator.VectorTransform(r, vessel.ReferenceTransform);
    }

    virtual protected void SetIcon()
    {
        stackIcon.SetIconColor(XKCDColors.SkyBlue);
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
        if (!HydroJebCore.InFlight)
            return;
        if (this == CurCam)
        {
            CurCam = null;
            panel.ResetHeight();
            if (CamActivate)
                CamActivate = false;
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
        if (CamActivate)
        {
            HydroFlightCameraManager.SetNullTarget();
            HydroFlightCameraManager.SetTransformParent(transform);
            HydroFlightCameraManager.SetPosition(CamPos);
            HydroFlightCameraManager.SetRotation(CamForward, CamUp);
            HydroFlightCameraManager.SetFoV(CamDefFoV / Mag);
            HydroFlightCameraManager.SetNearClipPlane(CamClip);
            RequestResource("ElectricCharge", Behaviours.Electric_Consumption_Camera * TimeWarp.deltaTime);
        }
    }

    ModulePartRename ModuleRename { get { return (ModulePartRename)Modules["ModulePartRename"]; } }
    public override string ToString()
    {
        if (ModuleRename.Renamed)
            return ModuleRename.nameString;
        else
            return RelPos().ToString("#0.00");
    }
}