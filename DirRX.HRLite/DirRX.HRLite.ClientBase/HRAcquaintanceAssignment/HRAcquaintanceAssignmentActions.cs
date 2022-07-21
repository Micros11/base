using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRLite.HRAcquaintanceAssignment;

namespace DirRX.HRLite.Client
{
  partial class HRAcquaintanceAssignmentActions
  {
    public virtual void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {

    }

    public virtual bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Functions.HRAcquaintanceAssignment.Remote.IsPossiblePerformAssignment(_obj);
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (Functions.Module.Remote.IsCurrentUserInWebAccessEmployees())
      {
        var task = HRAcquaintanceTasks.As(_obj.Task);
        var documents = task.DocumentGroup.InternalDocumentBases.Concat(task.AddendaGroup.InternalDocumentBases).Select(d => OfficialDocuments.As(d)).ToList();
        // Подписать документ.
        Functions.Module.EndorseDocument(_obj, documents, string.Empty, e);
      }
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Functions.HRAcquaintanceAssignment.Remote.IsPossiblePerformAssignment(_obj);
    }

  }

}