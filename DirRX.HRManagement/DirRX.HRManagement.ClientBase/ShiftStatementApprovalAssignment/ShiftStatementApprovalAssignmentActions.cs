using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.ShiftStatementApprovalAssignment;

namespace DirRX.HRManagement.Client
{
  partial class ShiftStatementApprovalAssignmentActions
  {
    public virtual void Deny(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(VacationShiftTasks.Resources.ErrorTextDenyNoActiveText);
        e.Params.AddOrUpdate("CanComplete", false);
      }
    }

    public virtual bool CanDeny(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      // Отказать может Сотрудник, если cтартовал Руководитель или Руководитель, если стартовал Сотрудник.
      var isEmployeeStarted = Equals(_obj.Employee, Employees.As(_obj.Author));
      return (_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId && isEmployeeStarted) ||
              (_obj.BlockUid == Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId && !isEmployeeStarted);
    }

    public virtual void Action(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var msg = string.Empty;
      var task = VacationShiftTasks.As(_obj.Task);
      msg += string.Format("IgnoreRequiredDuration: {1}\nIsVacationRecall: {2}\n IsRequiredDurationBroken: {3}\n\n",
                           _obj.IgnoreRequiredDuration, task.IsVacationRecall, _obj.IsRequiredDurationBroken);
      foreach (var vd in _obj.VacationDurations.ToList()) {
        msg += string.Format("Вид: {0} Planed: {1} Used: {2} Shifted: {3} Canceled: {4} Comped: {5} DaysAvailComp: {6} \n\n",
                             vd.VacationKind.Name, vd.DaysPlaned, vd.DaysUsed, vd.DaysShifted, vd.DaysCanceled, vd.DaysComped, vd.DaysAvailComp);
      }
      Dialogs.ShowMessage(msg);
    }

    public virtual bool CanAction(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Approve(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanApprove(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId;
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Stop(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var task = VacationShiftTasks.As(_obj.Task);
      Functions.Module.Remote.SendAbortationNotices(task, task.IsVacationRecall.Value ?
                                                    VacationShiftTasks.Resources.RecallAbortNoticeFormat(task.Employee.Name) :
                                                    VacationShiftTasks.Resources.ShiftAbortNoticeFormat(task.Employee.Name),
                                                    task.AbortingReason, task.Author, task.ThreadSubject);

      // Выяснить, требуется ли усиленная подпись.
      // Расчет на то, что кнопка прекратить доступна только в задании сотруднику.
      var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(task.Employee, _obj.BlockUid,
                                                                      new List<string>(),
                                                                      new List<string>() { Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId });
      
      // Подписать уведомление о начале отпуска, если задача на перенос стартована из задания на подтверждение дат отпуска.
      Functions.Module.SignVacationNotice(task, _obj.Performer, e, needAdvancedSign);
    }

    public virtual bool CanStop(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      // Прекратить может Руководитель, если cтартовал Руководитель или Сотрудник, если стартовал Сотрудник.
      var isEmployeeStarted = Equals(_obj.Employee, Employees.As(_obj.Author));
      return (_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId && !isEmployeeStarted) ||
              (_obj.BlockUid == Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId && isEmployeeStarted);
    }

    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var shiftTask = VacationShiftTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(shiftTask.Year.Value, shiftTask.Employee);
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {

    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId;
    }

    public virtual void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(VacationShiftTasks.Resources.ErrorTextReWorkNoActiveText);
        e.Params.AddOrUpdate("CanComplete", false);
      }
    }

    public virtual bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId;
    }

  }

}