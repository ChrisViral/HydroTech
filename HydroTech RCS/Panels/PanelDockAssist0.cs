using System;
using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants;
using HydroTech_RCS.Constants.Panels;
using HydroTech_RCS.Panels.UI;

namespace HydroTech_RCS.Panels
{
    public partial class PanelDockAssist : PanelwAP
    {
        protected SingleSelectionListUi<HydroPartModule> camListUi;

        protected SubList<HydroPartModule> targetList;
        protected SingleSelectionListUi<HydroPartModule> targetListUi;

        protected Vessel targetVessel = null;
        protected AffiliationList<HydroPartModule, Vessel> targetVesselList;
        protected SingleSelectionListUi<Vessel> targetVesselListUi;

        public static PanelDockAssist ThePanel
        {
            get { return (PanelDockAssist)HydroJebCore.panels[CoreConsts.pDock]; }
        }

        protected override int PanelID
        {
            get { return CoreConsts.pDock; }
        }

        public override string PanelTitle
        {
            get { return PanelTitles.docking; }
        }

        protected static APDockAssist Da
        {
            get { return APDockAssist.TheAutopilot; }
        }

        protected override bool Engaged
        {
            get { return Da.Engaged; }
            set { Da.Engaged = value; }
        }

        protected static ModuleDockAssistCam Cam
        {
            get { return Da.Cam; }
            set { Da.Cam = value; }
        }

        protected static ModuleDockAssistTarget Target
        {
            get { return Da.target; }
            set { Da.target = value; }
        }

        protected static bool Manual
        {
            get { return Da.Manual; }
            set { Da.Manual = value; }
        }

        protected static bool ShowLine
        {
            get { return Da.ShowLine; }
            set { Da.ShowLine = value; }
        }

        protected static bool AutoOrient
        {
            get { return Da.AutoOrient; }
            set { Da.AutoOrient = value; }
        }

        protected static bool KillRelV
        {
            get { return Da.KillRelV; }
            set { Da.KillRelV = value; }
        }

        protected static bool CamView
        {
            get { return Da.CamView; }
            set { Da.CamView = value; }
        }

        protected static int CamMag
        {
            get
            {
                if (Cam == null) { throw new Exception("PanelDockAssist.CamMag<get> before a camera is selected"); }
                return Cam.mag;
            }
            set
            {
                if (Cam == null) { throw new Exception("PanelDockAssist.CamMag<set> before a camera is selected"); }
                Cam.mag = value;
            }
        }

        protected static float AngularAcc
        {
            get { return Da.angularAcc; }
            set { Da.angularAcc = value; }
        }

        protected static float Acc
        {
            get { return Da.acc; }
            set { Da.acc = value; }
        }

        protected static float FsSpeed
        {
            get { return Da.finalStageSpeed; }
            set { Da.finalStageSpeed = value; }
        }

        protected static bool DriveTarget
        {
            get { return Da.DriveTarget; }
            set { Da.DriveTarget = value; }
        }

        public PanelDockAssist()
        {
            this.fileName = new FileName("dock", "cfg", HydroJebCore.panelSaveFolder);
            this.camListUi = new SingleSelectionListUi<HydroPartModule>(HydroJebCore.dockCams.listActiveVessel);
            this.targetVesselList = new AffiliationList<HydroPartModule, Vessel>(HydroJebCore.dockTargets.listInactiveVessel, (AffiliationList<HydroPartModule, Vessel>.GetItemFunction_Single)GetTargetVessel);
            this.targetVesselListUi = new SingleSelectionListUi<Vessel>(this.targetVesselList);
            this.targetList = new SubList<HydroPartModule>(HydroJebCore.dockTargets.listInactiveVessel, IsOnTargetVessel);
            this.targetListUi = new SingleSelectionListUi<HydroPartModule>(this.targetList);
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = WindowPositions.docking;
        }

        protected override void MakeAPSave()
        {
            Da.MakeSaveAtNextUpdate();
        }

        protected static bool NullCamera()
        {
            return Da.NullCamera();
        }

        protected static bool NullTarget()
        {
            return Da.NullTarget();
        }

        protected static bool TargetHasJeb()
        {
            return Da.TargetHasJeb();
        }

        protected Vessel GetTargetVessel(HydroPartModule mtgt)
        {
            return mtgt.vessel;
        }

        protected bool IsOnTargetVessel(HydroPartModule pm)
        {
            return pm.vessel == this.targetVessel;
        }

        public override void onFlightStart()
        {
            base.onFlightStart();
            this.ChoosingCamera = false;
            this.ChoosingTarget = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            this.targetVesselList.OnUpdate();
            this.targetList.OnUpdate();

            this.camListUi.OnUpdate();
            this.targetVesselListUi.OnUpdate();
            this.targetListUi.OnUpdate();
        }
    }
}