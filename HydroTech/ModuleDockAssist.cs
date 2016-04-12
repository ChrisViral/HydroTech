﻿using System;
using System.Linq;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech
{
    public abstract class ModuleDockAssist : PartModule
    {
        #region Static fields
        private static readonly Color[] colours = { Color.blue, Color.green, Color.red };
        #endregion

        #region KSPFields
        [KSPField(isPersistant = true, guiName = "Name", guiActive = true, guiActiveEditor = true)]
        public string partName = string.Empty;

        [KSPField(isPersistant = true)]
        public bool renamed;

        //TODO: use transforms instead of manually typing camera/target positions
        [KSPField(isPersistant = true)]
        public Vector3 assistPos = Vector3.zero;

        [KSPField(isPersistant = true)]
        public Vector3 assistUp = Vector3.up;

        [KSPField(isPersistant = true)]
        public Vector3 assistFwd = Vector3.forward;

        [KSPField(isPersistant = true)]
        public Vector3 previewPos = Vector3.back;

        [KSPField(isPersistant = true)]
        public Vector3 previewUp = Vector3.up;

        [KSPField(isPersistant = true)]
        public Vector3 previewFwd = Vector3.forward;

        [KSPField(isPersistant = true)]
        public float previewFoV = 90;
        #endregion

        #region Fields
        private string tempName;
        private Rect pos, drag;
        private int id;
        private bool visible, hid;
        private LineRenderer[] lines;
        private Vector3[] directions;
        protected Rigidbody rigidbody;
        #endregion

        #region Properties
        public Vector3 Dir
        {
            get { return ReverseTransform(this.assistFwd); }
        }

        public Vector3 Down
        {
            get { return ReverseTransform(-this.assistUp); }
        }

        public Vector3 Right
        {
            get { return -Vector3.Cross(this.Down, this.Dir); }
        }

        public Vector3 Pos
        {
            get { return this.part.GetComponentCached(ref this.rigidbody).worldCenterOfMass + ReverseTransform(this.assistPos); }
        }

        public Vector3 RelPos
        {
            get { return SwitchTransformCalculator.VectorTransform(this.Pos - this.vessel.findWorldCenterOfMass(), this.vessel.ReferenceTransform); }
        }
        #endregion

        #region Abstract properties
        protected abstract string ModuleShort { get; }
        #endregion

        #region KSPEvents
        [KSPEvent(guiName = "Rename", active = true, guiActive = true)]
        public void GUIRename()
        {
            if (!this.visible)
            {
                ModuleDockAssist module = this.vessel.FindPartModulesImplementing<ModuleDockAssist>().FirstOrDefault(m => m.visible);
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

        protected Vector3 ReverseTransform(Vector3 vec)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, this.transform.right, this.transform.up, this.transform.forward);
        }

        private void CreateLineRenderers()
        {
            Type t = typeof(LineRenderer);
            Shader shader = Shader.Find("Particles/Additive");
            this.lines = new GameObject(this.ModuleShort + "Assist", t, t, t).GetComponents<LineRenderer>();

            for (int i = 0; i < 3; i++)
            {
                LineRenderer lr = this.lines[i];
                Color colour = colours[i];
                lr.transform.parent = this.transform;
                lr.useWorldSpace = false;
                lr.transform.localPosition = this.assistPos;
                lr.transform.localEulerAngles = Vector3.zero;
                lr.material = new Material(shader);
                lr.SetColors(colour, colour);
                lr.SetVertexCount(2);
            }
        }

        public void ShowEditorAid()
        {
            for (int i = 0; i < 3; i++)
            {
                LineRenderer lr = this.lines[i];
                lr.SetWidth(0.01f, 0.01f);
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, this.directions[i]);
            }
        }

        public void HideEditorAid()
        {
            for (int i = 0; i < 3; i++)
            {
                LineRenderer lr = this.lines[i];
                lr.SetWidth(0, 0);
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, Vector3.zero);
            }
        }

        public void ShowPreview()
        {
            HydroCameraManager camMngr = HydroFlightManager.Instance.CameraManager;
            camMngr.Target = null;
            camMngr.TransformParent = this.transform;
            camMngr.FoV = this.previewFoV;
            camMngr.Position = this.previewPos;
            camMngr.SetLookRotation(this.previewFwd, this.previewUp);
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
            this.Fields["partName"].guiName = this.ModuleShort + "Name";

            if (HighLogic.LoadedSceneIsEditor)
            {
                CreateLineRenderers();
                this.directions =  new [] { this.assistFwd, this.assistUp, -Vector3.Cross(this.assistFwd, this.assistUp) };
                HideEditorAid();
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                this.id = GuidProvider.GetGuid<ModuleDockAssist>();
                this.pos = new Rect(Screen.width * 0.5f, Screen.height * 0.45f, 250, 100);
                this.drag = new Rect(0, 0, 250, 30);
                GameEvents.onShowUI.Add(ShowUI);
                GameEvents.onHideUI.Add(HideUI);
            }
        }

        public override string ToString()
        {
            return this.renamed ? this.partName : this.RelPos.ToString("#0.00");
        }

        protected virtual void OnDestroy()
        {
            GameEvents.onShowUI.Remove(ShowUI);
            GameEvents.onHideUI.Remove(HideUI);
        }
        #endregion
    }
}