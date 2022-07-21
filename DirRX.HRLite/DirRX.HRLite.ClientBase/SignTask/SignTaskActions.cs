using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignTask;

namespace DirRX.HRLite.Client
{
  partial class SignTaskActions
  {

    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = HRManagement.PublicFunctions.Module.GetReasonBeforeAbort();
      if (abortReason != string.Empty)
      {
        _obj.AbortingReason = abortReason;
        _obj.Save();
        base.Abort(e);
        var documentApprovalTasks = HRManagement.DocumentApprovalTasks.GetAll(t => Equals(t.MainTask, _obj) && t.Status == Sungero.Workflow.Task.Status.InProcess);
        if (documentApprovalTasks.Any())
        {
          var documentApprovalTask = documentApprovalTasks.Single();
          documentApprovalTask.AbortingReason = abortReason;
          documentApprovalTask.Save();
          documentApprovalTask.Abort();
        }
      }
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanAbort(e);
    }

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

  }

}