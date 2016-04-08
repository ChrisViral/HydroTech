using System;
using CMD = HydroTech.File.LoadSaveFileBasic.CMD;

namespace HydroTech.File
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HydroSLNodeInfo : Attribute
    {
        #region Fields
        public int i = 0;
        public string name = "NODE";
        #endregion
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HydroSLField : Attribute
    {
        #region Fields
        public CMD cmd = CMD.NONE;
        public bool isTesting = false;
        public string saveName = string.Empty;
        #endregion
    }

    public interface IHydroCustomClassSingleSL { }

    public interface IHydroCustomClassComplexSL { }
}