using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.Department;

namespace DirRX.HRSolution
{
  partial class DepartmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      if (DirRX.EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        _obj.State.Properties.SignKindDirRX.IsVisible = false;
    }

  }
}