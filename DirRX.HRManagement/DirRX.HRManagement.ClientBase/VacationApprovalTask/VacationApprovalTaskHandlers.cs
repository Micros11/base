using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.Company;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationApprovalTask;

namespace DirRX.HRManagement
{
  partial class VacationApprovalTaskClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      PublicFunctions.Module.Remote.CheckVacationsModuleLicense();
      _obj.State.Attachments.EmployeeAttachmentGroup.IsVisible = false;
      _obj.State.Attachments.OrderAttachmentGroup.IsVisible = false;
      _obj.State.Attachments.StatementAttachmentGroup.IsVisible = false;
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      // Выводить некритичные ошибки, если отпуск оплачиваемый.
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
      if (e.NewValue != e.OldValue && e.NewValue.HasValue)
      {
        if (e.NewValue.Value <= 0)
          e.AddError(VacationApprovalTasks.Resources.NegativeDurationError);
        else if (_obj.DateBegin != null)
        {
          var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(_obj.Employee, _obj.DateBegin.Value, e.NewValue.Value);
          if (!string.IsNullOrEmpty(endDateCheck))
            e.AddError(endDateCheck);
        }
      }
    }

    public virtual void DateBeginValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue)
        if (e.NewValue.Value < Calendar.Today)
          e.AddError(VacationApprovalTasks.Resources.StartDateLessTodayError);
        else if (_obj.Duration != null)
        {
          var endDateCheck = Functions.Module.Remote.CheckPossibleEndDate(_obj.Employee, e.NewValue.Value, _obj.Duration.Value);
          if (!string.IsNullOrEmpty(endDateCheck))
            e.AddError(endDateCheck);
        }
    }

  }

}