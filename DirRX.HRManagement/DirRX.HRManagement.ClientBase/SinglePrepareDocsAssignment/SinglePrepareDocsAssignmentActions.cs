using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SinglePrepareDocsAssignment;

namespace DirRX.HRManagement.Client
{
  partial class SinglePrepareDocsAssignmentActions
  {
    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationSingleSchedulingTask.ReworkPrepareDocumentAssignmentBlockId;
    }

    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var approvalTask = VacationSingleSchedulingTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(approvalTask.Year.Value, approvalTask.Employee);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationSingleSchedulingTask.PrepareOrderBlockId || _obj.BlockUid == Constants.VacationSingleSchedulingTask.ReworkPrepareDocumentAssignmentBlockId;
    }

    public virtual void GenerateOrder(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = VacationSingleSchedulingTasks.As(_obj.Task);
      Functions.VacationSingleSchedulingTask.Remote.ReCreateVacationOrder(task, _obj.Order);
      Dialogs.NotifyMessage(SinglePrepareDocsAssignments.Resources.OrderReform);
    }

    public virtual bool CanGenerateOrder(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess && _obj.BlockUid == Constants.VacationSingleSchedulingTask.PrepareOrderBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationSingleSchedulingTask.ReworkPrepareDocumentAssignmentBlockId;
    }

  }

}