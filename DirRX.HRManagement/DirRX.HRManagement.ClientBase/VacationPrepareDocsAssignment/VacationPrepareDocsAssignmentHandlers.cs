using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationPrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class VacationPrepareDocsAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.StatementAttachmentGroup.IsVisible = VacationApprovalTasks.As(_obj.Task).Statement != null;
    }

  }
}