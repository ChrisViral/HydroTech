using System.Collections.Generic;
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
        private readonly Dictionary<Part, Highlighter> parts = new Dictionary<Part, Highlighter>();
        #endregion

        #region Properties
        public RCSCalculator ActiveRCS { get; private set; }
        #endregion

        #region Methods
        public void SetActive(bool state)
        {
            this.active = state;
        }

        public void SetHighlight(Part part, bool set)
        {
            Highlighter highlighter;
            if (set)
            {
                if (!this.parts.TryGetValue(part, out highlighter))
                {
                    GameObject go = part.transform.GetChild(0).gameObject;
                    highlighter = go.GetComponent<Highlighter>() ?? go.AddComponent<Highlighter>();
                    this.parts.Add(part, highlighter);
                    highlighter.ConstantOn(XKCDColors.LightSeaGreen);
                    part.SetHighlightColor(XKCDColors.LightSeaGreen);
                    part.SetHighlight(true, false);
                }
            }
            else
            {
                if (this.parts.TryGetValue(part, out highlighter))
                {
                    highlighter.Off();
                    part.SetHighlightDefault();
                    this.parts.Remove(part);
                }
            }
        }

        public void Restart()
        {
            this.parts.Clear();
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.ActiveRCS = new RCSCalculator();
            GameEvents.onEditorRestart.Add(Restart);
        }

        private void FixedUpdate()
        {
            if (!this.active) { return; }

            EditorMainPanel.Instance.DockAssist.FixedUpdate();
            this.ActiveRCS.OnEditorUpdate();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GameEvents.onEditorRestart.Remove(Restart);
            }
        }
        #endregion
    }
}
