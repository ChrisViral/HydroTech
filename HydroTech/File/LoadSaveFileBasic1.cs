using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HydroTech.File
{
    public partial class LoadSaveFileBasic
    {
        public enum CMD
        {
            NONE,
            RECT_TOP_LEFT
        }

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
                else if (result is IHydroCustomClassSingleSl)
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
                if (result is IHydroCustomClassComplexSl)
                {
                    ConfigNode fieldNode = node.GetNode(name);
                    foreach (FieldInfo field in result.GetType().GetFields())
                    {
                        HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                        if (att == null || att.isTesting) { continue; }
                        HydroSLNodeInfo[] nodes = (HydroSLNodeInfo[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfo));
                        if (nodes.Count() == 0) { ReadField(result, field, fieldNode, att.saveName, att.cmd); }
                        else
                        {
                            Dictionary<int, string> nodesDict = new Dictionary<int, string>();
                            foreach (HydroSLNodeInfo nodeInfo in nodes) { nodesDict.Add(nodeInfo.i, nodeInfo.name); }
                            ConfigNode tempNode = fieldNode;
                            for (int i = 0; i < nodesDict.Count; i++)
                            {
                                if (tempNode.HasNode(nodesDict[i])) { tempNode = tempNode.GetNode(nodesDict[i]); }
                                else
                                {
                                    tempNode = tempNode.AddNode(nodesDict[i]);
                                }
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
                else
                {
                    Write(node, name, RectToVector4((Rect)value));
                }
            }
            else if (value.GetType().IsEnum) { Write(node, name, (int)value); }
            else if (value is IHydroCustomClassSingleSl)
            {
                foreach (FieldInfo field in value.GetType().GetFields())
                {
                    HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                    if (att == null || att.isTesting) { continue; }
                    Write(node, name, field.GetValue(value), att.cmd);
                    break;
                }
            }
            else if (value is IHydroCustomClassComplexSl)
            {
                ConfigNode fieldNode = node.GetNode(name);
                if (value is IHydroCustomClassComplexSl)
                {
                    foreach (FieldInfo field in value.GetType().GetFields())
                    {
                        HydroSLField att = (HydroSLField)Attribute.GetCustomAttribute(field, typeof(HydroSLField));
                        if (att == null || att.isTesting) { continue; }
                        HydroSLNodeInfo[] nodes = (HydroSLNodeInfo[])Attribute.GetCustomAttributes(field, typeof(HydroSLNodeInfo));
                        if (nodes.Count() == 0) { Write(fieldNode, att.saveName, field.GetValue(value), att.cmd); }
                        else
                        {
                            Dictionary<int, string> nodesDict = new Dictionary<int, string>();
                            foreach (HydroSLNodeInfo nodeInfo in nodes) { nodesDict.Add(nodeInfo.i, nodeInfo.name); }
                            ConfigNode tempNode = fieldNode;
                            for (int i = 0; i < nodesDict.Count; i++)
                            {
                                if (tempNode.HasNode(nodesDict[i])) { tempNode = tempNode.GetNode(nodesDict[i]); }
                                else
                                {
                                    tempNode = tempNode.AddNode(nodesDict[i]);
                                }
                            }
                            Write(tempNode, att.saveName, field.GetValue(value), att.cmd);
                        }
                    }
                }
            }
            else
            {
                node.AddValue(name, value);
            }
        }
    }
}