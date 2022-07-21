using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationShiftTask;

namespace DirRX.HRManagement
{

  partial class VacationShiftTaskSharedHandlers
  {
    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.VacationShiftTaskEmployeeChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue) {
        var employeeName = e.NewValue != null ? e.NewValue.Name : string.Empty;
        if (_obj.IsVacationRecall == true)
          _obj.Subject = VacationShiftTasks.Resources.VacationRecallTaskSubjectFormat(employeeName);
        else
          _obj.Subject = VacationShiftTasks.Resources.VacationShiftTaskSubjectFormat(employeeName);

        if (e.NewValue == null) {
          // сбросить все предупреждения, т.к. всё равно из-за очистки сотрудника все проверки не имеют смысла
          e.Params.Remove("Errors");
          e.Params.Remove("Warnings");
          // сбросить флаг, что надо показать галочку "Игнорировать..."
          _obj.IsRequiredDurationBroken = false;
          // сбросить галочку "Игнорировать..."
          _obj.IgnoreRequiredDuration = false;
        }
        // при изменении сотрудника запустить цепочку очистки полей карточки
        _obj.RecallDate = null;
      }
    }

    public virtual void RecallDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue != null) {
          // сбросить флаг, что надо показать галочку "Игнорировать..."
          _obj.IsRequiredDurationBroken = false;
          // сбросить галочку "Игнорировать..."
          _obj.IgnoreRequiredDuration = false;
          // найти часть отпуска, которая "окружает" дату отзыва
          _obj.Vacation1 = Functions.Vacation.Remote.GetRecallVacations(_obj.Employee, _obj.RecallDate).FirstOrDefault();
          // если хоть по одному виду отпуска получилось так, что запланированных дней стало меньше
          // чем перненесенных+компенсированных+отмененных, то почистить все новые даты отпуска
          foreach (var vd in _obj.VacationDurations) {
            if (vd.DaysPlaned < (vd.DaysShifted + vd.DaysCanceled + vd.DaysComped)) {
              Functions.VacationShiftTask.Remote.CleanFirstNewVacationDate(_obj);
              Functions.VacationShiftTask.Remote.CleanSecondNewVacationDate(_obj);
              Functions.VacationShiftTask.Remote.CleanThirdNewVacationDate(_obj);
              if (_obj.VacationKind1 == null && _obj.Vacation1 != null)
                _obj.VacationKind1 = _obj.Vacation1.VacationKind;
              break;
            }
          }
        } 
        else {
          // сбросить все предупреждения, т.к. всё равно без даты отзыва все проверки не имеют смысла
          e.Params.Remove("Errors");
          e.Params.Remove("Warnings");
          // сбросить флаг, что надо показать галочку "Игнорировать..."
          _obj.IsRequiredDurationBroken = false;
          // сбросить галочку "Игнорировать..."
          _obj.IgnoreRequiredDuration = false;
          // если дату отзыва опустошили - почистить первый отпуск и запустить цепочку,
          // которая приведет к чистке всего остального
          _obj.Vacation1 = null;
        }
      }
    }
    
    #region Обработчики Vacation1, Vacation2, Vacation3

    public virtual void Vacation1Changed(DirRX.HRManagement.Shared.VacationShiftTaskVacation1ChangedEventArgs e)
    {
      // поведение зависит от того, отзыв у нас или перенос
      if (_obj.IsVacationRecall.Value) {
        string errorType = string.Empty;
        string requiredDurationError = string.Empty;
        bool isRequiredDurationBroken = false;
        if (e.NewValue != e.OldValue) {
          // если изменилась первая часть отпуска, то и вторая изменится. Поэтому почистим её.
          _obj.Vacation2 = null;
          Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
          Functions.VacationShiftTask.Remote.VacationKindsCheckUp(_obj);
          _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation1 != null;
        } 
        else {
          // пересчитать дни, которые осталось перераспределить
          // нужно для ситуации, когда чуть подвинули дату отзыва и первая часть отпуска не изменилась.
          // например, отпуск с 1.4 по 10.4, а дату отзывы поменяли с 5.4 на 7.4
          Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
        }
        if (e.NewValue != null) {
          // если отправляют задачу по себе, то проверка предупреждающая, т.к. сам сотрудник всё равно не может себе разрешить нарушение правила
          var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
          if (isFromEssAccount || (!isFromEssAccount) && Equals(Employees.Current, _obj.Employee)) {
            errorType = "Warnings";
            requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationWarning;
          } 
          else {
            errorType = "Errors";
            requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationError;
          }
          isRequiredDurationBroken = Functions.VacationShiftTask.Remote.CheckRequiredDurationonVacation1(_obj);

          if (_obj.VacationKind1 == null)
            _obj.VacationKind1 = _obj.Vacation1.VacationKind;
        }
        if (isRequiredDurationBroken) {
          // вывести сообщение об ошибке
          e.Params.AddOrUpdate(errorType, string.Join(" ", requiredDurationError));
          // установить флаг, что найдено нарушение правила 14-ти дней
          _obj.IsRequiredDurationBroken = true;
        }
        else {
          // убрать соощение, если оно было ранее
          e.Params.Remove(errorType);
          // сбросить флаг, что надо показать галочку "Игнорировать..."
          _obj.IsRequiredDurationBroken = false;
          // сбросить галочку "Игнорировать..."
          _obj.IgnoreRequiredDuration = false;
        }
      } 
      else {
        if (e.NewValue != e.OldValue) {
          Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
          Functions.VacationShiftTask.Remote.VacationKindsCheckUp(_obj);
          Functions.VacationShiftTask.Remote.SortFields(_obj);
          
          if (e.NewValue != null)
            if (_obj.VacationKind1 == null)
              _obj.VacationKind1 = _obj.Vacation1.VacationKind;
        }
        _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation1 != null;
        _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation2 != null;
      }
      Functions.VacationShiftTask.SetEnabledProperties(_obj);
      Functions.VacationShiftTask.SetVisibleProperties(_obj);
    }

    public virtual void Vacation2Changed(DirRX.HRManagement.Shared.VacationShiftTaskVacation2ChangedEventArgs e)
    {
      if (_obj.IsVacationRecall.Value) {
        if (e.NewValue != e.OldValue) {
          _obj.Vacation3 = null;
          Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
          Functions.VacationShiftTask.Remote.VacationKindsCheckUp(_obj);
          _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation2 != null;
        }
      } 
      else {
        if (e.NewValue != e.OldValue) {
          Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
          Functions.VacationShiftTask.Remote.VacationKindsCheckUp(_obj);
          Functions.VacationShiftTask.Remote.SortFields(_obj);
        }
        _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation2 != null;
      }
    }
    
    public virtual void Vacation3Changed(DirRX.HRManagement.Shared.VacationShiftTaskVacation3ChangedEventArgs e)
    {
      if (_obj.IsVacationRecall.Value) {
        if (e.NewValue != e.OldValue) {
          Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
          Functions.VacationShiftTask.Remote.VacationKindsCheckUp(_obj);
        }
      } 
      else {
        if (e.NewValue != e.OldValue) {
          Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
          Functions.VacationShiftTask.Remote.VacationKindsCheckUp(_obj);
          Functions.VacationShiftTask.Remote.SortFields(_obj);
        }
      }
    }
    #endregion

    #region обработчики первой строки новых дат отпусков
    public virtual void VacationKind1Changed(DirRX.HRManagement.Shared.VacationShiftTaskVacationKind1ChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue) {
        if (e.NewValue == null) {
          _obj.DataBegin1 = null;
          _obj.Duration2 = null;
        }
        Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
      }
    }
    
    public virtual void DataBegin1Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd1 = Functions.VacationShiftTask.Remote.FillEndDateAndSetDays(_obj, e.NewValue, _obj.Duration1);
    }

    public virtual void Duration1Changed(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd1 = Functions.VacationShiftTask.Remote.FillEndDateAndSetDays(_obj, _obj.DataBegin1, e.NewValue);
    }

    public virtual void DataEnd1Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      
      if (e.NewValue.HasValue && e.NewValue != e.OldValue &&
          _obj.VacationKind1 != null && _obj.VacationKind1.Paid.Value &&
          _obj.DataBegin1.HasValue)
      {
        var warnings = new List<string>();
        warnings.AddRange(Functions.VacationShiftTask.CheckOneVacationDates(_obj, _obj.DataBegin1.Value, e.NewValue.Value));
        e.Params.AddOrUpdate("Warnings", string.Join(" ", warnings));
      }
      
    }
    #endregion

    #region обработчики второй строки новых дат отпусков
    public virtual void VacationKind2Changed(DirRX.HRManagement.Shared.VacationShiftTaskVacationKind2ChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue) {
        if (e.NewValue == null) {
          _obj.DataBegin2 = null;
          _obj.Duration2 = null;
        }
        Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
      }
    }

    public virtual void DataBegin2Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd2 = Functions.VacationShiftTask.Remote.FillEndDateAndSetDays(_obj, e.NewValue, _obj.Duration2);
    }

    public virtual void Duration2Changed(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd2 = Functions.VacationShiftTask.Remote.FillEndDateAndSetDays(_obj, _obj.DataBegin2, e.NewValue);
    }

    public virtual void DataEnd2Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      
      if (e.NewValue.HasValue && e.NewValue != e.OldValue &&
          _obj.VacationKind2 != null && _obj.VacationKind2.Paid.Value &&
          _obj.DataBegin2.HasValue)
      {
        var warnings = new List<string>();
        warnings = Functions.VacationShiftTask.CheckOneVacationDates(_obj, _obj.DataBegin2.Value, e.NewValue.Value);
        e.Params.AddOrUpdate("Warnings", string.Join(" ", warnings));
      }
    }
    
    #endregion
    
    #region обработчики третьей строки новых дат отпусков
    public virtual void VacationKind3Changed(DirRX.HRManagement.Shared.VacationShiftTaskVacationKind3ChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue) {
        if (e.NewValue == null) {
          _obj.DataBegin3 = null;
          _obj.Duration3 = null;
        }
        Functions.VacationShiftTask.Remote.SetLeftDays(_obj);
      }
    }

    public virtual void DataBegin3Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd3 = Functions.VacationShiftTask.Remote.FillEndDateAndSetDays(_obj, e.NewValue, _obj.Duration3);
    }

    public virtual void Duration3Changed(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.DataEnd3 = Functions.VacationShiftTask.Remote.FillEndDateAndSetDays(_obj, _obj.DataBegin3, e.NewValue);
    }

    public virtual void DataEnd3Changed(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue &&
          _obj.VacationKind3 != null && _obj.VacationKind3.Paid.Value &&
          _obj.DataBegin3.HasValue)
      {
        var warnings = new List<string>();
        warnings = Functions.VacationShiftTask.CheckOneVacationDates(_obj, _obj.DataBegin3.Value, e.NewValue.Value);
        e.Params.AddOrUpdate("Warnings", string.Join(" ", warnings));
      }
    }
    #endregion

    public virtual void CompensationChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        Functions.VacationShiftTask.Remote.SetLeftDays(_obj, e.NewValue.Value);

      // Если установили, что надо компенсировать - почистить недозаполненные строки
      // в противном случае заполнить вид в первой строке, если она пустая
      if (e.NewValue.Value) {
        if (_obj.VacationKind1 != null && (_obj.DataBegin1 == null || _obj.Duration1 == null))
          Functions.VacationShiftTask.Remote.CleanFirstNewVacationDate(_obj);
        if (_obj.VacationKind2 != null && (_obj.DataBegin2 == null || _obj.Duration2 == null))
          Functions.VacationShiftTask.Remote.CleanSecondNewVacationDate(_obj);
        if (_obj.VacationKind3 != null && (_obj.DataBegin3 == null || _obj.Duration3 == null))
          Functions.VacationShiftTask.Remote.CleanThirdNewVacationDate(_obj);
      } 
      else {
        if (_obj.VacationKind1 == null && _obj.Vacation1 != null)
          _obj.VacationKind1 = _obj.Vacation1.VacationKind;
      }
      
      Functions.VacationShiftTask.SetEnabledProperties(_obj);
    }

    public virtual void IsVacationRecallChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      var employeeName = _obj.Employee != null ? _obj.Employee.Name : string.Empty;
      if (e.NewValue.Value)
      {
        _obj.Subject = VacationShiftTasks.Resources.VacationRecallTaskSubjectFormat(employeeName);
        _obj.ActiveText = VacationShiftTasks.Resources.VacationRecallTaskActiveText;
      }
      else
      {
        _obj.Subject = VacationShiftTasks.Resources.VacationShiftTaskSubjectFormat(employeeName);
        _obj.ActiveText = VacationShiftTasks.Resources.VacationShiftTaskActiveText;
      }
    }
    
    public virtual void OrderChanged(DirRX.HRManagement.Shared.VacationShiftTaskOrderChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
        _obj.OrderAttachmentGroup.HROrders.Add(e.NewValue);
    }
    
    public virtual void StatementChanged(DirRX.HRManagement.Shared.VacationShiftTaskStatementChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        _obj.StatementAttachmentGroup.HRDocumentBases.Clear();
        _obj.StatementAttachmentGroup.HRDocumentBases.Add(e.NewValue);
      }
      if (e.NewValue == null)
        _obj.StatementAttachmentGroup.HRDocumentBases.Clear();
    }

  }
}