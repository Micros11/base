using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSingleSchedulingTask;

namespace DirRX.HRManagement
{
  partial class VacationSingleSchedulingTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var unScheduledDurations = Functions.Module.GetEmployeeUnScheduledDurations(_obj.Employee, _obj.Year.Value);
      if (!unScheduledDurations.Any())
      {
        var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false);
        e.AddError(VacationSingleSchedulingTasks.Resources.StartShedulingErrorFormat(employeeShortName, _obj.Year.Value.Year));
      }
    }

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;

      Functions.Module.SendAbortationNotices(_obj, VacationSingleSchedulingTasks.Resources.AbortNoticeFormat(_obj.Employee.Name), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      _obj.Year = Calendar.Today;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      _obj.Subject = VacationSingleSchedulingTasks.Resources.TaskSubjectFormat(_obj.Year.Value.Year, _obj.Employee.Name);
    }
  }

}