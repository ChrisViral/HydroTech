using System.Collections.Generic;

namespace HydroTech.Panels.UI
{
    public class UISingleSelectionList<T> : UIMultiPageList<T> where T : class
    {
        #region Properties
        protected T curSelect;
        public T CurSelect
        {
            get { return this.curSelect; }
            protected set { this.curSelect = value; }
        }
        #endregion

        #region Constructor
        public UISingleSelectionList(List<T> l, int n = 5) : base(l, n) { }
        #endregion

        #region Methods
        public void SetSelectionToItem(T item)
        {
            this.CurSelect = this.list.Contains(item) ? item : null;
        }

        public void SetToCurSelPage()
        {
            if (this.CurSelect == null) { this.curPage = 0; }
            else
            {
                int count = 0;
                foreach (T item in this.list)
                {
                    if (EqualityComparer<T>.Default.Equals(item, this.CurSelect))
                    {
                        this.curPage = count / this.perPage;
                        break;
                    }
                    count++;
                }
            }
        }
        #endregion

        #region Overrides
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (this.CurSelect != null && !this.list.Contains(this.CurSelect))
            {
                this.CurSelect = null;
                this.curPage = 0;
            }
        }
        #endregion
    }
}