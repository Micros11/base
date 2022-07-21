using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferServicesAssignment;

namespace DirRX.HRManagement.Server
{
  partial class TransferServicesAssignmentFunctions
  {
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetTransferServicesAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.TransferServicesTask.AHOServiceBlockId:
          instruction = TransferServicesAssignments.Resources.SupplyResponsibleAssignmentInstruction;
          break;
        case Constants.TransferServicesTask.ITServiceBlockId:
          instruction = TransferServicesAssignments.Resources.ITResponsibleAssignmentInstruction;
          break;
        case Constants.TransferServicesTask.SBServiceBlockId:
          instruction = TransferServicesAssignments.Resources.SecurityResponsibleAssignmentInstruction;
          break;
        case Constants.TransferServicesTask.AccountBlockId:
          instruction = TransferServicesAssignments.Resources.AccountResponsibleAssignmentInstruction;
          break;
        case Constants.TransferServicesTask.CWCAccountBlockId:
          instruction = TransferServicesAssignments.Resources.AccountResponsibleAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}