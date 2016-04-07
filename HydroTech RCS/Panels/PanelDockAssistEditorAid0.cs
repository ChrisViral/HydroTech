using HydroTech_FC;
using HydroTech_RCS.Constants;
using HydroTech_RCS.Constants.Panels;
using HydroTech_RCS.Panels.UI;

namespace HydroTech_RCS.Panels
{
    public partial class PanelDockAssistEditorAid : Panel, IPanelEditor
    {
        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "Minimized")]
        public bool editorHide;

        protected override int PanelID
        {
            get { return CoreConsts.pDock; }
        }

        public override string PanelTitle
        {
            get { return this.editorHide ? PanelTitles.dockAssistEditorAidHide : PanelTitles.dockAssistEditorAid; }
        }

        public PanelDockAssistEditorAid()
        {
            this.fileName = new FileName("dockeditor", "cfg", HydroJebCore.panelSaveFolder);
            this.cams = new AffiliationList<Part, ModuleDockAssistCam>(null, // EditorLogic.SortedShipList has not been initialized yet
                (AffiliationList<Part, ModuleDockAssistCam>.GetItemFunction_Multi)GetCam);
            this.targets = new AffiliationList<Part, ModuleDockAssistTarget>(null, (AffiliationList<Part, ModuleDockAssistTarget>.GetItemFunction_Multi)GetTgt);
            this.camset = new DictionaryFromList<ModuleDockAssistCam, DaEditorSet>(this.cams, new DaEditorSet(false));
            this.tgtset = new DictionaryFromList<ModuleDockAssistTarget, DaEditorSet>(this.targets, new DaEditorSet(false));
            this.camUi = new MultiPageListUi<ModuleDockAssistCam>(this.cams, 2);
            this.tgtUi = new MultiPageListUi<ModuleDockAssistTarget>(this.targets, 2);
        }

        public void ShowInEditor()
        {
            this.Active = true;
            Load();
            AddPanel();
            OnEditorUpdate();
            UpdateAllRenames();
        }

        public void HideInEditor()
        {
            this.PanelShown = false;
            this.Active = false;
        }

        public void OnEditorUpdate()
        {
            this.cams.SetParent(EditorLogic.SortedShipList);
            this.targets.SetParent(EditorLogic.SortedShipList);
            this.cams.OnUpdate();
            this.targets.OnUpdate();
            this.camset.OnUpdate();
            this.tgtset.OnUpdate();
            this.camUi.OnUpdate();
            this.tgtUi.OnUpdate();
            if (this.needSave) { Save(); }
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = WindowPositions.dockAssistEditor;
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShown = true;
            this.editorHide = false;
            this.showCams = true;
        }
    }
}