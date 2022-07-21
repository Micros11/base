using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SchedulingPrintDocAssignment;

namespace DirRX.HRManagement.Server
{
  partial class SchedulingPrintDocAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetSchedulingPrintDocAssignmentState()
    {
      var task = VacationSchedulingTasks.As(_obj.Task);
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationSchedulingTask.PrintDocumentsBlockId:
          instruction = VacationSchedulingTasks.Resources.SignAllPaperDocInstruction;
          break;
        case Constants.VacationSchedulingTask.ConvertToPdfABlockId:
          instruction = VacationSchedulingTasks.Resources.AuthorConvertErrorsReworkInstructionFormat(VacationSchedulingTasks.As(_obj.Task).ConvertErrorMessage);
          break;
      }
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}