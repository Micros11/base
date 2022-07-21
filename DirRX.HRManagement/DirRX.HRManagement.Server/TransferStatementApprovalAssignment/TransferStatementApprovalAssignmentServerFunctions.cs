using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferStatementApprovalAssignment;

namespace DirRX.HRManagement.Server
{
  partial class TransferStatementApprovalAssignmentFunctions
  {

    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetTransferStatementApprovalAssignmentState()
    {
      var task = TransferTasks.As(_obj.Task);
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.TransferTask.ApprovalOldManagerAssignmentBlockId:
          if (TransferStatementApprovalAssignments.GetAll(a => Equals(a.Task, task) && a.BlockUid == Constants.TransferTask.ApprovalOldManagerAssignmentBlockId && a.Id != _obj.Id).Any())
            instruction = TransferTasks.Resources.CurrentManagerReApprovalAssignmentInstruction;
          else
            instruction = TransferTasks.Resources.CurrentManagerApprovalAssignmentInstruction;
          break;
        case Constants.TransferTask.ApprovalNewManagerAssignmentBlockId:
          if (TransferStatementApprovalAssignments.GetAll(a => Equals(a.Task, task) && a.BlockUid == Constants.TransferTask.ApprovalNewManagerAssignmentBlockId && a.Id != _obj.Id).Any())
            instruction = TransferTasks.Resources.NewManagerReApprovalAssignmentInstruction;
          else
            instruction = TransferTasks.Resources.NewManagerApprovalAssignmentInstruction;
          break;
        case Constants.TransferTask.EmployeeStetementBlockId:
          if (task.IsHRStarted.Value || task.IsManagerStarted.Value)
            instruction = TransferTasks.Resources.EmployeeStetementInstruction;
          else
            instruction = TransferTasks.Resources.EmployeeSelfStartStetementInstruction;
          break;
        case Constants.TransferTask.InitiatorReWorkBlockId:
          if (task.IsHRStarted.Value)
            instruction = TransferTasks.Resources.HRReWorkInstruction;
          else if (task.IsManagerStarted.Value)
            instruction = TransferTasks.Resources.ManagerReWorkInstruction;
          else
            instruction = TransferTasks.Resources.EmployeeReWorkInstruction;
          break;
        #region Блоки версии V1, для обеспечения совместимости
        case Constants.TransferTask.ApprovalHiringResponsibleAssignmentBlockId:
          if (TransferSignDocsAssignments.GetAll(a => Equals(a.Task, task) && a.Result == DirRX.HRManagement.TransferSignDocsAssignment.Result.ReApprove).Any())
            instruction = TransferTasks.Resources.ReApproveAssignmentInstruction;
          else
            instruction = TransferTasks.Resources.HRApprovalAssignmentInstruction;
          break;
        case Constants.TransferTask.SEOBlockId:
          instruction = TransferTasks.Resources.HRManagerApprovalAssignmentInstruction;
          break;
        #endregion
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}