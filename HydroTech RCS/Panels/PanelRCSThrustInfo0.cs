using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Core;
    using Constants.Panels;
    using Autopilots.Modules;

    public partial class PanelRCSThrustInfo : Panel, IPanelEditor
    {
        public static PanelRCSThrustInfo thePanel { get { return (PanelRCSThrustInfo)HydroJebCore.panels[PanelIDs.RCSInfo]; } }

        public PanelRCSThrustInfo()
        {
            fileName = new FileName("rcsinfo", "cfg", HydroJebCore.PanelSaveFolder);
        }

        protected override int PanelID { get { return PanelIDs.RCSInfo; } }
        public override string PanelTitle
        {
            get
            {
                return (editor && editorHide) ?
                    PanelTitles.RCSInfo_EditorHide
                    : PanelTitles.RCSInfo;
            }
        }

        [HydroSLNodeInfo(name = "PANELEDITOR")]
        [HydroSLField(saveName = "WindowPos", cmd = CMD.Rect_TopLeft)]
        public Rect windowRectEditor = new Rect();

        protected override void SetDefaultWindowRect()
        {
            windowRect = WindowPositions.RCSInfo;
            windowRectEditor = WindowPositions.RCSInfo_Editor;
        }

        protected bool editor = false;

        [HydroSLNodeInfo(name = "PANELEDITOR")]
        [HydroSLField(saveName = "Minimized")]
        public bool editorHide = false;

        [HydroSLNodeInfo(name = "PANELEDITOR")]
        [HydroSLNodeInfo(i = 1, name = "SETTINGS")]
        [HydroSLField(saveName = "ShowRotation")]
        public bool showRotation = true;

        protected bool _PanelShown_Editor = false;
        public override bool PanelShown
        {
            get
            {
                if (editor)
                    return _PanelShown_Editor;
                else
                    return base.PanelShown;
            }
            set
            {
                if (editor)
                {
                    if (!Active)
                        return;
                    if (value != _PanelShown_Editor)
                    {
                        if (value)
                            AddPanel();
                        else
                            RemovePanel();
                    }
                    _PanelShown_Editor = value;
                }
                else
                    base.PanelShown = value;
            }
        }

        public void ShowInEditor()
        {
            Active = true;
            editor = true;
            Load();
            AddPanel();
        }
        public void HideInEditor()
        {
            PanelShown = false;
            Active = false;
        }
        public void OnEditorUpdate()
        {
            if (needSave)
                Save();
        }

        protected static CalculatorRCSThrust theCalculator { get { return HydroJebCore.activeVesselRCS; } }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (theCalculator.AllRCSEnabledChanged)
                ResetHeight();
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            _PanelShown_Editor = true;
            editorHide = false;
            showRotation = true;
        }
    }
}