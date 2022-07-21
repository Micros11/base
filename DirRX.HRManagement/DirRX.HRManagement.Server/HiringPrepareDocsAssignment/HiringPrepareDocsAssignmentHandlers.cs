using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringPrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class HiringPrepareDocsAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      if (!_obj.HRDocumentAttachmentGroup.All.Any())
        e.AddError(DirRX.HRManagement.HiringPrepareDocsAssignments.Resources.ErrorTextNoHRDocs);
    }
  }
}