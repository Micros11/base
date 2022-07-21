using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.ReworkPersDocAssignment;

namespace DirRX.HRLite.Server
{
  partial class ReworkPersDocAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetReworkPersDocAssignmentState()
    {
      var instruction = string.Empty;
      if (_obj.BlockUid == Constants.HRAcquaintanceTask.ConvertToPdfaByInitiatorBlockId)
        instruction = DirRX.HRLite.ReworkPersDocAssignments.Resources.AuthorConvertErrorsReworkInstructionFormat(HRAcquaintanceTasks.As(_obj.Task).ConvertErrorMessage);
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}