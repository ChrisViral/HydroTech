using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Constants.PartModules.PartRename;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;

public class ModulePartRename : HydroPartModulewPanel
{
    private static bool registered;

    [KSPField(guiActive = false, guiName = "Name")]
    public string nameString = "";

    protected bool renamed;

    protected string tempName = "";

    public bool Renamed
    {
        get { return this.renamed; }
        set
        {
            this.Fields["nameString"].guiActive = value;
            this.renamed = value;
        }
    }

    protected override int QueueSpot
    {
        get { return ManagerConsts.renderMgrModulePartRename; }
    }

    protected override string PanelTitle
    {
        get { return "Rename part"; }
    }

    protected override bool Registered
    {
        get { return registered; }
        set { registered = value; }
    }

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);
        if (node.HasValue(ConfigNodes.name))
        {
            this.nameString = node.GetValue(ConfigNodes.name);
            this.tempName = this.nameString;
            this.Renamed = true;
        }
        else
        {
            this.Renamed = false;
        }
    }

    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);
        if (this.Renamed) { node.AddValue(ConfigNodes.name, this.nameString); }
    }

    [KSPEvent(guiActive = true, guiName = "Rename")]
    protected void RenameEvent()
    {
        if (!this.PanelShown) { this.PanelShown = true; }
    }

    protected override void windowGUI(int id)
    {
        GUILayout.BeginVertical();
        this.tempName = GUILayout.TextField(this.tempName);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {
            if (this.tempName != "")
            {
                this.nameString = this.tempName;
                this.Renamed = true;
                this.PanelShown = false;
            }
        }
        if (GUILayout.Button("Clear"))
        {
            this.tempName = "";
            this.nameString = "";
            this.Renamed = false;
            this.PanelShown = false;
        }
        if (GUILayout.Button("Cancel"))
        {
            this.tempName = this.nameString;
            this.PanelShown = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    public void EditorRename(bool renamed, string name)
    {
        this.renamed = renamed;
        this.nameString = name;
    }
}