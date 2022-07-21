using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.InformingTask;

namespace DirRX.HRLite
{
  partial class InformingTaskSharedHandlers
  {

    public override void SubjectChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      // TODO: удалить код после исправления бага 17930 (сейчас этот баг в TFS недоступен, он про автоматическое обрезание темы).
      if (e.NewValue != null && e.NewValue.Length > InformingTasks.Info.Properties.Subject.Length)
        _obj.Subject = e.NewValue.Substring(0, InformingTasks.Info.Properties.Subject.Length);
    }

    public virtual void AttachmentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      _obj.Subject = _obj.Subject + " " + e.Attachment.DisplayValue;
    }

    public virtual void AttachmentGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      _obj.Subject = _obj.Subject.Replace(e.Attachment.DisplayValue, string.Empty);
    }
  }
}