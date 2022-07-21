using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationApprovalTask;

namespace DirRX.HRManagement.Client
{
  partial class VacationApprovalTaskActions
  {
    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.Module.VacationScheduleChart(_obj.Year.Value, _obj.Employee);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

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