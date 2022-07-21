using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.AcquaintanceNotESSEmployeeAssignment;

namespace DirRX.HRLite.Client
{
  partial class AcquaintanceNotESSEmployeeAssignmentActions
  {
    public virtual void ShowEmployeeNoESS(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var employees = new List<Sungero.Company.IEmployee>();
      var participants = Functions.HRAcquaintanceTask.Remote.GetParticipants(DirRX.HRLite.HRAcquaintanceTasks.As(_obj.Task));
      employees.AddRange(EssPlatform.PublicFunctions.Module.GetEmployeesWithoutPersonalAccountRegistred(participants));
      employees.AddRange(participants.Where(emp => HRSolution.Employees.As(emp).ConsentDirRX.Value != HRSolution.Employee.ConsentDirRX.Signed).ToList());
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