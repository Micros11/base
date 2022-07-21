using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.VacationStatementApprovalAssignment;

namespace DirRX.HRManagement.Client
{
  partial class VacationStatementApprovalAssignmentActions
  {
    public virtual void Deny(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(VacationApprovalTasks.Resources.ErrorTextDenyNoActiveText);
        return;
      }
      
      _obj.State.Properties.DateBegin.IsRequired = false;
      _obj.State.Properties.DateEnd.IsRequired = false;
    }

    public virtual bool CanDeny(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationApprovalTask.ApprovingBlockId;
    }

    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var approvalTask = VacationApprovalTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(approvalTask.Year.Value, approvalTask.Employee);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Stop(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var task = VacationApprovalTasks.As(_obj.Task);
      Functions.Module.Remote.SendAbortationNotices(task, VacationApprovalTasks.Resources.AbortNoticeFormat(task.Employee.Name), task.AbortingReason, task.Author, task.ThreadSubject);
    }

    public virtual bool CanStop(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId;
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var errorList = Functions.VacationStatementApprovalAssignment.Remote.CheckAllVacationDates(_obj);
      
      foreach (var error in errorList)
        e.AddError(error);
      
      if (errorList.Any())
        return;
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId;
    }

    public virtual void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(VacationApprovalTasks.Resources.ErrorTextReWorkNoActiveText);
        return;
      }
      
      _obj.State.Properties.DateBegin.IsRequired = false;
      _obj.State.Properties.DateEnd.IsRequired = false;
    }

    public virtual bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId;
    }

  }

}