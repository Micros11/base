using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationShiftTask;

namespace DirRX.HRManagement.Shared
{
  partial class VacationShiftTaskFunctions
  {
    /// <summary>
    /// Изменить видимость строк для ввода дат, признака Компенсировать/отменить и предзаполнить вид отпуска, если доступен только один.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.SetVisibleProperties().
    /// </summary>
    public void SetVisibleProperties()
    {
      // скрывать поле с частью отпуска, если предыдущая незаполнена
      _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation1 != null;
      _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation1 != null && _obj.Vacation2 != null;

      // Если перенос стартован из подтверждения отпуска, Периоды отпуска будут автоматом заполнены из задачи на подтверждение. Пустые строки отображать не нужно.
      var isAlertTaskContext = _obj.ApprovalAssignment != null;
      if (isAlertTaskContext)
      {
        _obj.State.Properties.Vacation2.IsVisible = _obj.Vacation2 != null;
        _obj.State.Properties.Vacation3.IsVisible = _obj.Vacation3 != null;
      }
      
      // Отобразить поля, если запущен Отзыв из отпуска.
      var isRecallContext = _obj.IsVacationRecall.Value;
      _obj.State.Properties.Employee.IsVisible = isRecallContext;
      _obj.State.Properties.RecallDate.IsVisible = isRecallContext; 
      _obj.State.Properties.RecallDate.IsRequired = isRecallContext;
      
      // Показать области вложений, если не новая задача      
      _obj.State.Attachments.EmployeeAttachmentGroup.IsVisible = !_obj.State.IsInserted;
      _obj.State.Attachments.OrderAttachmentGroup.IsVisible = !_obj.State.IsInserted;
      _obj.State.Attachments.StatementAttachmentGroup.IsVisible = !_obj.State.IsInserted;
      
      // Поле для ввода дат появляется только когда есть дни для распределения отпуска и предыдущая строка заполнена.
      var hasDayToShifted = _obj.VacationDurations.Sum(v => v.DaysPlaned) > _obj.VacationDurations.Sum(v => v.DaysShifted) + _obj.VacationDurations.Sum(v => v.DaysCanceled) + _obj.VacationDurations.Sum(v => v.DaysComped);
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
        var isCompEnabled = Functions.VacationShiftTask.isCompensationEnable(_obj);
        _obj.State.Properties.Compensation.IsEnabled = isCompEnabled;
        _obj.EnabledCompensationLabel = VacationShiftTasks.Resources.EnabledCompensation; 
        _obj.State.Properties.EnabledCompensationLabel.IsVisible = !isCompEnabled;
      }
      else 
      {
        _obj.Compensation = false;
        _obj.State.Properties.EnabledCompensationLabel.IsVisible = false;
      }      
      
      // Показать галочку "Игнорировать проверку на 14 дней", 
      // если ранее была обнаружено нарушение правила 14-ти дней
      // и если сотрудник в задаче не равен текущему
      var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
      _obj.State.Properties.IgnoreRequiredDuration.IsVisible = _obj.IsRequiredDurationBroken.HasValue && _obj.IsRequiredDurationBroken.Value && _obj.Employee != null &&
                                                               !isFromEssAccount && !Equals(Employees.Current, _obj.Employee);
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
      
      var warningList = Functions.VacationShiftTask.Remote.CheckNotCriticalVacationsDates(_obj, vacationDate);
      return warningList;
    }
    
    /// <summary>
    /// Получить видимость признака Компенсировать/Отменить. Он виден, если есть отпуска "Оплачиваемые необязательные" или "Неоплачиваемые необязательные".
    /// </summary>
    /// <param name="vacation1">Первый период отпуска.</param>
    /// <param name="vacation2">Второй период отпуска.</param>
    /// <param name="vacation3">Третий период отпуска.</param>
    /// <returns>True, если признак Компенсировать/Отменить доступен.</returns>
    public static bool isCompensationVisible(IVacation vacation1, IVacation vacation2, IVacation vacation3)
    {
      return (vacation1 != null && ((!vacation1.VacationKind.Required.Value && vacation1.VacationKind.Paid.Value) || !vacation1.VacationKind.Paid.Value)) || 
             (vacation2 != null && ((!vacation2.VacationKind.Required.Value && vacation2.VacationKind.Paid.Value) || !vacation2.VacationKind.Paid.Value)) || 
             (vacation3 != null && ((!vacation3.VacationKind.Required.Value && vacation3.VacationKind.Paid.Value) || !vacation3.VacationKind.Paid.Value));
    }
        
