using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRSolution.Module.EssPlatform.Server
{
  partial class ModuleFunctions
  {
    public override void ActivateESSUser(DirRX.EssPlatformSolution.IEmployee employee)
    {
      base.ActivateESSUser(employee);
      
      // Запустить задачи на подписание согласия на КЭДО в каждой из НОР сотрудника.
      var employees = DirRX.HRSolution.Employees.GetAll(e => e.Status == Sungero.Company.Employee.Status.Active &&
                                                        e.Department != null && e.Department.BusinessUnit != null && Equals(employee.Person, e.Person));
      var businessUnits = employees.Select(e => e.Department.BusinessUnit).Distinct();
      var personId = employee.Person.Id;
      
      foreach (var businessUnit in businessUnits)
      {
        // Определить сотрудника для указания в задаче:
        var employeeForTask = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(personId, businessUnit);
        if (employeeForTask != null && DirRX.HRSolution.Employees.As(employeeForTask).ConsentDirRX == HRSolution.Employee.ConsentDirRX.NotSent)
        {
          var task = HRLite.ConsentTasks.Create();
          task.Employee = DirRX.HRSolution.Employees.As(employeeForTask);
          task.Start();
        }
      }
    }
    
    public override void DeleteESSUser(DirRX.EssPlatformSolution.IEmployee employee)
    {
      base.DeleteESSUser(employee);
      
      // Завершить все задачи на подписание согласия на КЭДО, созданные на всех сотрудников персоны.
      var employees = Sungero.Company.Employees.GetAll(e => e.Status == Sungero.Company.Employee.Status.Active && Equals(employee.Person, e.Person));
      var tasks = DirRX.HRLite.ConsentTasks.GetAll(t => employees.Contains(t.Employee) && t.Status == DirRX.HRLite.ConsentTask.Status.InProcess);
      foreach (var task in tasks)
        task.Abort();
    }
    
    public override List<Sungero.Company.IEmployee> GetEmployeesWithPersonalAccountRegistred(List<Sungero.Company.IEmployee> employees)
    {
      var registeredEmployees = base.GetEmployeesWithPersonalAccountRegistred(employees);
      return registeredEmployees.Where(t => Equals(HRSolution.Employees.As(t).ConsentDirRX, HRSolution.Employee.ConsentDirRX.Signed)).ToList();
    }
  }
}