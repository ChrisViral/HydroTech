using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.UI
{
    public class UILinkedToggles<T>
    {
        public class Toggle
        {
            #region Fields
            private readonly UILinkedToggles<T> toggles; 
            #endregion

            #region Properties
            private bool active;
            public bool Active
            {
                get { return this.active; }
                set
                {
                    if (value != this.active)
                    {
                        if (value) { this.toggles.onShow(this.Value); }
                        else { this.toggles.onHide(this.Value); }

                        this.active = value;
                    }
                }
            }

            public string Name { get; }

            public T Value { get; }
            #endregion

            #region Constructor
            public Toggle(T value, string name, UILinkedToggles<T> toggles)
            {
                this.Value = value;
                this.Name = name;
                this.toggles = toggles;
            }
            #endregion
        }

        #region Fields
        private readonly Callback<T> onShow, onHide;
        private readonly List<Toggle> toggles = new List<Toggle>();
        private readonly GUIStyle normal, active;
        #endregion

        #region Properties
        public Toggle Toggled { get; private set; }
        #endregion

        #region Constructor
        public UILinkedToggles(GUIStyle normalStyle, GUIStyle activeStyle, Callback<T> onShow, Callback<T> onHide)
        {
            this.normal = normalStyle;
            this.active = activeStyle;
            this.onShow = onShow;
            this.onHide = onHide;
        }

        public UILinkedToggles(IEnumerable<T> collection, GUIStyle normalStyle, GUIStyle activeStyle, Func<T, string> getName, Callback<T> onShow, Callback<T> onHide) : this(normalStyle, activeStyle, onShow, onHide)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection), "Cannot initialize with null collection"); }
            using (IEnumerator<T> e = collection.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    this.toggles.Add(new Toggle(e.Current, getName(e.Current), this));
                }
            }
        }
        #endregion

        #region Methods
        public T DrawUI()
        {
            T value = default(T);
            for (int i = 0; i < this.toggles.Count; i++)
            {
                Toggle t = this.toggles[i];
                t.Active = GUILayout.Toggle(t.Active, t.Name, t.Active ? this.active : this.normal);
                if (t.Active)
                {
                    value = t.Value;
                    if (this.Toggled != t)
                    {
                        if (this.Toggled != null) { this.Toggled.Active = false; }
                        this.Toggled = t;
                    }
                }
                else if (this.Toggled == t) { this.Toggled = null; }
            }
            return value;
        }

        public void AddToggle(T value, string name)
        {
            this.toggles.Add(new Toggle(value, name, this));
        }

        public void RemoveToggle(T value)
        {
            this.toggles.RemoveAll(t => EqualityComparer<T>.Default.Equals(t.Value, value));
        }
        #endregion
    }
}
