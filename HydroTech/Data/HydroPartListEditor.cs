using System.Collections.Generic;
using System.Linq;

namespace HydroTech.Data
{
    public class HydroPartListEditor : List<Part>
    {
        #region Methods
        public void Initialize(out bool clear)
        {
            clear = this.Any(p => p == null);
            if (clear) { Clear(); }
        }

        public void Update()
        {
            RemoveAll(p => p == null);
        }
        #endregion
    }
}