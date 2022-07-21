using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementTask;

namespace DirRX.HRLite
{
  partial class StatementTaskSharedHandlers
  {

    public virtual void DocumentGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
    }

    public virtual void DocumentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      var document = Sungero.Docflow.InternalDocumentBases.As(e.Attachment);
      _obj.Subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(StatementTasks.Resources.StatementTaskSubject, document.Name);
    }

  }
}