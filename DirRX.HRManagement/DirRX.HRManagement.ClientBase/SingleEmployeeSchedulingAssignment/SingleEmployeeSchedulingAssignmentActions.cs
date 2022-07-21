using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SingleEmployeeSchedulingAssignment;

namespace DirRX.HRManagement.Client
{
  partial class SingleEmployeeSchedulingAssignmentActions
  {
    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return SingleEmployeeSchedulingAssignments.GetAll().Where(a => a.BlockUid == Constants.VacationSingleSchedulingTask.ManagerSchedulingBlockId && a.Task == _obj.Task).Any() &&
        _obj.BlockUid == Constants.VacationSingleSchedulingTask.EmployeeSchedulingBlockId;
    }

    public virtual void Approve(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanApprove(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationSingleSchedulingTask.EmployeeSchedulingBlockId;
    }

    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = VacationSingleSchedulingTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(task.Year.Value, task.Employee.Department.BusinessUnit, task.Employee.Department);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(VacationSingleSchedulingTasks.Resources.ErrorTextReWorkNoActiveText);
    }

    public virtual bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationSingleSchedulingTask.EmployeeSchedulingBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return !SingleEmployeeSchedulingAssignments.GetAll().Where(a => a.BlockUid == Constants.VacationSingleSchedulingTask.ManagerSchedulingBlockId && a.Task == _obj.Task).Any() &&
        _obj.BlockUid == Constants.VacationSingleSchedulingTask.EmployeeSchedulingBlockId;
      
    }

  }

}