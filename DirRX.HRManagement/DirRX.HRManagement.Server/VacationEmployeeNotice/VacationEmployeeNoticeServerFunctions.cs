using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationEmployeeNotice;

namespace DirRX.HRManagement.Server
{
  partial class VacationEmployeeNoticeFunctions
  {
    
    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>     
    [Remote(IsPure = true)]
    public StateView GetVacationEmployeeNoticeState()
    {
      // Заполнить инструкцию датами отпуска
      var employee = Sungero.Company.Employees.As(_obj.Performer);
      var year = VacationDepartmentSchedulingTasks.As(_obj.Task).Year.Value;
      var employeeVacations = Functions.Vacation.GetPaidVacations(employee, year).ToList();
      List<string> vacationDates = new List<string>();
      for (var i = 0; i < employeeVacations.Count; i++)
        vacationDates.Add(string.Format("{0}. {1} - {2}", i + 1, employeeVacations[i].StartDate.Value.ToShortDateString(), employeeVacations[i].FinDate.Value.ToShortDateString()));
      var allDateInOneRow = string.Join(Environment.NewLine, vacationDates);
      var instruction = VacationDepartmentSchedulingTasks.Resources.VacationDatesInstructionFormat(year.Year, allDateInOneRow);
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}