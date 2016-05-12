using UnityEngine;
using static HydroTech.Utils.GUIUtils;

namespace HydroTech.Panels
{
    /// <summary>
    /// Base class for all HydroTech info panels
    /// </summary>
    public abstract class Panel
    {
        #region Fields
        protected Rect window, drag;    //Window rects
        private readonly int id;        //Window ID
        #endregion

        #region Properties
        /// <summary>
        /// If the panel is currently rendered
        /// </summary>
        public virtual bool Visible { get; set; }
        #endregion

        #region Abstract properties
        /// <summary>
        /// Panel title
        /// </summary>
        public abstract string Title { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Base Panel constructors, initiates the window components
        /// </summary>
        /// <param name="window">Panel window Rect</param>
        /// <param name="id">Panel window ID</param>
        protected Panel(Rect window, int id)
        {
            this.window = window;
            this.drag = new Rect(0, 0, this.window.width, 30);
            this.id = id;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets height of the Panel
        /// </summary>
        public void ResetHeight() => this.window.height = 0;

        /// <summary>
        /// Renders the Panel
        /// </summary>
        internal void Draw() => this.window = ClampedWindow(this.id, this.window, Window, this.Title);
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Panel window function signature
        /// </summary>
        /// <param name="id">Window ID</param>
        protected abstract void Window(int id);
        #endregion

        #region Virtual methods
        /// <summary>
        /// Information to run on flight startup
        /// </summary>
        public virtual void OnFlightStart() { }

        /// <summary>
        /// Information to run on update cycle
        /// </summary>
        public virtual void OnUpdate() { }
        #endregion
    }
}