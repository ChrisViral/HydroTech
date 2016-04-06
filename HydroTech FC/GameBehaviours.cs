using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    static public class GameBehaviours
    {
#if DEBUG
        static public void print(object message) { MonoBehaviour.print(message); }
        static public void warning(object message) { PDebug.Warning(message); }
        static public void error(object message) { PDebug.Error(message); }
#endif

        static public void ExceptionHandler(Exception e, String funcName)
        {
            throw new Exception("An exception has been thrown in " + funcName
                + "\nException type:\n" + e.GetType().ToString()
                + "\nThe message is:\n" + e.Message
                + "\n\nStack:\n" + e.StackTrace + "\n\n");
        }
    }
}