using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using Sungero.Workflow;
using DirRX.HRManagement.VacationShiftTask;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class VacationShiftTaskRouteHandlers
  {

    public virtual bool Decision39Result()
    {
      // Условие выполняется если задача инициирована руководителем (не сотрудником) из задания на подтверждение отпуска (VacationApprovalAssignment)
      var byEmloyee = Equals(_obj.Employee, Employees.As(_obj.Author));
      var isApprovalAssignment = VacationApprovalAssignments.Is(_obj.ApprovalAssignment);
      return isApprovalAssignment && !byEmloyee;
    }
    #region Преобразование в Pdf.
    public virtual void Script36Execute()
    {
      // Получить заявление в Pdf с подстановкой штампа сотрудника
      var statement = _obj.Statement;
      // Задать списки для сотрудника и якорей подписей
      var employees = new List<IEmployee>() { statement.Employee };

      Logger.DebugFormat("==>> Script36Execute statement.Employee {0}", statement.Employee);
      
      var employeeAnchors = new List<string>() { HRManagement.Resources.EmployeeAnchor };
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(statement, employees, employeeAnchors);

      // Получить приказ в Pdf с подстановкой штампов
      var order = _obj.Order;
      // Задать списки для подписантов со стороны организации и якорей подписей
      var ourSignatories = new List<IEmployee>() { order.OurSignatory };
      Logger.DebugFormat("==>> Script36Execute order.OurSignatory {0}", order.OurSignatory);

      var ourSignatoryAnchors = new List<string>() { HRManagement.Resources.ManagerAnchor };
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(order, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
    }
    #endregion
    
    #region Доработка-V3, устаревшее
    public virtual void StartAssignment35(DirRX.HRManagement.IShiftStatementApprovalAssignment assignment, DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      Functions.VacationShiftTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
    }

    public virtual void CompleteAssignment35(DirRX.HRManagement.IShiftStatementApprovalAssignment assignment, DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      Functions.VacationShiftTask.FillVacationDataFromAssignmentToTask(_obj, assignment);
    }

    public virtual void StartBlock35(DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      e.Block.Performers.Add(Functions.Module.GetManager(_obj.Employee));
      e.Block.Subject = VacationShiftTasks.Resources.RecallReApproveInitiatorAssignmentSubjectFormat(_obj.RecallDate.Value.ToShortDateString());
    }
    #endregion
    
    #region Согласование заявления сотрудником - V3, устаревшее
    public virtual void StartBlock31(DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = VacationShiftTasks.Resources.RecallEmployeeApprovalAssignmentSubjectFormat(_obj.RecallDate.Value.ToShortDateString());
    }

    public virtual void StartAssignment31(DirRX.HRManagement.IShiftStatementApprovalAssignment assignment, DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      Functions.VacationShiftTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
    }
    #endregion

    public virtual bool Decision30Result()
    {
      return Equals(_obj.Employee, Employees.As(_obj.Author));
    }

    #region Доработка параметров переноса сотрудником
    public virtual void StartAssignment22(DirRX.HRManagement.IShiftStatementApprovalAssignment assignment, DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      Functions.VacationShiftTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
      
      // FIXME Отключение "колоколов"
      /*
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      // HACK Если задание не на отзыв из отпуска.
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent
          && assignment.BlockUid == Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId
          && !VacationShiftTasks.As(assignment.Task).IsVacationRecall.Value)
      {
        var vacation = VacationShiftTasks.As(assignment.Task).Vacation1.DisplayValue;
        //FIXME Падает ошибка при определении reason, если руководитель делает перенос отпуска за сотрудника. При подключённом ЛК у сотрудника.
        var reason = ShiftStatementApprovalAssignments.GetAll(x => x.Task.Id == assignment.Task.Id && x.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId)
          .OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
        var content = DirRX.HRManagement.VacationShiftTasks.Resources.DeniedPersonnalOfficeNoticeFormat(vacation, reason);
        
        //EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, content, "VacationShiftTask", "ShiftStatementApprovalAssignment", new List<Sungero.Content.IElectronicDocument>());
      }
       */
    }

    public virtual void CompleteAssignment22(DirRX.HRManagement.IShiftStatementApprovalAssignment assignment, DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      if (assignment.Result.Value == DirRX.HRManagement.ShiftStatementApprovalAssignment.Result.ForReapproving)
        Functions.VacationShiftTask.FillVacationDataFromAssignmentToTask(_obj, assignment);
    }

    public virtual void StartBlock22(DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      if (!_obj.IsVacationRecall.Value)
        if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Employee)))
          e.Block.ShowInSelfServiceOffice = true;

      e.Block.Performers.Add(_obj.Employee);
      if (_obj.IsVacationRecall.Value)
        e.Block.Subject = VacationShiftTasks.Resources.RecallReApproveInitiatorAssignmentSubjectFormat(_obj.RecallDate.Value.ToShortDateString());
      else
        e.Block.Subject = VacationShiftTasks.Resources.ShiftReApproveInitiatorAssignmentSubject;
      
    }
    #endregion
    
    #region Внесение изменений в график отпусков.
    public virtual void Script23Execute()
    {
      // Если перенос стартован из отзыва и уже идет на дату отзыва, создать отпуск для того периода, который сотрудник использует.
      if (_obj.IsVacationRecall.Value)
      {
        var vacation = _obj.Vacation1;
        var substitution = Substitutions.GetAll(l => l.User == vacation.Employee && l.StartDate == vacation.StartDate && l.EndDate == vacation.FinDate).FirstOrDefault();
        
        if (_obj.RecallDate > vacation.StartDate)
        {
          var duration = Functions.Module.DurationInCalendarDaysWihtOutHoliday(vacation.StartDate.Value, _obj.RecallDate.Value.AddDays(-1), _obj.Employee);
          var vacationDates = Structures.Vacation.VacationDates.Create(vacation.StartDate.Value, _obj.RecallDate.Value.AddDays(-1), duration, DateStatuses.New, vacation.VacationKind);
          Functions.Module.CreateVacation(vacationDates, vacation.Employee, vacation.Year.Value, vacation.Status.Value);

          if (substitution != null)
          {
            substitution.EndDate = _obj.RecallDate.Value.AddDays(-1);
            substitution.Save();
          }
        }
        else
        {
          if (substitution != null)
          {
            var tasks = VacationAlertTasks.GetAll(t => t.Substitution == substitution);
            foreach (var task in tasks)
              task.Substitution = Substitutions.Null;
            Substitutions.Delete(substitution);
          }
        }
      }
      
      // Изменить статус перенесенного/отозванного отпуска.
      var shiftVacations = new List<IVacation>();
      if (_obj.Vacation1 != null)
        shiftVacations.Add(_obj.Vacation1);
      if (_obj.Vacation2 != null)
        shiftVacations.Add(_obj.Vacation2);
      if (_obj.Vacation3 != null)
        shiftVacations.Add(_obj.Vacation3);
      
      foreach (var shiftVacation in shiftVacations)
      {
        if (_obj.VacationDurations.Any(v => Equals(v.VacationKind, shiftVacation.VacationKind) && v.DaysPlaned == v.DaysComped && v.DaysUsed == 0))
          shiftVacation.Status = DirRX.HRManagement.Vacation.Status.Closed;
        else if (_obj.VacationDurations.Any(v => Equals(v.VacationKind, shiftVacation.VacationKind) && v.DaysPlaned == v.DaysCanceled))
          shiftVacation.Status = DirRX.HRManagement.Vacation.Status.Canceled;
        else
          shiftVacation.Status = DirRX.HRManagement.Vacation.Status.Shifted;
        shiftVacation.Save();
      }
      
      // Создать новые записи справочника Отпуска согласно введенным датам.
      var newVacationsDates = new List<Structures.Vacation.IVacationDates>();
      if (_obj.DataBegin1.HasValue && _obj.Duration1.HasValue)
        newVacationsDates.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin1.Value, _obj.DataEnd1.Value, _obj.Duration1.Value, DateStatuses.New, _obj.VacationKind1));
      if (_obj.DataBegin2.HasValue && _obj.Duration2.HasValue)
        newVacationsDates.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin2.Value, _obj.DataEnd2.Value, _obj.Duration2.Value, DateStatuses.New, _obj.VacationKind2));
      if (_obj.DataBegin3.HasValue && _obj.Duration3.HasValue)
        newVacationsDates.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin3.Value, _obj.DataEnd3.Value, _obj.Duration3.Value, DateStatuses.New, _obj.VacationKind3));
      
      foreach (var newVacationsDate in newVacationsDates)
        Functions.Module.CreateVacation(newVacationsDate, _obj.Employee, _obj.Year.Value, HRManagement.Vacation.Status.Active);
      
      // Если отпуск компенсирован частично (перенесен не весь или уже частично использован), создать отпуск продолжительностью равной количеству дней компенсации.
      foreach (var vacKind in _obj.VacationDurations.Where(v => v.DaysComped != 0 && (v.DaysPlaned != v.DaysComped || v.DaysUsed > 0)))
      {
        var vacDates = shiftVacations.Where(v => Equals(v.VacationKind, vacKind.VacationKind)).FirstOrDefault();
        var dateBegin = _obj.IsVacationRecall.Value ? _obj.RecallDate.Value : vacDates.StartDate.Value;
        var dateEnd = Functions.Module.GetVacationEndDate(dateBegin, vacKind.DaysComped.Value, _obj.Employee);
        var vacationDates = Structures.Vacation.VacationDates.Create(dateBegin, dateEnd, vacKind.DaysComped.Value, DateStatuses.New, vacDates.VacationKind);
        Functions.Module.CreateVacation(vacationDates, _obj.Employee, _obj.Year.Value, HRManagement.Vacation.Status.Closed);
      }
    }
    #endregion Внесение изменений в график отпусков.
    
    #region Подготовка и подписание приказа на перенос отпуска.
    
    public virtual bool Decision18Result()
    {
      return Functions.Module.IsAllPaperVacationDoc(_obj.Employee.Department.BusinessUnit) || !Functions.Module.IsEmployeeUsingESS(_obj.Employee);
    }

    public virtual void StartBlock19(DirRX.HRManagement.Server.ShiftPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = _obj.IsVacationRecall.Value ?
        VacationShiftTasks.Resources.SignAllPaperRecallDocAssignmentSubjectFormat(employee.Name) :
        VacationShiftTasks.Resources.SignAllPaperDocAssignmentSubjectFormat(employee.Name);
    }
    
    public virtual void StartBlock8(DirRX.HRManagement.Server.ShiftPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = _obj.IsVacationRecall.Value ?
        VacationShiftTasks.Resources.PrepareRecallOrderAssignmentSubjectFormat(employee.Name) :
        VacationShiftTasks.Resources.PrepareOrderAssignmentSubjectFormat(employee.Name);
      _obj.Order = Functions.VacationShiftTask.CreateShiftVacationOrder(_obj);
    }
    
    public virtual void StartBlock9(DirRX.HRManagement.Server.ShiftSignDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = _obj.IsVacationRecall.Value ?
        VacationShiftTasks.Resources.SignAssignmentRecallSubjectFormat(employee.Name) :
        VacationShiftTasks.Resources.SignAssignmentSubjectFormat(employee.Name);
    }
    
    public virtual void StartBlock10(DirRX.HRManagement.Server.ShiftPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = _obj.IsVacationRecall.Value ?
        VacationShiftTasks.Resources.VacationRecallDocsReWorkAssignmentSubjectFormat(employee.Name) :
        VacationShiftTasks.Resources.VacationDocsReWorkAssignmentSubjectFormat(employee.Name);
    }
    
    public virtual void StartBlock12(DirRX.HRManagement.Server.ShiftSignDocsAssignmentArguments e)
    {
      if (!_obj.IsVacationRecall.Value)
        if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Employee)))
          e.Block.ShowInSelfServiceOffice = true;
      
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = _obj.IsVacationRecall.Value ?
        VacationShiftTasks.Resources.SignEmployeeRecallAssignmentSubject :
        VacationShiftTasks.Resources.SignEmployeeAssignmentSubject;
    }
    
    public virtual void StartAssignment12(DirRX.HRManagement.IShiftSignDocsAssignment assignment, DirRX.HRManagement.Server.ShiftSignDocsAssignmentArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      // HACK Если задание не на отзыв из отпуска.
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent
          && assignment.BlockUid == Constants.VacationShiftTask.EmployeeSignOrderAssignmentBlockId
          && !VacationShiftTasks.As(assignment.Task).IsVacationRecall.Value)
      {
        var task = HRManagement.VacationShiftTasks.As(_obj);
        var document = HRManagement.Functions.VacationShiftTask.GetEmployeeOrder(task);
        // FIXME Отключение "колоколов"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, assignment.ActiveText, "VacationShiftTask", "ShiftSignDocsAssignment", document);
      }
    }
    #endregion
    
    #region Согласование заявления на перенос.
    
    public virtual void StartNotice37(DirRX.HRManagement.IShiftNotice notice, DirRX.HRManagement.Server.ShiftNoticeArguments e)
    {
      var employee = DirRX.HRSolution.Employees.As(notice.Author);
      if (employee.PersonalAccountStatusDirRX != DirRX.HRSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
      {
        var vacation = VacationShiftTasks.As(notice.Task).Vacation1.DisplayValue;
        var reason = ShiftStatementApprovalAssignments.GetAll(x => x.Task.Id == notice.Task.Id && x.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId)
          .OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
        var content = DirRX.HRManagement.VacationShiftTasks.Resources.DeniedPersonnalOfficeNoticeFormat(vacation, reason);
        // FIXME Отключение "колоколов"
        // DirRX.EssPlatform.PublicFunctions.Module.SendEssNotification(employee, e.Block.Subject, content, "VacationShiftTask", "Notice", new List<Sungero.Content.IElectronicDocument>());
      }
    }

    public virtual void StartBlock37(DirRX.HRManagement.Server.ShiftNoticeArguments e)
    {
      if (!_obj.IsVacationRecall.Value)
        if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Author)))
          e.Block.ShowInSelfServiceOffice = true;

      e.Block.Performers.Add(_obj.Author);
      if (Equals(_obj.Employee, Employees.As(_obj.Author)))
      {
        if (_obj.IsVacationRecall.Value)
          e.Block.Subject = VacationShiftTasks.Resources.RecallDeniedNoticeSubject;
        else
          e.Block.Subject = VacationShiftTasks.Resources.ShiftDeniedNoticeSubject;
      }
      else
      {
        if (_obj.IsVacationRecall.Value)
          e.Block.Subject = VacationShiftTasks.Resources.RecallDeniedEmployeeNoticeSubjectFormat(_obj.Employee.Name);
        else
          e.Block.Subject = VacationShiftTasks.Resources.ShiftDeniedEmployeeNoticeSubjectFormat(_obj.Employee.Name);
      }
    }
    
    public virtual void StartBlock4(DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      var manager = Functions.Module.GetManager(_obj.Employee);
      if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(manager)))
        e.Block.ShowInSelfServiceOffice = false;
      
      e.Block.Performers.Add(manager);
      if (!_obj.IsVacationRecall.Value)
        e.Block.Subject = VacationShiftTasks.Resources.ShiftApprovalAssignmentSubjectFormat(_obj.Employee.Name);
      else
        e.Block.Subject = VacationShiftTasks.Resources.RecallApprovalAssignmentSubjectFormat(_obj.RecallDate.Value.ToShortDateString(), _obj.Employee.Name);
    }

    public virtual void StartAssignment4(DirRX.HRManagement.IShiftStatementApprovalAssignment assignment, DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      Functions.VacationShiftTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
    }
    
    public virtual void CompleteAssignment4(DirRX.HRManagement.IShiftStatementApprovalAssignment assignment, DirRX.HRManagement.Server.ShiftStatementApprovalAssignmentArguments e)
    {
      if (assignment.Result.Value == DirRX.HRManagement.ShiftStatementApprovalAssignment.Result.Refuse)
        Functions.VacationShiftTask.FillVacationDataFromAssignmentToTask(_obj, assignment);
    }

    public virtual void StartBlock14(DirRX.HRManagement.Server.ShiftSignDocsAssignmentArguments e)
    {
      if (!_obj.IsVacationRecall.Value)
        if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Employee)))
          e.Block.ShowInSelfServiceOffice = true;
      
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = _obj.IsVacationRecall.Value ?
        VacationShiftTasks.Resources.SignRecallStatementAssignmentSubjectFormat(_obj.RecallDate.Value.ToShortDateString()) :
        VacationShiftTasks.Resources.SignStatementAssignmentSubject;
      _obj.Statement = Functions.VacationShiftTask.CreateShiftVacationStatement(_obj);
    }
    
    public virtual void StartAssignment14(DirRX.HRManagement.IShiftSignDocsAssignment assignment, DirRX.HRManagement.Server.ShiftSignDocsAssignmentArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      // HACK Если задание не на отзыв из отпуска.
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent
          && assignment.BlockUid == Constants.VacationShiftTask.SignStatementAssignmentBlockId
          && !VacationShiftTasks.As(assignment.Task).IsVacationRecall.Value)
      {
        var task = HRManagement.VacationShiftTasks.As(_obj);
        var document = HRManagement.Functions.VacationShiftTask.GetEmployeeStatement(task);
        // FIXME Отключение "колоколов"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, assignment.ActiveText, "VacationShiftTask", "ShiftSignDocsAssignment", document);
      }
    }
    #endregion
  }
}