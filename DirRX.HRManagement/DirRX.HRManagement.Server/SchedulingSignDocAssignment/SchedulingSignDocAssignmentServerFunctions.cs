using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SchedulingSignDocAssignment;

namespace DirRX.HRManagement.Server
{
  partial class SchedulingSignDocAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetSchedulingSignDocAssignmentState()
    {
      var instruction = DirRX.HRManagement.VacationSchedulingTasks.Resources.SignSeoAssignmentInstructionFormat(VacationSchedulingTasks.As(_obj.Task).Year.Value.Year);
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}