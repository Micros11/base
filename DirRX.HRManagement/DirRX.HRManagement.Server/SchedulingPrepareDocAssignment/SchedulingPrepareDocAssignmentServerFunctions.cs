using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SchedulingPrepareDocAssignment;
using Sungero.Company;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class SchedulingPrepareDocAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetSchedulingPrepareDocAssignmentState()
    {
      var instruction = VacationSchedulingTasks.Resources.CheckVacationScheduleInstructionFormat(VacationSchedulingTasks.As(_obj.Task).Year.Value.Year);
      return Functions.Module.GetAssignmentStateView(instruction);
    }
    
    /// <summary>
    /// Проверить, что график отпусков не сформирован.
    /// </summary>
    /// <returns>True, если график отпусков не сформирован.</returns>
    [Remote]
    public bool IsVacationScheduleNotCreated()
    {
      return _obj.VacationSchedule == null;
    }
  }
}