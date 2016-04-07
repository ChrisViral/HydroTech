
namespace HydroTech_RCS.Constants
{
    public static class GeneralConsts
    {
        #region PartModules
        public const string name = "PartNewName";
        #endregion

        #region Units
        private const string dot = "·";
        private const string index = "^";
        private const string divide = "/";
        public const string length = "m";
        public const string time = "s";
        public const string angle = "rad";
        public const string force = "N";
        public const string speed = length + dot + time + index + "-1";
        public const string speedSimple = length + divide + time;
        public const string acceleration = length + dot + time + index + "-2";
        public const string angularV = angle + dot + time + index + "-1";
        public const string angularVSimple = angle + divide + time;
        public const string angularAcc = angle + dot + time + index + "-2";
        public const string torque = force + dot + length;
        #endregion
    }
}
