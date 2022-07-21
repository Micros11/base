using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SinglePrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class SinglePrepareDocsAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.StatementAttachmentGroup.IsVisible = VacationSingleSchedulingTasks.As(_obj.Task).Statement != null;
    }

  }
}