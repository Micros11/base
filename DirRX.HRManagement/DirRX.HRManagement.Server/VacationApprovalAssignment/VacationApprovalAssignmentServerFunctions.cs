using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationApprovalAssignment;

namespace DirRX.HRManagement.Server
{
  partial class VacationApprovalAssignmentFunctions
  {
    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetVacationApprovalAssignmentState()
    {
      var instruction = string.Empty;
      if (_obj.BlockUid == Constants.VacationAlertTask.EmployeeConfirmationBlockId)
      {
        var employee = Employees.As(_obj.Performer);
        if (Functions.Module.IsAllPaperVacationDoc(employee.Department.BusinessUnit) || !Functions.Module.IsEmployeeUsingESS(employee))
          instruction = VacationAlertTasks.Resources.VacationApprovalAssignmentAllPaperInstruction;
        else
          instruction = VacationAlertTasks.Resources.VacationApprovalAssignmentInstruction;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}