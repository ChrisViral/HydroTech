using HydroTech.Autopilots.Calculators;
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
        private RCSCalculator rcsCalculator;
        private bool active;
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
            this.rcsCalculator = new RCSCalculator();
        }

        private void Update()
        {
            if (!this.active) { return; }

            this.rcsCalculator.OnEditorUpdate();
        }

        private void OnDestroy()
        {
            Instance = null;
        }
        #endregion
    }
}
