using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public static class GameBehaviours
    {
#if DEBUG
        public static void print(object message) { MonoBehaviour.print(message); }
        public static void warning(object message) { PDebug.Warning(message); }
        public static void error(object message) { PDebug.Error(message); }
#endif

        public static void ExceptionHandler(Exception e, String funcName)
        {
            throw new Exception("An exception has been thrown in " + funcName
                + "\nException type:\n" + e.GetType().ToString()
                + "\nThe message is:\n" + e.Message
                + "\n\nStack:\n" + e.StackTrace + "\n\n");
        }
    }
}