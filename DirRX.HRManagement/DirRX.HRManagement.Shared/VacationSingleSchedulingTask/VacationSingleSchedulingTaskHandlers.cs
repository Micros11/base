using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSingleSchedulingTask;

namespace DirRX.HRManagement
{
  partial class VacationSingleSchedulingTaskSharedHandlers
  {

    public virtual void YearChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (_obj.Employee != null && e.NewValue.HasValue)
        _obj.Subject = VacationSingleSchedulingTasks.Resources.TaskSubjectFormat(e.NewValue.Value.Year, _obj.Employee.Name);
    }

    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.VacationSingleSchedulingTaskEmployeeChangedEventArgs e)
    {
      if (e.NewValue != null)
      {
        _obj.EmployeeAttachmentGroup.Employees.Clear();
        _obj.EmployeeAttachmentGroup.Employees.Add(e.NewValue);
        if (_obj.Year.HasValue)
          _obj.Subject = VacationSingleSchedulingTasks.Resources.TaskSubjectFormat(_obj.Year.Value.Year, e.NewValue.Name);
      }
    }

    public virtual void StatementChanged(DirRX.HRManagement.Shared.VacationSingleSchedulingTaskStatementChangedEventArgs e)
    {
      _obj.StatementAttachmentGroup.HRDocumentBases.Add(e.NewValue);
    }

  }
}