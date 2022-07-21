using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.SingleSignDocsAssignment;

namespace DirRX.HRManagement.Client
{
  partial class SingleSignDocsAssignmentActions
  {
    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Не подписывать документ, если идет работа под пользователем личного кабинета.
      var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
      if (!isFromEssAccount)
      {
        var documents = new List<IOfficialDocument>();
        var task = VacationSingleSchedulingTasks.As(_obj.Task);
        // Если есть приказ, подписываем его, иначе - заявление.
        var order = Functions.VacationSingleSchedulingTask.Remote.GetEmployeeOrder(task);
        if (order != null)
          documents.Add(order);
        else
          documents.Add(Functions.VacationSingleSchedulingTask.Remote.GetEmployeeStatement(task));

        // Выяснить, требуется ли усиленная подпись.
        var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                        new List<string>() { Constants.VacationSingleSchedulingTask.SignSeoDocAssignmentBlockId },
                                                                        new List<string>() { Constants.VacationSingleSchedulingTask.SignInitiatorStatementBlockId, Constants.VacationSingleSchedulingTask.SignInitiatorOrderBlockId });
        // Подписать документ.
        Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), documents, e.Action.LocalizedName, e, needAdvancedSign);
      }
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = VacationSingleSchedulingTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(task.Year.Value, task.Employee.Department.BusinessUnit, task.Employee.Department);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationSingleSchedulingTask.SignSeoDocAssignmentBlockId || _obj.BlockUid == Constants.VacationSingleSchedulingTask.SignInitiatorStatementBlockId;
    }

    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(VacationSingleSchedulingTasks.Resources.ErrorTextReWorkNoActiveText);
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationSingleSchedulingTask.SignSeoDocAssignmentBlockId;
    }

  }

}