using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.HRManagement.VacationApprovalTask;

namespace DirRX.HRManagement.Server
{
  partial class VacationApprovalTaskRouteHandlers
  {

    public virtual void StartAssignment9(DirRX.HRManagement.IVacationSignDocsAssignment assignment, DirRX.HRManagement.Server.VacationSignDocsAssignmentArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent && assignment.BlockUid == Constants.VacationApprovalTask.SignInitiatorOrderBlockId)
      {
        var task = HRManagement.VacationApprovalTasks.As(_obj);
        var document = HRManagement.Functions.VacationApprovalTask.GetEmployeeOrder(task);
        // FIXME Отключение "колокола"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, assignment.ActiveText, "VacationApprovalTask", "VacationSignDocsAssignment", document);
      }
    }

    public virtual void StartAssignment43(DirRX.HRManagement.IVacationSignDocsAssignment assignment, DirRX.HRManagement.Server.VacationSignDocsAssignmentArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent && assignment.BlockUid == Constants.VacationApprovalTask.SignInitiatorStatementBlockId)
      {
        var task = HRManagement.VacationApprovalTasks.As(_obj);
        var document = HRManagement.Functions.VacationApprovalTask.GetEmployeeStatement(task);
        // FIXME Отключение "колокола"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, assignment.ActiveText, "VacationApprovalTask", "VacationSignDocsAssignment", document);
      }
    }

    #region Преобразование в Pdf.
    public virtual void Script45Execute()
    {
      // Получить заявление в Pdf с подстановкой штампа сотрудника
      var statement = _obj.Statement;
      // Задать списки для сотрудника и якорей подписей
      var employees = new List<IEmployee>() { statement.Employee };
      var employeeAnchors = new List<string>() { HRManagement.Resources.EmployeeAnchor };
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(statement, employees, employeeAnchors);

      // Получить приказ в Pdf с подстановкой штампов
      var order = _obj.Order;
      // Задать списки для подписантов со стороны организации и якорей подписей
      var ourSignatories = new List<IEmployee>() { order.OurSignatory };
      var ourSignatoryAnchors = new List<string>() { HRManagement.Resources.ManagerAnchor };
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(order, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
    }
    
    public virtual void StartNotice46(DirRX.HRManagement.IVacationNotice notice, DirRX.HRManagement.Server.VacationNoticeArguments e)
    {
      var employee = DirRX.HRSolution.Employees.As(notice.Author);
      if (employee.PersonalAccountStatusDirRX != DirRX.HRSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
      {
        var mainTask = VacationApprovalTasks.As(notice.Task);
        var vacationKind = mainTask.VacationKind.Name;
        var begin = mainTask.DateBegin.Value.ToShortDateString();
        var end = mainTask.DateEnd.Value.ToShortDateString();
        var reason = VacationStatementApprovalAssignments.GetAll(x => x.Task.Id == notice.Task.Id && x.BlockUid == Constants.VacationApprovalTask.ApprovingBlockId)
          .OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
        var content = DirRX.HRManagement.VacationApprovalTasks.Resources.DeniedPersonalOfficeNoticeFormat(vacationKind, begin, end, reason);
        // FIXME Отключение "колокола"
        // DirRX.EssPlatform.PublicFunctions.Module.SendEssNotification(employee, e.Block.Subject, content, "VacationApprovalTask", "Notice", new List<Sungero.Content.IElectronicDocument>());
      }
    }

    public virtual void StartBlock46(DirRX.HRManagement.Server.VacationNoticeArguments e)
    {
      if(HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Author)))
        e.Block.ShowInSelfServiceOffice = true;
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = VacationApprovalTasks.Resources.DeniedNoticeSubject;
    }
    
    public virtual void StartBlock41(DirRX.HRManagement.Server.VacationStatementApprovalAssignmentArguments e)
    {
      e.Block.Performers.Add(Functions.Module.GetManager(_obj.Employee));
      e.Block.Subject = VacationApprovalTasks.Resources.ApprovalAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartAssignment41(DirRX.HRManagement.IVacationStatementApprovalAssignment assignment, DirRX.HRManagement.Server.VacationStatementApprovalAssignmentArguments e)
    {
      Functions.VacationApprovalTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
    }
    
    public virtual void CompleteAssignment41(DirRX.HRManagement.IVacationStatementApprovalAssignment assignment, DirRX.HRManagement.Server.VacationStatementApprovalAssignmentArguments e)
    {
      if (assignment.Result == DirRX.HRManagement.VacationStatementApprovalAssignment.Result.Refuse)
      {
        _obj.State.Properties.DateBegin.IsRequired = false;
        _obj.State.Properties.DateEnd.IsRequired = false;
      }
      Functions.VacationApprovalTask.FillVacationDataFromAssignmentToTask(_obj, assignment);
    }

    public virtual void StartBlock42(DirRX.HRManagement.Server.VacationStatementApprovalAssignmentArguments e)
    {
      if(HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Employee)))
        e.Block.ShowInSelfServiceOffice = true;
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = VacationApprovalTasks.Resources.ReApproveInitiatorAssignmentSubjectFormat(_obj.DateBegin.Value.ToShortDateString(), _obj.DateEnd.Value.ToShortDateString());
    }
    
    public virtual void StartAssignment42(DirRX.HRManagement.IVacationStatementApprovalAssignment assignment, DirRX.HRManagement.Server.VacationStatementApprovalAssignmentArguments e)
    {
      Functions.VacationApprovalTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent && assignment.BlockUid == Constants.VacationApprovalTask.SignInitiatorStatementBlockId)
      {
        var mainTask = VacationApprovalTasks.As(assignment.Task);
        var vacationKind = mainTask.VacationKind.Name;
        var begin = mainTask.DateBegin.Value.ToShortDateString();
        var end = mainTask.DateEnd.Value.ToShortDateString();
        var reason = VacationStatementApprovalAssignments.GetAll(x => x.Task.Id == assignment.Task.Id && x.BlockUid == Constants.VacationApprovalTask.ApprovingBlockId)
          .OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
        var content = DirRX.HRManagement.VacationApprovalTasks.Resources.DeniedPersonalOfficeNoticeFormat(vacationKind, begin, end, reason);
        // FIXME Отключение "колокола"
        // DirRX.EssPlatform.PublicFunctions.Module.SendEssNotification(emp, e.Block.Subject, content, "VacationApprovalTask", "VacationStatementApprovalAssignment", new List<Sungero.Content.IElectronicDocument>());
      }
    }

    public virtual void CompleteAssignment42(DirRX.HRManagement.IVacationStatementApprovalAssignment assignment, DirRX.HRManagement.Server.VacationStatementApprovalAssignmentArguments e)
    {
      if (assignment.Result.Value == DirRX.HRManagement.VacationStatementApprovalAssignment.Result.ForReapproving)
        Functions.VacationApprovalTask.FillVacationDataFromAssignmentToTask(_obj, assignment);
    }

    public virtual void StartBlock43(DirRX.HRManagement.Server.VacationSignDocsAssignmentArguments e)
    {
      if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Employee)))
        e.Block.ShowInSelfServiceOffice = true;
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = VacationApprovalTasks.Resources.SignStatementAssignmentSubject;
      
      var document = Functions.VacationApprovalTask.CreateVacationStatement(_obj);
      _obj.Statement = document;
    }
    #endregion

    #region Подписание приказа
    public virtual void StartBlock7(DirRX.HRManagement.Server.VacationPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationApprovalTasks.Resources.PrepareOrderAssignmentSubjectFormat(employee.Name);
      var order = Functions.VacationApprovalTask.CreateVacationOrder(_obj);
      e.Block.Order = order;
    }
    
    public virtual void CompleteAssignment7(DirRX.HRManagement.IVacationPrepareDocsAssignment assignment, DirRX.HRManagement.Server.VacationPrepareDocsAssignmentArguments e)
    {
      _obj.Order = assignment.Order;
    }
    
    public virtual void StartBlock8(DirRX.HRManagement.Server.VacationSignDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationApprovalTasks.Resources.SignAssignmentSubjectFormat(employee.Name);
    }
    
    public virtual void StartBlock9(DirRX.HRManagement.Server.VacationSignDocsAssignmentArguments e)
    {
      if (HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Employee)))
        e.Block.ShowInSelfServiceOffice = true;
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = VacationApprovalTasks.Resources.SignEmployeeAssignmentSubjectFormat(_obj.DateBegin.Value.ToShortDateString(), _obj.DateEnd.Value.ToShortDateString());
    }
    
    public virtual void StartBlock19(DirRX.HRManagement.Server.VacationPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationApprovalTasks.Resources.VacationDocsReWorkAssignmentSubjectFormat(employee.Name);
    }
    
    public virtual void StartBlock37(DirRX.HRManagement.Server.VacationPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationApprovalTasks.Resources.SignAllPaperDocAssignmentSubjectFormat(employee.Name);
    }
    
    public virtual bool Decision33Result()
    {
      return Functions.Module.IsAllPaperVacationDoc(_obj.Employee.Department.BusinessUnit) || !Functions.Module.IsEmployeeUsingESS(_obj.Employee);
    }
    #endregion

    #region Проведение расчетов
    public virtual void StartBlock11(DirRX.HRManagement.Server.VacationPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var employeeName = employee.Name;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetAccounter(businessUnit);
      e.Block.Performers.Add(perfomer);
      if (_obj.VacationKind.Paid.Value)
        e.Block.Subject = VacationApprovalTasks.Resources.AccounterAssignmentSubjectFormat(employeeName);
      else
        e.Block.Subject = VacationApprovalTasks.Resources.AccounterWithoutPayAssignmentSubjectFormat(employeeName);

    }
    #endregion
    
    #region Внесение изменений в график отпусков, создание замещения
    public virtual void Script38Execute()
    {
      if (_obj.Substitute != null)
      {
        var reason = string.Empty;
        if (!_obj.VacationKind.Paid.Value)
          reason = VacationApprovalTasks.Resources.VacationWithoutPayment;
        else
          reason = VacationAlertTasks.Resources.Vacation;
        
        Functions.Module.CreateSubstitution(_obj.Employee,
                                            _obj.Substitute,
                                            _obj.DateBegin.Value,
                                            _obj.DateEnd.Value,
                                            reason);
      }
      
      var date = Structures.Vacation.VacationDates.Create(_obj.DateBegin.Value, _obj.DateEnd.Value, _obj.Duration.Value, DirRX.HRManagement.Constants.Module.DateStatus.New, _obj.VacationKind);
      Functions.Module.CreateVacation(date, _obj.Employee, _obj.DateBegin.Value, HRManagement.Vacation.Status.Approved);
    }
    #endregion
  }
}