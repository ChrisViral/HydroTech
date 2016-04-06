using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels.UI
{
    using UnityEngine;

    public class MultiPageListUI<T>
        where T : class
    {
        public delegate void DrawSingleItemUI(T item);

        public MultiPageListUI(List<T> l, int n = 5) { list = l; perPage = n; }

        protected List<T> list = null;
        protected int perPage = 5;
        protected int curPage = 0; // Start from 0

        protected bool LastPage() { return curPage == (list.Count - 1) / perPage; }

        public virtual void OnUpdate()
        {
            if (list.Count - 1 < perPage * (curPage + 1))
                curPage = (list.Count - 1) / perPage;
        }

        public virtual void OnDrawUI(DrawSingleItemUI drawFunction, out bool pageChanged, out bool zero)
        {
            int count = 0;
            foreach (T item in list)
            {
                if (count / perPage == curPage)
                    drawFunction(item);
                count++;
            }
            zero = (count == 0);
            if (LastPage() && count % perPage != 0)
                for (int i = count % perPage; i < perPage; i++)
                    drawFunction(null);
            pageChanged = false;
            if (count > perPage)
            {
                GUILayout.BeginHorizontal();
                if (curPage == 0)
                    GUILayout.Button("Prev", Panel.BtnStyle(Color.red));
                else
                {
                    if (GUILayout.Button("Prev"))
                    {
                        curPage--;
                        pageChanged = true;
                    }
                }
                if (LastPage())
                    GUILayout.Button("Next", Panel.BtnStyle(Color.red));
                else
                {
                    if (GUILayout.Button("Next"))
                    {
                        curPage++;
                        pageChanged = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
            return;
        }
    }
}