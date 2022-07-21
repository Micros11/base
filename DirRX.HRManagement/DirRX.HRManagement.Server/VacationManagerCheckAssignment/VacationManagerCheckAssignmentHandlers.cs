using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationManagerCheckAssignment;

namespace DirRX.HRManagement
{
  partial class VacationManagerCheckAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      var task = VacationDepartmentSchedulingTasks.As(_obj.Task);
      var year = task.Year.Value;
      var employees = Employees.GetAll(m => m.Department.Equals(task.Department) && m.Status == Sungero.Company.Employee.Status.Active);
      foreach (var employee in employees)
      {
        var vacationDateList = Functions.Module.CreateVacationDateStructure(employee, year);
        var errorList = Functions.Module.CheckVacationsDates(vacationDateList, employee, year);
        errorList.AddRange(Functions.Module.CheckVacationKinds(vacationDateList, employee, year));
        var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, DeclensionCase.Genitive, false);
        foreach (var error in errorList)
          e.AddError(VacationManagerCheckAssignments.Resources.EmployeeVacationErrorFormat(employeeShortName, error));
      }
      if (!e.IsValid)
        return;

    }
  }

}