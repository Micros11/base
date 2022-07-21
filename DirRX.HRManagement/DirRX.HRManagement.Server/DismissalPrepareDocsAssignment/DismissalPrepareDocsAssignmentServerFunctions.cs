using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalPrepareDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class DismissalPrepareDocsAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetDismissalPrepareDocsAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.DismissalTask.PrepareDocsAssignmentBlockId:
          instruction = DismissalTasks.Resources.PrepareDocAssignmentInstruction;
          break;
        case Constants.DismissalTask.ReWorkDocumentAssignmentBlockId:
          instruction = DismissalTasks.Resources.DismissalDocsReWorkAssignmentInstruction;
          break;
        case Constants.DismissalTask.DismissialBlockId:
          instruction = DismissalTasks.Resources.DismissEmployeeAssignmentInstruction;
          break;
        case Constants.DismissalTask.PrintOrderBlockId:
          instruction = DismissalTasks.Resources.DismissEmployeeAndPrintDocAssignmentInstruction;
          break;
        case Constants.DismissalTask.AccounterBlockId:
          instruction = DismissalTasks.Resources.AccounterDismissEmployeeAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}