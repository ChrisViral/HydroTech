using System.Collections.Generic;
using HydroTech.Data;
using HydroTech.Panels.UI;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelDockAssistEditor : Panel
    {
        private struct DAEditorSet
        {
            #region Fields
            public bool drawn;
            public bool renamed;
            public string name;
            #endregion

            #region Constructors
            public DAEditorSet(bool drawn, bool renamed = false, string name = "")
            {
                this.drawn = drawn;
                this.renamed = renamed;
                this.name = name;
            }
            #endregion
        }

        #region Fields    
        public bool showCams = true;
        private Vector2 scroll;
        private AffiliationList<Part, ModuleDockAssistCam> cams;
        private AffiliationList<Part, ModuleDockAssistTarget> targets;
        private DictionaryFromList<ModuleDockAssistCam, DAEditorSet> camSet;
        private DictionaryFromList<ModuleDockAssistTarget, DAEditorSet> tgtSet;
        private UIMultiPageList<ModuleDockAssistCam> camUI;
        private UIMultiPageList<ModuleDockAssistTarget> tgtUI;
        #endregion

        #region Constructor
        public PanelDockAssistEditor() : base(new Rect((Screen.width * 0.95f) - 250, 360, 250, 0), GuidProvider.GetGuid<PanelDockAssistEditor>(), "Docking Assistants") { }
        #endregion

        #region Method
        public void OnEditorStart()
        {
            this.cams = new AffiliationList<Part, ModuleDockAssistCam>(null, (AffiliationList<Part, ModuleDockAssistCam>.GetItemFunctionMulti)GetCam);
            this.targets = new AffiliationList<Part, ModuleDockAssistTarget>(null, (AffiliationList<Part, ModuleDockAssistTarget>.GetItemFunctionMulti)GetTgt);
            this.camSet = new DictionaryFromList<ModuleDockAssistCam, DAEditorSet>(this.cams, new DAEditorSet(false));
            this.tgtSet = new DictionaryFromList<ModuleDockAssistTarget, DAEditorSet>(this.targets, new DAEditorSet(false));
            this.camUI = new UIMultiPageList<ModuleDockAssistCam>(this.cams, 2);
            this.tgtUI = new UIMultiPageList<ModuleDockAssistTarget>(this.targets, 2);
        }

        public void ShowInEditor()
        {
            this.Visible = true;
            OnEditorUpdate();
            UpdateAllRenames();
        }

        public void HideInEditor()
        {
            this.Visible = false;
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
                set.renamed = mcam.renamed;
                set.name = mcam.partName;
                this.camSet[mcam] = set;
            }
            foreach (ModuleDockAssistTarget mtgt in this.targets)
            {
                DAEditorSet set = this.tgtSet[mtgt];
                set.renamed = mtgt.renamed;
                set.name = mtgt.partName;
                this.tgtSet[mtgt] = set;
            }
        }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginHorizontal();
            this.showCams = GUIUtils.TwinToggle(this.showCams, "Cameras", "Targets", GUI.skin.button);
            GUILayout.EndHorizontal();

            this.scroll = GUILayout.BeginScrollView(this.scroll, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);

            GUILayout.EndScrollView();
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
            set.renamed = GUILayout.Toggle(set.renamed, "Rename");
            if (set.renamed) { set.name = GUILayout.TextField(set.name); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name") && !string.IsNullOrEmpty(set.name))
            {
                mcam.SetName(set.name);
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
            set.renamed = GUILayout.Toggle(set.renamed, "Rename");
            if (set.renamed) { set.name = GUILayout.TextField(set.name); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name") && !string.IsNullOrEmpty(set.name))
            {
                mtgt.SetName(set.name);
                UpdateAllRenames();
            }
            this.tgtSet[mtgt] = set;
        }
        #endregion
    }
}