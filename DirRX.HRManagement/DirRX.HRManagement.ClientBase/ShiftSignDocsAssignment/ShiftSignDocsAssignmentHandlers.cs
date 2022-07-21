using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftSignDocsAssignment;

namespace DirRX.HRManagement
{
  partial class ShiftSignDocsAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.EmployeeAttachmentGroup.IsVisible = _obj.BlockUid == Constants.VacationShiftTask.SignSeoDocAssignmentBlockId;
      _obj.State.Attachments.OrderAttachmentGroup.IsVisible = _obj.BlockUid != Constants.VacationShiftTask.SignStatementAssignmentBlockId;
    }
  }
}