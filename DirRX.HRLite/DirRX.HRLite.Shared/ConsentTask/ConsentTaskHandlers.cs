using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.ConsentTask;

namespace DirRX.HRLite
{
  partial class ConsentTaskSharedHandlers
  {

    public virtual void AgreementGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      var document = Sungero.Docflow.InternalDocumentBases.As(e.Attachment);
      var subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(DirRX.HRLite.ConsentTasks.Resources.SignConsentTaskSubject, document.Name);
      _obj.Subject = subject.Length > 250 ? subject.Substring(0, 250) : subject;
    }

  }
}