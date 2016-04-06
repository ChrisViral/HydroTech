using System;
using System.Collections.Generic;
using HydroTech_FC;
using HydroTech_RCS.Panels.UI;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelDockAssistEditorAid
    {
        protected struct DaEditorSet
        {
            public bool drawn;
            public string name;
            public bool renamed;

            public DaEditorSet(bool dr = false, bool rn = false, string n = "")
            {
                this.drawn = dr;
                this.renamed = rn;
                this.name = n;
            }
        }

        protected AffiliationList<Part, ModuleDockAssistCam> cams = null;
        protected DictionaryFromList<ModuleDockAssistCam, DaEditorSet> camset = null;
        protected MultiPageListUi<ModuleDockAssistCam> camUi = null;

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLNodeInfo(i = 1, name = "SETTINGS"), HydroSLField(saveName = "ShowCams")]
        public bool showCams = true;

        protected AffiliationList<Part, ModuleDockAssistTarget> targets = null;
        protected DictionaryFromList<ModuleDockAssistTarget, DaEditorSet> tgtset = null;
        protected MultiPageListUi<ModuleDockAssistTarget> tgtUi = null;

        protected List<ModuleDockAssistCam> GetCam(Part p)
        {
            List<ModuleDockAssistCam> list = new List<ModuleDockAssistCam>();
            foreach (PartModule pm in p.Modules) { if (pm is ModuleDockAssistCam) { list.Add((ModuleDockAssistCam)pm); } }
            return list;
        }

        protected List<ModuleDockAssistTarget> GetTgt(Part p)
        {
            List<ModuleDockAssistTarget> list = new List<ModuleDockAssistTarget>();
            foreach (PartModule pm in p.Modules) { if (pm is ModuleDockAssistTarget) { list.Add((ModuleDockAssistTarget)pm); } }
            return list;
        }

        protected virtual void CamUi(ModuleDockAssistCam mcam)
        {
            if (mcam == null) { return; }
            GUILayout.Label(mcam.part.partInfo.title);
            DaEditorSet set = this.camset[mcam];
            set.drawn = GUILayout.Toggle(set.drawn, "Visual Aid");
            if (set.drawn != this.camset[mcam].drawn)
            {
                if (set.drawn) { mcam.ShowEditorAid(); }
                else
                { mcam.HideEditorAid(); }
            }
            GUILayout.BeginHorizontal();
            set.renamed = GUILayout.Toggle(set.renamed, set.renamed ? "" : "Rename");
            if (set.renamed) { set.name = GUILayout.TextField(set.name); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name"))
            {
                mcam.ModuleRename.EditorRename(set.renamed, set.name);
                UpdateAllRenames();
            }
            this.camset[mcam] = set;
        }

        protected virtual void TgtUi(ModuleDockAssistTarget mtgt)
        {
            if (mtgt == null) { return; }
            GUILayout.Label(mtgt.part.partInfo.title);
            DaEditorSet set = this.tgtset[mtgt];
            set.drawn = GUILayout.Toggle(set.drawn, "Visual Aid");
            if (set.drawn != this.tgtset[mtgt].drawn)
            {
                if (set.drawn) { mtgt.ShowEditorAid(); }
                else
                { mtgt.HideEditorAid(); }
            }
            GUILayout.BeginHorizontal();
            set.renamed = GUILayout.Toggle(set.renamed, set.renamed ? "" : "Rename");
            if (set.renamed) { set.name = GUILayout.TextField(set.name); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name"))
            {
                mtgt.ModuleRename.EditorRename(set.renamed, set.name);
                UpdateAllRenames();
            }
            this.tgtset[mtgt] = set;
        }

        protected void UpdateAllRenames()
        {
            foreach (ModuleDockAssistCam mcam in this.cams)
            {
                DaEditorSet set = this.camset[mcam];
                set.renamed = mcam.ModuleRename.Renamed;
                set.name = mcam.ModuleRename.nameString;
                this.camset[mcam] = set;
            }
            foreach (ModuleDockAssistTarget mtgt in this.targets)
            {
                DaEditorSet set = this.tgtset[mtgt];
                set.renamed = mtgt.ModuleRename.Renamed;
                set.name = mtgt.ModuleRename.nameString;
                this.tgtset[mtgt] = set;
            }
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
            if (GUILayout.Button("Cameras", this.showCams ? BtnStyle(Color.green) : BtnStyle()))
            {
                this.showCams = true;
                this.needSave = true;
                ResetHeight();
            }
            if (GUILayout.Button("Targets", this.showCams ? BtnStyle() : BtnStyle(Color.green)))
            {
                this.showCams = false;
                this.needSave = true;
                ResetHeight();
            }
            GUILayout.EndHorizontal();
            bool pageChanged;
            bool noItem;
            if (this.showCams) { this.camUi.OnDrawUi(CamUi, out pageChanged, out noItem); }
            else
            { this.tgtUi.OnDrawUi(TgtUi, out pageChanged, out noItem); }
            if (pageChanged) { ResetHeight(); }
            if (noItem) { GUILayout.Label("Not installed"); }

            GUI.DragWindow();
        }
    }
}