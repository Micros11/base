using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferAssignment;

namespace DirRX.HRManagement.Server
{
  partial class TransferAssignmentFunctions
  {

    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetTransferAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        #region Блоки версии V1, для обеспечения совместимости
        case Constants.TransferTask.AHOServiceBlockId:
          instruction = TransferTasks.Resources.SupplyResponsibleAssignmentInstruction;
          break;
        case Constants.TransferTask.ITServiceBlockId:
          instruction = TransferTasks.Resources.ITResponsibleAssignmentInstruction;
          break;
        case Constants.TransferTask.SBServiceBlockId:
          instruction = TransferTasks.Resources.SecurityResponsibleAssignmentInstruction;
          break;
        case Constants.TransferTask.AccountBlockId:
          instruction = TransferTasks.Resources.AccountResponsibleAssignmentInstruction;
          break;
        #endregion
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
    
  }
}