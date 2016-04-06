using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    public static class GameStates
    {
        public static bool InEditor { get { return HighLogic.LoadedSceneIsEditor; } }
        public static bool InFlight { get { return HighLogic.LoadedSceneIsFlight; } }

        public static Vessel ActiveVessel { get { return FlightGlobals.ActiveVessel; } }
    }
}