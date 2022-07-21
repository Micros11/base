using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationManagerNotice;

namespace DirRX.HRManagement.Server
{
  partial class VacationManagerNoticeFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetVacationManagerNoticeState()
    {
      var instruction = string.Empty;
      if (_obj.BlockUid == Constants.VacationAlertTask.ManagerSubstitutionNoticeBlockId)
      {
        var alertTask = VacationAlertTasks.As(_obj.Task);
        var employee = alertTask.Vacation.Employee;
        var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, DeclensionCase.Genitive, false);
        var alertVacations = alertTask.Vacations;
        instruction = VacationAlertTasks.Resources.VacationApprovedSubjectFormat(employeeShortName,
                                                                                 alertVacations.OrderBy(v => v.Vacation.StartDate).First().Vacation.StartDate.Value.ToShortDateString(),
                                                                                 alertVacations.OrderByDescending(v => v.Vacation.FinDate).First().Vacation.FinDate.Value.ToShortDateString())
          + Environment.NewLine;
        var substitution = alertTask.Substitution;
        if (substitution != null)
          instruction += VacationAlertTasks.Resources.VacationApprovedInstructionFormat(substitution.Substitute.Name);
        else
          instruction += VacationAlertTasks.Resources.VacationApprovedWithoutSubstitutionInstruction;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}