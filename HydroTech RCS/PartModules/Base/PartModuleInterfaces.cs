using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.PartModules.Base
{
    public interface IPartPreview
    {
        void DoPreview();
    }

    public interface IDAPartEditorAid
    {
        void ShowEditorAid();
        void HideEditorAid();
    }
}