using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    public interface IPanelEditor
    {
        void ShowInEditor();
        void HideInEditor();
        void OnEditorUpdate();
    }
}