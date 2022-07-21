using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationDepartmentSchedulingTask;

namespace DirRX.HRManagement
{
  partial class VacationDepartmentSchedulingTaskSharedHandlers
  {

    public virtual void YearChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (_obj.Department != null)
        _obj.Subject = VacationDepartmentSchedulingTasks.Resources.TaskSubjectFormat(e.NewValue.Value.Year, _obj.Department.Name);
    }

    public virtual void DepartmentChanged(DirRX.HRManagement.Shared.VacationDepartmentSchedulingTaskDepartmentChangedEventArgs e)
    {
      if (_obj.Year != null)
        _obj.Subject = VacationDepartmentSchedulingTasks.Resources.TaskSubjectFormat(_obj.Year.Value.Year, e.NewValue.Name);
    }

  }
}