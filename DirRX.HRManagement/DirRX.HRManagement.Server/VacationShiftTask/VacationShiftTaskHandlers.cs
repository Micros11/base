using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationShiftTask;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement
{
  partial class VacationShiftTaskEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> EmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // TODO тут бы добавить обработку замещения, чтобы доставать не только по своему департаменту, но и по замещениям. Но надо делать везде поддержку замещений.
      // Надо достать сотрудников тех подразделений, у которых руководителем является текущий сотрудник.
      var departments = Sungero.Company.Departments.GetAll().Where(d => d.Manager == Employees.Current);
      return query.Where(v => departments.Any(d => d.Equals(v.Department)) || v == Employees.Current);
    }
  }

  #region фильтрация доступных отпусков
  partial class VacationShiftTaskVacation1PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> Vacation1Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Получить отпуска, которые потенциально доступны для изменения в рамках задачи
      var result = Functions.VacationShiftTask.VacationShiftFilter(_obj, _obj.Vacation1);
      // Отбросить отпуска, которые уже выбраны в других полях
      result = result.Where(v => !Equals(v, _obj.Vacation2) && !Equals(v, _obj.Vacation3));
      return (IQueryable<T>)result;
    }
  }
  
  partial class VacationShiftTaskVacation2PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> Vacation2Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Получить отпуска, которые потенциально доступны для изменения в рамках задачи
      var result = Functions.VacationShiftTask.VacationShiftFilter(_obj, _obj.Vacation2);
      // Отбросить отпуска, которые уже выбраны в других полях
      result = result.Where(v => !Equals(v, _obj.Vacation1) && !Equals(v, _obj.Vacation3));
      // При отзыве в Vacation2 будут доступны только те отуска, которые начинаются на следующий рабочий день после завершения отпуска из Vacation1
      if (_obj.IsVacationRecall.Value) {
        result = result.Where(v => v.StartDate == Calendar.NextWorkingDay(_obj.Vacation1.FinDate.Value));
      }
      return (IQueryable<T>)result;
    }
  }
  
  partial class VacationShiftTaskVacation3PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> Vacation3Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Получить отпуска, которые потенциально доступны для изменения в рамках задачи
      var result = Functions.VacationShiftTask.VacationShiftFilter(_obj, _obj.Vacation3);
      // Отбросить отпуска, которые уже выбраны в других полях
      result = result.Where(v => !Equals(v, _obj.Vacation1) && !Equals(v, _obj.Vacation2));
      // При отзыве в Vacation3 будут доступны только те отуска, которые начинаются на следующий рабочий день после завершения отпуска из Vacation2
      if (_obj.IsVacationRecall.Value) {
        result = result.Where(v => v.StartDate == Calendar.NextWorkingDay(_obj.Vacation2.FinDate.Value));
      }
      return (IQueryable<T>)result;
    }
  }
  #endregion

  #region фильтрация видов отпусков
  partial class VacationShiftTaskVacationKind1PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> VacationKind1Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Functions.VacationShiftTask.VacationKindFilter(_obj, _obj.VacationKind1);
    }
  }

  partial class VacationShiftTaskVacationKind2PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> VacationKind2Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Functions.VacationShiftTask.VacationKindFilter(_obj, _obj.VacationKind2);
    }
  }
  
  partial class VacationShiftTaskVacationKind3PropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> VacationKind3Filtering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Functions.VacationShiftTask.VacationKindFilter(_obj, _obj.VacationKind3);
    }
  }
  #endregion

  partial class VacationShiftTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Employee = Sungero.Company.Employees.As(_obj.Author);
      _obj.Year = Calendar.SqlMinValue;
      _obj.IsVacationRecall = false;
      _obj.Compensation = false;
      _obj.IgnoreRequiredDuration = false;
      _obj.IsRequiredDurationBroken = false;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Functions.VacationShiftTask.SortFields(_obj);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      _obj.EmployeeAttachmentGroup.Employees.Clear();
      _obj.EmployeeAttachmentGroup.Employees.Add(_obj.Employee);

      var errorList = new List<string>();
      var warningList = new List<string>();
      
      // Надо выбрать хотя бы один отпуск для изменения
      if (_obj.Vacation1 == null)
        errorList.Add(VacationShiftTasks.Resources.NeedSelectVacationForShift);
      
      // В новых частях отпуска должны быть заполнены либо все поля, либо ни одного
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
      foreach (var errMsg in Functions.VacationShiftTask.CheckAllVacationDates(_obj))
        errorList.Add(errMsg);

      // Если речь идет об Основном отпуске и не установлена галочка "игнорировать проверку на 14 дней", выполнить эту проверку
      var mainPaidVacationKind = Functions.Module.GetPaidVacationKind(_obj.Employee);
      if (((_obj.Vacation1 != null && Equals(_obj.Vacation1.VacationKind, mainPaidVacationKind)) ||
           (_obj.Vacation2 != null && Equals(_obj.Vacation2.VacationKind, mainPaidVacationKind)) ||
           (_obj.Vacation3 != null && Equals(_obj.Vacation3.VacationKind, mainPaidVacationKind))) &&
           !_obj.IgnoreRequiredDuration.Value)
      {
        var requiredDurationError = Functions.VacationShiftTask.CheckRequiredDuration(_obj);
        if (requiredDurationError != string.Empty) {
          // Если при задании первой порции отпуска не было найдено нарушение правила 14-ти дней
          // это означает, что могли задать отпуска не нарушая правила, но не сделали этого
          // поэтому вывести критичную ошибку
          var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
          if (!_obj.IsRequiredDurationBroken.Value || (!isFromEssAccount && !Equals(Employees.Current, _obj.Employee)))
            errorList.Add(requiredDurationError);
          else
            warningList.Add(requiredDurationError);
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

      // Если перенос отпуска был стартован из задания на подтверждение, выполнить задание.
      // TODO Если с задачей на изменение отпуска что-то произойдет, то подтверждение не сработает.
      if (_obj.ApprovalAssignment != null)
        Functions.Module.CompleteAlertAssignment(_obj);
    }

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;

      Functions.Module.SendAbortationNotices(_obj, _obj.IsVacationRecall.Value ? VacationShiftTasks.Resources.RecallAbortNoticeFormat(_obj.Employee.Name) : VacationShiftTasks.Resources.ShiftAbortNoticeFormat(_obj.Employee.Name),
                                             _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

  }

}