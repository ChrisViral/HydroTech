using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using HydroTech_FC;
    using UI;
    using Constants.Core;
    using Constants.Panels;

    public partial class PanelDockAssistEditorAid : Panel, IPanelEditor
    {
        public PanelDockAssistEditorAid()
        {
            fileName = new FileName("dockeditor", "cfg", HydroJebCore.PanelSaveFolder);
            cams = new AffiliationList<Part, ModuleDockAssistCam>(
                null, // EditorLogic.SortedShipList has not been initialized yet
                (AffiliationList<Part, ModuleDockAssistCam>.GetItemFunction_Multi)GetCam
                );
            targets = new AffiliationList<Part, ModuleDockAssistTarget>(
                null,
                (AffiliationList<Part, ModuleDockAssistTarget>.GetItemFunction_Multi)GetTgt
                );
            camset = new DictionaryFromList<ModuleDockAssistCam, DAEditorSet>(cams, new DAEditorSet(false));
            tgtset = new DictionaryFromList<ModuleDockAssistTarget, DAEditorSet>(targets, new DAEditorSet(false));
            camUI = new MultiPageListUI<ModuleDockAssistCam>(cams, 2);
            tgtUI = new MultiPageListUI<ModuleDockAssistTarget>(targets, 2);
        }

        protected override int PanelID { get { return PanelIDs.Dock; } }
        public override string PanelTitle
        {
            get
            {
                return editorHide ?
                    PanelTitles.DockAssistEditorAid_Hide
                    : PanelTitles.DockAssistEditorAid;
            }
        }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.DockAssistEditor; }

        [HydroSLNodeInfo(name = "PANELEDITOR")]
        [HydroSLField(saveName = "Minimized")]
        public bool editorHide = false;

        public void ShowInEditor()
        {
            Active = true;
            Load();
            AddPanel();
            OnEditorUpdate();
            UpdateAllRenames();
        }

        public void HideInEditor()
        {
            PanelShown = false;
            Active = false;
        }

        public void OnEditorUpdate()
        {
            cams.SetParent(EditorLogic.SortedShipList);
            targets.SetParent(EditorLogic.SortedShipList);
            cams.OnUpdate();
            targets.OnUpdate();
            camset.OnUpdate();
            tgtset.OnUpdate();
            camUI.OnUpdate();
            tgtUI.OnUpdate();
            if (needSave)
                Save();
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            _PanelShown = true;
            editorHide = false;
            showCams = true;
        }
    }
}