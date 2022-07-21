using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.SingleEmployeeSchedulingAssignment;

namespace DirRX.HRManagement
{
  partial class SingleEmployeeSchedulingAssignmentVacationsClientHandlers
  {

    public virtual void VacationsDurationValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue)
      {
        if (e.NewValue.Value <= 0)
          e.AddError(SingleEmployeeSchedulingAssignments.Resources.NegativeDurationError);
        else if (_obj.DateBegin != null)
        {
          var assignment = _obj.SingleEmployeeSchedulingAssignment;
          var employee = Employees.As(assignment.Performer);
          var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(employee, _obj.DateBegin.Value, e.NewValue.Value);
          if (!string.IsNullOrEmpty(endDateCheck))
            e.AddError(endDateCheck);
        }
      }
    }

    public virtual void VacationsDateBeginValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.Duration != null)
      {
        var assignment = _obj.SingleEmployeeSchedulingAssignment;
        var employee = Employees.As(assignment.Performer);
        var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(employee, e.NewValue.Value, _obj.Duration.Value);
        if (!string.IsNullOrEmpty(endDateCheck))
          e.AddError(endDateCheck);
      }
    }
  }

  partial class SingleEmployeeSchedulingAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var isEnabledDateFields = _obj.BlockUid == Constants.VacationSingleSchedulingTask.EmployeeSchedulingBlockId;
      _obj.State.Properties.Vacations.IsEnabled = isEnabledDateFields;
      var task = VacationSingleSchedulingTasks.As(_obj.Task);
      _obj.State.Attachments.EmployeeAttachmentGroup.IsVisible = !isEnabledDateFields;
      _obj.State.Attachments.OrderAttachmentGroup.IsVisible = task.Statement != null;
      _obj.State.Attachments.StatementAttachmentGroup.IsVisible = task.Order != null;

      foreach (var vacationRow in _obj.Vacations)
      {
        var warningList = Functions.SingleEmployeeSchedulingAssignment.Remote.CheckOneVacationDates(_obj, vacationRow.DateBegin.Value, vacationRow.DateEnd.Value);
        foreach (var warning in warningList)
          e.AddWarning(warning);
      }
      
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