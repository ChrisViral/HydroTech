using HydroTech_FC;
using HydroTech_RCS;
using HydroTech_RCS.Autopilots.ASAS;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Panels;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;

#if DEBUG
public class ModuleHydroAsas : HydroPartModulewPanel
{
    public enum AsasMode
    {
        NONE = 0,
        HOLDDIR = 1,
        KILLROT = 2
    }

    protected static bool registered;

    private bool asas;
    private Vector3 dir;

    [KSPField(guiActive = true, guiName = "ASAS Mode")]
    public AsasMode mode = AsasMode.NONE;

    [KSPField(isPersistant = true)]
    public int Mode;

    private Vector3 right;

    [KSPField(isPersistant = true, guiActive = true, guiName = "ASAS Strength")]
    public float strength = 1.0F;

    private AsasMode tempMode = AsasMode.NONE;
    private string tempStrengthText = "1.00";

    private bool AsasState
    {
        get { return HydroActionGroupManager.ActiveVessel.SAS; }
    }

    private Vessel ActiveVessel
    {
        get { return GameStates.ActiveVessel; }
    }

    protected override int QueueSpot
    {
        get { return ManagerConsts.renderMgrModuleHydroAsas; }
    }

    protected override string PanelTitle
    {
        get { return "ASAS Behaviour"; }
    }

    protected override bool Registered
    {
        get { return registered; }
        set { registered = value; }
    }

    private void SetDir()
    {
        this.dir = this.ActiveVessel.transform.up;
        this.right = this.ActiveVessel.transform.right;
    }

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);
        this.mode = (AsasMode)this.Mode;
        this.tempMode = this.mode;
        this.tempStrengthText = this.strength.ToString("#0.00");
    }

    public override void OnSave(ConfigNode node)
    {
        this.Mode = (int)this.mode;
        base.OnSave(node);
    }

    [KSPEvent(guiActive = true, guiName = "Set ASAS Behaviour")]
    public void SetAsas()
    {
        if (!this.PanelShown) { this.PanelShown = true; }
    }

    protected override void windowGUI(int id)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("HOLDDIR", this.tempMode == AsasMode.HOLDDIR ? BtnStyle(Color.green) : BtnStyle())) { this.tempMode = AsasMode.HOLDDIR; }
        if (GUILayout.Button("KILLROT", this.tempMode == AsasMode.KILLROT ? BtnStyle(Color.green) : BtnStyle())) { this.tempMode = AsasMode.KILLROT; }
        if (GUILayout.Button("NONE", this.tempMode == AsasMode.NONE ? BtnStyle(Color.green) : BtnStyle())) { this.tempMode = AsasMode.NONE; }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Strength: ");
        this.tempStrengthText = GUILayout.TextField(this.tempStrengthText);
        GUILayout.EndHorizontal();
        float tryParse;
        bool parseOk = float.TryParse(this.tempStrengthText, out tryParse);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK", parseOk ? BtnStyle() : BtnStyle(Color.red)) && parseOk)
        {
            this.mode = this.tempMode;
            this.strength = tryParse;
            this.PanelShown = false;
        }
        if (GUILayout.Button("Cancel"))
        {
            this.tempMode = this.mode;
            this.tempStrengthText = this.strength.ToString("#0.00");
            this.PanelShown = false;
        }
        GUILayout.EndHorizontal();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!HydroJebCore.IsActiveJeb((HydroJeb)this.part)) { return; }
        if (this.AsasState != this.asas)
        {
            if (!this.asas)
            {
                SetDir();
                this.asas = true;
                this.ActiveVessel.OnFlyByWire += Drive;
            }
            else
            {
                this.asas = false;
                this.ActiveVessel.OnFlyByWire -= Drive;
            }
        }
    }

    private void Drive(FlightCtrlState ctrlState)
    {
        PanelDebug.ThePanel.RemoveWatch("HoldDirCal");
        PanelDebug.ThePanel.RemoveWatch("_aV");
        if (!HydroJebCore.IsActiveJeb((HydroJeb)this.part) || !this.AsasState) { return; }
        bool userInput = ctrlState.yaw != 0 || ctrlState.roll != 0 || ctrlState.pitch != 0;
        if (userInput)
        {
            SetDir();
            return;
        }
        if (this.mode == AsasMode.HOLDDIR)
        {
            HoldDirStateCalculator cal = new HoldDirStateCalculator();
            cal.Calculate(this.dir, this.right, this.ActiveVessel);
            cal.RotationMultiplier(this.strength);
            PanelDebug.ThePanel.AddWatch("HoldDirCal", cal);
            cal.SetCtrlStateRotation(ctrlState);
        }
        else if (this.mode == AsasMode.KILLROT)
        {
            KillRotationCalculator cal = new KillRotationCalculator();
            cal.Calculate(this.ActiveVessel);
            cal.SetCtrlStateRotation(ctrlState);
        }
    }
}
#endif