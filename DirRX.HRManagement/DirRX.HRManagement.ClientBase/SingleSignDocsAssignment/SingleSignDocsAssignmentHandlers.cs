using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SingleSignDocsAssignment;

namespace DirRX.HRManagement
{
  partial class SingleSignDocsAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.OrderAttachmentGroup.IsVisible = _obj.BlockUid != Constants.VacationSingleSchedulingTask.SignInitiatorStatementBlockId;
      _obj.State.Attachments.EmployeeAttachmentGroup.IsVisible = _obj.BlockUid != Constants.VacationSingleSchedulingTask.SignInitiatorStatementBlockId &&
        _obj.BlockUid != Constants.VacationSingleSchedulingTask.SignInitiatorOrderBlockId;
    }

  }
}