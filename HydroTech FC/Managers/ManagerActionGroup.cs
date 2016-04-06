using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    static public class HydroActionGroupManager
    {
        static public bool GetState(Vessel vessel, KSPActionGroup action)
        {
            return vessel.ActionGroups[action];
        }
        static public void SetState(Vessel vessel, KSPActionGroup action, bool active)
        {
            vessel.ActionGroups.SetGroup(action, active);
        }

        public class ActionGroupState
        {
            public ActionGroupState(Vessel v) { vessel = v; }
            private Vessel vessel = null;

            public bool GetState(KSPActionGroup action)
            {
                return HydroActionGroupManager.GetState(vessel, action);
            }
            public void SetState(KSPActionGroup action, bool active)
            {
                HydroActionGroupManager.SetState(vessel, action, active);
            }

            public bool this[KSPActionGroup action]
            {
                get { return GetState(action); }
                set { SetState(action, value); }
            }

            public bool Abort
            {
                get { return GetState(KSPActionGroup.Abort); }
                set { SetState(KSPActionGroup.Abort, value); }
            }
            public bool Brakes
            {
                get { return GetState(KSPActionGroup.Brakes); }
                set { SetState(KSPActionGroup.Brakes, value); }
            }
            public bool C1
            {
                get { return GetState(KSPActionGroup.Custom01); }
                set { SetState(KSPActionGroup.Custom01, value); }
            }
            public bool C2
            {
                get { return GetState(KSPActionGroup.Custom02); }
                set { SetState(KSPActionGroup.Custom02, value); }
            }
            public bool C3
            {
                get { return GetState(KSPActionGroup.Custom03); }
                set { SetState(KSPActionGroup.Custom03, value); }
            }
            public bool C4
            {
                get { return GetState(KSPActionGroup.Custom04); }
                set { SetState(KSPActionGroup.Custom04, value); }
            }
            public bool C5
            {
                get { return GetState(KSPActionGroup.Custom05); }
                set { SetState(KSPActionGroup.Custom05, value); }
            }
            public bool C6
            {
                get { return GetState(KSPActionGroup.Custom06); }
                set { SetState(KSPActionGroup.Custom06, value); }
            }
            public bool C7
            {
                get { return GetState(KSPActionGroup.Custom07); }
                set { SetState(KSPActionGroup.Custom07, value); }
            }
            public bool C8
            {
                get { return GetState(KSPActionGroup.Custom08); }
                set { SetState(KSPActionGroup.Custom08, value); }
            }
            public bool C9
            {
                get { return GetState(KSPActionGroup.Custom09); }
                set { SetState(KSPActionGroup.Custom09, value); }
            }
            public bool C0
            {
                get { return GetState(KSPActionGroup.Custom10); }
                set { SetState(KSPActionGroup.Custom10, value); }
            }
            public bool Gear
            {
                get { return GetState(KSPActionGroup.Gear); }
                set { SetState(KSPActionGroup.Gear, value); }
            }
            public bool Light
            {
                get { return GetState(KSPActionGroup.Light); }
                set { SetState(KSPActionGroup.Light, value); }
            }
            public bool RCS
            {
                get { return GetState(KSPActionGroup.RCS); }
                set { SetState(KSPActionGroup.RCS, value); }
            }
            public bool SAS
            {
                get { return GetState(KSPActionGroup.SAS); }
                set { SetState(KSPActionGroup.SAS, value); }
            }
            public bool Stage
            {
                get { return GetState(KSPActionGroup.Stage); }
                set { SetState(KSPActionGroup.Stage, value); }
            }
        }

        static public ActionGroupState ActiveVessel { get { return new ActionGroupState(GameStates.ActiveVessel); } }
        static public ActionGroupState Vessel(Vessel v) { return new ActionGroupState(v); }
    }
}