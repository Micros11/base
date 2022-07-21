using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRLite.EmployeeSignAssignment;

namespace DirRX.HRLite.Client
{
  partial class EmployeeSignAssignmentActions
  {
    public virtual void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Functions.EmployeeSignAssignment.Remote.IsPossiblePerformAssignment(_obj);
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (Functions.Module.Remote.IsCurrentUserInWebAccessEmployees())
      {
        var task = SignTasks.As(_obj.Task);
        var documents = task.DocumentGroup.BilaterialDocuments.Concat(task.AddendaGroup.BilaterialDocuments).Select(d => OfficialDocuments.As(d)).ToList();
        // Подписать документ.
        Functions.Module.ApproveDocument(_obj, documents, string.Empty, e, true);
      }
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Functions.EmployeeSignAssignment.Remote.IsPossiblePerformAssignment(_obj);
    }
  }
}