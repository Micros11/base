using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignNotESSEmployeeAssigment;

namespace DirRX.HRLite.Client
{
  partial class SignNotESSEmployeeAssigmentActions
  {
    public virtual void ShowEmployeeNoESS(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var employees = new List<Sungero.Company.IEmployee>();
      var document = _obj.DocumentGroup.BilaterialDocuments.FirstOrDefault();
      var employeesList = document.Employees.Select(t => t.Employee).ToList();
      employees.AddRange(EssPlatform.PublicFunctions.Module.GetEmployeesWithoutPersonalAccountRegistred(employeesList));
      employees.AddRange(employeesList.Where(emp => HRSolution.Employees.As(emp).ConsentDirRX.Value != HRSolution.Employee.ConsentDirRX.Signed).ToList());
      employees.ShowModal();
    }

    public virtual bool CanShowEmployeeNoESS(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}