using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.UI
{
    /// <summary>
    /// A set of linked exclusive UI toggles (radio buttons)
    /// </summary>
    /// <typeparam name="T">The object type returned by the active toggle</typeparam>
    public class UILinkedToggles<T>
    {
        /// <summary>
        /// Toggle construct
        /// </summary>
        public class Toggle
        {
            #region Fields
            //Instance of the LinkedToggles to which this toggle belongs            
            private readonly UILinkedToggles<T> toggles; 
            #endregion

            #region Properties
            private bool active;
            /// <summary>
            /// If this is the active Toggle
            /// </summary>
            public bool Active
            {
                get { return this.active; }
                set
                {
                    if (value != this.active)
                    {
                        //On select/deselect of the toggle
                        if (value) { this.toggles.onShow(this.Value); }
                        else { this.toggles.onHide(this.Value); }
                        this.active = value;
                    }
                }
            }

            /// <summary>
            /// String name of the Toggle
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Value associated with this Toggle
            /// </summary>
            public T Value { get; }
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new Toggle instance
            /// </summary>
            /// <param name="value">Value of the Toggle</param>
            /// <param name="name">Name of the Toggle</param>
            /// <param name="toggles">Instance UI list to which this toggle belongs</param>
            public Toggle(string name, T value, UILinkedToggles<T> toggles)
            {
                this.Name = name;
                this.Value = value;
                this.toggles = toggles;
            }
            #endregion
        }

        #region Fields
        private readonly Callback<T> onShow, onHide;                    //On Show/Hide callbacks
        private readonly List<Toggle> toggles = new List<Toggle>();     //List of all current toggles
        private readonly GUIStyle normal, active;                       //Normal/Selected GUIStyles
        #endregion

        #region Properties
        /// <summary>
        /// Current active Toggled
        /// </summary>
        public Toggle Toggled { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new blank UILinkedToggles
        /// </summary>
        /// <param name="normalStyle">Normal GUIStyle</param>
        /// <param name="activeStyle">SelectedGUIStyle</param>
        /// <param name="onShow">Callback when selecting a toggle</param>
        /// <param name="onHide">Callback when hiding a toggle</param>
        public UILinkedToggles(GUIStyle normalStyle, GUIStyle activeStyle, Callback<T> onShow, Callback<T> onHide)
        {
            this.normal = normalStyle;
            this.active = activeStyle;
            this.onShow = onShow;
            this.onHide = onHide;
        }

        /// <summary>
        /// Creates a new UILinkedToggles from an existing collection
        /// </summary>
        /// <param name="collection">Collection to create the toggles from</param>
        /// <param name="getName">Function to obtain a string name for the toggle from a <typeparamref name="T"/></param>
        /// <param name="normalStyle">Normal GUIStyle</param>
        /// <param name="activeStyle">SelectedGUIStyle</param>
        /// <param name="onShow">Callback when selecting a toggle</param>
        /// <param name="onHide">Callback when hiding a toggle</param>
        public UILinkedToggles(IEnumerable<T> collection, Func<T, string> getName, GUIStyle normalStyle, GUIStyle activeStyle, Callback<T> onShow, Callback<T> onHide) : this(normalStyle, activeStyle, onShow, onHide)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection), "Cannot initialize with null collection"); }
            using (IEnumerator<T> e = collection.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    this.toggles.Add(new Toggle(getName(e.Current), e.Current, this));
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the UILinkedToggles
        /// </summary>
        /// <returns>The current selected value</returns>
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

        /// <summary>
        /// Adds a new Toggle to the list
        /// </summary>
        /// <param name="name">Name of the Toggle</param>
        /// <param name="value">Value of the Toggle</param>
        public void AddToggle(string name, T value)
        {
            this.toggles.Add(new Toggle(name, value, this));
        }

        /// <summary>
        /// Removes the first Toggle to have the given value
        /// </summary>
        /// <param name="value">Value of the Toggle to find</param>
        public void RemoveToggle(T value)
        {
            EqualityComparer<T> comp = EqualityComparer<T>.Default;
            for (int i = 0; i < this.toggles.Count; i++)
            {
                if (comp.Equals(this.toggles[i].Value, value))
                {
                    this.toggles.RemoveAt(i);
                    return;
                }
            }
        }
        #endregion
    }
}
