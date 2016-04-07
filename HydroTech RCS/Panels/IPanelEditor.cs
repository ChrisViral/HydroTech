
namespace HydroTech_RCS.Panels
{
    public interface IPanelEditor
    {
        #region Methods
        void ShowInEditor();

        void HideInEditor();

        void OnEditorUpdate();
        #endregion
    }
}