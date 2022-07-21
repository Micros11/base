using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.GoRoundNotice;

namespace DirRX.HRManagement.Server
{
  partial class GoRoundNoticeFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetGoRoundNoticeState()
    {
      var instruction = string.Empty;
      if (_obj.BlockUid == Constants.DismissalTask.GoRoundNoticeBlockId)
        instruction = Functions.DismissalTask.GetGoRoundInstruction(DismissalTasks.As(_obj.Task));
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}