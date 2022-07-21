using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.Vacation;

namespace DirRX.HRManagement
{
  partial class VacationSharedHandlers
  {

    public virtual void VacationKindChanged(DirRX.HRManagement.Shared.VacationVacationKindChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        Functions.Vacation.FormatName(_obj);
    }

    public virtual void VacationDurationChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.StartDate.HasValue && _obj.Employee != null)
        _obj.FinDate = Functions.Module.Remote.GetVacationEndDate(_obj.StartDate.Value, e.NewValue.Value, _obj.Employee);
    }

    public virtual void DepartmentChanged(DirRX.HRManagement.Shared.VacationDepartmentChangedEventArgs e)
    {
      var department = e.NewValue;
      if (department != e.OldValue)
      {
        _obj.BusinessUnit = department != null ? department.BusinessUnit : null;
        if (e.OldValue != null && _obj.Employee != null)
          if (!_obj.Employee.Department.Equals(department))
            _obj.Employee = null;
      }
    }

    public virtual void FinDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue)
        Functions.Vacation.FormatName(_obj);
    }

    public virtual void StartDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue)
      {
        Functions.Vacation.FormatName(_obj);
        _obj.WorkCalendars = WorkingTime.GetAllCachedByYear(e.NewValue.Value.Year, _obj.Employee).Where(n => n.Year == e.NewValue.Value.Year).FirstOrDefault();
        
        if (_obj.VacationDuration != null  && _obj.Employee != null)
          _obj.FinDate = Functions.Module.Remote.GetVacationEndDate(e.NewValue.Value, _obj.VacationDuration.Value, _obj.Employee);
      }
    }

    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.VacationEmployeeChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        Functions.Vacation.FormatName(_obj);
        _obj.Department = e.NewValue.Department;
        
        if (_obj.StartDate.HasValue)
          _obj.WorkCalendars = WorkingTime.GetAllCachedByYear(_obj.StartDate.Value.Year, e.NewValue).Where(n => n.Year == _obj.StartDate.Value.Year).FirstOrDefault();
        
        if (_obj.StartDate.HasValue && _obj.VacationDuration != null)
          _obj.FinDate = Functions.Module.Remote.GetVacationEndDate(_obj.StartDate.Value, _obj.VacationDuration.Value, e.NewValue);
      }
    }

  }
}