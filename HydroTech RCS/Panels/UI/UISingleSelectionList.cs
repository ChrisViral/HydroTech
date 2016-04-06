using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels.UI
{
    using UnityEngine;

    public class SingleSelectionListUI<T> : MultiPageListUI<T>
        where T : class
    {
        public SingleSelectionListUI(List<T> l, int n = 5) : base(l, n) { }

        protected T _CurSelect = null;
        public T curSelect
        {
            get { return _CurSelect; }
            protected set { _CurSelect = value; }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (curSelect != null && !list.Contains(curSelect))
            {
                curSelect = null;
                curPage = 0;
            }
        }

        public void SetSelectionToItem(T item)
        {
            if (list.Contains(item))
                curSelect = item;
            else
                curSelect = null;
        }

        public void SetToCurSelPage()
        {
            if (curSelect == null)
                curPage = 0;
            else
            {
                int count = 0;
                foreach (T item in list)
                {
                    if (Equals(item, curSelect))
                    {
                        curPage = count / perPage;
                        break;
                    }
                    count++;
                }
            }
        }
    }
}