using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationEmployeeSchedulingAssignment;

namespace DirRX.HRManagement
{
  partial class VacationEmployeeSchedulingAssignmentVacationsClientHandlers
  {

    public virtual void VacationsDateBeginValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.Duration != null)
      {
        var assignment = _obj.VacationEmployeeSchedulingAssignment;
        var employee = Employees.As(assignment.Performer);
        var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(employee, e.NewValue.Value, _obj.Duration.Value);
        if (!string.IsNullOrEmpty(endDateCheck))
          e.AddError(endDateCheck);
      }
    }

    public virtual void VacationsDurationValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue)
      {
        if (e.NewValue.Value <= 0)
          e.AddError(VacationDepartmentSchedulingTasks.Resources.NegativeDurationError);
        else if (_obj.DateBegin != null)
        {
          var assignment = _obj.VacationEmployeeSchedulingAssignment;
          var employee = Employees.As(assignment.Performer);
          var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(employee, _obj.DateBegin.Value, e.NewValue.Value);
          if (!string.IsNullOrEmpty(endDateCheck))
            e.AddError(endDateCheck);
        }
      }
    }
  }

  partial class VacationEmployeeSchedulingAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.VacationEmployeeSchedulingAssignment.Remote.FillAssignmentDurations(_obj);
    }
    
    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      // Выводить не критичные ошибки, если параметр не пустой.
      var errors = string.Empty;
      if (!e.Params.TryGetValue("Errors", out errors) || string.IsNullOrEmpty(errors))
        return;
      e.AddWarning(errors);
    }
  }
}