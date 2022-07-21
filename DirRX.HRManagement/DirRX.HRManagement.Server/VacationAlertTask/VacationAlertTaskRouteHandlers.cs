using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using Sungero.Company;
using DirRX.HRManagement.VacationAlertTask;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement.Server
{
  partial class VacationAlertTaskRouteHandlers
  {

    #region Преобразование в Pdf.
    public virtual void Script37Execute()
    {
      // Получить уведомление в Pdf с подстановкой штампа сотрудника
      var notification = _obj.Notification;
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(notification, new List<IEmployee>() { notification.Employee }, new List<string>() { HRManagement.Resources.EmployeeAnchor });
    }
    #endregion

    #region Выдать права на уведомление сотруднику отдела кадров и ответственному по отпускам
    public virtual void Script35Execute()
    {
      var employee = _obj.Vacations.First().Vacation.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      _obj.Notification.AccessRights.Grant(Functions.Module.GetHRResponsible(businessUnit), DefaultAccessRightsTypes.Change);
      _obj.Notification.AccessRights.Grant(Functions.Module.GetVacationResponsible(businessUnit), DefaultAccessRightsTypes.Change);
    }
    #endregion

    #region Подписание инициатором бумажного экземпляра в случае, если отпуск не перенесли
    public virtual void StartBlock31(DirRX.HRManagement.Server.VacationAlertAssignmentArguments e)
    {
      var alertVacations = _obj.Vacations;
      e.Block.Performers.Add(alertVacations.First().Vacation.Employee);
      e.Block.Subject = DirRX.HRManagement.VacationAlertTasks.Resources.PrintAndSingPaperVacationAlertSubject;
    }

    public virtual bool Decision32Result()
    {
      var employee = _obj.Vacations.First().Vacation.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      return Functions.Module.IsAllPaperVacationDoc(businessUnit) || !Functions.Module.IsEmployeeUsingESS(employee);
    }
    #endregion

    #region Прекращение задачи на перенос отпуска
    public virtual void Script30Execute()
    {
      var shiftTask = _obj.ShiftTask;
      if (shiftTask.Status == Sungero.Workflow.Task.Status.InProcess)
      {
        Functions.Module.StopVacationShiftTask(shiftTask);
        _obj.IsShiftAbortedByDeadline = true;
      }
    }
    #endregion

    #region Подтверждение ответственным по отпускам передачи бумажного экземпляра
    public virtual void StartBlock27(DirRX.HRManagement.Server.VacationAlertAssignmentArguments e)
    {
      var alertVacations = _obj.Vacations;
      var employee = alertVacations.First().Vacation.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetVacationResponsible(businessUnit);
      var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, DeclensionCase.Nominative, false);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationAlertTasks.Resources.SignPaperDocAssignmentSubjectFormat(employeeShortName);
    }

    public virtual bool Decision18Result()
    {
      var employee = _obj.Vacations.First().Vacation.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      return Functions.Module.IsAllPaperVacationDoc(businessUnit) || !Functions.Module.IsEmployeeUsingESS(employee);
    }
    #endregion

    #region Перевод отпуска в состояние "Подтвержден" и проставление даты отправки уведомления
    public virtual void Script17Execute()
    {
      foreach (var data in _obj.Vacations)
      {
        var vacation = data.Vacation;
        vacation.Status = HRManagement.Vacation.Status.Approved;
        if (vacation.NoteDate == null)
          vacation.NoteDate = _obj.Started.Value;
      }
      _obj.Vacation.Save();
    }
    #endregion
    
    #region Уведомление руководителю о созданном замещении
    public virtual void StartBlock11(DirRX.HRManagement.Server.VacationManagerNoticeArguments e)
    {
      var alertVacations = _obj.Vacations;
      var employee = _obj.Vacation.Employee;
      if (_obj.Manager != null && !Equals(_obj.Manager, employee))
      {
        e.Block.Performers.Add(_obj.Manager);
        var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, DeclensionCase.Genitive, false);
        e.Block.Subject = VacationAlertTasks.Resources.VacationApprovedSubjectFormat(employeeShortName,
                                                                                     alertVacations.OrderBy(v => v.Vacation.StartDate).First().Vacation.StartDate.Value.ToShortDateString(),
                                                                                     alertVacations.OrderByDescending(v => v.Vacation.FinDate).First().Vacation.FinDate.Value.ToShortDateString());
        
      }
    }
    #endregion

    #region Создание замещения
    public virtual void Script14Execute()
    {
      if (_obj.Substitute != null)
      {
        var vacations = _obj.Vacations;
        _obj.Substitution = Functions.Module.CreateSubstitution(vacations.First().Vacation.Employee,
                                                                _obj.Substitute,
                                                                vacations.OrderBy(v => v.Vacation.StartDate).First().Vacation.StartDate.Value,
                                                                vacations.OrderByDescending(v => v.Vacation.FinDate).First().Vacation.FinDate.Value,
                                                                VacationAlertTasks.Resources.Vacation);
      }
    }
    #endregion
    
    #region Уведомление руководителю
    public virtual void StartBlock10(DirRX.HRManagement.Server.VacationManagerNoticeArguments e)
    {
      var alertVacations = _obj.Vacations;
      var employee = alertVacations.First().Vacation.Employee;
      if (_obj.Manager != null && !Equals(_obj.Manager, employee))
      {
        e.Block.Performers.Add(_obj.Manager);
        var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, DeclensionCase.Genitive, false);
        e.Block.Subject = VacationAlertTasks.Resources.ManagerNoticeSubjectFormat(employeeShortName,
                                                                                  alertVacations.OrderBy(v => v.Vacation.StartDate).First().Vacation.StartDate.Value.ToShortDateString(),
                                                                                  alertVacations.OrderByDescending(v => v.Vacation.FinDate).First().Vacation.FinDate.Value.ToShortDateString());
      }
    }
    #endregion

    #region Подтверждение дат отпуска сотрудником
    public virtual void StartBlock4(DirRX.HRManagement.Server.VacationApprovalAssignmentArguments e)
    {
      var alertVacations = _obj.Vacations;
      var employee = alertVacations.First().Vacation.Employee;
      if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(employee)))
        e.Block.ShowInSelfServiceOffice = true;
      e.Block.Performers.Add(employee);
      e.Block.Subject = VacationAlertTasks.Resources.VacationApprovalAssignmentSubjectFormat(alertVacations.OrderBy(v => v.Vacation.StartDate).First().Vacation.StartDate.Value.ToShortDateString(),
                                                                                             alertVacations.OrderByDescending(v => v.Vacation.FinDate).First().Vacation.FinDate.Value.ToShortDateString());
      if (_obj.Manager != null && !Equals(_obj.Manager, employee))
        e.Block.Substitute = _obj.Manager;
    }
    
    public virtual void CompleteAssignment4(DirRX.HRManagement.IVacationApprovalAssignment assignment, DirRX.HRManagement.Server.VacationApprovalAssignmentArguments e)
    {
      _obj.ShiftTask = assignment.ShiftTask;
      _obj.Substitute = assignment.Substitute;
    }
    #endregion
    
    #region Мониторинг завершения задачи на перенос отпуска
    public virtual void StartBlock6(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      var monitoringPeriod = Functions.Module.GetDocflowParamValue(ParamKeys.AlertMonitoringTimeKey);
      if (string.IsNullOrEmpty(monitoringPeriod))
        e.Block.Period = TimeSpan.FromHours(Constants.VacationAlertTask.DefaultMonitoringPeriod);
      else
        e.Block.Period = TimeSpan.FromSeconds(double.Parse(monitoringPeriod));

      var alertVacation = _obj.Vacation;
      var businessUnit = alertVacation.BusinessUnit;
      var automaticExecutionPeriod = Functions.Module.GetModuleSettings(businessUnit).DaysCountVacationPay.Value + 1;
      var automaticExecutionDay = Functions.Module.GetPaymentDayAfterDate(businessUnit, alertVacation.StartDate.Value.AddDays(-automaticExecutionPeriod)).BeginningOfDay();
      
      e.Block.RelativeDeadline = automaticExecutionDay - Calendar.Now;
    }
    
    public virtual bool Monitoring6Result()
    {
      _obj.IsShiftOrderSigned = Functions.Module.VacationShiftIsComplete(_obj.ShiftTask);
      var isShiftTaskCancel = Functions.Module.VacationShiftIsCancel(_obj.ShiftTask);
      return _obj.IsShiftOrderSigned.Value || isShiftTaskCancel;
    }
    #endregion
    
    #region Перенос отпуска состоялся?
    public virtual bool Decision7Result()
    {
      return _obj.IsShiftOrderSigned.Value;
    }
    #endregion
    
    #region Начисление отпускных бухгалтерией
    public virtual void StartBlock5(DirRX.HRManagement.Server.VacationAlertAssignmentArguments e)
    {
      var alertVacations = _obj.Vacations;
      var employee = alertVacations.First().Vacation.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetAccounter(businessUnit);
      var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, DeclensionCase.Genitive, false);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationAlertTasks.Resources.AccounterAssignmentSubjectFormat(employeeShortName,
                                                                                      alertVacations.OrderBy(v => v.Vacation.StartDate).First().Vacation.StartDate.Value.ToShortDateString(),
                                                                                      alertVacations.OrderByDescending(v => v.Vacation.FinDate).First().Vacation.FinDate.Value.ToShortDateString());
    }
    #endregion
  }
}