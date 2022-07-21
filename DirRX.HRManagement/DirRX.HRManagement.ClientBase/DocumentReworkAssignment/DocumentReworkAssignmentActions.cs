using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DocumentReworkAssignment;

namespace DirRX.HRManagement.Client
{
  partial class DocumentReworkAssignmentActions
  {
    public virtual void Abort(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(DirRX.HRManagement.DocumentReworkAssignments.Resources.ErrorTextAbortNoActiveText);
        return;
      }
      DocumentApprovalTasks.As(_obj.Task).AbortingReason = _obj.ActiveText;
    }

    public virtual bool CanAbort(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}