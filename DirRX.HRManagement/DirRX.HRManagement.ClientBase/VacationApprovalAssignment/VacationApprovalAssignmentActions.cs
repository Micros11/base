using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.VacationApprovalAssignment;

namespace DirRX.HRManagement.Client
{

  partial class VacationApprovalAssignmentActions
  {

    public virtual void Shift(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanShift(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void StartShiftTask(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var alertVacations = VacationAlertTasks.As(_obj.Task).Vacations.OrderBy(l => l.Vacation.StartDate.Value)
        .Select(x => x.Vacation)
        .Take(3)
        .ToList();
      PublicFunctions.Module.Remote.CreateNewVacationShiftTask(alertVacations, _obj).Show();
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanStartShiftTask(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Подписать уведомление о начале отпуска.
      var task = VacationAlertTasks.As(_obj.Task);
      var vacationNotice = Functions.VacationAlertTask.Remote.GetEmployeeNotice(task);
      var documents = new List<IOfficialDocument>();
      documents.Add(vacationNotice);

      // Выяснить, требуется ли усиленная подпись.
      var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                      new List<string>(),
                                                                      new List<string>() { Constants.VacationAlertTask.EmployeeConfirmationBlockId });
      // Подписать документ.
      Functions.Module.ApproveDocument(_obj, documents, VacationApprovalAssignments.Resources.CommentInVacationNoticeSignature, e, needAdvancedSign);
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var vacation = VacationAlertTasks.As(_obj.Task).Vacation;
      Functions.Module.VacationScheduleChart(vacation.Year.Value, vacation.BusinessUnit, vacation.Department);
    }

  }

}