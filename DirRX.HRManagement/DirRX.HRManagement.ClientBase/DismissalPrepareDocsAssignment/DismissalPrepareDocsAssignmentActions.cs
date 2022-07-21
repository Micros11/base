using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalPrepareDocsAssignment;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Client
{
  partial class DismissalPrepareDocsAssignmentActions
  {
    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.DismissalTask.ReWorkDocumentAssignmentBlockId;
    }

    public virtual void GenerateOrder(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = DismissalTasks.As(_obj.Task);
      if (_obj.Order == null)
      {
        var order = Functions.DismissalTask.Remote.CreateDismissalOrder(task);
        _obj.Order = order;
        _obj.HRDocumentAttachmentGroup.HROrders.Add(order);
        _obj.Save();
      }
      else
      {
        Functions.DismissalTask.Remote.ReCreateDismissalOrder(task, _obj.Order);
        Dialogs.NotifyMessage(DismissalPrepareDocsAssignments.Resources.OrderReform);
      }
    }

    public virtual bool CanGenerateOrder(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess && _obj.BlockUid == Constants.DismissalTask.PrepareDocsAssignmentBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.DismissalTask.ReWorkDocumentAssignmentBlockId;
    }

  }

}