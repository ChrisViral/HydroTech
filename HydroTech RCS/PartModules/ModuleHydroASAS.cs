using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HydroTech_FC;
using HydroTech_RCS;
using HydroTech_RCS.PartModules.Base;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Autopilots.ASAS;
#if DEBUG
public class ModuleHydroASAS : HydroPartModulewPanel
{
    public enum ASASMode { NONE = 0, HOLDDIR = 1, KILLROT = 2 }

    [KSPField(guiActive = true, guiName = "ASAS Mode")]
    public ASASMode mode = ASASMode.NONE;
    [KSPField(isPersistant = true)]
    public int Mode = 0;
    [KSPField(isPersistant = true, guiActive = true, guiName = "ASAS Strength")]
    public float strength = 1.0F;

    private bool ASAS = false;
    private bool ASASState { get { return HydroActionGroupManager.ActiveVessel.SAS; } }
    private Vector3 dir = new Vector3();
    private Vector3 right = new Vector3();
    private void SetDir() { dir = ActiveVessel.transform.up; right = ActiveVessel.transform.right; }
    private Vessel ActiveVessel { get { return GameStates.ActiveVessel; } }

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);
        mode = (ASASMode)Mode;
        tempMode = mode;
        tempStrength_Text = strength.ToString("#0.00");
    }

    public override void OnSave(ConfigNode node)
    {
        Mode = (int)mode;
        base.OnSave(node);
    }

    [KSPEvent(guiActive = true, guiName = "Set ASAS Behaviour")]
    public void SetASAS()
    {
        if (!PanelShown)
            PanelShown = true;
    }

    protected override int QueueSpot { get { return ManagerConsts.RenderMgr_ModuleHydroASAS; } }
    protected override string PanelTitle { get { return "ASAS Behaviour"; } }

    protected static bool _Registered = false;
    protected override bool Registered
    {
        get { return _Registered; }
        set { _Registered = value; }
    }

    private ASASMode tempMode = ASASMode.NONE;
    private String tempStrength_Text = "1.00";
    protected override void windowGUI(int ID)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("HOLDDIR", tempMode == ASASMode.HOLDDIR ? BtnStyle(Color.green) : BtnStyle()))
            tempMode = ASASMode.HOLDDIR;
        if (GUILayout.Button("KILLROT", tempMode == ASASMode.KILLROT ? BtnStyle(Color.green) : BtnStyle()))
            tempMode = ASASMode.KILLROT;
        if (GUILayout.Button("NONE", tempMode == ASASMode.NONE ? BtnStyle(Color.green) : BtnStyle()))
            tempMode = ASASMode.NONE;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Strength: ");
        tempStrength_Text = GUILayout.TextField(tempStrength_Text);
        GUILayout.EndHorizontal();
        float tryParse;
        bool parseOK = float.TryParse(tempStrength_Text, out tryParse);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK", parseOK ? BtnStyle() : BtnStyle(Color.red)) && parseOK)
        {
            mode = tempMode;
            strength = tryParse;
            PanelShown = false;
        }
        if (GUILayout.Button("Cancel"))
        {
            tempMode = mode;
            tempStrength_Text = strength.ToString("#0.00");
            PanelShown = false;
        }
        GUILayout.EndHorizontal();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!HydroJebCore.isActiveJeb((HydroJeb)part))
            return;
        if (ASASState != ASAS)
        {
            if (!ASAS)
            {
                SetDir();
                ASAS = true;
                ActiveVessel.OnFlyByWire += Drive;
            }
            else
            {
                ASAS = false;
                ActiveVessel.OnFlyByWire -= Drive;
            }
        }
    }

    private void Drive(FlightCtrlState ctrlState)
    {
        HydroTech_RCS.Panels.PanelDebug.thePanel.RemoveWatch("HoldDirCal");
        HydroTech_RCS.Panels.PanelDebug.thePanel.RemoveWatch("_aV");
        if (!HydroJebCore.isActiveJeb((HydroJeb)part) || !ASASState)
            return;
        bool userInput = ctrlState.yaw != 0 || ctrlState.roll != 0 || ctrlState.pitch != 0;
        if (userInput)
        {
            SetDir();
            return;
        }
        if (mode == ASASMode.HOLDDIR)
        {
            HoldDirStateCalculator cal = new HoldDirStateCalculator();
            cal.Calculate(dir, right, ActiveVessel);
            cal.RotationMultiplier(strength);
            HydroTech_RCS.Panels.PanelDebug.thePanel.AddWatch("HoldDirCal", cal);
            cal.SetCtrlStateRotation(ctrlState);
        }
        else if (mode == ASASMode.KILLROT)
        {
            KillRotationCalculator cal = new KillRotationCalculator();
            cal.Calculate(ActiveVessel);
            cal.SetCtrlStateRotation(ctrlState);
        }
        else
            return;
    }
}
#endif