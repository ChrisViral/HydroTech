using HydroTech.Autopilots.Calculators;
using HydroTech.Panels;
using UnityEngine;

namespace HydroTech.Managers
{
    /// <summary>
    /// HydroTech editor general manager
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class HydroEditorManager : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// Current instance
        /// </summary>
        public static HydroEditorManager Instance { get; private set; }
        #endregion

        #region Fields
        private bool active;    //Active flag
        #endregion

        #region Properties
        /// <summary>
        /// Current RCS calculator
        /// </summary>
        public RCSCalculator ActiveRCS { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the state of the manager
        /// </summary>
        /// <param name="state">State to set</param>
        public void SetActive(bool state) => this.active = state;
        #endregion

        #region Static methods
        /// <summary>
        /// If a given part must be highlighted
        /// </summary>
        /// <param name="part">Part to check</param>
        /// <returns>If the part must be highlighted</returns>
        private static bool MustHighlight(Part part)
        {
            foreach (PartModule pm in part.Modules)
            {
                if ((pm as ModuleDockAssist)?.highlight ?? false) { return true; }
            }
            return false;
        }
        #endregion

        #region Functions
        /// <summary>
        /// Awake function
        /// </summary>
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.ActiveRCS = new RCSCalculator();
        }

        /// <summary>
        /// OnDestroy function
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; }
        }

        /// <summary>
        /// Update function
        /// </summary>
        private void Update()
        {
            foreach (Part p in EditorLogic.SortedShipList)
            {
                if (MustHighlight(p))
                {
                    if (!p.highlighter.highlighted)
                    {
                        p.highlighter.SeeThroughOn();
                        p.highlighter.ConstantOn(XKCDColors.ElectricLime);
                    }
                }
                else if (p.highlighter.highlighted) { p.highlighter.Off(); }
            }
        }

        /// <summary>
        /// FixedUpdate function
        /// </summary>
        private void FixedUpdate()
        {
            if (!this.active) { return; }

            EditorMainPanel.Instance.DockAssist.FixedUpdate();
            this.ActiveRCS.OnEditorUpdate();
        }
        #endregion
    }
}
