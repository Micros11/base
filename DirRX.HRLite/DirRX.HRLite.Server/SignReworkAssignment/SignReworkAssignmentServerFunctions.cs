using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignReworkAssignment;

namespace DirRX.HRLite.Server
{
  partial class SignReworkAssignmentFunctions
  {

    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetSignReworkAssignmentState()
    {
      var instruction = string.Empty;
      if (_obj.BlockUid == Constants.SignTask.ConvertToPdfaByInitiatorBlockId)
        instruction = DirRX.HRLite.SignReworkAssignments.Resources.AuthorConvertErrorsReworkInstructionFormat(SignTasks.As(_obj.Task).ConvertErrorMessage);
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}