using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.Company;
using Sungero.CoreEntities;

namespace DirRX.HRManagement.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void EmployeesConsentUpdate(DirRX.HRManagement.Server.AsyncHandlerInvokeArgs.EmployeesConsentUpdateInvokeArgs args)
    {
      var employeesIds = args.EmployeesIds;
      var employeesIdsList = employeesIds.Split(';').ToList();
      
      Logger.DebugFormat("HRManagement.AsyncHandlers.EmployeesConsentUpdate started, iteration: {0}, employeesIds: {1}", args.RetryIteration, employeesIds);
      
      if (args.RetryIteration > Constants.Module.EmployeesConsentUpdateMaxIteration)
      {
        Logger.ErrorFormat("HRManagement.AsyncHandlers.EmployeesConsentUpdate didn`t update Employees, iteration: {0}, employeesIds: {1}", args.RetryIteration, employeesIds);
        args.Retry = false;
        return;
      }
      
      try
      {
        var employee = HRSolution.Employees.Get(Convert.ToInt32(employeesIdsList.FirstOrDefault()));
        var employees = HRSolution.Employees.GetAll(w => employeesIdsList.Contains(w.Id.ToString()) && Equals(w.BusinessUnitDirRX, employee.BusinessUnitDirRX) && !Equals(w, employee));
        var consent = employee.ConsentDirRX;
        
        foreach (var item in employees)
        {
          if (item.ConsentDirRX != consent)
          {
            item.ConsentDirRX = consent;
            item.Save();
          }
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("HRManagement.AsyncHandlers.EmployeesConsentUpdate: could not update Employees. EmployeesIds: {0}. {1}. {2}", employeesIds, ex.Message, ex.StackTrace);
        args.Retry = true;
      }
    }

    public virtual void AbortDepartmentScheduling(DirRX.HRManagement.Server.AsyncHandlerInvokeArgs.AbortDepartmentSchedulingInvokeArgs args)
    {
      var subTasks = VacationDepartmentSchedulingTasks.GetAll(j => j.MainSchedulingTask.Id == args.MainTaskID &&
                                                              j.Status.Value == Sungero.Workflow.Task.Status.InProcess);
      foreach (var subTask in subTasks)
      {
        subTask.AbortingReason = args.AbortReason;
        subTask.Abort();
      }
    }

    public virtual void SetApprovedVacationStatus(DirRX.HRManagement.Server.AsyncHandlerInvokeArgs.SetApprovedVacationStatusInvokeArgs args)
    {
      var vacationList = Vacations.GetAll(v => (v.Year.Value.Year == args.Year) && (v.Employee.Department.Id == args.DepartmentID) && (v.Status == DirRX.HRManagement.Vacation.Status.OnApproval));
      if (vacationList.Any())
      {
        foreach (var vacation in vacationList)
        {
          vacation.Status = DirRX.HRManagement.Vacation.Status.Active;
          vacation.Save();
        }
      }
    }

    public virtual void DeleteSubstitution(DirRX.HRManagement.Server.AsyncHandlerInvokeArgs.DeleteSubstitutionInvokeArgs args)
    {
      var mainTask = VacationAlertTasks.As(Sungero.Workflow.Tasks.GetAll(l => l.Id == args.TaskID).FirstOrDefault());
      var substitution = Substitutions.GetAll(l => l.Id == args.SubstitutionID).FirstOrDefault();
      var perfomer = substitution.User;
      mainTask.Substitution = null;
      mainTask.Save();
      var noticeSubject = Resources.SubstitutionWasDeleted;
      Substitutions.Delete(substitution);
      var perfomers = new List<IUser>();
      perfomers.Add(perfomer);
      Sungero.Docflow.PublicFunctions.Module.Remote.SendNoticesAsSubtask(noticeSubject, perfomers, mainTask, string.Empty, Users.Current, mainTask.ThreadSubject);
    }

    public virtual void CreateSubstitution(DirRX.HRManagement.Server.AsyncHandlerInvokeArgs.CreateSubstitutionInvokeArgs args)
    {
      var mainTask = VacationAlertTasks.As(Sungero.Workflow.Tasks.GetAll(l => l.Id == args.TaskID).FirstOrDefault());
      var who = Employees.GetAll(l => l.Id == args.EmployeeID).FirstOrDefault();
      var byWhom = Employees.GetAll(l => l.Id == args.SubstituteID).FirstOrDefault();
      var substitution = Functions.Module.CreateSubstitution(who, byWhom, args.From, args.To, args.Comment);
      mainTask.Substitution = substitution;
      mainTask.Save();
      var noticeSubject = Resources.SubstitutionWasCreatedFormat(byWhom.Name);
      var perfomers = new List<IUser>();
      perfomers.Add(who);
      Sungero.Docflow.PublicFunctions.Module.Remote.SendNoticesAsSubtask(noticeSubject, perfomers, mainTask, string.Empty, Users.Current, mainTask.ThreadSubject);
    }

    public virtual void ChangeSubstitution(DirRX.HRManagement.Server.AsyncHandlerInvokeArgs.ChangeSubstitutionInvokeArgs args)
    {
      var substitution = Substitutions.GetAll(l => l.Id == args.SubstitutionID).FirstOrDefault();
      var employee = Employees.GetAll(l => l.Id == args.NewSubstituteID).FirstOrDefault();
      substitution.Substitute = employee;
      substitution.Save();
      var mainTask = Sungero.Workflow.Tasks.GetAll(l => l.Id == args.TaskID).FirstOrDefault();
      var noticeSubject = Resources.SubstitutionWasChangedFormat(employee.Name);
      var perfomers = new List<IUser>();
      perfomers.Add(substitution.User);
      Sungero.Docflow.PublicFunctions.Module.Remote.SendNoticesAsSubtask(noticeSubject, perfomers, mainTask, string.Empty, Users.Current, mainTask.ThreadSubject);
    }
  }
}