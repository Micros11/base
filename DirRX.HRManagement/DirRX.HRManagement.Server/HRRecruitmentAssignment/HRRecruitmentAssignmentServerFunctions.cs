using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRRecruitmentAssignment;

namespace DirRX.HRManagement.Server
{
  partial class HRRecruitmentAssignmentFunctions
  {
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetHRRecruitmentAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.RecruitmentTask.HRMeetingBlockId:
          instruction = RecruitmentTasks.Resources.PlanMeetingHRAssignmentInstruction;
          break;
        case Constants.RecruitmentTask.HRMeetingResultBlockId:
          instruction = RecruitmentTasks.Resources.PlanSeoAssignmentInstruction;
          break;
        case Constants.RecruitmentTask.HRSBResultBlockId:
          instruction = RecruitmentTasks.Resources.PlanSecurityAssignmentInstruction;
          break;
        case Constants.RecruitmentTask.HiringBlockId:
          instruction = RecruitmentTasks.Resources.PrepareHiringAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}