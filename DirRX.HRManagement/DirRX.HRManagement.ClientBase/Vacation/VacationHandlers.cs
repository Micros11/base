using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.Company;
using Sungero.CoreEntities;
using DirRX.HRManagement.Vacation;

namespace DirRX.HRManagement
{
  partial class VacationClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var vacationAccess = Functions.Vacation.Remote.GetVacationAccess(_obj);
      
      if (vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.FullAccess)
      {
        _obj.State.IsEnabled = true;
        _obj.State.Properties.NoteDate.IsVisible = true;
      }
      else if (vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.Change)
      {
        _obj.State.IsEnabled = true;
        _obj.State.Properties.Year.IsEnabled = false;
        _obj.State.Properties.Status.IsEnabled = false;
        _obj.State.Properties.NoteDate.IsVisible = false;
      }
      else
      {
        _obj.State.IsEnabled = false;
        _obj.State.Properties.NoteDate.IsVisible = false;
      }
      
      // Если подразделение в карточке не соответствует подразделению сотрудника - заменить и предупредить
      if (vacationAccess != DirRX.HRManagement.Constants.Vacation.VacationAccess.Read)
      {
        if (_obj.Employee != null && !_obj.Department.Equals(_obj.Employee.Department))
        {
          _obj.Department = _obj.Employee.Department;
          e.AddInformation(DirRX.HRManagement.Vacations.Resources.VacationDepartmentChangedInfo);
        }
      }
    }

    public virtual void StartDateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.VacationDuration != null)
      {
        var employee = _obj.Employee ?? Employees.Current;
        var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(employee, e.NewValue.Value, _obj.VacationDuration.Value);
        if (!string.IsNullOrEmpty(endDateCheck))
          e.AddError(endDateCheck);
      }
    }

    public virtual void VacationDurationValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue)
      {
        if (e.NewValue.Value <= 0)
          e.AddError(VacationDepartmentSchedulingTasks.Resources.NegativeDurationError);
        else if (_obj.StartDate != null)
        {
          var employee = _obj.Employee ?? Employees.Current;
          var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(employee, _obj.StartDate.Value, e.NewValue.Value);
          if (!string.IsNullOrEmpty(endDateCheck))
            e.AddError(endDateCheck);
        }
      }
    }
  }
}