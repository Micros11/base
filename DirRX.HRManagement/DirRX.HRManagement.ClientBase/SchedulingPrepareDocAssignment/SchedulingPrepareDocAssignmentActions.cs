using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.SchedulingPrepareDocAssignment;

namespace DirRX.HRManagement.Client
{
  partial class SchedulingPrepareDocAssignmentActions
  {
    public virtual void CheckVacationSchedule(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = VacationSchedulingTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(task.Year.Value, task.BusinessUnit, Departments.Null);
    }

    public virtual bool CanCheckVacationSchedule(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess;
    }

    public virtual void CreateVacationSchedule(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = VacationSchedulingTasks.As(_obj.Task);
      var year = task.Year.Value;
      if (Functions.SchedulingPrepareDocAssignment.Remote.IsVacationScheduleNotCreated(_obj))
      {
        var document = HRDocuments.Create();
        var vacationSchedule = Functions.Module.Remote.CreateNewVacationScheduleDocument(task.BusinessUnit, year, document);
        _obj.VacationSchedule = vacationSchedule;
        _obj.VacationScheduleGroup.HRDocuments.Add(vacationSchedule);
        _obj.Save();
      }
      else
      {
        var document = _obj.VacationSchedule;
        Functions.Module.Remote.CreateNewVacationScheduleDocument(task.BusinessUnit, year, document);
        Dialogs.NotifyMessage(SchedulingPrepareDocAssignments.Resources.DocumentsReform);
      }
    }

    public virtual bool CanCreateVacationSchedule(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess;
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