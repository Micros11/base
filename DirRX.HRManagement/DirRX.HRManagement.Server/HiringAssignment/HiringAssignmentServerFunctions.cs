using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringAssignment;

namespace DirRX.HRManagement.Server
{
  partial class HiringAssignmentFunctions
  {
    
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public Sungero.Core.StateView GetHiringAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.HiringTask.ITServiceBlockId:
          instruction = HiringTasks.Resources.ITResponsibleAssignmentInstruction;
          break;
        case Constants.HiringTask.SBServiceBlockId:
          instruction = HiringTasks.Resources.SecurityResponsibleAssignmentInstruction;
          break;
        case Constants.HiringTask.AHOServiceBlockId:
          instruction = HiringTasks.Resources.SupplyResponsibleAssignmentInstruction;
          break;
        case Constants.HiringTask.ManagerMeetBlockId:
          instruction = HiringTasks.Resources.ManagerAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}