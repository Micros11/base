using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationStatementApprovalAssignment;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement
{
  partial class VacationStatementApprovalAssignmentClientHandlers
  {

    public virtual void DateBeginValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.Duration != null)
      {
        var task = VacationApprovalTasks.As(_obj.Task);
        var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(task.Employee, e.NewValue.Value, _obj.Duration.Value);
        if (!string.IsNullOrEmpty(endDateCheck))
          e.AddError(endDateCheck);
      }
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      // Выводить не критичные ошибки, если параметр не пустой.
      if (_obj.VacationKind != null && _obj.VacationKind.Paid.Value)
      {
        var errors = string.Empty;
        if (!e.Params.TryGetValue("Errors", out errors) || string.IsNullOrEmpty(errors))
          return;
        e.AddWarning(errors);
      }
    }

    public virtual void DurationValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue)
      {
        if (e.NewValue.Value <= 0)
          e.AddError(VacationApprovalTasks.Resources.NegativeDurationError);
        else if (_obj.DateBegin != null)
        {
          var task = VacationApprovalTasks.As(_obj.Task);
          var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(task.Employee, _obj.DateBegin.Value, e.NewValue.Value);
          if (!string.IsNullOrEmpty(endDateCheck))
            e.AddError(endDateCheck);
        }
      }
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var isEnabledDateFields = _obj.BlockUid == Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId;
      _obj.State.Properties.DateBegin.IsEnabled = isEnabledDateFields;
      _obj.State.Properties.Duration.IsEnabled = isEnabledDateFields;
      _obj.State.Properties.VacationKind.IsEnabled = isEnabledDateFields;
      _obj.State.Attachments.EmployeeAttachmentGroup.IsVisible = _obj.BlockUid != Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId;
      if ((_obj.BlockUid == Constants.VacationApprovalTask.ApprovingBlockId || _obj.BlockUid == Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId)
          && _obj.VacationKind.Paid.Value == true)
      {
        var approvalTask = VacationApprovalTasks.As(_obj.Task);
        var warningList = Functions.VacationApprovalTask.Remote.CheckNotCriticalVacationsDates(approvalTask, approvalTask.Employee,
                                                                                               Structures.Vacation.VacationDates.Create(_obj.DateBegin.Value, _obj.DateEnd.Value,
                                                                                                                                        _obj.Duration.Value, DateStatuses.New, _obj.VacationKind));
        foreach (var warning in warningList)
          e.AddWarning(warning);
      }
    }

  }
}