    /// <summary>
    /// Получить доступность признака Компенсировать/Отменить. Он доступен, если есть отпуска "Оплачиваемые необязательные" или "Неоплачиваемые необязательные"
    ///   и распределены все "Оплачиваемые обязательные" отпуска.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.isCompensationEnable().
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
    /// Установить доступность свойств карточки в зависимости от контекста старта задачи и текущего состояния полей.
    /// Если по контексту старта задачи свойство недоступно для изменения - это имеет приоритет над всеми остальными зависимостями.
    /// </summary>
    /// <remarks>
    /// Контексты:
    /// 1. Отзыв или перенос?. 
    ///    - Если отзыв, то поле Vacation1 менять нельзя, т.к. будет заполняться автоматически на основе даты отзыва
    ///    - если перенос, то поле "Сотрудник" менять нельзя, т.к. оно вообще не будет видно на карточке
    /// 2. Просто перенос или из задачи на подтверждение?
    ///    - если из задачи на подтверждение, то нельзя менять части отпуска (Vacation1, Vacation2, Vacation3), т.к. они будут заполнены автоматом из задачи на подтверждение.
    /// 3. Автор задачи на отзыв. 
    ///    - поле "Сотрудник" доступно для изменения, если стартовал руководитель (входит в роль "Руководители подразделений").
    /// </remarks>
    /// <remarks>
    /// От каких полей зависит:
    /// 1. "Компенсировать/отменить". 
    ///   - Если это свойство заполнено (true), то все остальные свойства менять нельзя. Это имеет приоритет над остальными зависямостями от состояния свойств
    /// 2. Первая часть отпуска (Vacation1)
    ///   - Если пустое - то "строки" для новых частей отпуска недоступны для изменения, т.к. заполнять их нечем - нечего еще распределять.
    /// </remarks>
    public virtual void SetEnabledProperties()
    {            
      // == Контекст старта задачи
      // true - отзыв, false - перенос
      var isRecallContext = _obj.IsVacationRecall.Value;
      // true - перенос из задачи на подтверждение
      var isAlertTaskContext = _obj.ApprovalAssignment != null;
      // true - автор задачи - руководитель, false - автор задачи - сотрудник
      var isAuthorDepartmentManager = _obj.Author.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.DepartmentManagersRole);

      // == Текущее заполнение свойств
      // Компенсация/отмену не установили?
      var isCompensationEmpty = !_obj.Compensation.Value;      
      // Есть что распределять?
      var isVacation1NotEmpty = _obj.Vacation1 != null; 
      
      _obj.State.Properties.Employee.IsEnabled   = isCompensationEmpty && isRecallContext && isAuthorDepartmentManager;
      _obj.State.Properties.RecallDate.IsEnabled = isCompensationEmpty && isRecallContext;

      _obj.State.Properties.Vacation1.IsEnabled = isCompensationEmpty && !isAlertTaskContext && !isRecallContext;
      _obj.State.Properties.Vacation2.IsEnabled = isCompensationEmpty && !isAlertTaskContext;
      _obj.State.Properties.Vacation3.IsEnabled = isCompensationEmpty && !isAlertTaskContext;

      _obj.State.Properties.VacationKind1.IsEnabled = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.DataBegin1.IsEnabled    = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.Duration1.IsEnabled     = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.VacationKind2.IsEnabled = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.DataBegin2.IsEnabled    = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.Duration2.IsEnabled     = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.VacationKind3.IsEnabled = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.DataBegin3.IsEnabled    = isCompensationEmpty && isVacation1NotEmpty;
      _obj.State.Properties.Duration3.IsEnabled     = isCompensationEmpty && isVacation1NotEmpty;
    }
    
  }
}