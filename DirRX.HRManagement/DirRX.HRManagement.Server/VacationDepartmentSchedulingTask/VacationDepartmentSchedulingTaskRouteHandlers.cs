using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using Sungero.Company;
using DirRX.HRManagement.VacationDepartmentSchedulingTask;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement.Server
{
  partial class VacationDepartmentSchedulingTaskRouteHandlers
  {

    #region Планирование отпуска сотрудниками
    public virtual void CompleteAssignment3(DirRX.HRManagement.IVacationEmployeeSchedulingAssignment assignment, DirRX.HRManagement.Server.VacationEmployeeSchedulingAssignmentArguments e)
    {
      var employee = Employees.As(assignment.Performer);
      var year = _obj.Year.Value;
      // Создать отпуска в справочнике.
      foreach (var vacationRow in assignment.Vacations)
      {
        var date = Structures.Vacation.VacationDates.Create(vacationRow.DateBegin.Value, vacationRow.DateEnd.Value, vacationRow.Duration.Value, DateStatuses.New, vacationRow.VacationKind);
        Functions.Module.CreateVacation(date, employee, year, HRManagement.Vacation.Status.OnApproval);
      }
    }

    public virtual void StartAssignment3(DirRX.HRManagement.IVacationEmployeeSchedulingAssignment assignment, DirRX.HRManagement.Server.VacationEmployeeSchedulingAssignmentArguments e)
    {
      if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(assignment.Performer)))
        assignment.ShowInSelfServiceOffice = true;
      
      var vacationPlaningDuration = Functions.Module.GetModuleSettings(_obj.Department.BusinessUnit).VacationPlaningDuration.Value;
      assignment.Deadline = Calendar.Today.AddDays(vacationPlaningDuration);
      
      var emp = DirRX.EssPlatformSolution.Employees.As(assignment.Performer);
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      /*
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent
          && assignment.BlockUid == Constants.VacationDepartmentSchedulingTask.EmployeeSchedulingBlockId)
      {
        // FIXME Отключение "колокола"
        //EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, assignment.ActiveText, "VacationDepartmentSchedulingTask", "VacationEmployeeSchedulingAssignment", new List<Sungero.Content.IElectronicDocument>());
      }
       */
      
    }

    public virtual void StartBlock3(DirRX.HRManagement.Server.VacationEmployeeSchedulingAssignmentArguments e)
    {
      e.Block.Year = _obj.Year.Value.Year;
      var employees = Employees.GetAll(m => m.Department.Equals(_obj.Department) && m.Status == Sungero.Company.Employee.Status.Active).ToList();
      foreach (var employee in employees)
        e.Block.Performers.Add(employee);
      e.Block.Subject = VacationDepartmentSchedulingTasks.Resources.EmployeeSchedulingSubjectFormat(_obj.Year.Value.Year);
    }
    #endregion
    
    #region Проверка отпусков руководителем
    public virtual void StartBlock5(DirRX.HRManagement.Server.VacationManagerCheckAssignmentArguments e)
    {
      e.Block.Performers.Add(Functions.Module.GetManager(_obj.Department));
      e.Block.Subject = VacationDepartmentSchedulingTasks.Resources.ManagerCheckSubjectFormat(_obj.Year.Value.Year, _obj.Department.Name);
    }
    
    public virtual void StartAssignment5(DirRX.HRManagement.IVacationManagerCheckAssignment assignment, DirRX.HRManagement.Server.VacationManagerCheckAssignmentArguments e)
    {
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(Employees.As(assignment.Performer));
      var vacationCheckDuration = Functions.Module.GetModuleSettings(businessUnit).VacationCheckDuration.Value;
      assignment.Deadline = Calendar.Today.AddDays(vacationCheckDuration);
    }
    
    public virtual void CompleteAssignment5(DirRX.HRManagement.IVacationManagerCheckAssignment assignment, DirRX.HRManagement.Server.VacationManagerCheckAssignmentArguments e)
    {
      Functions.Module.AddDocflowParam(string.Format(Constants.Module.ParamKey.SchedulingKeyDepartment, _obj.Department.Id), false.ToString());
    }
    #endregion
    
    #region Уведомление сотрудникам об отпусках
    public virtual void StartBlock8(DirRX.HRManagement.Server.VacationEmployeeNoticeArguments e)
    {
      var employees = Employees.GetAll(m => m.Department.Equals(_obj.Department) && m.Status == Sungero.Company.Employee.Status.Active);
      foreach (var employee in employees)
        e.Block.Performers.Add(employee);
      e.Block.Subject = VacationDepartmentSchedulingTasks.Resources.DepartmentSchedulingEndFormat(_obj.Year.Value.Year, _obj.Department);
    }
    #endregion
    
    #region Уведомление ответственному о завершении
    public virtual void StartBlock9(Sungero.Workflow.Server.NoticeArguments e)
    {
      var perfomer = Functions.Module.GetVacationResponsible(_obj.Department.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationDepartmentSchedulingTasks.Resources.DepartmentSchedulingEndFormat(_obj.Year.Value.Year, _obj.Department);
    }
    #endregion

  }
}