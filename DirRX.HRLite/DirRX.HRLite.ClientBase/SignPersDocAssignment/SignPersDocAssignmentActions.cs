﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRLite.SignPersDocAssignment;

namespace DirRX.HRLite.Client
{
  partial class SignPersDocAssignmentActions
  {
    public virtual void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    { 
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(HRAcquaintanceTasks.Resources.ErrorTextForRevisionNoActiveText);
    }

    public virtual bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(HRAcquaintanceTasks.Resources.ErrorTextRefuseNoActiveText); 
    }

    public virtual bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var task = HRAcquaintanceTasks.As(_obj.Task);
      var documents = task.DocumentGroup.InternalDocumentBases.Concat(task.AddendaGroup.InternalDocumentBases).Select(d => OfficialDocuments.As(d)).ToList();

      // Подписать документ.
      Functions.Module.ApproveDocument(_obj, documents, string.Empty, e, true);
      
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }
  }
}