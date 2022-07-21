using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftPrepareDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class ShiftPrepareDocsAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetVacationShiftAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationShiftTask.ShiftPrepareDocsAssignmentBlockId:
          instruction = VacationShiftTasks.Resources.PrepareOrderAssignmentInstruction;
          break;
        case Constants.VacationShiftTask.ReWorkDocumentAssignmentBlockId:
          instruction = VacationShiftTasks.Resources.VacationDocsReWorkAssignmentInstruction;
          break;
        case Constants.VacationShiftTask.PrintPaperAssignmentBlockId:
          instruction = VacationShiftTasks.Resources.SignAllPaperDocAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}