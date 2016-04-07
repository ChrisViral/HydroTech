using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HydroTech_FC
{
    public abstract partial class LoadSaveFileBasic
    {
        protected virtual void LoadDefault()
        {
            this.needSave = true;
        }

        protected virtual void OnLoad(ConfigNode node)
        {
            if (ResetRequest(node)) { return; }
            foreach (FieldInfo field in GetType().GetFields())
            {
                HydroSlFieldAttribute att = (HydroSlFieldAttribute)Attribute.GetCustomAttribute(field, typeof(HydroSlFieldAttribute));
                if (att == null || att.isTesting) { continue; }
                HydroSlNodeInfoAttribute[] nodes = (HydroSlNodeInfoAttribute[])Attribute.GetCustomAttributes(field, typeof(HydroSlNodeInfoAttribute));
                if (nodes.Count() == 0) { ReadField(field, node, att.saveName, att.cmd); }
                else
                {
                    Dictionary<int, string> nodesDict = new Dictionary<int, string>();
                    foreach (HydroSlNodeInfoAttribute nodeInfo in nodes) { nodesDict.Add(nodeInfo.i, nodeInfo.name); }
                    ConfigNode tempNode = node;
                    for (int i = 0; i < nodesDict.Count; i++)
                    {
                        if (tempNode.HasNode(nodesDict[i])) { tempNode = tempNode.GetNode(nodesDict[i]); }
                        else
                        {
                            tempNode = tempNode.AddNode(nodesDict[i]);
                        }
                    }
                    ReadField(field, tempNode, att.saveName, att.cmd);
                }
            }
        }

        protected bool needSave;

        protected virtual void OnSave(ConfigNode node)
        {
            node.AddValue("Reset", false);
            foreach (FieldInfo field in GetType().GetFields())
            {
                HydroSlFieldAttribute att = (HydroSlFieldAttribute)Attribute.GetCustomAttribute(field, typeof(HydroSlFieldAttribute));
                if (att == null || att.isTesting) { continue; }
                HydroSlNodeInfoAttribute[] nodes = (HydroSlNodeInfoAttribute[])Attribute.GetCustomAttributes(field, typeof(HydroSlNodeInfoAttribute));
                if (nodes.Count() == 0) { Write(node, att.saveName, field.GetValue(this), att.cmd); }
                else
                {
                    Dictionary<int, string> nodesDict = new Dictionary<int, string>();
                    foreach (HydroSlNodeInfoAttribute nodeInfo in nodes) { nodesDict.Add(nodeInfo.i, nodeInfo.name); }
                    ConfigNode tempNode = node;
                    for (int i = 0; i < nodesDict.Count; i++)
                    {
                        if (tempNode.HasNode(nodesDict[i])) { tempNode = tempNode.GetNode(nodesDict[i]); }
                        else
                        {
                            tempNode = tempNode.AddNode(nodesDict[i]);
                        }
                    }
                    Write(tempNode, att.saveName, field.GetValue(this), att.cmd);
                }
            }
        }

        protected FileName fileName = null;

        public void Load()
        {
            LoadDefault();
            ConfigNode node = ConfigNode.Load(this.fileName.ToString());
            if (node != null) // file exists
            {
                OnLoad(node);
            }
        }

        public void Save()
        {
            ConfigNode node = new ConfigNode();
            OnSave(node);
            node.Save(this.fileName.ToString());
            this.needSave = false;
        }

        protected static bool ResetRequest(ConfigNode node)
        {
            return node.HasValue("Reset") && bool.Parse(node.GetValue("Reset"));
        }

        protected static bool ReadField(object obj, FieldInfo field, ConfigNode node, string name, Cmd cmd)
        {
            object val = field.GetValue(obj);
            if (Read(node, name, ref val, cmd))
            {
                field.SetValue(obj, val);
                return true;
            }
            return false;
        }

        protected bool ReadField(FieldInfo field, ConfigNode node, string name, Cmd cmd)
        {
            return ReadField(this, field, node, name, cmd);
        }
    }
}