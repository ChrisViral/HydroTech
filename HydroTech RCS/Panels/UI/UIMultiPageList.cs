using System.Collections.Generic;
using UnityEngine;

namespace HydroTech_RCS.Panels.UI
{
    public class UIMultiPageList<T> where T : class
    {
        #region Delegates
        public delegate void DrawSingleItemUI(T item);
        #endregion

        #region Fields
        protected int curPage; //Start from 0
        protected List<T> list;
        protected int perPage;
        #endregion

        #region Properties
        protected int LastPage
        {
            get { return (this.list.Count - 1) / this.perPage; }
        }

        protected bool OnLastPage
        {
            get { return this.curPage == this.LastPage;}
        }
        #endregion

        #region Constructor
        public UIMultiPageList(List<T> l, int n = 5)
        {
            this.list = l;
            this.perPage = n;
        }
        #endregion

        #region Virtual methods
        public virtual void OnUpdate()
        {
            if (this.list.Count - 1 < this.perPage * (this.curPage + 1)) { this.curPage = this.LastPage; }
        }

        public virtual void OnDrawUI(DrawSingleItemUI drawFunction, out bool pageChanged, out bool zero)
        {
            int count = 0;
            foreach (T item in this.list)
            {
                if (count / this.perPage == this.curPage) { drawFunction(item); }
                count++;
            }
            zero = count == 0;
            if (this.OnLastPage && count % this.perPage != 0)
            {
                for (int i = count % this.perPage; i < this.perPage; i++)
                {
                    drawFunction(null);
                }
            }
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
                if (this.OnLastPage) { GUILayout.Button("Next", Panel.BtnStyle(Color.red)); }
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
        #endregion
    }
}