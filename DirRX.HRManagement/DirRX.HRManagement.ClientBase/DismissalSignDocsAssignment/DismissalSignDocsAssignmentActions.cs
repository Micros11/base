using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.DismissalSignDocsAssignment;

namespace DirRX.HRManagement.Client
{
  partial class DismissalSignDocsAssignmentActions
  {
    public virtual void ReApprove(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(DismissalTasks.Resources.ErrorTextReApproveNoActiveText);
    }

    public virtual bool CanReApprove(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.DismissalTask.SignEmployeeStatementAssignmentBlockId;
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Если есть приказ, подписываем его, иначе - заявление.
      var documents = new List<IOfficialDocument>();
      var task = DismissalTasks.As(_obj.Task);
      var order = Functions.DismissalTask.Remote.GetEmployeeOrder(task);
      if (order != null)
        documents.Add(order);
      else
        documents.Add(Functions.DismissalTask.Remote.GetEmployeeStatement(task));
      
      // Выяснить, требуется ли усиленная подпись
      bool needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                       new List<string>() { Constants.DismissalTask.SignSEODocAssignmentBlockId },
                                                                       new List<string>() { Constants.DismissalTask.SignEmployeeStatementAssignmentBlockId, Constants.DismissalTask.SignEmployeeDocAssignmentBlockId });
      // Подписать документ
      Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), documents, e.Action.LocalizedName, e, needAdvancedSign);
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(DismissalTasks.Resources.ErrorTextReWorkNoActiveText);
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.DismissalTask.SignSEODocAssignmentBlockId;
    }

  }

}