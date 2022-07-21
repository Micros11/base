using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringSignDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class HiringSignDocsAssignmentFunctions
  {
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public Sungero.Core.StateView GetEmplContractSignAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.HiringTask.SignDocBySeoBlockId:
          instruction = HiringTasks.Resources.SignSeoOrderAndEmploymentContractAssignmentInstruction;
          break;
        case Constants.HiringTask.SignDocByEmployeeBlockId:
          instruction = HiringTasks.Resources.SignEmploymentContractAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}