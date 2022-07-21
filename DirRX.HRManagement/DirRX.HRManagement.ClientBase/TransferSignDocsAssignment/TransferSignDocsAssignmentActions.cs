using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.TransferSignDocsAssignment;

namespace DirRX.HRManagement.Client
{
  partial class TransferSignDocsAssignmentActions
  {
    public virtual void ReApprove(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(TransferTasks.Resources.ErrorTextReApproveNoActiveText);
    }

    public virtual bool CanReApprove(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.TransferTask.SignStatementAssignmentBlockId;
    }

    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(TransferTasks.Resources.ErrorTextReWorkNoActiveText);
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.TransferTask.SignManagerDocAssignmentBlockId;
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Если есть приказ, подписываем его и доп. соглашение, иначе - заявление.
      var documents = new List<IOfficialDocument>();
      var task = TransferTasks.As(_obj.Task);
      var order = Functions.TransferTask.Remote.GetEmployeeOrder(task);
      if (order != null)
      {
        documents.Add(order);
        documents.Add(Functions.TransferTask.Remote.GetAdditionalAgreement(task));
      }
      else
        documents.Add(task.Statement);

      // Выяснить, требуется ли усиленная подпись.
      var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                      new List<string>() { Constants.TransferTask.SignManagerDocAssignmentBlockId },
                                                                      new List<string>() { Constants.TransferTask.SignStatementAssignmentBlockId, Constants.TransferTask.SignEmployeeDocAssignmentBlockId });
      // Подписать документ.
      Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), documents, e.Action.LocalizedName, e, needAdvancedSign);
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }
}