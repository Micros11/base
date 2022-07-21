using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationStatementApprovalAssignment;

namespace DirRX.HRManagement
{
  partial class VacationStatementApprovalAssignmentSharedHandlers
  {

    public virtual void VacationKindChanged(DirRX.HRManagement.Shared.VacationStatementApprovalAssignmentVacationKindChangedEventArgs e)
    {
      var errors = new List<string>();
      if (e.NewValue != null && e.NewValue != e.OldValue && e.NewValue.Paid.Value && _obj.DateEnd.HasValue)
        errors = Functions.VacationApprovalTask.Remote.CheckOneVacationDates(VacationApprovalTasks.As(_obj.Task), _obj.DateBegin.Value, _obj.DateEnd.Value);
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
    }

    public virtual void DateEndChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      var errors = new List<string>();
      if (e.NewValue.HasValue && e.NewValue != e.OldValue && _obj.VacationKind != null && _obj.VacationKind.Paid.Value)
        errors = Functions.VacationApprovalTask.Remote.CheckOneVacationDates(VacationApprovalTasks.As(_obj.Task), _obj.DateBegin.Value, e.NewValue.Value);
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
    }

    public virtual void DurationChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      var task = VacationApprovalTasks.As(_obj.Task);
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.DateBegin.HasValue)
        _obj.DateEnd = Functions.Module.Remote.GetVacationEndDate(_obj.DateBegin.Value, e.NewValue.Value, task.Employee);
    }

    public virtual void DateBeginChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      var task = VacationApprovalTasks.As(_obj.Task);
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.Duration.HasValue)
        _obj.DateEnd = Functions.Module.Remote.GetVacationEndDate(e.NewValue.Value, _obj.Duration.Value, task.Employee);
    }
  }
}