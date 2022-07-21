using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.RecruitmentAssignment;

namespace DirRX.HRManagement.Server
{
  partial class RecruitmentAssignmentFunctions
  {

    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetRecruitmentAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.RecruitmentTask.ManagerBlockId:
          instruction = RecruitmentTasks.Resources.ManagerAssignmentInstruction;
          break;
        case Constants.RecruitmentTask.ManagerReportBlockId:
          instruction = RecruitmentTasks.Resources.ManagerReportAssignmentInstruction;
          break;
        case Constants.RecruitmentTask.ApproveBySeoBlockId:
          instruction = RecruitmentTasks.Resources.SeoReportAssignmentInstruction;
          break;
        case Constants.RecruitmentTask.SBBlockId:
          instruction = RecruitmentTasks.Resources.SecurityAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}