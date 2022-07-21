using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DocumentApprovalTask;

namespace DirRX.HRManagement.Client
{
  partial class DocumentApprovalTaskActions
  {
    public override void Restart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Restart(e);
    }

    public override bool CanRestart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = HRManagement.PublicFunctions.Module.GetReasonBeforeAbort();
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

  }

}