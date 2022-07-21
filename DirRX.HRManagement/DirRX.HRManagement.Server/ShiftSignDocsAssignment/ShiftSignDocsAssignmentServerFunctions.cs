using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftSignDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class ShiftSignDocsAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetSignShiftOrderAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationShiftTask.SignStatementAssignmentBlockId:
          instruction = VacationShiftTasks.Resources.SignStatementAssignmentInstruction;
          break;
        case Constants.VacationShiftTask.SignSeoDocAssignmentBlockId:
          instruction = VacationShiftTasks.Resources.SignSeoAssignmentInstruction;
          break;
        case Constants.VacationShiftTask.EmployeeSignOrderAssignmentBlockId:
          instruction = VacationShiftTasks.Resources.SignEmployeeAssignmenInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}