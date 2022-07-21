using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DocumentApprovalTask;

namespace DirRX.HRManagement
{
  partial class DocumentApprovalTaskSharedHandlers
  {

    public override void SubjectChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      // TODO: удалить код после исправления бага 17930 (сейчас этот баг в TFS недоступен, он про автоматическое обрезание темы).
      if (e.NewValue != null && e.NewValue.Length > DocumentApprovalTasks.Info.Properties.Subject.Length)
        _obj.Subject = e.NewValue.Substring(0, DocumentApprovalTasks.Info.Properties.Subject.Length);
    }

    public virtual void DocumentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      var document = Sungero.Docflow.InternalDocumentBases.As(e.Attachment);
      _obj.Subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(DocumentApprovalTasks.Resources.DocumentApprovalTaskSubject, document.Name);
    }

  }
}