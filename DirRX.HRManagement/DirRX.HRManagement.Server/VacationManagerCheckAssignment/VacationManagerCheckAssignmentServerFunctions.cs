using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationManagerCheckAssignment;

namespace DirRX.HRManagement.Server
{
  partial class VacationManagerCheckAssignmentFunctions
  {
    /// <summary>
    /// Получить список отпусков по подразделению.
    /// </summary>
    /// <returns>Список отпусков по подразделению.</returns>
    [Remote(IsPure = true)]
    public IQueryable<IVacation> VacationsForDepartment()
    {     
      var task = VacationDepartmentSchedulingTasks.As(_obj.Task);
      var departmentList = new List<Sungero.Company.IDepartment>();
      departmentList.Add(task.Department);
      return Functions.Vacation.GetPaidVacations(departmentList, task.Year.Value, true);
    }

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>    
    [Remote]
    public StateView GetVacationEmployeeCheckState()
    {
      var instruction = VacationDepartmentSchedulingTasks.Resources.ManagerSchedulingInstructionFormat(VacationDepartmentSchedulingTasks.As(_obj.Task).Year.Value.Year);
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}