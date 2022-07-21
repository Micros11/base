using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSchedulingTask;

namespace DirRX.HRManagement.Client
{
  partial class VacationSchedulingTaskActions
  {
    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = Functions.Module.GetReasonBeforeAbort();
      if (!string.IsNullOrEmpty(abortReason))
      {
        Functions.VacationSchedulingTask.Remote.AbortSubTasks(_obj, abortReason);
        _obj.AbortingReason = abortReason;
        _obj.Save();
        base.Abort(e);
      }
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanAbort(e);
    }

    public virtual void FindDepartmentSchedulingTasks(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.VacationSchedulingTask.Remote.FindDepartmentSchedulingTasks(_obj).ShowModal();
    }

    public virtual bool CanFindDepartmentSchedulingTasks(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}