using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationPrepareDocsAssignment;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Client
{
  partial class VacationPrepareDocsAssignmentActions
  {
    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var approvalTask = VacationApprovalTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(approvalTask.Year.Value, approvalTask.Employee);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationApprovalTask.PrepareOrderBlockId || _obj.BlockUid == Constants.VacationApprovalTask.ReworkPrepareDocumentAssignmentBlockId;
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationApprovalTask.ReworkPrepareDocumentAssignmentBlockId;
    }
    
    public virtual void GenerateOrder(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = VacationApprovalTasks.As(_obj.Task);
      Functions.VacationApprovalTask.Remote.ReCreateVacationOrder(task, _obj.Order);
      Dialogs.NotifyMessage(VacationPrepareDocsAssignments.Resources.OrderReform);
    }

    public virtual bool CanGenerateOrder(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess && _obj.BlockUid == Constants.VacationApprovalTask.PrepareOrderBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationApprovalTask.ReworkPrepareDocumentAssignmentBlockId;
    }

  }

}