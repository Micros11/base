using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SinglePrepareDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class SinglePrepareDocsAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetSinglePrepareDocsAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationSingleSchedulingTask.PrepareOrderBlockId:
          instruction = VacationApprovalTasks.Resources.PrepareOrderAssignmentInstruction;
          break;
        case Constants.VacationSingleSchedulingTask.ReworkPrepareDocumentAssignmentBlockId:
          instruction = VacationApprovalTasks.Resources.VacationDocsReWorkAssignmentInstruction;
          break;
        case Constants.VacationSingleSchedulingTask.PrintPaperAssignmentBlockId:
          instruction = VacationApprovalTasks.Resources.SignAllPaperDocAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}