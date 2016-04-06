using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HydroTech_RCS;
using HydroTech_RCS.PartModules.Base;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Constants.PartModules.PartRename;

public class ModulePartRename : HydroPartModulewPanel
{
    [KSPField(guiActive = false, guiName = "Name")]
    public String nameString = "";

    protected bool _Renamed = false;
    public bool Renamed
    {
        get { return _Renamed; }
        set
        {
            Fields["nameString"].guiActive = value;
            _Renamed = value;
        }
    }

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);
        if (node.HasValue(ConfigNodes.Name))
        {
            nameString = node.GetValue(ConfigNodes.Name);
            tempName = nameString;
            Renamed = true;
        }
        else
            Renamed = false;
    }

    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);
        if (Renamed)
            node.AddValue(ConfigNodes.Name, nameString);
    }

    [KSPEvent(guiActive = true, guiName = "Rename")]
    protected void RenameEvent()
    {
        if (!PanelShown)
            PanelShown = true;
    }

    protected override int QueueSpot { get { return ManagerConsts.RenderMgr_ModulePartRename; } }
    protected override string PanelTitle { get { return "Rename part"; } }

    static private bool _Registered = false;
    protected override bool Registered
    {
        get { return _Registered; }
        set { _Registered = value; }
    }

    protected String tempName = "";
    protected override void windowGUI(int ID)
    {
        GUILayout.BeginVertical();
        tempName = GUILayout.TextField(tempName);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {
            if (tempName != "")
            {
                nameString = tempName;
                Renamed = true;
                PanelShown = false;
            }
        }
        if (GUILayout.Button("Clear"))
        {
            tempName = "";
            nameString = "";
            Renamed = false;
            PanelShown = false;
        }
        if (GUILayout.Button("Cancel"))
        {
            tempName = nameString;
            PanelShown = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    public void EditorRename(bool renamed, String name)
    {
        _Renamed = renamed;
        nameString = name;
    }
}