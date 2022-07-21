using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftStatementApprovalAssignment;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement
{

  #region фильтрация доступных отпусков
  partial class ShiftStatementApprovalAssignmentVacation1PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> Vacation1Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Получить отпуска, которые потенциально доступны для изменения в рамках задачи
      var result = Functions.ShiftStatementApprovalAssignment.VacationShiftFilter(_obj, _obj.Vacation1);
      // Отбросить отпуска, которые уже выбраны в других полях
      result = result.Where(v => !Equals(v, _obj.Vacation2) && !Equals(v, _obj.Vacation3));
      return (IQueryable<T>)result;
    }
  }
  
  partial class ShiftStatementApprovalAssignmentVacation2PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> Vacation2Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Получить отпуска, которые потенциально доступны для изменения в рамках задачи
      var result = Functions.ShiftStatementApprovalAssignment.VacationShiftFilter(_obj, _obj.Vacation2);
      // Отбросить отпуска, которые уже выбраны в других полях
      result = result.Where(v => !Equals(v, _obj.Vacation1) && !Equals(v, _obj.Vacation3));
      // При отзыве в Vacation2 будут доступны только те отуска, которые начинаются на следующий рабочий день после завершения отпуска из Vacation1
      if (VacationShiftTasks.As(_obj.Task).IsVacationRecall.Value) {
        result = result.Where(v => v.StartDate == Calendar.NextWorkingDay(_obj.Vacation1.FinDate.Value));
      }
      return (IQueryable<T>)result;
    }
  }

  partial class ShiftStatementApprovalAssignmentVacation3PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> Vacation3Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Получить отпуска, которые потенциально доступны для изменения в рамках задачи
      var result = Functions.ShiftStatementApprovalAssignment.VacationShiftFilter(_obj, _obj.Vacation3);
      // Отбросить отпуска, которые уже выбраны в других полях
      result = result.Where(v => !Equals(v, _obj.Vacation1) && !Equals(v, _obj.Vacation2));
      // При отзыве в Vacation3 будут доступны только те отуска, которые начинаются на следующий рабочий день после завершения отпуска из Vacation2
      if (VacationShiftTasks.As(_obj.Task).IsVacationRecall.Value) {
        result = result.Where(v => v.StartDate == Calendar.NextWorkingDay(_obj.Vacation2.FinDate.Value));
      }
      return (IQueryable<T>)result;
    }
  }
  #endregion
  
  #region фильтрация видов отпусков
  partial class ShiftStatementApprovalAssignmentVacationKind1PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> VacationKind1Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Functions.ShiftStatementApprovalAssignment.VacationKindFilter(_obj, _obj.VacationKind1);
    }
  }

  partial class ShiftStatementApprovalAssignmentVacationKind2PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> VacationKind2Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Functions.ShiftStatementApprovalAssignment.VacationKindFilter(_obj, _obj.VacationKind2);
    }
  }

  partial class ShiftStatementApprovalAssignmentVacationKind3PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> VacationKind3Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Functions.ShiftStatementApprovalAssignment.VacationKindFilter(_obj, _obj.VacationKind3);
    }
  }
  #endregion

  partial class ShiftStatementApprovalAssignmentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Compensation = false;
      _obj.IgnoreRequiredDuration = false;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Functions.ShiftStatementApprovalAssignment.SortFields(_obj);
    }
    
    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      var errorList = new List<string>();
      var warningList = new List<string>();
      
      // Запустить проверки, если возвращают на согласование с доработки или согласовывают.
      if ((_obj.BlockUid == Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId &&
            Equals(_obj.Result.Value, ShiftStatementApprovalAssignment.Result.ForReapproving))
          || ((_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId) &&
              Equals(_obj.Result.Value, ShiftStatementApprovalAssignment.Result.Approve))) {
        
        // надо выбрать хотя бы один отпуск для изменения
        if (_obj.Vacation1 == null)
          errorList.Add(VacationShiftTasks.Resources.NeedSelectVacationForShift);
        
        // в новых частях отпуска должны быть заполнены либо все поля, либо ни одного
        if ((_obj.DataBegin1 != null || _obj.Duration1 != null || _obj.VacationKind1 != null) &&
            (_obj.DataBegin1 == null || _obj.Duration1 == null || _obj.VacationKind1 == null))
          errorList.Add(VacationShiftTasks.Resources.EnterNewDates1);
        if ((_obj.DataBegin2 != null || _obj.Duration2 != null || _obj.VacationKind2 != null) &&
            (_obj.DataBegin2 == null || _obj.Duration2 == null || _obj.VacationKind2 == null))
          errorList.Add(VacationShiftTasks.Resources.EnterNewDates2);
        if ((_obj.DataBegin3 != null || _obj.Duration3 != null || _obj.VacationKind3 != null) &&
            (_obj.DataBegin3 == null || _obj.Duration3 == null || _obj.VacationKind3 == null))
          errorList.Add(VacationShiftTasks.Resources.EnterNewDates3);

        // Проверить корректность введенных дат.
        foreach (var errMsg in Functions.ShiftStatementApprovalAssignment.CheckAllVacationDates(_obj))
          errorList.Add(errMsg);

        // Если речь идет об Основном отпуске и не установлена галочка "игнорировать проверку на 14 дней", выполнить эту проверку
        var mainPaidVacationKind = Functions.Module.GetPaidVacationKind(_obj.Employee);
        if (((_obj.Vacation1 != null && Equals(_obj.Vacation1.VacationKind, mainPaidVacationKind)) ||
             (_obj.Vacation2 != null && Equals(_obj.Vacation2.VacationKind, mainPaidVacationKind)) ||
             (_obj.Vacation3 != null && Equals(_obj.Vacation3.VacationKind, mainPaidVacationKind))) &&
            !_obj.IgnoreRequiredDuration.Value)
        {
          // Если при задании первой порции отпуска не было найдено нарушение правила 14-ти дней
          // это означает, что могли задать отпуска не нарушая правила, но не сделали этого
          // В этом случае полноценная проверка на 14 дней всегда критична, вне зависимости от того
          // сотрудник отправляет на согласование или руководитель отправляет на доработку
          if (!_obj.IsRequiredDurationBroken.Value) {
            var requiredDurationError = string.Empty;
            requiredDurationError = Functions.ShiftStatementApprovalAssignment.CheckRequiredDuration(_obj);
            if (requiredDurationError != string.Empty)
              errorList.Add(requiredDurationError);
          } 
          else {
            // а вот если правило 14-ти дней нарушено уже при выборе первой порции (точнее - при заполнении даты отзыва)
            // то для согласования руководителем - это критично, а если сотрудник отправляет на доработку - нет
            if (_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId && Equals(_obj.Result.Value, ShiftStatementApprovalAssignment.Result.Approve))
              errorList.Add(VacationShiftTasks.Resources.VacationRequiredDurationError);
            else
              warningList.Add(VacationShiftTasks.Resources.VacationRequiredDurationWarning);
          }
        }
      }

      if (errorList.Any())
      {
        foreach (var errorMsg in errorList)
          e.AddError(errorMsg);
        // предпреждения стоит выводить только если были критичные ошибки - иначе всё равно не увидят
        foreach (var warningMsg in warningList)
          e.AddWarning(warningMsg);
        // при наличии критичных ошибок выполнять дальше не имеет смысла
        return;
      }
    }
  }
}