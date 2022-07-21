using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalStatementApprovalAssignment;

namespace DirRX.HRManagement
{
  partial class DismissalStatementApprovalAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.DismissalDate.IsEnabled = _obj.BlockUid == Constants.DismissalTask.ApprovalManagerAssignmentBlockId;
    }

  }
}