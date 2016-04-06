using System;

namespace HydroTech_FC
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HydroSlNodeInfoAttribute : Attribute
    {
        public int i = 0;
        public string name = "NODE";
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HydroSlFieldAttribute : Attribute
    {
        public LoadSaveFileBasic.Cmd cmd = LoadSaveFileBasic.Cmd.NONE;
        public bool isTesting = false;
        public string saveName = "";
    }

    public interface IHydroCustomClassSingleSl { }

    public interface IHydroCustomClassComplexSl { }

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