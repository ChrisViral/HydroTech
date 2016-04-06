using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Core;
    using PartModules.Base;

    public partial class PanelDockAssist // Choosing target
    {
        protected Vessel _PreviewVessel = null;
        protected Vessel PreviewVessel
        {
            get { return _PreviewVessel; }
            set
            {
                if (value != _PreviewVessel)
                {
                    if (value == null)
                        HydroFlightCameraManager.RetrieveLast();
                    else
                    {
                        if (_PreviewVessel == null && PreviewPart == null)
                            HydroFlightCameraManager.SaveCurrent();
                        HydroFlightCameraManager.SetCallback(DoPreviewVessel);
                    }
                    _PreviewVessel = value;
                }
            }
        }
        protected void DoPreviewVessel()
        {
            HydroFlightCameraManager.SetFoV(Behaviours.DefaultFoV_PreviewVessel);
            HydroFlightCameraManager.SetTarget(PreviewVessel);
        }

        protected bool _ChoosingVessel = false;
        protected bool ChoosingVessel
        {
            get { return _ChoosingVessel; }
            set
            {
                if (value != _ChoosingVessel)
                {
                    ResetHeight();
                    if (value)
                    {
                        targetVesselListUI.SetSelectionToItem(TargetVessel);
                        targetVesselListUI.SetToCurSelPage();
                    }
                }
                _ChoosingVessel = value;
            }
        }

        protected bool _ChoosingTarget = false;
        protected bool ChoosingTarget
        {
            get { return _ChoosingTarget; }
            set
            {
                if (value != _ChoosingTarget)
                {
                    ResetHeight();
                    if (value)
                    {
                        DA.cameraPaused = true;
                        if (TargetVessel == null)
                            ChoosingVessel = true;
                        else
                            PreviewVessel = TargetVessel;
                        targetListUI.SetSelectionToItem(Target);
                        targetListUI.SetToCurSelPage();
                        targetListUI.SetSelectionToItem(null);
                    }
                    else
                    {
                        PreviewPart = null;
                        targetListUI.SetSelectionToItem(null);
                        DA.cameraPaused = false;
                    }
                }
                _ChoosingTarget = value;
            }
        }

        protected virtual void drawVesselBtn(Vessel v)
        {
            if (v == null)
                GUILayout.Button("");
            else
            {
                if (GUILayout.Button(
                    v.vesselName,
                    v == TargetVessel ?
                    v == PreviewVessel ? BtnStyle_Wrap(Color.blue) : BtnStyle_Wrap(Color.green)
                    :
                    v == PreviewVessel ? BtnStyle_Wrap(Color.yellow) : BtnStyle_Wrap()
                    ))
                    targetVesselListUI.SetSelectionToItem(v);
            }
        }

        protected virtual void drawTargetBtn(HydroPartModule mtgt)
        {
            if (mtgt == null)
                GUILayout.Button("");
            else
            {
                if (GUILayout.Button(
                    ((ModuleDockAssistTarget)mtgt).ToString(),
                    mtgt == Target ?
                    (IPartPreview)mtgt == PreviewPart ? BtnStyle_Wrap(Color.blue) : BtnStyle_Wrap(Color.green)
                    :
                    (IPartPreview)mtgt == PreviewPart ? BtnStyle_Wrap(Color.yellow) : BtnStyle_Wrap()
                    ))
                    targetListUI.SetSelectionToItem(mtgt);
            }
        }

        protected void DrawChoosingVesselUI()
        {
            GUILayout.Label("Vessel:");
            bool pageChanged; bool noTargetVessel;
            targetVesselListUI.OnDrawUI(drawVesselBtn, out pageChanged, out noTargetVessel);
            PreviewVessel = targetVesselListUI.curSelect;
            if (pageChanged)
                ResetHeight();
            if (noTargetVessel)
                GUILayout.Label("Nothing in sight");
            GUILayout.BeginHorizontal();
            if (PreviewVessel == null)
                GUILayout.Button("OK", BtnStyle(Color.red));
            else
                if (GUILayout.Button("OK"))
                {
                    TargetVessel = PreviewVessel;
                    ChoosingVessel = false;
                }
            if (GUILayout.Button("Cancel"))
            {
                ChoosingVessel = false;
                if (TargetVessel == null)
                {
                    ChoosingTarget = false;
                    PreviewVessel = null;
                }
            }
            GUILayout.EndHorizontal();
        }

        protected void DrawChoosingTargetUI()
        {
            GUILayout.Label("Vessel");
            if (GUILayout.Button(
                TargetVessel.vesselName,
                targetVesselList.Contains(TargetVessel) ? BtnStyle_Wrap(Color.green) : BtnStyle_Wrap(Color.red)
                ))
                ChoosingVessel = true;
            GUILayout.Label("Target:");
            bool pageChanged; bool noTarget;
            targetListUI.OnDrawUI(drawTargetBtn, out pageChanged, out noTarget);
            PreviewPart = (ModuleDockAssistTarget)targetListUI.curSelect;
            if (PreviewPart != null)
                _PreviewVessel = null;
            if (pageChanged)
                ResetHeight();
            if (noTarget)
                GUILayout.Label("Not installed");
            GUILayout.BeginHorizontal();
            if (PreviewPart == null)
                GUILayout.Button("OK", BtnStyle(Color.red));
            else
                if (GUILayout.Button("OK"))
                {
                    Target = (ModuleDockAssistTarget)PreviewPart;
                    ChoosingTarget = false;
                }
            if (GUILayout.Button("Cancel"))
            {
                ChoosingTarget = false;
                PreviewVessel = null;
            }
            if (Target == null)
                GUILayout.Button("Clear choice", BtnStyle(Color.red));
            else
                if (GUILayout.Button("Clear choice"))
                {
                    TargetVessel = null;
                    Target = null;
                    ChoosingTarget = false;
                    PreviewVessel = null;
                }
            GUILayout.EndHorizontal();
        }
    }
}