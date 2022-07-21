using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftStatementApprovalAssignment;

namespace DirRX.HRManagement.Shared
{
  partial class ShiftStatementApprovalAssignmentFunctions
  {
    /// <summary>
    /// Изменить видимость строк для ввода дат и предзаполнить вид отпуска если доступен только один.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.ChangeLineVisible().
    /// </summary>
    public void SetVisibleProperties()
    {
      var task = VacationShiftTasks.As(_obj.Task);
      // пропустить функцию, если задача пустая - это может произойти на старте блока
      if (task != null) {
        // отпуска
        _obj.State.Properties.Vacation2.IsVisible = true;
        _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation1 != null;
        _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation1 != null && _obj.Vacation2 != null;
        
        // Если перенос стартован из подтверждения отпуска, Периоды отпуска будут автоматом заполнены из задачи на подтверждение. Пустые строки отображать не нужно.
        var isAlertTaskContext = VacationShiftTasks.As(_obj.Task).ApprovalAssignment != null;
        if (isAlertTaskContext)
        {
          _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation2 != null;
          _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation3 != null;
        }
        
        // Поле для ввода дат появляется только тогда, когда есть дни для распределения отпуска и предыдущая строка заполнена.
        var hasDayToShifted = _obj.VacationDurations.Sum(v => v.DaysPlaned) != _obj.VacationDurations.Sum(v => v.DaysShifted);
        var needShowSecondLine = (hasDayToShifted && _obj.Duration1.HasValue && _obj.DataBegin1.HasValue) || (_obj.Duration2.HasValue || _obj.DataBegin2.HasValue || _obj.VacationKind2 != null);
        _obj.State.Properties.DataBegin2.IsVisible = needShowSecondLine;
        _obj.State.Properties.DataEnd2.IsVisible = needShowSecondLine;
        _obj.State.Properties.Duration2.IsVisible = needShowSecondLine;
        _obj.State.Properties.VacationKind2.IsVisible = needShowSecondLine;
        
        var needShowThirdLine = (hasDayToShifted && _obj.Duration2.HasValue && _obj.DataBegin2.HasValue && _obj.Duration1.HasValue && _obj.DataBegin1.HasValue) || (_obj.Duration3.HasValue || _obj.DataBegin3.HasValue || _obj.VacationKind3 != null);
        _obj.State.Properties.DataBegin3.IsVisible = needShowThirdLine;
        _obj.State.Properties.DataEnd3.IsVisible = needShowThirdLine;
        _obj.State.Properties.Duration3.IsVisible = needShowThirdLine;
        _obj.State.Properties.VacationKind3.IsVisible = needShowThirdLine;
        
        // Изменить видимость и доступность признака Компенсировать/Отменить.
        var isCompVisible = Functions.VacationShiftTask.isCompensationVisible(_obj.Vacation1, _obj.Vacation2, _obj.Vacation3);
        _obj.State.Properties.Compensation.IsVisible = isCompVisible;
        if (isCompVisible)
        {
          var isCompEnabled = Functions.ShiftStatementApprovalAssignment.isCompensationEnable(_obj);
          // _obj.State.Properties.Compensation.IsEnabled = isCompEnabled && _obj.BlockUid != Constants.VacationShiftTask.ApprovingAssignmentBlockID;
          _obj.EnabledCompensationLabel = VacationShiftTasks.Resources.EnabledCompensation;
          _obj.State.Properties.EnabledCompensationLabel.IsVisible = !isCompEnabled;
        }
        else
        {
          _obj.Compensation = false;
          _obj.State.Properties.EnabledCompensationLabel.IsVisible = false;
        }
        
        // Отобразить поля, если запущен Отзыв из отпуска.
        var isRecallContext = VacationShiftTasks.As(_obj.Task).IsVacationRecall.Value;
        _obj.State.Properties.RecallDate.IsVisible = isRecallContext;
        _obj.State.Properties.RecallDate.IsRequired = isRecallContext;
        _obj.State.Properties.Employee.IsVisible = isRecallContext;
        
        // Показать галочку "Игнорировать проверку на 14 дней":
        //   если это блок согласования с руководителем и ранее была обнаружена проблема с 14 днями
        if (_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId)
          _obj.State.Properties.IgnoreRequiredDuration.IsVisible = _obj.IsRequiredDurationBroken.HasValue && _obj.IsRequiredDurationBroken.Value;
        // если блок  доработки сотрудником - показать галочку, если она была уже установлена
        if (_obj.BlockUid == Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId)
          _obj.State.Properties.IgnoreRequiredDuration.IsVisible = _obj.IgnoreRequiredDuration.HasValue && _obj.IgnoreRequiredDuration.Value;
        
      }
    }
    
