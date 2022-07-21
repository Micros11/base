using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferStatementApprovalAssignment;

namespace DirRX.HRManagement.Client
{
  partial class TransferStatementApprovalAssignmentActions
  {
    public virtual void Abort(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(TransferStatementApprovalAssignments.Resources.ErrorTextAbortNoActiveText);
        return;
      }
    }

    public virtual bool CanAbort(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      // "Прервать" можно в блоке доработки инициатором или в блоке согласования сотрудником, если он инициировал сам.
      var task = TransferTasks.As(_obj.Task);
      return _obj.BlockUid == Constants.TransferTask.InitiatorReWorkBlockId ||
        (_obj.BlockUid == Constants.TransferTask.EmployeeStetementBlockId && !task.IsHRStarted.Value && !task.IsManagerStarted.Value);
    }

    public virtual void Deny(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(TransferTasks.Resources.ErrorTextRefuseNoActiveText);
        return;
      }
      _obj.State.Properties.JobTitle.IsRequired = false;
      _obj.State.Properties.Department.IsRequired = false;
      _obj.State.Properties.TransferDate.IsRequired = false;
    }

    public virtual bool CanDeny(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      // "Отказать" можно в блоках руководителей или в блоке согласования сотрудником, если инициировал руководитель или СОК.
      var task = TransferTasks.As(_obj.Task);
      return _obj.BlockUid == Constants.TransferTask.ApprovalNewManagerAssignmentBlockId || _obj.BlockUid == Constants.TransferTask.ApprovalOldManagerAssignmentBlockId ||
        (_obj.BlockUid == Constants.TransferTask.EmployeeStetementBlockId && (task.IsHRStarted.Value || task.IsManagerStarted.Value));
    }

    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(TransferStatementApprovalAssignments.Resources.ErrorTextReWorkNoActiveText);
        return;
      }
      _obj.State.Properties.JobTitle.IsRequired = false;
      _obj.State.Properties.Department.IsRequired = false;
      _obj.State.Properties.TransferDate.IsRequired = false;
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.TransferTask.InitiatorReWorkBlockId;
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var task = TransferTasks.As(_obj.Task);
      if (!task.IsManagerStarted.Value && !task.IsHRStarted.Value && _obj.BlockUid == Constants.TransferTask.InitiatorReWorkBlockId && string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(TransferTasks.Resources.EmptyActiveTextError);
      if ((task.IsManagerStarted.Value || task.IsHRStarted.Value) && _obj.TransferDate <= Calendar.Today)
        e.AddError(TransferTasks.Resources.TransferDateError);
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.TransferTask.InitiatorReWorkBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (_obj.TransferDate <= Calendar.Today)
        e.AddError(TransferTasks.Resources.TransferDateError);
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.TransferTask.InitiatorReWorkBlockId;
    }

  }

}