using HydroTech.Constants;
using HydroTech.PartModules.Base;
using UnityEngine;

namespace HydroTech.PartModules
{
    public class ModulePartRename : HydroPartModulePanel
    {
        #region KSPFields
        [KSPField(guiActive = false, guiName = "Name")]
        public string nameString = string.Empty;
        #endregion

        #region Fields
        protected string tempName = string.Empty;
        #endregion

        #region Properties
        protected bool renamed;
        public bool Renamed
        {
            get { return this.renamed; }
            set
            {
                this.Fields["nameString"].guiActive = value;
                this.renamed = value;
            }
        }

        private static bool registered;
        protected override bool Registered
        {
            get { return registered; }
            set { registered = value; }
        }

        protected override string PanelTitle
        {
            get { return "Rename part"; }
        }
        #endregion

        #region KSPEvents
        [KSPEvent(guiActive = true, guiName = "Rename")]
        protected void RenameEvent()
        {
            if (!this.PanelShown) { this.PanelShown = true; }
        }
        #endregion

        #region Methods
        public void EditorRename(bool renamed, string name)
        {
            this.renamed = renamed;
            this.nameString = name;
        }
        #endregion

        #region Overrides
        public override void OnLoad(ConfigNode node)
        {
            if (node.HasValue("PartNewName"))
            {
                this.nameString = node.GetValue("PartNewName");
                this.tempName = this.nameString;
                this.Renamed = true;
            }
            else { this.Renamed = false; }
        }

        public override void OnSave(ConfigNode node)
        {
            if (this.Renamed) { node.AddValue("PartNewName", this.nameString); }
        }

        protected override void WindowGUI(int id)
        {
            GUILayout.BeginVertical();
            this.tempName = GUILayout.TextField(this.tempName);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
            {
                if (this.tempName != string.Empty)
                {
                    this.nameString = this.tempName;
                    this.Renamed = true;
                    this.PanelShown = false;
                }
            }
            if (GUILayout.Button("Clear"))
            {
                this.tempName = string.Empty;
                this.nameString = string.Empty;
                this.Renamed = false;
                this.PanelShown = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.tempName = this.nameString;
                this.PanelShown = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        #endregion
    }
}