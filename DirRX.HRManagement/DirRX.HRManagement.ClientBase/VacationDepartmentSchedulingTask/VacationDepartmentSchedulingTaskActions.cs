using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationDepartmentSchedulingTask;

namespace DirRX.HRManagement.Client
{
  partial class VacationDepartmentSchedulingTaskActions
  {
    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = Functions.Module.GetReasonBeforeAbort();
      if (!string.IsNullOrEmpty(abortReason))
      {
        _obj.AbortingReason = abortReason;
        _obj.Save();
        base.Abort(e);
      }
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanAbort(e);
    }
  }
}
