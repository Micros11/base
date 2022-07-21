using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferStatementApprovalAssignment;

namespace DirRX.HRManagement
{
  partial class TransferStatementApprovalAssignmentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var errors = string.Empty;
      if (!e.Params.TryGetValue("Errors", out errors) || string.IsNullOrEmpty(errors))
        return;
      e.AddError(errors);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.TransferStatementApprovalAssignment.SetEnabledProperties(_obj);
    }

  }
}