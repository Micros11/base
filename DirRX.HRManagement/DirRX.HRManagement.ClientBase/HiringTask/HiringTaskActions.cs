using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringTask;

namespace DirRX.HRManagement.Client
{
  partial class HiringTaskActions
  {
    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = Functions.Module.GetReasonBeforeAbort();
      if (abortReason != string.Empty)
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

    public override void Start(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (!e.Validate())
        return;
      DirRX.HRManagement.Functions.HiringTask.Remote.CreateEmployee(_obj);
      base.Start(e);
    }

    public override bool CanStart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanStart(e);
    }

  }

}