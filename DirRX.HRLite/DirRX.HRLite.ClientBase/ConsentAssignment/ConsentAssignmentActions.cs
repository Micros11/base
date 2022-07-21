using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.ConsentAssignment;

namespace DirRX.HRLite.Client
{
  partial class ConsentAssignmentActions
  {
    public virtual void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Functions.ConsentAssignment.Remote.IsPossiblePerformAssignment(_obj);
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var task = ConsentTasks.As(_obj.Task);
      var documents = task.AgreementGroup.StatementDocuments.Select(d => Sungero.Docflow.OfficialDocuments.As(d)).ToList();
      // Подписать документ.
      Functions.Module.ApproveDocument(_obj, documents, string.Empty, e, true);
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Functions.ConsentAssignment.Remote.IsPossiblePerformAssignment(_obj);
    }

  }

}