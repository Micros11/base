using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftStatementApprovalAssignment;

namespace DirRX.HRManagement
{

  partial class ShiftStatementApprovalAssignmentSharedHandlers
  {

    public virtual void RecallDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue != null) {
          // найти часть отпуска, которая "окружает" дату отзыва
          _obj.Vacation1 = Functions.Vacation.Remote.GetRecallVacations(_obj.Employee, _obj.RecallDate).FirstOrDefault();
          // если хоть по одному виду отпуска получилось так, что запланированных дней стало меньше
          // чем перненесенных+компенсированных+отмененных, то почистить все новые даты отпуска
          foreach (var vd in _obj.VacationDurations) {
            if (vd.DaysPlaned < (vd.DaysShifted + vd.DaysCanceled + vd.DaysComped)) {
              Functions.ShiftStatementApprovalAssignment.Remote.CleanFirstNewVacationDate(_obj);
              Functions.ShiftStatementApprovalAssignment.Remote.CleanSecondNewVacationDate(_obj);
              Functions.ShiftStatementApprovalAssignment.Remote.CleanThirdNewVacationDate(_obj);
              if (_obj.VacationKind1 == null && _obj.Vacation1 != null)
                _obj.VacationKind1 = _obj.Vacation1.VacationKind;
              break;
            }
          }
        } 
        else {
          // если дату отзыва опустошили - почистить первый отпуск и запустить цепочку,
          // которая приведет к чистке всего остального
          _obj.Vacation1 = null;
        }
        // если задание "На доработку", то при изменении даты отзыва сбрасывать галочку "Игнорировать..."
        _obj.IgnoreRequiredDuration = false;
      }
    }
    
    #region Обработчики Vacation1, Vacation2, Vacation3
    public virtual void Vacation1Changed(DirRX.HRManagement.Shared.ShiftStatementApprovalAssignmentVacation1ChangedEventArgs e)
    {
      // поведение зависит от того, отзыв у нас или перенос
      if (VacationShiftTasks.As(_obj.Task).IsVacationRecall.Value) {
        if (e.NewValue != e.OldValue) {
          // если изменилась первая часть отпуска, то и вторая изменится. Поэтому почистим её.
          _obj.Vacation2 = null;
          
          Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.VacationKindsCheckUp(_obj);
          
          _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation1 != null;
        } 
        else {
          // пересчитать дни, которые осталось перераспределить
          // нужно для ситуации, когда чуть подвинули дату отзыва и первая часть отпуска не изменилась.
          // например, отпуск с 1.4 по 10.4, а дату отзывы поменяли с 5.4 на 7.4
          Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
        }
        if (e.NewValue != null) {
          var requiredDurationError = string.Empty;
          requiredDurationError = Functions.ShiftStatementApprovalAssignment.Remote.CheckRequiredDurationonVacation1(_obj);
          // если задание руководителю - то ошибка критичная, если сотруднику - warning
          var errorType = "Warnings";

          // если задание "согласование с руководителем", то сообщение будет критичное
          if (_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId)
            errorType = "Errors";
          if (requiredDurationError != string.Empty) {
            e.Params.AddOrUpdate(errorType, string.Join(" ", requiredDurationError));
            _obj.IsRequiredDurationBroken = true;
          }
          else {
            e.Params.Remove(errorType);
            _obj.IsRequiredDurationBroken = false;
          }
          if (_obj.VacationKind1 == null)
            _obj.VacationKind1 = _obj.Vacation1.VacationKind;
        }
        
      } 
      else {
        if (e.NewValue != e.OldValue) {
          Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.VacationKindsCheckUp(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.SortFields(_obj);
          
          if (e.NewValue != null)
            if (_obj.VacationKind1 == null)
              _obj.VacationKind1 = _obj.Vacation1.VacationKind;
        }
        _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation1 != null;
        _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation2 != null;
      }
      Functions.ShiftStatementApprovalAssignment.SetEnabledProperties(_obj);
    }
    
    public virtual void Vacation2Changed(DirRX.HRManagement.Shared.ShiftStatementApprovalAssignmentVacation2ChangedEventArgs e)
    {
      if (VacationShiftTasks.As(_obj.Task).IsVacationRecall.Value) {
        if (e.NewValue != e.OldValue) {
          _obj.Vacation3 = null;
          Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.VacationKindsCheckUp(_obj);
          _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation2 != null;
        }
      } 
      else {
        if (e.NewValue != e.OldValue) {
          Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.VacationKindsCheckUp(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.SortFields(_obj);
        }
        _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation2 != null;
      }
    }
    
    public virtual void Vacation3Changed(DirRX.HRManagement.Shared.ShiftStatementApprovalAssignmentVacation3ChangedEventArgs e)
    {
      if (VacationShiftTasks.As(_obj.Task).IsVacationRecall.Value) {
        if (e.NewValue != e.OldValue) {
          Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.VacationKindsCheckUp(_obj);
        }
      } 
      else {
        if (e.NewValue != e.OldValue) {
          Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.VacationKindsCheckUp(_obj);
          Functions.ShiftStatementApprovalAssignment.Remote.SortFields(_obj);
        }
      }
    }
    #endregion
    
    #region обработчики первой строки новых дат отпусков
    
    public virtual void VacationKind1Changed(DirRX.HRManagement.Shared.ShiftStatementApprovalAssignmentVacationKind1ChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue) {
        if (e.NewValue == null) {
          _obj.DataBegin1 = null;
          _obj.Duration1 = null;
        }
        Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
      }
    }
    
    public virtual void DataBegin1Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd1 = Functions.ShiftStatementApprovalAssignment.Remote.FillEndDateAndSetDays(_obj, e.NewValue, _obj.Duration1);
    }
    
    public virtual void Duration1Changed(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd1 = Functions.ShiftStatementApprovalAssignment.Remote.FillEndDateAndSetDays(_obj, _obj.DataBegin1, e.NewValue);
    }

    public virtual void DataEnd1Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue &&
          _obj.VacationKind1 != null && _obj.VacationKind1.Paid.Value &&
          _obj.DataBegin1.HasValue)
      {
        var warnings = new List<string>();
        warnings.AddRange(Functions.ShiftStatementApprovalAssignment.CheckOneVacationDates(_obj, _obj.DataBegin1.Value, e.NewValue.Value));
        e.Params.AddOrUpdate("Warnings", string.Join(" ", warnings));
      }
    }
    #endregion

    #region обработчики второй строки новых дат отпусков

    public virtual void VacationKind2Changed(DirRX.HRManagement.Shared.ShiftStatementApprovalAssignmentVacationKind2ChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue) {
        if (e.NewValue == null) {
          _obj.DataBegin2 = null;
          _obj.Duration2 = null;
        }
        Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
      }
    }
    
    public virtual void DataBegin2Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd2 = Functions.ShiftStatementApprovalAssignment.Remote.FillEndDateAndSetDays(_obj, e.NewValue, _obj.Duration2);
    }
    
    public virtual void Duration2Changed(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd2 = Functions.ShiftStatementApprovalAssignment.Remote.FillEndDateAndSetDays(_obj, _obj.DataBegin2, e.NewValue);
    }

    public virtual void DataEnd2Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue &&
          _obj.VacationKind2 != null && _obj.VacationKind2.Paid.Value &&
          _obj.DataBegin2.HasValue)
      {
        var warnings = new List<string>();
        warnings = Functions.ShiftStatementApprovalAssignment.CheckOneVacationDates(_obj, _obj.DataBegin2.Value, e.NewValue.Value);
        e.Params.AddOrUpdate("Warnings", string.Join(" ", warnings));
      }
    }
    #endregion

    #region обработчики третьей строки новых дат отпусков

    public virtual void VacationKind3Changed(DirRX.HRManagement.Shared.ShiftStatementApprovalAssignmentVacationKind3ChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue) {
        if (e.NewValue == null) {
          _obj.DataBegin3 = null;
          _obj.Duration3 = null;
        }
        Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj);
      }
    }
    
    public virtual void DataBegin3Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd3 = Functions.ShiftStatementApprovalAssignment.Remote.FillEndDateAndSetDays(_obj, e.NewValue, _obj.Duration3);
    }
    
    public virtual void Duration3Changed(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd3 = Functions.ShiftStatementApprovalAssignment.Remote.FillEndDateAndSetDays(_obj, _obj.DataBegin3, e.NewValue);
    }
    
    public virtual void DataEnd3Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue &&
          _obj.VacationKind3 != null && _obj.VacationKind3.Paid.Value &&
          _obj.DataBegin3.HasValue)
      {
        var warnings = new List<string>();
        warnings = Functions.ShiftStatementApprovalAssignment.CheckOneVacationDates(_obj, _obj.DataBegin3.Value, e.NewValue.Value);
        e.Params.AddOrUpdate("Warnings", string.Join(" ", warnings));
      }
    }
    #endregion
    
    public virtual void CompensationChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        Functions.ShiftStatementApprovalAssignment.Remote.SetLeftDays(_obj, e.NewValue.Value);

      // Если установили, что надо компенсировать - почистить недозаполненные строки
      // в противном случае заполнить вид в первой строке, если она пустая
      if (e.NewValue.Value) {
        if (_obj.VacationKind1 != null && (_obj.DataBegin1 == null || _obj.Duration1 == null))
          Functions.ShiftStatementApprovalAssignment.Remote.CleanFirstNewVacationDate(_obj);
        if (_obj.VacationKind2 != null && (_obj.DataBegin2 == null || _obj.Duration2 == null))
          Functions.ShiftStatementApprovalAssignment.Remote.CleanSecondNewVacationDate(_obj);
        if (_obj.VacationKind3 != null && (_obj.DataBegin3 == null || _obj.Duration3 == null))
          Functions.ShiftStatementApprovalAssignment.Remote.CleanThirdNewVacationDate(_obj);
      } 
      else {
        if (_obj.VacationKind1 == null && _obj.Vacation1 != null)
          _obj.VacationKind1 = _obj.Vacation1.VacationKind;
      }
      
      Functions.ShiftStatementApprovalAssignment.SetEnabledProperties(_obj);
    }
    
  }
}