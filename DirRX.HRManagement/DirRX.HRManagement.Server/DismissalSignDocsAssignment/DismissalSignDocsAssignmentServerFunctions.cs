using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalSignDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class DismissalSignDocsAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetOrderSigningAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.DismissalTask.SignEmployeeStatementAssignmentBlockId:
          instruction = DismissalTasks.Resources.SignStatementAssignmentInstruction;
          break;
        case Constants.DismissalTask.SignSEODocAssignmentBlockId:
          instruction = DismissalTasks.Resources.SignSeoAssignmentInstruction;
          break;
        case Constants.DismissalTask.SignEmployeeDocAssignmentBlockId:
          instruction = DismissalTasks.Resources.SignEmployeeAssignmenInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}