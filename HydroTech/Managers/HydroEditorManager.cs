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

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.ActiveRCS = new RCSCalculator();
        }

        private void Start()
        {
            EditorMainPanel.Instance.DockAssist.OnEditorStart();
        }

        private void Update()
        {
            if (!this.active) { return; }

            this.ActiveRCS.OnEditorUpdate();
        }

        private void OnDestroy()
        {
            Instance = null;
        }
        #endregion
    }
}
