using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferSignDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class TransferSignDocsAssignmentFunctions
  {
    
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public Sungero.Core.StateView GetSigningAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.TransferTask.SignManagerDocAssignmentBlockId:
          instruction = TransferTasks.Resources.SignSeoAssignmentInstruction;
          break;
        case Constants.TransferTask.SignEmployeeDocAssignmentBlockId:
          instruction = TransferTasks.Resources.SignEmployeeAssignmenInstruction;
          break;
        #region Блок версии V1, для обеспечения совместимости
        case Constants.TransferTask.SignStatementAssignmentBlockId:
          instruction = TransferTasks.Resources.SignStatementAssignmentInstruction;
          break;
        #endregion
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}