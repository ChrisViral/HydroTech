using HydroTech_FC;
using HydroTech_RCS.Autopilots.Calculators;
using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelRcsThrustInfo : Panel, IPanelEditor
    {
        protected bool editor;

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "Minimized")]
        public bool editorHide;

        protected bool panelShownEditor;

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLNodeInfo(i = 1, name = "SETTINGS"), HydroSLField(saveName = "ShowRotation")]
        public bool showRotation = true;

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "WindowPos", cmd = CMD.Rect_TopLeft)]
        public Rect windowRectEditor;

        public static PanelRcsThrustInfo ThePanel
        {
            get { return (PanelRcsThrustInfo)HydroJebCore.panels[CoreConsts.rcsInfo]; }
        }

        protected override int PanelID
        {
            get { return CoreConsts.rcsInfo; }
        }

        public override string PanelTitle
        {
            get { return this.editor && this.editorHide ? PanelConsts.rcsInfoEditorHideTitle : PanelConsts.rcsInfoTitle; }
        }

        public override bool PanelShown
        {
            get { return this.editor ? this.panelShownEditor : base.PanelShown; }
            set
            {
                if (this.editor)
                {
                    if (!this.Active) { return; }
                    if (value != this.panelShownEditor)
                    {
                        if (value) { AddPanel(); }
                        else
                        {
                            RemovePanel();
                        }
                    }
                    this.panelShownEditor = value;
                }
                else
                {
                    base.PanelShown = value;
                }
            }
        }

        protected static RCSCalculator TheCalculator
        {
            get { return HydroJebCore.activeVesselRcs; }
        }

        public PanelRcsThrustInfo()
        {
            this.fileName = new FileName("rcsinfo", "cfg", HydroJebCore.panelSaveFolder);
        }

        public void ShowInEditor()
        {
            this.Active = true;
            this.editor = true;
            Load();
            AddPanel();
        }

        public void HideInEditor()
        {
            this.PanelShown = false;
            this.Active = false;
        }

        public void OnEditorUpdate()
        {
            if (this.needSave) { Save(); }
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = PanelConsts.rcsInfo;
            this.windowRectEditor = PanelConsts.rcsInfoEditor;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (TheCalculator.AllRcsEnabledChanged) { ResetHeight(); }
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShownEditor = true;
            this.editorHide = false;
            this.showRotation = true;
        }
    }
}