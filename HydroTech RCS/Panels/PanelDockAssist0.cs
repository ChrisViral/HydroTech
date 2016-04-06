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
        public static PanelDockAssist thePanel { get { return (PanelDockAssist)HydroJebCore.panels[PanelIDs.Dock]; } }

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

        protected static APDockAssist DA { get { return APDockAssist.theAutopilot; } }

        protected override void MakeAPSave() { DA.MakeSaveAtNextUpdate(); }

        protected override bool Engaged
        {
            get { return DA.engaged; }
            set { DA.engaged = value; }
        }
        protected static ModuleDockAssistCam Cam
        {
            get { return DA.cam; }
            set { DA.cam = value; }
        }
        protected static ModuleDockAssistTarget Target
        {
            get { return DA.target; }
            set { DA.target = value; }
        }
        protected static bool NullCamera() { return DA.NullCamera(); }
        protected static bool NullTarget() { return DA.NullTarget(); }
        protected static bool Manual
        {
            get { return DA.manual; }
            set { DA.manual = value; }
        }
        protected static bool ShowLine
        {
            get { return DA.showLine; }
            set { DA.showLine = value; }
        }
        protected static bool AutoOrient
        {
            get { return DA.autoOrient; }
            set { DA.autoOrient = value; }
        }
        protected static bool KillRelV
        {
            get { return DA.killRelV; }
            set { DA.killRelV = value; }
        }
        protected static bool CamView
        {
            get { return DA.camView; }
            set { DA.camView = value; }
        }
        protected static int CamMag
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
        protected static float AngularAcc
        {
            get { return DA.angularAcc; }
            set { DA.angularAcc = value; }
        }
        protected static float Acc
        {
            get { return DA.acc; }
            set { DA.acc = value; }
        }
        protected static float FSSpeed
        {
            get { return DA.finalStageSpeed; }
            set { DA.finalStageSpeed = value; }
        }
        protected static bool TargetHasJeb() { return DA.TargetHasJeb(); }
        protected static bool DriveTarget
        {
            get { return DA.driveTarget; }
            set { DA.driveTarget = value; }
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