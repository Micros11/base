using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferPrepareDocsAssignment;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Client
{
  partial class TransferPrepareDocsAssignmentActions
  {
    
    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.TransferTask.ReworkTransferPrepareDocsAssignmentBlockId;
    }

    public virtual void RegenerateEmployeeDocuments(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (Functions.TransferPrepareDocsAssignment.Remote.IsHRDocumentsNotCreated(_obj))
      {
        var task = TransferTasks.As(_obj.Task);
        var agreement = Functions.TransferTask.Remote.CreateTransferHRDocument(task);
        _obj.AdditionalAgreement = agreement;
        
        var order = Functions.TransferTask.Remote.CreateTransferOrder(task, agreement);
        _obj.Order = order;
      }
      else
      {
        Functions.TransferPrepareDocsAssignment.Remote.ReCreateEmployeeHRDocuments(_obj);
        Dialogs.NotifyMessage(TransferPrepareDocsAssignments.Resources.DocumentsReform);
      }
    }

    public virtual bool CanRegenerateEmployeeDocuments(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Документы можно генерировать только на первом круге в блоке СОК
      // Если задачу инициировал СОК - документы не генерировать
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
        _obj.BlockUid == Constants.TransferTask.TransferPrepareDocsAssignmentBlockId &&
        !TransferPrepareDocsAssignments.GetAll(v => v.Task.Equals(_obj.Task) && v.BlockUid == _obj.BlockUid && v.Id != _obj.Id).Any() &&
        !TransferTasks.As(_obj.Task).IsHRStarted.Value;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.TransferTask.ReworkTransferPrepareDocsAssignmentBlockId;
    }

  }

}