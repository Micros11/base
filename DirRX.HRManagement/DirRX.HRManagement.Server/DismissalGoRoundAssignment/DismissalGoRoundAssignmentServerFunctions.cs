using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalGoRoundAssignment;

namespace DirRX.HRManagement.Server
{
  partial class DismissalGoRoundAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetDismissalGoRoundAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.DismissalTask.SBServiceBlockId:
          instruction = DismissalTasks.Resources.RespGoRoundAssignmentInstruction;
          break;
        case Constants.DismissalTask.ITServiceBlockId:
          instruction = DismissalTasks.Resources.RespGoRoundAssignmentInstruction;
          break;
        case Constants.DismissalTask.AHOServiceBlockId:
          instruction = DismissalTasks.Resources.RespGoRoundAssignmentInstruction;
          break;
        case Constants.DismissalTask.SBServiceControlBlockId:
          instruction = DismissalTasks.Resources.RespGoRoundControlAssignmentInstruction;
          break;
        case Constants.DismissalTask.ITServiceControlBlockId:
          instruction = DismissalTasks.Resources.RespGoRoundControlAssignmentInstruction;
          break;
        case Constants.DismissalTask.AHOServiceControlBlockId:
          instruction = DismissalTasks.Resources.RespGoRoundControlAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}