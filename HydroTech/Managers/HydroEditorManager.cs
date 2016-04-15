using HighlightingSystem;
using HydroTech.Autopilots.Calculators;
using HydroTech.Panels;
using UnityEngine;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class HydroEditorManager : MonoBehaviour
    {
        #region Instance
        public static HydroEditorManager Instance { get; private set; }
        #endregion

        #region Fields
        private bool active;
        #endregion

        #region Properties
        public RCSCalculator ActiveRCS { get; private set; }
        #endregion

        #region Methods
        public void SetActive(bool state)
        {
            this.active = state;
        }
        #endregion

        #region Static methods
        private static bool MustHighlight(Part part)
        {
            foreach (PartModule pm in part.Modules)
            {
                if (pm is ModuleDockAssist && ((ModuleDockAssist)pm).highlight) { return true; }
            }
            return false;
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.ActiveRCS = new RCSCalculator();
        }

        private void Update()
        {
            foreach (Part p in EditorLogic.SortedShipList)
            {
                Highlighter h = p.highlighter;
                if (MustHighlight(p))
                {
                    if (!h.highlighted)
                    {
                        p.highlighter.SeeThroughOn();
                        p.highlighter.ConstantOn(XKCDColors.ElectricLime);
                        print("highlighting");
                    }
                }
                else if (h.highlighted)
                {
                    p.highlighter.Off();
                    print("removing highlight");
                }
            }
        }

        private void FixedUpdate()
        {
            if (!this.active) { return; }

            EditorMainPanel.Instance.DockAssist.FixedUpdate();
            this.ActiveRCS.OnEditorUpdate();
        }

        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; }
        }
        #endregion
    }
}
