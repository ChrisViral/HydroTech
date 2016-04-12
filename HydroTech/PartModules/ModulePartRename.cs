using System.Linq;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.PartModules
{
    public class ModulePartRename : PartModule
    {
        #region KSPFields
        [KSPField(isPersistant = true, guiName = "Name", guiActive = true, guiActiveEditor = true)]
        public string partName = string.Empty;

        [KSPField(isPersistant = true)]
        public bool renamed;
        #endregion

        #region Fields
        private string tempName;
        private Rect pos, drag;
        private int id;
        private bool visible, hid;
        #endregion

        #region KSPEvents
        [KSPEvent(guiName = "Rename", active = true, guiActive = true)]
        public void GUIRename()
        {
            if (!this.visible)
            {
                ModulePartRename module = this.vessel.FindPartModulesImplementing<ModulePartRename>().FirstOrDefault(m => m.visible);
                if (module != null) { module.visible = false; }
                this.visible = true;
            }
        }
        #endregion

        #region Methods
        public void SetName(string name)
        {
            this.partName = name;
            this.renamed = true;
        }

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical();
            this.tempName = GUILayout.TextField(this.tempName);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ok") && !string.IsNullOrEmpty(this.tempName))
            {
                this.partName = this.tempName;
                this.renamed = true;
                this.visible = false;
            }
            if (GUILayout.Button("Clear"))
            {
                this.tempName = string.Empty;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.visible = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void ShowUI()
        {
            this.hid = false;
        }

        private void HideUI()
        {
            this.hid = true;
        }
        #endregion

        #region Functions
        private void Update()
        {
            if (!FlightGlobals.ready && this.visible && !this.vessel.isActiveVessel)
            {
                this.visible = false;
            }        
        }

        private void OnDestroy()
        {
            GameEvents.onShowUI.Remove(ShowUI);
            GameEvents.onHideUI.Remove(HideUI);
        }

        private void OnGUI()
        {
            if (this.visible && !this.hid)
            {
                GUI.skin = GUIUtils.Skin;

                this.pos = KSPUtil.ClampRectToScreen(GUILayout.Window(this.id, this.pos, Window, "Rename Part"));
            }
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }

            this.id = GuidProvider.GetGuid<ModulePartRename>();
            this.pos = new Rect(Screen.width * 0.5f, Screen.height * 0.45f, 250, 100);
            this.drag = new Rect(0, 0, 250, 30);
            GameEvents.onShowUI.Add(ShowUI);
            GameEvents.onHideUI.Add(HideUI);
        }
        #endregion
    }
}