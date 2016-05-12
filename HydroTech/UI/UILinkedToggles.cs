using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.UI
{
    /// <summary>
    /// A set of linked exclusive UI toggles (radio buttons)
    /// </summary>
    /// <typeparam name="T">The object type returned by the active toggle</typeparam>
    public class UILinkedToggles<T> where T : class
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
                        if (value) { this.toggles.onShow?.Invoke(this.Value); }
                        else { this.toggles.onHide?.Invoke(); }
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

            #region Constructors
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
        private readonly Func<T, string> getName;           //Function to obtain string toggle name from T
        private readonly GUIStyle style;                    //Normal/Selected GUIStyles
        private readonly Callback<T> onShow;                //On Show callback
        private readonly Callback onHide;                   //OnHide callback
        private List<Toggle> toggles = new List<Toggle>();  //List of all current toggles
        private Dictionary<T, Toggle> dict = new Dictionary<T, Toggle>(); //Dictionary of values/indexes, to ensure uniqueness
        #endregion

        #region Properties
        /// <summary>
        /// Current active Toggled
        /// </summary>
        public Toggle Toggled { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new blank UILinkedToggles
        /// </summary>
        /// <param name="getName">Function to obtain a string name for the toggle from a <typeparamref name="T"/></param>
        /// <param name="style">GUIStyle of the toggles</param>
        /// <param name="onShow">Callback when selecting a toggle</param>
        /// <param name="onHide">Callback when hiding a toggle</param>
        public UILinkedToggles(Func<T, string> getName, GUIStyle style, Callback<T> onShow = null, Callback onHide = null)
        {
            this.getName = getName;
            this.style = style;
            this.onShow = onShow;
            this.onHide = onHide;
        }

        /// <summary>
        /// Creates a new UILinkedToggles from an existing collection
        /// </summary>
        /// <param name="collection">Collection to create the toggles from</param>
        /// <param name="getName">Function to obtain a string name for the toggle from a <typeparamref name="T"/></param>
        /// <param name="style">GUIStyle of the toggles</param>
        /// <param name="onShow">Callback when selecting a toggle</param>
        /// <param name="onHide">Callback when hiding a toggle</param>
        public UILinkedToggles(IEnumerable<T> collection, Func<T, string> getName, GUIStyle style, Callback<T> onShow = null, Callback onHide = null) : this(getName, style, onShow, onHide)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection), "Cannot initialize with null collection"); }
            if (getName == null)    { throw new ArgumentNullException(nameof(getName), "Name getter function cannot be null"); }
            
            foreach (T t in collection)
            {
                if (this.dict.ContainsKey(t)) { continue; }  //Skip if already added

                Toggle toggle = new Toggle(getName(t), t, this);
                this.toggles.Add(toggle);
                this.dict.Add(t, toggle);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the UILinkedToggles
        /// </summary>
        /// <returns>The current selected value</returns>
        public T Draw()
        {
            for (int i = 0; i < this.toggles.Count; i++)
            {
                Toggle t = this.toggles[i];
                t.Active = GUILayout.Toggle(t.Active, t.Name, this.style);
                if (t.Active)
                {
                    if (this.Toggled != t)
                    {
                        if (this.Toggled != null) { this.Toggled.Active = false; }
                        this.Toggled = t;
                    }
                }
                else if (this.Toggled == t) { this.Toggled = null; }
            }
            return this.Toggled?.Value;
        }

        /// <summary>
        /// Update the backup list of toggles with a newer reference list. Old toggles are deleted, new toggles are added, toggles already present are kept the same.
        /// </summary>
        /// <param name="reference">Reference list to update with.</param>
        public void Update(List<T> reference)
        {
            List<Toggle> toggles = new List<Toggle>(reference.Count);
            Dictionary<T, Toggle> dict = new Dictionary<T, Toggle>(reference.Count);
            foreach (T t in reference)
            {
                Toggle toggle;
                if (!this.dict.TryGetValue(t, out toggle)) { toggle = new Toggle(this.getName(t), t, this); }
                toggles.Add(toggle);
                dict.Add(t, toggle);
            }

            this.toggles = toggles;
            this.dict = dict;
        }
        #endregion
    }
}