    /// <summary>
    /// Получить доступность признака Компенсировать/Отменить. Он доступен, если есть отпуска "Оплачиваемые необязательные" или "Неоплачиваемые необязательные"
    ///   и распределены все "Оплачиваемые обязательные" отпуска.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.isCompensationEnable().
    /// </summary>
    /// <returns>True, если признак Компенсировать/Отменить доступен.</returns>
    public bool isCompensationEnable()
    {
      var paidKind = Functions.Module.Remote.GetPaidVacationKind(_obj.Employee);
      var allReqDay = true;
      var vacPaidDuration = _obj.VacationDurations.Where(v => Equals(v.VacationKind, paidKind)).FirstOrDefault();
      if (vacPaidDuration != null)
        allReqDay = vacPaidDuration.DaysPlaned == vacPaidDuration.DaysShifted;

      foreach (var vacDuration in _obj.VacationDurations.Where(v => !Equals(v.VacationKind, paidKind)))
      {
        if (vacDuration.VacationKind.Required.Value && vacDuration.VacationKind.Paid.Value)
          return (vacDuration.DaysPlaned - vacDuration.DaysShifted <= vacDuration.DaysAvailComp) && allReqDay;
        else
          return true && allReqDay;
      }
      return false;
    }
    
    /// <summary>
    /// Установить доступность свойств карточки в зависимости от
    ///    - контекста старта задачи
    ///    - текущего блока (Согласование vs. Доработка и Руководитель vs. сотрудник)
    ///    - текущего состояния свойств.
    /// </summary>
    /// <remarks>
    /// Сотрудника нельзя менять в принципе - он задан при старте задачи.
    /// На согласовании менять нельзя ничего в принципе за исключением галочки "Компенсировать/отменить", если текущий исполнитель - руководитель.
    /// На доработке приниципы ограничения полей такие же, как и при старте задачи. С учетом того, что вместо автора задачи анализируется текущий исполнитель.
    /// </remarks>
    public virtual void SetEnabledProperties()
    {
      // Если задача пустая, то ничего не делать.
      // Связано с тем, что Compensation заполняется в Created(), а в это время еще задача не заполнена
      if (_obj.Task != null) {
        // == Контекст старта задачи
        var task = VacationShiftTasks.As(_obj.Task);
        // Это отзыв?
        var isRecallContext = task.IsVacationRecall.Value;
        // Перенос из задачи на подтверждение?
        var isAlertTaskContext = task.ApprovalAssignment != null;
        
        // == контекст блока
        // true - этап "Доработка", false - этап "Согласование"
        var isRework = _obj.BlockUid == Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId;
        
        // == Текущее заполнение свойств
        // Компенсация/отмену не установили?
        var isCompensationEmpty = !_obj.Compensation.Value;
        // Есть что распределять?
        var isVacation1NotEmpty = _obj.Vacation1 != null;
        
        _obj.State.Properties.Employee.IsEnabled   = false;
        _obj.State.Properties.RecallDate.IsEnabled = isRework && isCompensationEmpty && isRecallContext;
        
        _obj.State.Properties.Vacation1.IsEnabled = isRework && isCompensationEmpty && !isAlertTaskContext && !isRecallContext;
        _obj.State.Properties.Vacation2.IsEnabled = isRework && isCompensationEmpty && !isAlertTaskContext;
        _obj.State.Properties.Vacation3.IsEnabled = isRework && isCompensationEmpty && !isAlertTaskContext;
        
        _obj.State.Properties.VacationKind1.IsEnabled = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.DataBegin1.IsEnabled    = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.Duration1.IsEnabled     = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.VacationKind2.IsEnabled = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.DataBegin2.IsEnabled    = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.Duration2.IsEnabled     = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.VacationKind3.IsEnabled = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.DataBegin3.IsEnabled    = isRework && isCompensationEmpty && isVacation1NotEmpty;
        _obj.State.Properties.Duration3.IsEnabled     = isRework && isCompensationEmpty && isVacation1NotEmpty;
        
        _obj.State.Properties.Compensation.IsEnabled  = isRework;
        
        // галочку "Игнорировать..." можноменять только на этапе согласования с руководителем.
        _obj.State.Properties.IgnoreRequiredDuration.IsEnabled = !isRework;
      }
    }

    /// <summary>
    /// Проверить правильность указанной даты отпуска.
    /// </summary>
    /// <param name="beginDate">Дата начала отпуска.</param>
    /// <param name="endDate">Дата конца отпуска.</param>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public]
    public virtual List<string> CheckOneVacationDates(DateTime beginDate, DateTime endDate)
    {
      var vacationDate = Functions.Module.Remote.CreateVacationDateStructure(beginDate, endDate);
      
      var warningList = Functions.ShiftStatementApprovalAssignment.Remote.CheckNotCriticalVacationsDates(_obj, vacationDate);
      return warningList;
    }
  }
}