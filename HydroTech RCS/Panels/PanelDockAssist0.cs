using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using HydroTech_FC;
    using Autopilots;
    using UI;
    using Constants.Core;
    using Constants.Panels;

    public partial class PanelDockAssist : PanelwAP
    {
        static public PanelDockAssist thePanel { get { return (PanelDockAssist)HydroJebCore.panels[PanelIDs.Dock]; } }

        public PanelDockAssist()
        {
            fileName = new FileName("dock", "cfg", HydroJebCore.PanelSaveFolder);
            camListUI = new SingleSelectionListUI<HydroPartModule>(HydroJebCore.dockCams.listActiveVessel);
            targetVesselList = new AffiliationList<HydroPartModule, Vessel>(
                HydroJebCore.dockTargets.listInactiveVessel,
                (AffiliationList<HydroPartModule, Vessel>.GetItemFunction_Single)GetTargetVessel
                );
            targetVesselListUI = new SingleSelectionListUI<Vessel>(targetVesselList);
            targetList = new SubList<HydroPartModule>(HydroJebCore.dockTargets.listInactiveVessel, isOnTargetVessel);
            targetListUI = new SingleSelectionListUI<HydroPartModule>(targetList);
        }

        protected override int PanelID { get { return PanelIDs.Dock; } }
        public override string PanelTitle { get { return PanelTitles.Docking; } }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.Docking; }

        static protected APDockAssist DA { get { return APDockAssist.theAutopilot; } }

        protected override void MakeAPSave() { DA.MakeSaveAtNextUpdate(); }

        protected override bool Engaged
        {
            get { return DA.Engaged; }
            set { DA.Engaged = value; }
        }
        static protected ModuleDockAssistCam Cam
        {
            get { return DA.Cam; }
            set { DA.Cam = value; }
        }
        static protected ModuleDockAssistTarget Target
        {
            get { return DA.Target; }
            set { DA.Target = value; }
        }
        static protected bool NullCamera() { return DA.NullCamera(); }
        static protected bool NullTarget() { return DA.NullTarget(); }
        static protected bool Manual
        {
            get { return DA.Manual; }
            set { DA.Manual = value; }
        }
        static protected bool ShowLine
        {
            get { return DA.ShowLine; }
            set { DA.ShowLine = value; }
        }
        static protected bool AutoOrient
        {
            get { return DA.AutoOrient; }
            set { DA.AutoOrient = value; }
        }
        static protected bool KillRelV
        {
            get { return DA.KillRelV; }
            set { DA.KillRelV = value; }
        }
        static protected bool CamView
        {
            get { return DA.CamView; }
            set { DA.CamView = value; }
        }
        static protected int CamMag
        {
            get 
            {
                if (Cam == null)
                    throw (new Exception("PanelDockAssist.CamMag<get> before a camera is selected"));
                return Cam.mag; 
            }
            set 
            {
                if (Cam == null)
                    throw (new Exception("PanelDockAssist.CamMag<set> before a camera is selected"));
                Cam.mag = value;
            }
        }
        static protected float AngularAcc
        {
            get { return DA.AngularAcc; }
            set { DA.AngularAcc = value; }
        }
        static protected float Acc
        {
            get { return DA.Acc; }
            set { DA.Acc = value; }
        }
        static protected float FSSpeed
        {
            get { return DA.FinalStageSpeed; }
            set { DA.FinalStageSpeed = value; }
        }
        static protected bool TargetHasJeb() { return DA.TargetHasJeb(); }
        static protected bool DriveTarget
        {
            get { return DA.DriveTarget; }
            set { DA.DriveTarget = value; }
        }

        protected SingleSelectionListUI<HydroPartModule> camListUI = null;

        protected Vessel TargetVessel = null;
        protected AffiliationList<HydroPartModule, Vessel> targetVesselList = null;
        protected Vessel GetTargetVessel(HydroPartModule mtgt) { return mtgt.vessel; }
        protected SingleSelectionListUI<Vessel> targetVesselListUI = null;

        protected SubList<HydroPartModule> targetList = null;
        protected bool isOnTargetVessel(HydroPartModule pm) { return pm.vessel == TargetVessel; }
        protected SingleSelectionListUI<HydroPartModule> targetListUI = null;

        public override void onFlightStart()
        {
            base.onFlightStart();
            ChoosingCamera = false;
            ChoosingTarget = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            targetVesselList.OnUpdate();
            targetList.OnUpdate();

            camListUI.OnUpdate();
            targetVesselListUI.OnUpdate();
            targetListUI.OnUpdate();
        }
    }
}