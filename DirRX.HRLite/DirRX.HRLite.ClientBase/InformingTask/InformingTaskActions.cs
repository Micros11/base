using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.InformingTask;

namespace DirRX.HRLite.Client
{
  partial class InformingTaskActions
  {
    public virtual void ShowEmployeeNoESS(Sungero.Domain.Client.ExecuteActionArgs e)
    {      
      var employees = new List<Sungero.Company.IEmployee>();
      var recipients = Functions.InformingTask.Remote.GetRecipients(_obj);
      employees.AddRange(EssPlatform.PublicFunctions.Module.GetEmployeesWithoutPersonalAccountRegistred(recipients));
      employees.AddRange(recipients.Where(emp => HRSolution.Employees.As(emp).ConsentDirRX != HRSolution.Employee.ConsentDirRX.Signed).ToList());
      employees.ShowModal();
    }

    public virtual bool CanShowEmployeeNoESS(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}