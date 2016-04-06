using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using System.Reflection;

    public abstract partial class LoadSaveFileBasic
    {
        protected virtual void LoadDefault() { needSave = true; }
        protected virtual void OnLoad(ConfigNode node)
        {
            if (ResetRequest(node))
                return;
            foreach (FieldInfo field in GetType().GetFields())
            {
                HydroSLFieldAttribute att = (HydroSLFieldAttribute)Attribute.GetCustomAttribute(field, typeof(HydroSLFieldAttribute));
                if (att == null || att.isTesting)
                    continue;
                HydroSLNodeInfoAttribute[] nodes = (HydroSLNodeInfoAttribute[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfoAttribute));
                if (nodes.Count() == 0)
                    ReadField(field, node, att.saveName, att.cmd);
                else
                {
                    Dictionary<int, string> nodesDict = new Dictionary<int, string>();
                    foreach (HydroSLNodeInfoAttribute nodeInfo in nodes)
                        nodesDict.Add(nodeInfo.i, nodeInfo.name);
                    ConfigNode tempNode = node;
                    for (int i = 0; i < nodesDict.Count; i++)
                    {
                        if (tempNode.HasNode(nodesDict[i]))
                            tempNode = tempNode.GetNode(nodesDict[i]);
                        else
                            tempNode = tempNode.AddNode(nodesDict[i]);
                    }
                    ReadField(field, tempNode, att.saveName, att.cmd);
                }
            }
        }

        protected bool needSave = false;
        protected virtual void OnSave(ConfigNode node)
        {
            node.AddValue("Reset", false);
            foreach (FieldInfo field in GetType().GetFields())
            {
                HydroSLFieldAttribute att = (HydroSLFieldAttribute)Attribute.GetCustomAttribute(field, typeof(HydroSLFieldAttribute));
                if (att == null || att.isTesting)
                    continue;
                HydroSLNodeInfoAttribute[] nodes = (HydroSLNodeInfoAttribute[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfoAttribute));
                if (nodes.Count() == 0)
                    Write(node, att.saveName, field.GetValue(this), att.cmd);
                else
                {
                    Dictionary<int, string> nodesDict = new Dictionary<int, string>();
                    foreach (HydroSLNodeInfoAttribute nodeInfo in nodes)
                        nodesDict.Add(nodeInfo.i, nodeInfo.name);
                    ConfigNode tempNode = node;
                    for (int i = 0; i < nodesDict.Count; i++)
                    {
                        if (tempNode.HasNode(nodesDict[i]))
                            tempNode = tempNode.GetNode(nodesDict[i]);
                        else
                            tempNode = tempNode.AddNode(nodesDict[i]);
                    }
                    Write(tempNode, att.saveName, field.GetValue(this), att.cmd);
                }
            }
        }

        protected FileName fileName = null;
        public void Load()
        {
            LoadDefault();
            ConfigNode node = ConfigNode.Load(fileName.ToString());
            if (node != null) // file exists
                OnLoad(node);
        }
        public void Save()
        {
            ConfigNode node = new ConfigNode();
            OnSave(node);
            node.Save(fileName.ToString());
            needSave = false;
        }

        protected static bool ResetRequest(ConfigNode node)
        {
            return node.HasValue("Reset")
                && bool.Parse(node.GetValue("Reset"));
        }

        protected static bool ReadField(object obj, FieldInfo field, ConfigNode node, string name, CMD cmd)
        {
            object val = field.GetValue(obj);
            if (Read(node, name, ref val, cmd))
            {
                field.SetValue(obj, val);
                return true;
            }
            return false;
        }
        protected bool ReadField(FieldInfo field, ConfigNode node, string name, CMD cmd)
        {
            return ReadField(this, field, node, name, cmd);
        }

#if DEBUG
        protected static void print(object message) { GameBehaviours.print(message); }
        protected static void warning(object message) { GameBehaviours.warning(message); }
        protected static void error(object message) { GameBehaviours.error(message); }
#endif
    }
}