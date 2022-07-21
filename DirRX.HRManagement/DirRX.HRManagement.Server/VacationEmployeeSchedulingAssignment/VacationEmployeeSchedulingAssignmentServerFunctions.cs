using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationEmployeeSchedulingAssignment;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement.Server
{
  partial class VacationEmployeeSchedulingAssignmentFunctions
  {

    /// <summary>
    /// Заполнить продолжительности отпусков в задании.
    /// </summary>
    [Remote]
    public void FillAssignmentDurations()
    {
      var employee = Employees.As(_obj.Performer);
      var year = VacationDepartmentSchedulingTasks.As(_obj.MainTask).Year;
      var employeeVacationDurations = Functions.Module.GetEmployeeVacationDuration(employee, year.Value);
      var instructionForDurations = new List<string>();
      if (employeeVacationDurations != null)
      {
        _obj.VacationDurations.Clear();
        foreach (var vacationDuration in employeeVacationDurations.AvailableDuration)
        {
          var assignmentDurationRow = _obj.VacationDurations.AddNew();
          assignmentDurationRow.VacationKind = vacationDuration.VacationKind;
          assignmentDurationRow.DaysCount = vacationDuration.DaysCount;
        }
      }
      Functions.VacationEmployeeSchedulingAssignment.FillInstruction(_obj);
    }
    
    #region Проверки по отпускам.
    
    /// <summary>
    /// Проверить правильность всех указанных дат отпуска.
    /// </summary>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckAllVacationDates()
    {
      return this.CheckCriticalVacationsDates();
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
      return this.CheckNotCriticalVacationsDates(vacationDate);
    }
    
    /// <summary>
    /// Создать структуру для хранения периодов отпусков.
    /// </summary>
    /// <returns>Список дат отпусков.</returns>
    [Public, Remote]
    public virtual List<Structures.Vacation.IVacationDates> CreateVacationDateStructure()
    {
      var vacationDateList = new List<Structures.Vacation.IVacationDates>();
      foreach (var vacationRow in _obj.Vacations)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(vacationRow.DateBegin.Value,
                                                                      vacationRow.DateEnd.Value,
                                                                      vacationRow.Duration.Value,
                                                                      DateStatuses.New,
                                                                      vacationRow.VacationKind));
      var task = VacationDepartmentSchedulingTasks.As(_obj.Task);
      var vacationList = Functions.Vacation.GetPaidVacations(Employees.As(_obj.Performer), task.Year.Value);
      vacationDateList.AddRange(Functions.Module.FormVacationDateStructure(vacationList));
      
      return vacationDateList;
    }
    
    /// <summary>
    /// Проверить правильность указанных дат отпуска на не критичные ошибки.
    /// </summary>
    /// <param name="vacationDate">Дата отпуска.</param>
    /// <returns>Не критичные ошибки в датах отпуска.</returns>
    [Public]
    public virtual List<string> CheckNotCriticalVacationsDates(Structures.Vacation.IVacationDates vacationDate)
    {
      List<string> errorList = new List<string>();
      
      // Проверить, что отпуск не заканчивается перед выходным или праздничным днем.
      var vacationEndBeforeWeekend = Functions.Module.CheckVacationEndBeforeWeekendOrHoliday(vacationDate, Employees.As(_obj.Performer));
      if (!string.IsNullOrWhiteSpace(vacationEndBeforeWeekend))
        errorList.Add(vacationEndBeforeWeekend);
      
      return errorList;
    }
    
    /// <summary>
    /// Проверить правильность указанных дат отпуска на критичные ошибки.
    /// Проверка наличия необходимой продолжительности производится только по обязательным оплачиваемым отпускам.
    /// Проверка общей продолжительности производится по каждому виду отпуска, которые указаны в записи справочника "Продолжительности отпусков".
    /// Проверка пересечений производится по всем отпускам, которые указал сотрудник, с добавлением отпусков за предыдущий год,
    /// чтобы исключить пересечения с отпусками, которые относятся к прошлому году но фактически оформлены на даты текущего.
    /// Проверка что даты не выходят за границы года отпуска выполняется по всем отпускам, которые указал сотрудник.
    /// </summary>
    /// <returns>Критичные ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckCriticalVacationsDates()
    {
      List<string> errorList = new List<string>();
      var year = VacationDepartmentSchedulingTasks.As(_obj.Task).Year.Value;
      var employee = Employees.As(_obj.Performer);
      
      // Проверить, что есть часть отпуска с необходимой продолжительностью.
      var vacationDates = this.CreateVacationDateStructure();
      var vacationDuration = Functions.Module.CheckVacationRequiredDuration(vacationDates, employee, year);
      if (!string.IsNullOrWhiteSpace(vacationDuration))
        errorList.Add(vacationDuration);
      
      // Проверить общую продолжительность отпусков.
      var employeePersonalDuration = Functions.Module.GetEmployeeVacationDuration(employee, year);
      if (employeePersonalDuration != null)
      {
        foreach (var kindDuration in employeePersonalDuration.AvailableDuration)
        {
          var vacationsTotalDuration = Functions.Module.CheckVacationsTotalDuration(vacationDates,
                                                                                    kindDuration.VacationKind,
                                                                                    kindDuration.DaysCount.Value);
          if (!string.IsNullOrWhiteSpace(vacationsTotalDuration))
            errorList.Add(vacationsTotalDuration);
        }
      }
      
      // Проверить, что отпуск не пересекается.
      // К проверяемым датам добавить даты отпусков предыдущего года.
      var vacationDatesForIntersections = Functions.Module.CreateVacationDateStructureForIntersection(employee, year, false);
      vacationDatesForIntersections.AddRange(vacationDates);
      var vacationIntersections = Functions.Module.CheckVacationIntersections(vacationDatesForIntersections);
      if (vacationIntersections.Any())
        errorList.AddRange(vacationIntersections);

      // Проверить, что даты не выходят за границы года отпуска.
      var vacationInYear = Functions.Module.CheckVacationDateInCalendarYear(vacationDates, employee, year, Resources.VacationEndOverBorderCalendarYear);
      if (!string.IsNullOrWhiteSpace(vacationInYear))
        errorList.Add(vacationInYear);
      
      // Проверить, что Дата начала отпуска не больше Даты окончания.
      var vacationDatesRangeError = Functions.Module.CheckVacationDatesRange(vacationDates);
      if (vacationDatesRangeError != null)
        errorList.AddRange(vacationDatesRangeError);
      
      // Проверить, что Дата начала + Длительность = Дата окончания с учетом календаря рабочего времени.
      var vacationDatesNotMatchDuration = Functions.Module.CheckVacatinDatesMatchDuration(vacationDates, employee);
      if (vacationDatesNotMatchDuration.Any())
        errorList.AddRange(vacationDatesNotMatchDuration);
      
      return errorList;
    }
    #endregion
    
    /// <summary>
    /// Заполнить инструкцию задания на планирования, в зависимости от доступных и распределенных дней отпуска.
    /// </summary>
    [Public, Remote]
    public virtual void FillInstruction()
    {
      var instructionForDurations = new List<string>();
      foreach (var duration in _obj.VacationDurations)
      {
        var availableDays = duration.DaysCount;
        if (_obj.Vacations.Any())
        {
          var durationOfOneKind = _obj.Vacations.Where(l => Equals(l.VacationKind, duration.VacationKind) && l.Duration.HasValue)
            .Sum(x => x.Duration.Value);
          availableDays -= durationOfOneKind;
        }
        instructionForDurations.Add("- " + duration.VacationKind.Name + ": " + availableDays);
      }
      _obj.Instruction = Functions.Module.TrimInstruction(VacationDepartmentSchedulingTasks.Resources.FillTableInstructionFormat(VacationDepartmentSchedulingTasks.As(_obj.Task).Year.Value.Year) +
                                                          Environment.NewLine +
                                                          VacationDepartmentSchedulingTasks.Resources.EmployeeSchedulingInstructionFormat(string.Join(Environment.NewLine, instructionForDurations)));
    }
  }
}