using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationEmployeeSchedulingAssignment;

namespace DirRX.HRManagement.Client
{
  partial class VacationEmployeeSchedulingAssignmentActions
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

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {

    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }
  }
}