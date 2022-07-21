using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationStatementApprovalAssignment;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement.Server
{
  partial class VacationStatementApprovalAssignmentFunctions
  {
    /// <summary>
    /// Получить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetVacationStatementApprovalAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.VacationApprovalTask.ApprovingBlockId:
          instruction = VacationApprovalTasks.Resources.ApprovalAssignmentInstruction;
          break;
        case Constants.VacationApprovalTask.InitiatorReApprovingAssignmentBlockId:
          instruction = VacationApprovalTasks.Resources.ReApproveInitiatorAssignmentInstruction;
          break;
      }
      return Functions.Module.GetAssignmentStateView(instruction);
    }

    #region Проверки по отпускам.
    
    /// <summary>
    /// Проверить правильность всех указанных дат отпуска.
    /// </summary>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckAllVacationDates()
    {
      var vacationApprovalTask = VacationApprovalTasks.As(_obj.Task);
      var vacationDateList = this.CreateVacationDateStructure();
      var errorList = Functions.VacationApprovalTask.CheckCriticalVacationsDates(vacationApprovalTask, vacationDateList);
      
      return errorList;
    }

    /// <summary>
    /// Создать структуру для хранения периодов отпусков.
    /// </summary>
    /// <returns>Список дат отпусков.</returns>
    [Public, Remote]
    public virtual List<Structures.Vacation.IVacationDates> CreateVacationDateStructure()
    {
      var vacationDateList = new List<Structures.Vacation.IVacationDates>();
      vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DateBegin.Value,
                                                                    _obj.DateEnd.Value,
                                                                    _obj.Duration.Value,
                                                                    DateStatuses.New,
                                                                    _obj.VacationKind));
      
      var vacationApprovalTask = VacationApprovalTasks.As(_obj.Task);
      // Добавить к проверяемым датам даты оплачиваемых отпусков.
      var vacationList = Functions.Vacation.GetPaidVacations(vacationApprovalTask.Employee, vacationApprovalTask.Year.Value);
      vacationDateList.AddRange(Functions.Module.FormVacationDateStructure(vacationList));
      return vacationDateList;
    }
    #endregion

  }
}