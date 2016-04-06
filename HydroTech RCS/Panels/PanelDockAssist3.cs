using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using PartModules;
    using PartModules.Base;

    public partial class PanelDockAssist // Choosing camera
    {
        protected IPartPreview _PreviewPart = null;
        protected IPartPreview PreviewPart
        {
            get { return _PreviewPart; }
            set
            {
                if (value != _PreviewPart)
                {
                    if (value == null)
                        HydroFlightCameraManager.RetrieveLast();
                    else
                    {
                        if (_PreviewPart == null && PreviewVessel == null)
                            HydroFlightCameraManager.SaveCurrent();
                        HydroFlightCameraManager.SetCallback(value.DoPreview);
                    }
                    _PreviewPart = value;
                }
            }
        }

        protected bool _ChoosingCamera = false;
        protected bool ChoosingCamera
        {
            get { return _ChoosingCamera; }
            set
            {
                if (value != _ChoosingCamera)
                {
                    ResetHeight();
                    if (value)
                    {
                        DA.CameraPaused = true;
                        camListUI.SetSelectionToItem(Cam);
                        camListUI.SetToCurSelPage();
                        camListUI.SetSelectionToItem(null);
                    }
                    else
                    {
                        PreviewPart = null;
                        camListUI.SetSelectionToItem(null);
                        DA.CameraPaused = false;
                    }
                }
                _ChoosingCamera = value;
            }
        }

        virtual protected void drawCamBtn(HydroPartModule mcam)
        {
            if (mcam == null)
                GUILayout.Button("");
            else
            {
                if (GUILayout.Button(
                    ((ModuleDockAssistCam)mcam).ToString(),
                    mcam == Cam ?
                    (IPartPreview)mcam == PreviewPart ? BtnStyle_Wrap(Color.blue) : BtnStyle_Wrap(Color.green)
                    :
                    (IPartPreview)mcam == PreviewPart ? BtnStyle_Wrap(Color.yellow) : BtnStyle_Wrap()
                    ))
                    camListUI.SetSelectionToItem(mcam);
            }
        }

        protected void DrawChoosingCameraUI()
        {
            GUILayout.Label("Camera:");
            bool pageChanged; bool noCam;
            camListUI.OnDrawUI(drawCamBtn, out pageChanged, out noCam);
            PreviewPart = (ModuleDockAssistCam)camListUI.curSelect;
            if (pageChanged)
                ResetHeight();
            if (noCam)
                GUILayout.Label("Not installed");
            GUILayout.BeginHorizontal();
            if (PreviewPart == null)
                GUILayout.Button("OK", BtnStyle(Color.red));
            else
                if (GUILayout.Button("OK"))
                {
                    Cam = (ModuleDockAssistCam)PreviewPart;
                    ChoosingCamera = false;
                }
            if (GUILayout.Button("Cancel"))
                ChoosingCamera = false;
            if (Cam == null)
                GUILayout.Button("Clear choice", BtnStyle(Color.red));
            else
                if (GUILayout.Button("Clear choice"))
                {
                    Cam = null;
                    ChoosingCamera = false;
                }
            GUILayout.EndHorizontal();
        }
    }
}