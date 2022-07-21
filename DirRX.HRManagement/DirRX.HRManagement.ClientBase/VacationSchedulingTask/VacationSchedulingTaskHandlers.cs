using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSchedulingTask;

namespace DirRX.HRManagement
{
  partial class VacationSchedulingTaskClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      PublicFunctions.Module.Remote.CheckVacationsModuleLicense();
      _obj.State.Attachments.VacationScheduleGroup.IsVisible = !_obj.State.IsInserted;
      _obj.State.Properties.Year.IsEnabled = Functions.Module.Remote.IsDebugEnabled();
    }
  }
}