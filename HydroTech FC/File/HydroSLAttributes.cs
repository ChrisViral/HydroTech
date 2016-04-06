using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HydroSLNodeInfoAttribute : Attribute
    {
        public int i = 0;
        public string name = "NODE";
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class HydroSLFieldAttribute : Attribute
    {
        public string saveName = "";
        public LoadSaveFileBasic.CMD cmd = LoadSaveFileBasic.CMD.NONE;
        public bool isTesting = false;
    }

    public interface IHydroCustomClassSingleSL { }
    public interface IHydroCustomClassComplexSL { }
    /*
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class HydroCustomClassSLMethodAttribute : Attribute
    {
        public bool newNode = true;
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class HydroCustomClassSLContectAttribute : Attribute
    {
        public string fieldName = "";
        public string saveName = "";
        public LoadSaveFileBasic.CMD cmd = LoadSaveFileBasic.CMD.NONE;
        public bool isTesting = false;
    }
    */
}