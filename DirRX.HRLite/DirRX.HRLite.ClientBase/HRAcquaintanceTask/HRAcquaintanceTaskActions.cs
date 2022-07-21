using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceTask;

namespace DirRX.HRLite.Client
{
  partial class HRAcquaintanceTaskActions
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

    public virtual void AcquaintanceReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.HRAcquaintanceTask.GetAcquaintanceControlReport(_obj).Open();
    }
    
    public virtual bool CanAcquaintanceReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Отчет по ознакомлению может сформировать Специалист по работе с персоналом для задачи в работе или 
      // завершенной после выполнения задания на подписание.
      var hrSpecialist = Functions.Module.Remote.GetRole(Constants.Module.Roles.HRSpecialist);
      return (_obj.Status.Value == HRAcquaintanceTask.Status.InProcess || _obj.Status.Value == HRAcquaintanceTask.Status.Completed) &&
        HRAcquaintanceAssignments.GetAll(a => a.Task.Equals(_obj)).Any() && Users.Current.IncludedIn(hrSpecialist);
    }

    public virtual void ShowEmployeeNoESS(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var employees = new List<Sungero.Company.IEmployee>();
      var participants = Functions.HRAcquaintanceTask.Remote.GetParticipants(_obj);
      employees.AddRange(EssPlatform.PublicFunctions.Module.GetEmployeesWithoutPersonalAccountRegistred(participants));
      employees.AddRange(participants.Where(emp => HRSolution.Employees.As(emp).ConsentDirRX.Value != HRSolution.Employee.ConsentDirRX.Signed).ToList());
      employees.ShowModal();
    }

    public virtual bool CanShowEmployeeNoESS(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      
      return true;
    }

  }

}