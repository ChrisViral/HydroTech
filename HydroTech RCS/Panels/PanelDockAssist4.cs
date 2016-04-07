using HydroTech_FC;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelDockAssist // Choosing target
    {
        protected bool choosingTarget;

        protected bool choosingVessel;
        protected Vessel previewVessel;

        protected Vessel PreviewVessel
        {
            get { return this.previewVessel; }
            set
            {
                if (value != this.previewVessel)
                {
                    if (value == null) { HydroFlightCameraManager.RetrieveLast(); }
                    else
                    {
                        if (this.previewVessel == null && this.PreviewPart == null) { HydroFlightCameraManager.SaveCurrent(); }
                        HydroFlightCameraManager.SetCallback(DoPreviewVessel);
                    }
                    this.previewVessel = value;
                }
            }
        }

        protected bool ChoosingVessel
        {
            get { return this.choosingVessel; }
            set
            {
                if (value != this.choosingVessel)
                {
                    ResetHeight();
                    if (value)
                    {
                        this.targetVesselListUi.SetSelectionToItem(this.targetVessel);
                        this.targetVesselListUi.SetToCurSelPage();
                    }
                }
                this.choosingVessel = value;
            }
        }

        protected bool ChoosingTarget
        {
            get { return this.choosingTarget; }
            set
            {
                if (value != this.choosingTarget)
                {
                    ResetHeight();
                    if (value)
                    {
                        Da.CameraPaused = true;
                        if (this.targetVessel == null) { this.ChoosingVessel = true; }
                        else
                        {
                            this.PreviewVessel = this.targetVessel;
                        }
                        this.targetListUi.SetSelectionToItem(Target);
                        this.targetListUi.SetToCurSelPage();
                        this.targetListUi.SetSelectionToItem(null);
                    }
                    else
                    {
                        this.PreviewPart = null;
                        this.targetListUi.SetSelectionToItem(null);
                        Da.CameraPaused = false;
                    }
                }
                this.choosingTarget = value;
            }
        }

        protected void DoPreviewVessel()
        {
            HydroFlightCameraManager.SetFoV(Behaviours.defaultFoVPreviewVessel);
            HydroFlightCameraManager.SetTarget(this.PreviewVessel);
        }

        protected virtual void DrawVesselBtn(Vessel v)
        {
            if (v == null) { GUILayout.Button(""); }
            else
            {
                if (GUILayout.Button(v.vesselName, v == this.targetVessel ? v == this.PreviewVessel ? BtnStyle_Wrap(Color.blue) : BtnStyle_Wrap(Color.green) : v == this.PreviewVessel ? BtnStyle_Wrap(Color.yellow) : BtnStyle_Wrap())) { this.targetVesselListUi.SetSelectionToItem(v); }
            }
        }

        protected virtual void DrawTargetBtn(HydroPartModule mtgt)
        {
            if (mtgt == null) { GUILayout.Button(""); }
            else
            {
                if (GUILayout.Button(((ModuleDockAssistTarget)mtgt).ToString(), mtgt == Target ? (IPartPreview)mtgt == this.PreviewPart ? BtnStyle_Wrap(Color.blue) : BtnStyle_Wrap(Color.green) : (IPartPreview)mtgt == this.PreviewPart ? BtnStyle_Wrap(Color.yellow) : BtnStyle_Wrap())) { this.targetListUi.SetSelectionToItem(mtgt); }
            }
        }

        protected void DrawChoosingVesselUi()
        {
            GUILayout.Label("Vessel:");
            bool pageChanged;
            bool noTargetVessel;
            this.targetVesselListUi.OnDrawUi(DrawVesselBtn, out pageChanged, out noTargetVessel);
            this.PreviewVessel = this.targetVesselListUi.CurSelect;
            if (pageChanged) { ResetHeight(); }
            if (noTargetVessel) { GUILayout.Label("Nothing in sight"); }
            GUILayout.BeginHorizontal();
            if (this.PreviewVessel == null) { GUILayout.Button("OK", BtnStyle(Color.red)); }
            else if (GUILayout.Button("OK"))
            {
                this.targetVessel = this.PreviewVessel;
                this.ChoosingVessel = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.ChoosingVessel = false;
                if (this.targetVessel == null)
                {
                    this.ChoosingTarget = false;
                    this.PreviewVessel = null;
                }
            }
            GUILayout.EndHorizontal();
        }

        protected void DrawChoosingTargetUi()
        {
            GUILayout.Label("Vessel");
            if (GUILayout.Button(this.targetVessel.vesselName, this.targetVesselList.Contains(this.targetVessel) ? BtnStyle_Wrap(Color.green) : BtnStyle_Wrap(Color.red))) { this.ChoosingVessel = true; }
            GUILayout.Label("Target:");
            bool pageChanged;
            bool noTarget;
            this.targetListUi.OnDrawUi(DrawTargetBtn, out pageChanged, out noTarget);
            this.PreviewPart = (ModuleDockAssistTarget)this.targetListUi.CurSelect;
            if (this.PreviewPart != null) { this.previewVessel = null; }
            if (pageChanged) { ResetHeight(); }
            if (noTarget) { GUILayout.Label("Not installed"); }
            GUILayout.BeginHorizontal();
            if (this.PreviewPart == null) { GUILayout.Button("OK", BtnStyle(Color.red)); }
            else if (GUILayout.Button("OK"))
            {
                Target = (ModuleDockAssistTarget)this.PreviewPart;
                this.ChoosingTarget = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.ChoosingTarget = false;
                this.PreviewVessel = null;
            }
            if (Target == null) { GUILayout.Button("Clear choice", BtnStyle(Color.red)); }
            else if (GUILayout.Button("Clear choice"))
            {
                this.targetVessel = null;
                Target = null;
                this.ChoosingTarget = false;
                this.PreviewVessel = null;
            }
            GUILayout.EndHorizontal();
        }
    }
}