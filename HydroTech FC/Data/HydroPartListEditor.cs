using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    public class HydroPartListEditor : List<Part>
    {
        public void OnStart(out bool clear)
        {
            clear = false;
            foreach(Part p in this)
                if (p == null)
                {
                    clear = true;
                    break;
                }
            if (clear)
                Clear();
        }

        public void OnUpdate()
        {
            List<Part> partsToRemove = new List<Part>();
            foreach (Part p in this)
                if (p == null)
                    partsToRemove.Add(p);
            foreach (Part p in partsToRemove)
                Remove(p);
        }
    }
}