using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationApprovalTask;

namespace DirRX.HRManagement
{
  partial class VacationApprovalTaskSharedHandlers
  {

    public virtual void VacationKindChanged(DirRX.HRManagement.Shared.VacationApprovalTaskVacationKindChangedEventArgs e)
    {
      var errors = new List<string>();
      if (e.NewValue != null && e.NewValue != e.OldValue && e.NewValue.Paid.Value && _obj.DateEnd.HasValue)
        errors = Functions.VacationApprovalTask.Remote.CheckOneVacationDates(_obj, _obj.DateBegin.Value, _obj.DateEnd.Value);
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
    }

    public virtual void DurationChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.DateBegin.HasValue)
        _obj.DateEnd = Functions.Module.Remote.GetVacationEndDate(_obj.DateBegin.Value, e.NewValue.Value, _obj.Employee);
    }
    
    public virtual void DateEndChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      var errors = new List<string>();
      if (e.NewValue.HasValue && e.NewValue != e.OldValue && _obj.VacationKind != null && _obj.VacationKind.Paid.Value)
        errors = Functions.VacationApprovalTask.Remote.CheckOneVacationDates(_obj, _obj.DateBegin.Value, e.NewValue.Value);
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
    }

    public virtual void DateBeginChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        _obj.Year = e.NewValue.HasValue ? e.NewValue.Value : Calendar.SqlMinValue;
        
        if (e.NewValue.HasValue && _obj.Duration.HasValue)
          _obj.DateEnd = Functions.Module.Remote.GetVacationEndDate(e.NewValue.Value, _obj.Duration.Value, _obj.Employee);
      }
    }

    public virtual void StatementChanged(DirRX.HRManagement.Shared.VacationApprovalTaskStatementChangedEventArgs e)
    {
      _obj.StatementAttachmentGroup.HRDocumentBases.Add(e.NewValue);
    }

    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.VacationApprovalTaskEmployeeChangedEventArgs e)
    {
      if (e.NewValue != null) {
        _obj.Subject = VacationApprovalTasks.Resources.VacationApprovalTaskSubjectFormat(e.NewValue.Name);
        _obj.EmployeeAttachmentGroup.Employees.Add(e.NewValue);
        var manager = Functions.Module.Remote.GetManager(e.NewValue);
        if (manager != null && !Equals(manager, e.NewValue))
          _obj.Substitute = manager;
      }
    }
  }
}