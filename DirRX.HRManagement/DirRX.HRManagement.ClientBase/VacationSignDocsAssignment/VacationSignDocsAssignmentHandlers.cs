using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSignDocsAssignment;

namespace DirRX.HRManagement
{
  partial class VacationSignDocsAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.OrderAttachmentGroup.IsVisible = _obj.BlockUid != Constants.VacationApprovalTask.SignInitiatorStatementBlockId;
      _obj.State.Attachments.EmployeeAttachmentGroup.IsVisible = _obj.BlockUid != Constants.VacationApprovalTask.SignInitiatorStatementBlockId &&
        _obj.BlockUid != Constants.VacationApprovalTask.SignInitiatorOrderBlockId;
    }
    
  }
}