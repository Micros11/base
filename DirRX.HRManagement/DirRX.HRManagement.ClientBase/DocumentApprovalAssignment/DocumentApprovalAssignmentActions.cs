using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DocumentApprovalAssignment;

namespace DirRX.HRManagement.Client
{
  partial class DocumentApprovalAssignmentActions
  {
    public virtual void Approved(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanApproved(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void ForRework(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var activeText = _obj.ActiveText;
      if (!string.IsNullOrEmpty(activeText))
        activeText = activeText.Trim();
      if (string.IsNullOrEmpty(activeText))
      {
        e.AddError(DirRX.HRManagement.DocumentApprovalAssignments.Resources.ErrorTextReWorkNoActiveText);
        return;
      }
    }

    public virtual bool CanForRework(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}