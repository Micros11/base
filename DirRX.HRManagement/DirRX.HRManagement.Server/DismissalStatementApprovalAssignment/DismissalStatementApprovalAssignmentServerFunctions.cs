using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalStatementApprovalAssignment;

namespace DirRX.HRManagement.Server
{
  partial class DismissalStatementApprovalAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetDismissalStatementApprovalAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.DismissalTask.ApprovalManagerAssignmentBlockId:
          if (DismissalSignDocsAssignments.GetAll(a => Equals(a.Task, DismissalTasks.As(_obj.Task)) && a.Result == DirRX.HRManagement.DismissalSignDocsAssignment.Result.ReApprove).Any())
            instruction = DismissalTasks.Resources.ReApproveAssignmentInstruction;
          else
            instruction = DismissalTasks.Resources.ApprovalAssignmentInstruction;
          break;
        case Constants.DismissalTask.ApprovalSEOAssignmentBlockId:
          instruction = DismissalTasks.Resources.ApprovalSEOAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}