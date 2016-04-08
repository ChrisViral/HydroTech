using System.Collections.Generic;
using System.Linq;

namespace HydroTech.Data
{
    public class SubList<T> : List<T>
    {
        #region Delegate
        public delegate bool Requirement(T item);
        #endregion

        #region Fields
        private readonly List<T> parentList;
        private readonly Requirement requirement;
        #endregion

        #region Constructor
        public SubList(List<T> parent, Requirement req)
        {
            this.parentList = parent;
            this.requirement = req;
        }
        #endregion

        public virtual void OnUpdate()
        {
            //I know this means a lot of wasted memory, but I gotta see if its really necessary first
            Clear();
            AddRange(this.parentList.Where(i => this.requirement(i)));
        }
    }
}