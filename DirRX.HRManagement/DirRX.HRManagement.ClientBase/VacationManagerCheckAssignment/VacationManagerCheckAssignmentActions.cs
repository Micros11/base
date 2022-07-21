using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationManagerCheckAssignment;

namespace DirRX.HRManagement.Client
{
  partial class VacationManagerCheckAssignmentActions
  {
    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = VacationDepartmentSchedulingTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(task.Year.Value, task.Department.BusinessUnit, task.Department);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void ShowDepartmentVacations(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.VacationManagerCheckAssignment.Remote.VacationsForDepartment(_obj).ShowModal();
    }

    public virtual bool CanShowDepartmentVacations(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}