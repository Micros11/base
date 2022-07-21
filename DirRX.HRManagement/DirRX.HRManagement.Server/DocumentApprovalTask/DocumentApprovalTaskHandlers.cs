using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DocumentApprovalTask;

namespace DirRX.HRManagement
{
  partial class DocumentApprovalTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      _obj.Subject = DocumentApprovalTasks.Resources.DocumentApprovalTaskSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
    }
  }

}