using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SingleSignDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class SingleSignDocsAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetSingleSignDocsAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationSingleSchedulingTask.SignInitiatorStatementBlockId:
          instruction = VacationApprovalTasks.Resources.SignStatementAssignmentInstruction;
          break;
        case Constants.VacationSingleSchedulingTask.SignSeoDocAssignmentBlockId:
          instruction = VacationApprovalTasks.Resources.SignSeoAssignmentInstruction;
          break;
        case Constants.VacationSingleSchedulingTask.SignInitiatorOrderBlockId:
          instruction = VacationApprovalTasks.Resources.SignEmployeeAssignmenInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}