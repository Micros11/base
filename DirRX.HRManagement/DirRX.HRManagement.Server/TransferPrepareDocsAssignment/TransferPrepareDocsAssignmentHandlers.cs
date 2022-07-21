using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferPrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class TransferPrepareDocsAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      if (!_obj.HRDocumentAttachmentGroup.All.Any())
        e.AddError(TransferPrepareDocsAssignments.Resources.NeedGenerateOrderAndAgreement);
    }
  }

}