using System;

namespace HydroTech_FC
{
    public static class GameBehaviours
    {
#if DEBUG
        public static void print(object message) { MonoBehaviour.print(message); }
        public static void warning(object message) { PDebug.Warning(message); }
        public static void error(object message) { PDebug.Error(message); }
#endif

        public static void ExceptionHandler(Exception e, string funcName)
        {
            throw new Exception("An exception has been thrown in " + funcName + "\nException type:\n" + e.GetType() + "\nThe message is:\n" + e.Message + "\n\nStack:\n" + e.StackTrace + "\n\n");
        }
    }
}