using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationApprovalTask;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class VacationApprovalTaskFunctions
  {
    /// <summary>
    /// Получить приказ об отпуске.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHROrder GetEmployeeOrder()
    {
      return _obj.Order;
    }
    
    /// <summary>
    /// Создать приказ об отпуске.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Remote]
    public IHROrder CreateVacationOrder()
    {
      var guid = DocKind.VacationOrderKind;
      var vacationKindName = _obj.VacationKind.Name.ToLower();
      var document = Functions.Module.CreateOrder(guid, _obj.Employee, _obj.Statement, _obj.Year.Value);

      if (_obj.VacationKind.Paid.Value)
      {
        document.VacationYear = _obj.Year.Value.Year.ToString();
        document.VacationDays = Resources.VacationOrderTotalDurationTextFormat(_obj.Duration);
      }
      else
      {
        document.VacationYear = string.Empty;
        document.PaidVacation = string.Empty;
        document.VacationDays = string.Empty;
      }
      
      document.BodyText = Resources.VacationOrderBodyPartFormat(vacationKindName, _obj.Duration, _obj.DateBegin.Value.ToLongDateString(), _obj.DateEnd.Value.ToLongDateString());
      document.Save();
      return document;
    }
    
    /// <summary>
    /// Переформировать приказ об отпуске.
    /// </summary>
    /// <param name="order">Приказ.</param>
    [Remote]
    public void ReCreateVacationOrder(IHROrder order)
    {
      Functions.Module.RegenerateOrder(order);
    }
    
    /// <summary>
    /// Получить заявление о переносе отпуска.
    /// </summary>
    /// <returns>Заявление.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetEmployeeStatement()
    {
      return _obj.Statement;
    }
    
    /// <summary>
    /// Создать заявление на отпуск.
    /// </summary>
    /// <returns>Заявление.</returns>
    public IHRDocument CreateVacationStatement()
    {
      var document = Functions.Module.CreateStatement(DocKind.VacationStatementKind, _obj.Employee, _obj.Started.Value, _obj.Year.Value);
      var vacationKindName = _obj.VacationKind.Name.ToLower();
      document.BodyText = Resources.VacationStatementBodyPartFormat(vacationKindName, _obj.Duration, _obj.DateBegin.Value.ToLongDateString());
      document.Save();
      return document;
    }
    
    /// <summary>
    /// Перенести данные о датах отпуска из задачи в задание.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public void FillVacationDataFromTaskToAssignment(IVacationStatementApprovalAssignment assignment)
    {
      assignment.VacationKind = _obj.VacationKind;
      assignment.DateBegin = _obj.DateBegin;
      assignment.DateEnd = _obj.DateEnd;
      assignment.Duration = _obj.Duration;
      assignment.Substitute = _obj.Substitute;
    }
    
    /// <summary>
    /// Перенести данные о датах отпуска из задания в задачу.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public void FillVacationDataFromAssignmentToTask(IVacationStatementApprovalAssignment assignment)
    {
      _obj.VacationKind = assignment.VacationKind;
      _obj.DateBegin = assignment.DateBegin;
      _obj.DateEnd = assignment.DateEnd;
      _obj.Duration = assignment.Duration;
      _obj.Substitute = assignment.Substitute;
    }
    
    #region Проверки по отпускам.
    
    /// <summary>
    /// Проверить правильность всех указанных дат отпуска.
    /// </summary>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckAllVacationDates()
    {
      var vacationDateList = this.CreateVacationDateStructure();
      var errorList = this.CheckCriticalVacationsDates(vacationDateList);
      
      return errorList;
    }

    /// <summary>
    /// Проверить правильность указанной даты отпуска.
    /// </summary>
    /// <param name="beginDate">Дата начала отпуска.</param>
    /// <param name="endDate">Дата конца отпуска.</param>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckOneVacationDates(DateTime beginDate, DateTime endDate)
    {
      var vacationDate = Functions.Module.CreateVacationDateStructure(beginDate, endDate);
      var errorList = this.CheckNotCriticalVacationsDates(_obj.Employee, vacationDate);
      
      return errorList;
    }
    
    /// <summary>
    /// Создать структуру для хранения периодов отпусков.
    /// </summary>
    /// <returns>Список дат отпусков.</returns>
    [Public, Remote]
    public virtual List<Structures.Vacation.IVacationDates> CreateVacationDateStructure()
    {
      var vacationDateList = new List<Structures.Vacation.IVacationDates>();
      vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DateBegin.Value,
                                                                    _obj.DateEnd.Value,
                                                                    _obj.Duration.Value,
                                                                    DateStatuses.New,
                                                                    _obj.VacationKind));
      
      // Добавить к проверяемым датам даты оплачиваемых отпусков.
      var vacationList = Functions.Vacation.GetPaidVacations(_obj.Employee, _obj.Year.Value);
      vacationDateList.AddRange(Functions.Module.FormVacationDateStructure(vacationList));
      
      return vacationDateList;
    }
    
    /// <summary>
    /// Проверить правильность указанных дат отпуска на не критичные ошибки.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="vacationDate">Дата отпуска.</param>
    /// <returns>Не критичные ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckNotCriticalVacationsDates(Sungero.Company.IEmployee employee, Structures.Vacation.IVacationDates vacationDate)
    {
      List<string> errorList = new List<string>();
      
      // Проверить, что отпуск не заканчивается перед выходным или праздничным днем.
      var vacationEndBeforeWeekend = Functions.Module.CheckVacationEndBeforeWeekendOrHoliday(vacationDate, employee);
      if (!string.IsNullOrWhiteSpace(vacationEndBeforeWeekend))
        errorList.Add(vacationEndBeforeWeekend);
      
      return errorList;
    }
    
    /// <summary>
    /// Проверить правильность указанных дат отпуска на критичные ошибки.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <returns>Критичные ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckCriticalVacationsDates(List<Structures.Vacation.IVacationDates> vacationDates)
    {
      List<string> errorList = new List<string>();
      
      // Проверить, что отпуск не пересекается.
      // К проверяемым датам добавить даты отпусков предыдущего года.
      var vacationDatesForIntersections = Functions.Module.CreateVacationDateStructureForIntersection(_obj.Employee, _obj.Year.Value, false);
      vacationDatesForIntersections.AddRange(vacationDates);
      var vacationIntersections = Functions.Module.CheckVacationIntersections(vacationDatesForIntersections);
      if (vacationIntersections.Any())
        errorList.AddRange(vacationIntersections);
      
      if (_obj.VacationKind.Paid.Value)
      {
        // Проверить интервал от текущей даты до следующего отпуска.
        var vacationsInterval = Functions.Module.CheckVacationsInterval(vacationDates, _obj.Employee);
        if (vacationsInterval.Any())
          errorList.AddRange(vacationsInterval);
      }
      
      // Проверить, что Дата начала отпуска не больше Даты окончания.
      var vacationDatesRangeError = Functions.Module.CheckVacationDatesRange(vacationDates);
      if (vacationDatesRangeError != null)
        errorList.AddRange(vacationDatesRangeError);
      
      // Проверить, что Дата начала + Длительность = Дата окончания с учетом календаря рабочего времени.
      var vacationDatesNotMatchDuration = Functions.Module.CheckVacatinDatesMatchDuration(vacationDates, _obj.Employee);
      if (vacationDatesNotMatchDuration.Any())
        errorList.AddRange(vacationDatesNotMatchDuration);
      
      return errorList;
    }
    #endregion
  }
}