using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.BusinessUnit;

namespace DirRX.HRSolution
{
  partial class BusinessUnitClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      var isEnabledESSSettings = _obj.UseESSDirRX.HasValue && _obj.UseESSDirRX.Value;
      _obj.State.Properties.RegulationDocument.IsVisible = isEnabledESSSettings;
      _obj.State.Properties.RegulationDocument.IsEnabled = isEnabledESSSettings;
      _obj.State.Properties.RegulationDocument.IsRequired = isEnabledESSSettings;
      _obj.State.Properties.ConsentTemplate.IsVisible = isEnabledESSSettings;
      _obj.State.Properties.ConsentTemplate.IsEnabled = isEnabledESSSettings;
      _obj.State.Properties.ConsentTemplate.IsRequired = isEnabledESSSettings;
    }

  }
}