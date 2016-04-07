using System.Collections.Generic;

namespace HydroTech_RCS.Panels.UI
{
    public class SingleSelectionListUi<T> : MultiPageListUi<T> where T : class
    {
        protected T curSelect;

        public T CurSelect
        {
            get { return this.curSelect; }
            protected set { this.curSelect = value; }
        }

        public SingleSelectionListUi(List<T> l, int n = 5) : base(l, n) { }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (this.CurSelect != null && !this.list.Contains(this.CurSelect))
            {
                this.CurSelect = null;
                this.curPage = 0;
            }
        }

        public void SetSelectionToItem(T item)
        {
            if (this.list.Contains(item)) { this.CurSelect = item; }
            else
            {
                this.CurSelect = null;
            }
        }

        public void SetToCurSelPage()
        {
            if (this.CurSelect == null) { this.curPage = 0; }
            else
            {
                int count = 0;
                foreach (T item in this.list)
                {
                    if (Equals(item, this.CurSelect))
                    {
                        this.curPage = count / this.perPage;
                        break;
                    }
                    count++;
                }
            }
        }
    }
}