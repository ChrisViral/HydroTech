using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class HydroRenderingManager : MonoBehaviour
    {
        #region Instance
        public static HydroRenderingManager Instance { get; private set; }
        #endregion

        #region Static Fields
        private static readonly List<Callback> drawQueue = new List<Callback>();
        private static bool visible;
        #endregion

        #region Methods
        public void AddToDrawQueue(Callback drawFunction)
        {
            if (drawQueue.Contains(drawFunction)) { throw new InvalidOperationException("AddToPostDrawQueue fail: draw function already added."); }
            drawQueue.Add(drawFunction);
#if DEBUG
            Debug.Log("Added a draw function");
#endif
        }

        public void RemoveFromDrawQueue(Callback drawFunction)
        {
            if (!drawQueue.Contains(drawFunction)) { throw new InvalidOperationException("RemoveFromPostDrawQueue fail: draw function not in the queue"); }
            drawQueue.Remove(drawFunction);
#if DEBUG
            Debug.Log("Removed a draw function");
#endif
        }

        private void ShowUI()
        {
            visible = true;
        }

        private void HideUI()
        {
            visible = false;
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else { Destroy(this); }
        }

        private void Start()
        {
            GameEvents.onShowUI.Add(ShowUI);
            GameEvents.onHideUI.Add(HideUI);
            GameEvents.onGUIAstronautComplexSpawn.Add(HideUI);
            GameEvents.onGUIAstronautComplexDespawn.Add(ShowUI);
            GameEvents.onGUIRnDComplexSpawn.Add(HideUI);
            GameEvents.onGUIRnDComplexDespawn.Add(ShowUI);
            GameEvents.onGUIMissionControlSpawn.Add(HideUI);
            GameEvents.onGUIMissionControlDespawn.Add(ShowUI);
            GameEvents.onGUIAdministrationFacilitySpawn.Add(HideUI);
            GameEvents.onGUIAdministrationFacilityDespawn.Add(ShowUI);
        }

        private void OnDestroy()
        {
            GameEvents.onShowUI.Remove(ShowUI);
            GameEvents.onHideUI.Remove(HideUI);
            GameEvents.onGUIAstronautComplexSpawn.Remove(HideUI);
            GameEvents.onGUIAstronautComplexDespawn.Remove(ShowUI);
            GameEvents.onGUIRnDComplexSpawn.Remove(HideUI);
            GameEvents.onGUIRnDComplexDespawn.Remove(ShowUI);
            GameEvents.onGUIMissionControlSpawn.Remove(HideUI);
            GameEvents.onGUIMissionControlDespawn.Remove(ShowUI);
            GameEvents.onGUIAdministrationFacilitySpawn.Remove(HideUI);
            GameEvents.onGUIAdministrationFacilityDespawn.Remove(ShowUI);
        }

        private void OnGUI()
        {
            if (visible)
            {
                for (int i = 0; i < drawQueue.Count; i++)
                {
                    drawQueue[i]();
                }
            }
        }
        #endregion
    }
}