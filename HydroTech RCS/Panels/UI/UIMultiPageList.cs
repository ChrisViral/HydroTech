using System.Collections.Generic;
using UnityEngine;

namespace HydroTech_RCS.Panels.UI
{
    public class MultiPageListUi<T> where T : class
    {
        protected int curPage; // Start from 0

        protected List<T> list;
        protected int perPage = 5;

        public delegate void DrawSingleItemUi(T item);

        public MultiPageListUi(List<T> l, int n = 5)
        {
            this.list = l;
            this.perPage = n;
        }

        protected bool LastPage()
        {
            return this.curPage == (this.list.Count - 1) / this.perPage;
        }

        public virtual void OnUpdate()
        {
            if (this.list.Count - 1 < this.perPage * (this.curPage + 1)) { this.curPage = (this.list.Count - 1) / this.perPage; }
        }

        public virtual void OnDrawUi(DrawSingleItemUi drawFunction, out bool pageChanged, out bool zero)
        {
            int count = 0;
            foreach (T item in this.list)
            {
                if (count / this.perPage == this.curPage) { drawFunction(item); }
                count++;
            }
            zero = count == 0;
            if (LastPage() && count % this.perPage != 0) { for (int i = count % this.perPage; i < this.perPage; i++) { drawFunction(null); } }
            pageChanged = false;
            if (count > this.perPage)
            {
                GUILayout.BeginHorizontal();
                if (this.curPage == 0) { GUILayout.Button("Prev", Panel.BtnStyle(Color.red)); }
                else
                {
                    if (GUILayout.Button("Prev"))
                    {
                        this.curPage--;
                        pageChanged = true;
                    }
                }
                if (LastPage()) { GUILayout.Button("Next", Panel.BtnStyle(Color.red)); }
                else
                {
                    if (GUILayout.Button("Next"))
                    {
                        this.curPage++;
                        pageChanged = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}