using HydroTech_FC;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelDockAssist // Choosing camera
    {
        protected bool choosingCamera;
        protected IPartPreview previewPart;

        protected IPartPreview PreviewPart
        {
            get { return this.previewPart; }
            set
            {
                if (value != this.previewPart)
                {
                    if (value == null) { HydroFlightCameraManager.RetrieveLast(); }
                    else
                    {
                        if (this.previewPart == null && this.PreviewVessel == null) { HydroFlightCameraManager.SaveCurrent(); }
                        HydroFlightCameraManager.SetCallback(value.DoPreview);
                    }
                    this.previewPart = value;
                }
            }
        }

        protected bool ChoosingCamera
        {
            get { return this.choosingCamera; }
            set
            {
                if (value != this.choosingCamera)
                {
                    ResetHeight();
                    if (value)
                    {
                        Da.CameraPaused = true;
                        this.camListUi.SetSelectionToItem(Cam);
                        this.camListUi.SetToCurSelPage();
                        this.camListUi.SetSelectionToItem(null);
                    }
                    else
                    {
                        this.PreviewPart = null;
                        this.camListUi.SetSelectionToItem(null);
                        Da.CameraPaused = false;
                    }
                }
                this.choosingCamera = value;
            }
        }

        protected virtual void DrawCamBtn(HydroPartModule mcam)
        {
            if (mcam == null) { GUILayout.Button(""); }
            else
            {
                if (GUILayout.Button(((ModuleDockAssistCam)mcam).ToString(), mcam == Cam ? (IPartPreview)mcam == this.PreviewPart ? BtnStyle_Wrap(Color.blue) : BtnStyle_Wrap(Color.green) : (IPartPreview)mcam == this.PreviewPart ? BtnStyle_Wrap(Color.yellow) : BtnStyle_Wrap())) { this.camListUi.SetSelectionToItem(mcam); }
            }
        }

        protected void DrawChoosingCameraUi()
        {
            GUILayout.Label("Camera:");
            bool pageChanged;
            bool noCam;
            this.camListUi.OnDrawUi(DrawCamBtn, out pageChanged, out noCam);
            this.PreviewPart = (ModuleDockAssistCam)this.camListUi.CurSelect;
            if (pageChanged) { ResetHeight(); }
            if (noCam) { GUILayout.Label("Not installed"); }
            GUILayout.BeginHorizontal();
            if (this.PreviewPart == null) { GUILayout.Button("OK", BtnStyle(Color.red)); }
            else if (GUILayout.Button("OK"))
            {
                Cam = (ModuleDockAssistCam)this.PreviewPart;
                this.ChoosingCamera = false;
            }
            if (GUILayout.Button("Cancel")) { this.ChoosingCamera = false; }
            if (Cam == null) { GUILayout.Button("Clear choice", BtnStyle(Color.red)); }
            else if (GUILayout.Button("Clear choice"))
            {
                Cam = null;
                this.ChoosingCamera = false;
            }
            GUILayout.EndHorizontal();
        }
    }
}