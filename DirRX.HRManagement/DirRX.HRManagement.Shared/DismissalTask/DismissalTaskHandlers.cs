using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalTask;

namespace DirRX.HRManagement
{
  partial class DismissalTaskSharedHandlers
  {

    public virtual void StatementChanged(DirRX.HRManagement.Shared.DismissalTaskStatementChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.StatementAttachmentGroup.HRDocumentBases.Clear();
      
      if (e.NewValue != null)
        _obj.StatementAttachmentGroup.HRDocumentBases.Add(e.NewValue);
    }

    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.DismissalTaskEmployeeChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
        _obj.Subject = DismissalTasks.Resources.DismissalTaskSubjectFormat(e.NewValue.Name);
      if (e.NewValue == null)
        _obj.Subject = DismissalTasks.Resources.DismissalTaskSubjectFormat(string.Empty);
    }

  }
}