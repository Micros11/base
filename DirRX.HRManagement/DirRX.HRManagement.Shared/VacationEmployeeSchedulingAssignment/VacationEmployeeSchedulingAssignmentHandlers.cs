﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationEmployeeSchedulingAssignment;

namespace DirRX.HRManagement
{

  partial class VacationEmployeeSchedulingAssignmentVacationsSharedHandlers
  {

    public virtual void VacationsVacationKindChanged(DirRX.HRManagement.Shared.VacationEmployeeSchedulingAssignmentVacationsVacationKindChangedEventArgs e)
    {
      var errors = new List<string>();
      if (e.NewValue != e.OldValue && e.NewValue != null && e.NewValue.Paid.Value && _obj.DateEnd.HasValue)
        errors = Functions.VacationEmployeeSchedulingAssignment.Remote.CheckOneVacationDates(_obj.VacationEmployeeSchedulingAssignment, _obj.DateBegin.Value, _obj.DateEnd.Value);
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
      Functions.VacationEmployeeSchedulingAssignment.Remote.FillInstruction(_obj.VacationEmployeeSchedulingAssignment);
    }

    public virtual void VacationsDurationChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (_obj.DateBegin.HasValue && e.NewValue.HasValue)
          _obj.DateEnd = Functions.Module.Remote.GetVacationEndDate(_obj.DateBegin.Value, e.NewValue.Value, _obj.VacationEmployeeSchedulingAssignment.Performer);
        else if (!e.NewValue.HasValue)
          _obj.DateEnd = null;
      }
    }

    public virtual void VacationsDateEndChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      var errors = new List<string>();
      if (e.NewValue != e.OldValue && e.NewValue.HasValue && _obj.VacationKind != null && _obj.VacationKind.Paid.Value)
        errors = Functions.VacationEmployeeSchedulingAssignment.Remote.CheckOneVacationDates(_obj.VacationEmployeeSchedulingAssignment, _obj.DateBegin.Value, e.NewValue.Value);
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
      Functions.VacationEmployeeSchedulingAssignment.Remote.FillInstruction(_obj.VacationEmployeeSchedulingAssignment);
    }

    public virtual void VacationsDateBeginChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue.HasValue && _obj.Duration.HasValue)
          _obj.DateEnd = Functions.Module.Remote.GetVacationEndDate(e.NewValue.Value, _obj.Duration.Value, Sungero.Company.Employees.As(_obj.VacationEmployeeSchedulingAssignment.Performer));
        else if (!e.NewValue.HasValue)
          _obj.DateEnd = null;
      }
    }
  }

  partial class VacationEmployeeSchedulingAssignmentVacationsSharedCollectionHandlers
  {

    public virtual void VacationsDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.VacationEmployeeSchedulingAssignment.Remote.FillInstruction(_obj);
    }
    
    public virtual void VacationsAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      // Задать порядковый номер.
      var lastNumber = _obj.Vacations.OrderBy(j => j.Number).LastOrDefault();
      if (lastNumber.Number.HasValue)
        _added.Number = lastNumber.Number + 1;
      else
        _added.Number = 1;
      if (!_added.State.IsCopied)
      {
        var availableKinds = _obj.VacationDurations.Select(l => l.VacationKind);
        var paidVacationKind = Functions.Module.Remote.GetPaidVacationKind(_obj.Performer);
        if (availableKinds.Any())
        {
          if (availableKinds.Contains(paidVacationKind))
            _added.VacationKind = paidVacationKind;
          else
            _added.VacationKind = availableKinds.FirstOrDefault();
        }
      }
      var task = VacationDepartmentSchedulingTasks.As(_obj.Task);
      _added.DateBegin = task.Year.Value.BeginningOfYear();
    }
  }
}