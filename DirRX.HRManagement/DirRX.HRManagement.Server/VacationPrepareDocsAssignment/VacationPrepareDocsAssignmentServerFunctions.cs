using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationPrepareDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class VacationPrepareDocsAssignmentFunctions
  {
    
    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetVacationAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationApprovalTask.PrepareOrderBlockId:
          instruction = VacationApprovalTasks.Resources.PrepareOrderAssignmentInstruction;
          break;
        case Constants.VacationApprovalTask.ReworkPrepareDocumentAssignmentBlockId:
          instruction = VacationApprovalTasks.Resources.VacationDocsReWorkAssignmentInstruction;
          break;
        case Constants.VacationApprovalTask.PrintDocumentBlockId:
          instruction = VacationApprovalTasks.Resources.SignAllPaperDocAssignmentInstruction;
          break;
        case Constants.VacationApprovalTask.AccountBlockId:
          if (VacationApprovalTasks.As(_obj.Task).VacationKind.Paid.Value)
            instruction = VacationApprovalTasks.Resources.AccounterAssignmentInstruction;
          else
            instruction = VacationApprovalTasks.Resources.AccounterWithoutPayAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);

    }

  }
}