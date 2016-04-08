using System.Collections.Generic;
using HydroTech.Constants;
using HydroTech.Data;
using HydroTech.File;
using HydroTech.Panels.UI;
using HydroTech.PartModules;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelDockAssistEditor : Panel, IPanelEditor
    {
        protected struct DAEditorSet
        {
            public bool drawn;
            public bool renamed;
            public string name;

            public DAEditorSet(bool dr = false, bool rn = false, string n = "")
            {
                this.drawn = dr;
                this.renamed = rn;
                this.name = n;
            }
        }

        #region Field
        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "Minimized")]
        public bool editorHide;

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLNodeInfo(i = 1, name = "SETTINGS"), HydroSLField(saveName = "ShowCams")]
        public bool showCams = true;

        protected AffiliationList<Part, ModuleDockAssistCam> cams;
        protected AffiliationList<Part, ModuleDockAssistTarget> targets;
        protected DictionaryFromList<ModuleDockAssistCam, DAEditorSet> camSet;
        protected DictionaryFromList<ModuleDockAssistTarget, DAEditorSet> tgtSet;
        protected UIMultiPageList<ModuleDockAssistCam> camUI;
        protected UIMultiPageList<ModuleDockAssistTarget> tgtUI;
        #endregion

        #region Propeties
        protected override int PanelID
        {
            get { return CoreConsts.pDock; }
        }

        public override string PanelTitle
        {
            get { return this.editorHide ? PanelConsts.dockAssistEditorAidHideTitle : PanelConsts.dockAssistEditorAidTitle; }
        }
        #endregion

        #region Constructor
        public PanelDockAssistEditor()
        {
            this.fileName = new FileName("dockeditor", "cfg", HydroJebCore.panelSaveFolder);
            this.cams = new AffiliationList<Part, ModuleDockAssistCam>(null, (AffiliationList<Part, ModuleDockAssistCam>.GetItemFunctionMulti)GetCam);
            this.targets = new AffiliationList<Part, ModuleDockAssistTarget>(null, (AffiliationList<Part, ModuleDockAssistTarget>.GetItemFunctionMulti)GetTgt);
            this.camSet = new DictionaryFromList<ModuleDockAssistCam, DAEditorSet>(this.cams, new DAEditorSet(false));
            this.tgtSet = new DictionaryFromList<ModuleDockAssistTarget, DAEditorSet>(this.targets, new DAEditorSet(false));
            this.camUI = new UIMultiPageList<ModuleDockAssistCam>(this.cams, 2);
            this.tgtUI = new UIMultiPageList<ModuleDockAssistTarget>(this.targets, 2);
        }
        #endregion

        #region Method
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
            this.cams.Update();
            this.targets.Update();
            this.camSet.Update();
            this.tgtSet.Update();
            this.camUI.OnUpdate();
            this.tgtUI.OnUpdate();
            if (this.needSave) { Save(); }
        }

        protected List<ModuleDockAssistCam> GetCam(Part p)
        {
            return p.FindModulesImplementing<ModuleDockAssistCam>();
        }

        protected List<ModuleDockAssistTarget> GetTgt(Part p)
        {
            return p.FindModulesImplementing<ModuleDockAssistTarget>();
        }

        protected void UpdateAllRenames()
        {
            foreach (ModuleDockAssistCam mcam in this.cams)
            {
                DAEditorSet set = this.camSet[mcam];
                set.renamed = mcam.ModuleRename.Renamed;
                set.name = mcam.ModuleRename.nameString;
                this.camSet[mcam] = set;
            }
            foreach (ModuleDockAssistTarget mtgt in this.targets)
            {
                DAEditorSet set = this.tgtSet[mtgt];
                set.renamed = mtgt.ModuleRename.Renamed;
                set.name = mtgt.ModuleRename.nameString;
                this.tgtSet[mtgt] = set;
            }
        }
        #endregion

        #region Overrides
        protected override void SetDefaultWindowRect()
        {
            this.windowRect = PanelConsts.dockAssistEditor;
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShown = true;
            this.editorHide = false;
            this.showCams = true;
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(this.editorHide ? "Maximize" : "Minimize"))
            {
                this.editorHide = !this.editorHide;
                this.needSave = true;
                this.windowRect.width = this.editorHide ? 100 : 250;
                ResetHeight();
            }
            if (this.editorHide)
            {
                GUILayout.EndHorizontal();
                GUI.DragWindow();
                return;
            }
            if (GUILayout.Button("Cameras", this.showCams ? GUIUtils.ButtonStyle(Color.green) : GUIUtils.ButtonStyle()))
            {
                this.showCams = true;
                this.needSave = true;
                ResetHeight();
            }
            if (GUILayout.Button("Targets", this.showCams ? GUIUtils.ButtonStyle() : GUIUtils.ButtonStyle(Color.green)))
            {
                this.showCams = false;
                this.needSave = true;
                ResetHeight();
            }
            GUILayout.EndHorizontal();
            bool pageChanged, noItem;
            if (this.showCams) { this.camUI.OnDrawUI(CamUI, out pageChanged, out noItem); }
            else { this.tgtUI.OnDrawUI(TgtUI, out pageChanged, out noItem); }
            if (pageChanged) { ResetHeight(); }
            if (noItem) { GUILayout.Label("Not installed"); }

            GUI.DragWindow();
        }

        protected virtual void CamUI(ModuleDockAssistCam mcam)
        {
            if (mcam == null) { return; }
            GUILayout.Label(mcam.part.partInfo.title);
            DAEditorSet set = this.camSet[mcam];
            set.drawn = GUILayout.Toggle(set.drawn, "Visual Aid");
            if (set.drawn != this.camSet[mcam].drawn)
            {
                if (set.drawn) { mcam.ShowEditorAid(); }
                else { mcam.HideEditorAid(); }
            }
            GUILayout.BeginHorizontal();
            set.renamed = GUILayout.Toggle(set.renamed, set.renamed ? string.Empty : "Rename");
            if (set.renamed) { set.name = GUILayout.TextField(set.name); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name"))
            {
                mcam.ModuleRename.EditorRename(set.renamed, set.name);
                UpdateAllRenames();
            }
            this.camSet[mcam] = set;
        }

        protected virtual void TgtUI(ModuleDockAssistTarget mtgt)
        {
            if (mtgt == null) { return; }
            GUILayout.Label(mtgt.part.partInfo.title);
            DAEditorSet set = this.tgtSet[mtgt];
            set.drawn = GUILayout.Toggle(set.drawn, "Visual Aid");
            if (set.drawn != this.tgtSet[mtgt].drawn)
            {
                if (set.drawn) { mtgt.ShowEditorAid(); }
                else { mtgt.HideEditorAid(); }
            }
            GUILayout.BeginHorizontal();
            set.renamed = GUILayout.Toggle(set.renamed, set.renamed ? string.Empty : "Rename");
            if (set.renamed) { set.name = GUILayout.TextField(set.name); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name"))
            {
                mtgt.ModuleRename.EditorRename(set.renamed, set.name);
                UpdateAllRenames();
            }
            this.tgtSet[mtgt] = set;
        }
        #endregion
    }
}