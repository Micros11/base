using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSingleSchedulingTask;

namespace DirRX.HRManagement
{
  partial class VacationSingleSchedulingTaskClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      PublicFunctions.Module.Remote.CheckVacationsModuleLicense();
    }

  }
}