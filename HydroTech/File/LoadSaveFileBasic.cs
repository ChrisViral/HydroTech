using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HydroTech.File
{
    //TODO: Get rid of this class, this level of complexity isn't necessary at all
    public abstract class LoadSaveFileBasic
    {
        public enum CMD
        {
            NONE,
            RECT_TOP_LEFT
        }

        #region Fields
        protected bool needSave;
        protected FileName fileName = null;
        #endregion

        #region Fields
        public void Load()
        {
            LoadDefault();
            ConfigNode node = ConfigNode.Load(this.fileName.ToString());
            if (node != null) //File exists
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

        protected bool ReadField(FieldInfo field, ConfigNode node, string name, CMD cmd)
        {
            return ReadField(this, field, node, name, cmd);
        }
        #endregion

        #region Static Methods
        protected static Rect ToRect(Vector2 vec2, Rect r)
        {
            return new Rect(vec2.x, vec2.y, r.width, r.height);
        }

        protected static Rect ToRect(Vector4 vec4)
        {
            return new Rect(vec4.x, vec4.y, vec4.z, vec4.w);
        }

        protected static Vector2 RectToVector2(Rect r)
        {
            return new Vector2(r.xMin, r.yMin);
        }

        protected static Vector4 RectToVector4(Rect r)
        {
            return new Vector4(r.xMin, r.yMin, r.width, r.height);
        }

        protected static bool ResetRequest(ConfigNode node)
        {
            return node.HasValue("Reset") && bool.Parse(node.GetValue("Reset"));
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

        public static bool Read(ConfigNode node, string name, ref object result, CMD cmd = CMD.NONE)
        {
            if (node.HasValue(name))
            {
                string value = node.GetValue(name);
                if (result is bool) { result = bool.Parse(value); }
                else if (result is int) { result = int.Parse(value); }
                else if (result is float) { result = float.Parse(value); }
                else if (result is Vector2) { result = ConfigNode.ParseVector2(value); }
                else if (result is Vector3) { result = ConfigNode.ParseVector3(value); }
                else if (result is Vector4) { result = ConfigNode.ParseVector4(value); }
                else if (result is Rect)
                {
                    if (cmd == CMD.RECT_TOP_LEFT)
                    {
                        object topleft = RectToVector2((Rect)result);
                        Read(node, name, ref topleft);
                        result = ToRect((Vector2)topleft, (Rect)result);
                    }
                    else
                    {
                        object vec4 = RectToVector4((Rect)result);
                        Read(node, name, ref vec4);
                        result = ToRect((Vector4)vec4);
                    }
                }
                else if (result.GetType().IsEnum)
                {
                    object i = (int)result;
                    Read(node, name, ref i);
                    result = i;
                }
                else if (result is IHydroCustomClassSingleSL)
                {
                    foreach (FieldInfo field in result.GetType().GetFields())
                    {
                        HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                        if (att == null || att.isTesting) { continue; }
                        object val = field.GetValue(result);
                        if (Read(node, name, ref val, cmd))
                        {
                            field.SetValue(result, val);
                            return true;
                        }
                        return false;
                    }
                }
                else
                {
                    result = value;
                }
                return true;
            }
            if (node.HasNode(name))
            {
                if (result is IHydroCustomClassComplexSL)
                {
                    ConfigNode fieldNode = node.GetNode(name);
                    foreach (FieldInfo field in result.GetType().GetFields())
                    {
                        HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                        if (att == null || att.isTesting) { continue; }
                        HydroSLNodeInfo[] nodes = (HydroSLNodeInfo[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfo));
                        if (nodes.Length == 0) { ReadField(result, field, fieldNode, att.saveName, att.cmd); }
                        else
                        {
                            Dictionary<int, string> nodesDict = nodes.ToDictionary(nodeInfo => nodeInfo.i, nodeInfo => nodeInfo.name);
                            ConfigNode tempNode = fieldNode;
                            for (int i = 0; i < nodesDict.Count; i++)
                            {
                                tempNode = tempNode.HasNode(nodesDict[i]) ? tempNode.GetNode(nodesDict[i]) : tempNode.AddNode(nodesDict[i]);
                            }
                            ReadField(result, field, tempNode, att.saveName, att.cmd);
                        }
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        public static void Write(ConfigNode node, string name, object value, CMD cmd = CMD.NONE)
        {
            if (value is Vector2) { node.AddValue(name, ConfigNode.WriteVector((Vector2)value)); }
            else if (value is Vector3) { node.AddValue(name, ConfigNode.WriteVector((Vector3)value)); }
            else if (value is Vector4) { node.AddValue(name, ConfigNode.WriteVector((Vector4)value)); }
            else if (value is Rect)
            {
                if (cmd == CMD.RECT_TOP_LEFT) { Write(node, name, RectToVector2((Rect)value)); }
                else { Write(node, name, RectToVector4((Rect)value)); }
            }
            else if (value.GetType().IsEnum) { Write(node, name, (int)value); }
            else if (value is IHydroCustomClassSingleSL)
            {
                foreach (FieldInfo field in value.GetType().GetFields())
                {
                    HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                    if (att == null || att.isTesting) { continue; }
                    Write(node, name, field.GetValue(value), att.cmd);
                    break;
                }
            }
            else if (value is IHydroCustomClassComplexSL)
            {
                ConfigNode fieldNode = node.GetNode(name);
                foreach (FieldInfo field in value.GetType().GetFields())
                {
                    HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                    if (att == null || att.isTesting) { continue; }
                    HydroSLNodeInfo[] nodes = (HydroSLNodeInfo[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfo));
                    if (nodes.Length == 0) { Write(fieldNode, att.saveName, field.GetValue(value), att.cmd); }
                    else
                    {
                        Dictionary<int, string> nodesDict = nodes.ToDictionary(nodeInfo => nodeInfo.i, nodeInfo => nodeInfo.name);
                        ConfigNode tempNode = fieldNode;
                        for (int i = 0; i < nodesDict.Count; i++)
                        {
                            tempNode = tempNode.HasNode(nodesDict[i]) ? tempNode.GetNode(nodesDict[i]) : tempNode.AddNode(nodesDict[i]);
                        }
                        Write(tempNode, att.saveName, field.GetValue(value), att.cmd);
                    }
                }
            }
            else { node.AddValue(name, value); }
        }
        #endregion

        #region Virtual Methods
        protected virtual void LoadDefault()
        {
            this.needSave = true;
        }

        protected virtual void OnLoad(ConfigNode node)
        {
            if (ResetRequest(node)) { return; }
            foreach (FieldInfo field in GetType().GetFields())
            {
                HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                if (att == null || att.isTesting) { continue; }
                HydroSLNodeInfo[] nodes = (HydroSLNodeInfo[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfo));
                if (nodes.Length == 0) { ReadField(field, node, att.saveName, att.cmd); }
                else
                {
                    Dictionary<int, string> nodesDict = nodes.ToDictionary(nodeInfo => nodeInfo.i, nodeInfo => nodeInfo.name);
                    ConfigNode tempNode = node;
                    for (int i = 0; i < nodesDict.Count; i++)
                    {
                        tempNode = tempNode.HasNode(nodesDict[i]) ? tempNode.GetNode(nodesDict[i]) : tempNode.AddNode(nodesDict[i]);
                    }
                    ReadField(field, tempNode, att.saveName, att.cmd);
                }
            }
        }

        protected virtual void OnSave(ConfigNode node)
        {
            node.AddValue("Reset", false);
            foreach (FieldInfo field in GetType().GetFields())
            {
                HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                if (att == null || att.isTesting) { continue; }
                HydroSLNodeInfo[] nodes = (HydroSLNodeInfo[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfo));
                if (nodes.Length == 0) { Write(node, att.saveName, field.GetValue(this), att.cmd); }
                else
                {
                    Dictionary<int, string> nodesDict = nodes.ToDictionary(nodeInfo => nodeInfo.i, nodeInfo => nodeInfo.name);
                    ConfigNode tempNode = node;
                    for (int i = 0; i < nodesDict.Count; i++)
                    {
                        tempNode = tempNode.HasNode(nodesDict[i]) ? tempNode.GetNode(nodesDict[i]) : tempNode.AddNode(nodesDict[i]);
                    }
                    Write(tempNode, att.saveName, field.GetValue(this), att.cmd);
                }
            }
        }
        #endregion
    }
}