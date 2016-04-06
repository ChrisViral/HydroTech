using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    static public class GameStates
    {
        static public bool InEditor { get { return HighLogic.LoadedSceneIsEditor; } }
        static public bool InFlight { get { return HighLogic.LoadedSceneIsFlight; } }

        static public Vessel ActiveVessel { get { return FlightGlobals.ActiveVessel; } }
    }
}