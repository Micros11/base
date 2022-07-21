using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalPrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class DismissalPrepareDocsAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      if (_obj.BlockUid == Constants.DismissalTask.PrepareDocsAssignmentBlockId && !_obj.HRDocumentAttachmentGroup.HROrders.Any())
        e.AddError(DismissalPrepareDocsAssignments.Resources.NeedGenerateOrder);
    }
  }

}