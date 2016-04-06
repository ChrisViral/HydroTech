using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using UI;

    public partial class PanelDockAssistEditorAid
    {
        protected struct DAEditorSet
        {
            public DAEditorSet(bool dr = false, bool rn = false, String n = "")
            {
                drawn = dr;
                renamed = rn;
                name = n;
            }

            public bool drawn;
            public bool renamed;
            public String name;
        }

        [HydroSLNodeInfo(name = "PANELEDITOR")]
        [HydroSLNodeInfo(i = 1, name = "SETTINGS")]
        [HydroSLField(saveName = "ShowCams")]
        public bool showCams = true;

        protected AffiliationList<Part, ModuleDockAssistCam> cams = null;
        protected AffiliationList<Part, ModuleDockAssistTarget> targets = null;
        protected DictionaryFromList<ModuleDockAssistCam, DAEditorSet> camset = null;
        protected DictionaryFromList<ModuleDockAssistTarget, DAEditorSet> tgtset = null;
        protected MultiPageListUI<ModuleDockAssistCam> camUI = null;
        protected MultiPageListUI<ModuleDockAssistTarget> tgtUI = null;

        protected List<ModuleDockAssistCam> GetCam(Part p)
        {
            List<ModuleDockAssistCam> list = new List<ModuleDockAssistCam>();
            foreach (PartModule pm in p.Modules)
                if (pm is ModuleDockAssistCam)
                    list.Add((ModuleDockAssistCam)pm);
            return list;
        }
        protected List<ModuleDockAssistTarget> GetTgt(Part p)
        {
            List<ModuleDockAssistTarget> list = new List<ModuleDockAssistTarget>();
            foreach (PartModule pm in p.Modules)
                if (pm is ModuleDockAssistTarget)
                    list.Add((ModuleDockAssistTarget)pm);
            return list;
        }

        virtual protected void CamUI(ModuleDockAssistCam mcam)
        {
            if (mcam == null)
                return;
            GUILayout.Label(mcam.part.partInfo.title);
            DAEditorSet set = camset[mcam];
            set.drawn = GUILayout.Toggle(set.drawn, "Visual Aid");
            if (set.drawn != camset[mcam].drawn)
            {
                if (set.drawn)
                    mcam.ShowEditorAid();
                else
                    mcam.HideEditorAid();
            }
            GUILayout.BeginHorizontal();
            set.renamed = GUILayout.Toggle(set.renamed, set.renamed ? "" : "Rename");
            if (set.renamed)
                set.name = GUILayout.TextField(set.name);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name"))
            {
                mcam.ModuleRename.EditorRename(set.renamed, set.name);
                UpdateAllRenames();
            }
            camset[mcam] = set;
        }

        virtual protected void TgtUI(ModuleDockAssistTarget mtgt)
        {
            if (mtgt == null)
                return;
            GUILayout.Label(mtgt.part.partInfo.title);
            DAEditorSet set = tgtset[mtgt];
            set.drawn = GUILayout.Toggle(set.drawn, "Visual Aid");
            if (set.drawn != tgtset[mtgt].drawn)
            {
                if (set.drawn)
                    mtgt.ShowEditorAid();
                else
                    mtgt.HideEditorAid();
            }
            GUILayout.BeginHorizontal();
            set.renamed = GUILayout.Toggle(set.renamed, set.renamed ? "" : "Rename");
            if (set.renamed)
                set.name = GUILayout.TextField(set.name);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update Name"))
            {
                mtgt.ModuleRename.EditorRename(set.renamed, set.name);
                UpdateAllRenames();
            }
            tgtset[mtgt] = set;
        }

        protected void UpdateAllRenames()
        {
            foreach (ModuleDockAssistCam mcam in cams)
            {
                DAEditorSet set = camset[mcam];
                set.renamed = mcam.ModuleRename.Renamed;
                set.name = mcam.ModuleRename.nameString;
                camset[mcam] = set;
            }
            foreach (ModuleDockAssistTarget mtgt in targets)
            {
                DAEditorSet set = tgtset[mtgt];
                set.renamed = mtgt.ModuleRename.Renamed;
                set.name = mtgt.ModuleRename.nameString;
                tgtset[mtgt] = set;
            }
        }

        protected override void WindowGUI(int WindowID)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(editorHide ? "Maximize" : "Minimize"))
            {
                editorHide = !editorHide;
                needSave = true;
                windowRect.width = editorHide ? 100 : 250;
                ResetHeight();
            }
            if (editorHide)
            {
                GUILayout.EndHorizontal();
                GUI.DragWindow();
                return;
            }
            if (GUILayout.Button("Cameras", showCams ? BtnStyle(Color.green) : BtnStyle()))
            {
                showCams = true;
                needSave = true;
                ResetHeight();
            }
            if (GUILayout.Button("Targets", showCams ? BtnStyle() : BtnStyle(Color.green)))
            {
                showCams = false;
                needSave = true;
                ResetHeight();
            }
            GUILayout.EndHorizontal();
            bool pageChanged;
            bool noItem;
            if (showCams)
                camUI.OnDrawUI(CamUI, out pageChanged, out noItem);
            else
                tgtUI.OnDrawUI(TgtUI, out pageChanged, out noItem);
            if (pageChanged)
                ResetHeight();
            if (noItem)
                GUILayout.Label("Not installed");

            GUI.DragWindow();
        }
    }
}