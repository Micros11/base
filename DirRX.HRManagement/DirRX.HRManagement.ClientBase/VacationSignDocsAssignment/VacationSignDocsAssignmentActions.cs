using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.VacationSignDocsAssignment;

namespace DirRX.HRManagement.Client
{
  partial class VacationSignDocsAssignmentActions
  {
    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var approvalTask = VacationApprovalTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(approvalTask.Year.Value, approvalTask.Employee);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationApprovalTask.SignSeoDocAssignmentBlockId || _obj.BlockUid == Constants.VacationApprovalTask.SignInitiatorStatementBlockId;
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Не подписывать документ, если идет работа под пользователем личного кабинета.
      var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
      if (!isFromEssAccount)
      {
        var documents = new List<IOfficialDocument>();
        var task = VacationApprovalTasks.As(_obj.Task);
        // Если есть приказ, подписываем его, иначе - заявление.
        var order = Functions.VacationApprovalTask.Remote.GetEmployeeOrder(task);
        if (order != null)
          documents.Add(order);
        else
          documents.Add(Functions.VacationApprovalTask.Remote.GetEmployeeStatement(task));

        // Выяснить, требуется ли усиленная подпись.
        var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                        new List<string>() { Constants.VacationApprovalTask.SignSeoDocAssignmentBlockId },
                                                                        new List<string>() { Constants.VacationApprovalTask.SignInitiatorStatementBlockId, Constants.VacationApprovalTask.SignInitiatorOrderBlockId });
        // Подписать документ.
        Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), documents, e.Action.LocalizedName, e, needAdvancedSign);
      }
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(VacationApprovalTasks.Resources.ErrorTextReWorkNoActiveText);
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationApprovalTask.SignSeoDocAssignmentBlockId;
    }

  }

}