using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSignDocsAssignment;

namespace DirRX.HRManagement.Server
{
  partial class VacationSignDocsAssignmentFunctions
  {
    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetVacationOrderSigningAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationApprovalTask.SignInitiatorStatementBlockId:
          instruction = VacationApprovalTasks.Resources.SignStatementAssignmentInstruction;
          var task = VacationApprovalTasks.As(_obj.Task);
          if (!Equals(task.Substitute, task.FirstSubstituteValue))
          {
            if (task.Substitute == null)
              instruction += Environment.NewLine + VacationApprovalTasks.Resources.SubstituteWasRemovedInstruction;
            else
            {
              var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(task.Substitute, DeclensionCase.Nominative, false);
              if (task.FirstSubstituteValue == null)
                instruction += Environment.NewLine + VacationApprovalTasks.Resources.SubstituteWasCreatedInstructionFormat(employeeShortName);
              else
                instruction += Environment.NewLine + VacationApprovalTasks.Resources.SubstituteWasChangedInstructionFormat(employeeShortName);
            }
          }
          break;
        case Constants.VacationApprovalTask.SignSeoDocAssignmentBlockId:
          instruction = VacationApprovalTasks.Resources.SignSeoAssignmentInstruction;
          break;
        case Constants.VacationApprovalTask.SignInitiatorOrderBlockId:
          instruction = VacationApprovalTasks.Resources.SignEmployeeAssignmenInstruction;
          break;
      }
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}