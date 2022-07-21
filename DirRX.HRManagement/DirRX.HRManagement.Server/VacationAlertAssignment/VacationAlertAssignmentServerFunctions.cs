using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationAlertAssignment;

namespace DirRX.HRManagement.Server
{
  partial class VacationAlertAssignmentFunctions
  {
    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetVacationAlertAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationAlertTask.EmployeePrintBlockId:
          if (VacationAlertTasks.As(_obj.Task).IsShiftAbortedByDeadline.Equals(true))
            instruction = VacationAlertTasks.Resources.SignVacationAlertShiftDeadlineDocInstruction;
          else
            instruction = VacationAlertTasks.Resources.SignVacationAlertShiftCanceledDocInstruction;
          break;
        case Constants.VacationAlertTask.AccountBlockId:
          // Если перенос инициировался, но не был согласован, добавить сообщение об этом в инструкцию.
          var abortInstruction = string.Empty;
          if (VacationAlertTasks.As(_obj.Task).ShiftTask != null)
          {
            if (VacationAlertTasks.As(_obj.Task).IsShiftAbortedByDeadline.Equals(true))
              abortInstruction = VacationAlertTasks.Resources.AccounterAssignmentShiftExpiredInstruction + Environment.NewLine;
            else
              abortInstruction = VacationAlertTasks.Resources.AccounterAssignmentAbortingInstruction + Environment.NewLine;
          }
          instruction = abortInstruction + VacationAlertTasks.Resources.AccounterAssignmentInstruction;
          break;
        case Constants.VacationAlertTask.ResponsibleConfirmationBlockId:
          instruction = VacationAlertTasks.Resources.SignPaperDocAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}