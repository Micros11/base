using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.Company;
using Sungero.CoreEntities;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Отправить уведомления о заданиях тем, кто замещает одну персону (совместителям).
    /// </summary>
    public virtual void JobSendNotificationToWhoSubstitute()
    {
      Functions.Module.SendNotificationToWhoSubstitute();
    }
    /// <summary>
    /// Автоматическое выполнение просроченных заданий на планирование отпуска.
    /// </summary>
    public virtual void AutoExecutionSchedulingAssignment()
    {
      var assignments = VacationEmployeeSchedulingAssignments.GetAll(l => l.Status == HRManagement.VacationEmployeeSchedulingAssignment.Status.InProcess &&
                                                                     l.Deadline < Calendar.Today);
      foreach (var assignment in assignments)
      {
        if (!Locks.GetLockInfo(assignment).IsLocked)
        {
          assignment.Vacations.Clear();
          var task = VacationDepartmentSchedulingTasks.As(assignment.Task);
          var year = task.Year.Value;
          var employee = Employees.As(assignment.Performer);
          var employeeVacationDuration = Functions.Module.GetEmployeeVacationDuration(employee, year);
          if (employeeVacationDuration != null)
          {
            var endOfYear = year.EndOfYear();
            var dateEnd = endOfYear;
            var newDateBegin = dateEnd;
            foreach (var availableDuration in employeeVacationDuration.AvailableDuration)
            {
              var daysUnPlaned = availableDuration.DaysCount.Value;
              var newRow = assignment.Vacations.AddNew();
              newDateBegin = dateEnd.AddDays(-daysUnPlaned + 1);
              newRow.DateBegin = newDateBegin;
              newRow.Duration = daysUnPlaned;
              newRow.VacationKind = availableDuration.VacationKind;
              dateEnd = newDateBegin.AddDays(-1);
            }
            assignment.ActiveText = Resources.SchedulingAssignmentAutoCompleteFormat(newDateBegin.ToShortDateString(), endOfYear.ToShortDateString());
          }
          else
            assignment.ActiveText = Resources.NoVacationDurationRecord;
          assignment.Complete(HRManagement.VacationEmployeeSchedulingAssignment.Result.Complete);
        }
      }
    }
    
    /// <summary>
    /// Отправка задач на планирование отпуска.
    /// </summary>
    public virtual void SchedulingTaskStart()
    {
      var businessUnits = BusinessUnits.GetAll(l => l.Status == Sungero.Company.BusinessUnit.Status.Active);
      var schedulingYear = Calendar.Today.NextYear().Year;
      foreach (var businessUnit in businessUnits)
      {
        var paramName = string.Format(ParamKeys.SchedulingKeyBusinessUnit, businessUnit.Id);
        // Если в НОР не идет процесс планирования отпусков, то отправить задачу.
        if (!Equals(Functions.Module.GetDocflowParamValue(paramName), true.ToString()))
        {
          var task = VacationSchedulingTasks.Create();
          task.BusinessUnit = businessUnit;
          task.Start();
        }
      }
    }

    /// <summary>
    /// Изменение cостояния отпусков на "Исполняется" и "Исполнен".
    /// </summary>
    public virtual void VacationSwitchStatus()
    {
      var today = Calendar.Today;
      var allApprovedVacations = Vacations.GetAll(l => l.StartDate <= today && today <= l.FinDate && Equals(l.Status, HRManagement.Vacation.Status.Approved));
      Functions.Module.VacationSwitchStatus(allApprovedVacations, HRManagement.Vacation.Status.Performing);
      var allPerformingVacations = Vacations.GetAll(l => l.FinDate < today && Equals(l.Status, HRManagement.Vacation.Status.Performing));
      Functions.Module.VacationSwitchStatus(allPerformingVacations, HRManagement.Vacation.Status.Closed);
    }
    
    /// <summary>
    /// Агент удаления документов.
    /// </summary>
    public virtual void DocumentDeleteJob()
    {
      var queueItems = DocumentDeleteQueueItems.GetAll()
        .Where(x => x.Retries == 0 && x.ProcessingStatus != HRManagement.DocumentDeleteQueueItem.ProcessingStatus.Processed)
        .ToList();
      
      // Ошибочные документы обрабатываются последними пачкой по 25.
      var repetedQueueItems = DocumentDeleteQueueItems.GetAll().Where(x => x.Retries > 0).OrderBy(y => y.Retries).Take(25).ToList();
      queueItems.AddRange(repetedQueueItems);
      
      foreach (var queueItem in queueItems)
      {
        // Удалять документы, созданные больше 10 минут назад, сделано на случай, если агент запустится до того, как временный отчет откроется у пользователя.
        var document = Sungero.Docflow.OfficialDocuments.GetAll(d => Equals(d.Id, queueItem.DocumentId) && d.Created < Calendar.UserNow.AddMinutes(-10)).FirstOrDefault();
        
        if (document == null)
          continue;
        
        var lockInfo = Locks.GetLockInfo(document);
        if (lockInfo.IsLocked)
          continue;
        
        var generated = Transactions.Execute(
          () =>
          {
            Functions.Module.DocumentDelete(document);
          });
        if (!generated)
        {
          Transactions.Execute(
            () =>
            {
              Sungero.ExchangeCore.PublicFunctions.QueueItemBase.QueueItemOnError(queueItem, Resources.DocumentDeleteFailed);
            });
          Logger.DebugFormat("{0} Id = '{1}'.", Resources.DocumentDeleteFailed, queueItem.DocumentId);
        }
        else
        {
          Transactions.Execute(
            () =>
            {
              DocumentDeleteQueueItems.Delete(queueItem);
            });
        }
      }
      
      // Оставляем удаление завершенных элементов очереди для старых записей.
      var processedQueueItems = DocumentDeleteQueueItems.GetAll()
        .Where(q => Equals(q.ProcessingStatus, HRManagement.DocumentDeleteQueueItem.ProcessingStatus.Processed))
        .ToList();
      foreach (var processedQueueItem in processedQueueItems)
      {
        DocumentDeleteQueueItems.Delete(processedQueueItem);
      }
    }
    
    /// <summary>
    /// Отправка задач на подтверждение отпуска по графику.
    /// Отпуск имеет статус Запланирован, создан не по ссылке Оформить заявление на отпуск, дата начала меньше сегодняшней на кол-во дней, указанных в настройке.
    /// </summary>
    public virtual void VacationMonitoring()
    {
      var vacations = Functions.Module.GetVacationForAlert();
      foreach (var vacation in vacations)
        Functions.Module.CreateAndStartAlertTask(vacation);
    }
    
    /// <summary>
    /// Автоматическое выполнение задания с уведомлением о начале отпуска.
    /// </summary>
    public virtual void AutoExecutionVacationApprovalAssignment()
    {
      var vacationApprovalAssignment = VacationApprovalAssignments.GetAll().Where(v => v.Status.Value == Sungero.Workflow.AssignmentBase.Status.InProcess);
      var today = Calendar.Today;
      
      foreach (var assignment in vacationApprovalAssignment)
      {
        if (!Locks.GetLockInfo(assignment).IsLocked)
        {
          var vacation = VacationAlertTasks.As(assignment.Task).Vacation;
          if (vacation == null)
            continue;
          var businessUnit = vacation.BusinessUnit;
          var automaticExecutionPeriod = Functions.Module.GetModuleSettings(businessUnit).DaysCountVacationPay.Value + 1;
          var automaticExecutionDay = Functions.Module.GetPaymentDayBeforeDate(businessUnit, vacation.StartDate.Value.AddDays(-automaticExecutionPeriod)).BeginningOfDay();
          if (today >= automaticExecutionDay)
            assignment.Complete(HRManagement.VacationApprovalAssignment.Result.Complete);
        }
      }
    }
  }
}