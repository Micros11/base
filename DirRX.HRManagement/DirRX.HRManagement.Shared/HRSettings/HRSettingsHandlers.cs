using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRSettings;
using ParamKeys = DirRX.HRManagement.Constants.HRSettings.ParamKey;

namespace DirRX.HRManagement
{
  partial class HRSettingsSharedHandlers
  {

    public virtual void BusinessUnitChanged(DirRX.HRManagement.Shared.HRSettingsBusinessUnitChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue)
      {
        if (!e.Params.Contains(ParamKeys.OldBusinessUnit))
        {
          if (e.OldValue != null)
            e.Params.Add(ParamKeys.OldBusinessUnit, e.OldValue.Sid.Value);
          else
            e.Params.Add(ParamKeys.OldBusinessUnit, Guid.Empty);
        }
      }
    }

  }
}