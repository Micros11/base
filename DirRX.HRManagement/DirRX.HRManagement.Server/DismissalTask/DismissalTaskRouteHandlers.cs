using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using Sungero.Company;
using DirRX.HRManagement.DismissalTask;

namespace DirRX.HRManagement.Server
{
  partial class DismissalTaskRouteHandlers
  {
    #region Преобразование в Pdf.
    public virtual void Script68Execute()
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
    #endregion

    #region Приказ об увольнении: подготовка и подписание.
    
    public virtual void StartBlock7(DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.PrepareDocAssignmentSubjectFormat(_obj.Employee.Name);
      e.Block.DismissalDate = _obj.DismissalDate;
    }
    
    public virtual void CompleteAssignment7(DirRX.HRManagement.IDismissalPrepareDocsAssignment assignment, DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      assignment.Order.AccessRights.Grant(Functions.Module.GetSecurityResponsible(businessUnit), DefaultAccessRightsTypes.Read);
      assignment.Order.AccessRights.Grant(Functions.Module.GetITResponsible(businessUnit), DefaultAccessRightsTypes.Read);
      assignment.Order.AccessRights.Grant(Functions.Module.GetSupplyResponsible(businessUnit), DefaultAccessRightsTypes.Read);
      assignment.Order.AccessRights.Save();
      _obj.Order = assignment.Order; 
    }
    
    public virtual void StartBlock9(DirRX.HRManagement.Server.DismissalSignDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.SignAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartBlock57(DirRX.HRManagement.Server.DismissalSignDocsAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = DismissalTasks.Resources.SignEmployeeAssignmentSubject;
    }
    
    #endregion
    
    #region Обходной лист.
    
    public virtual void StartBlock15(DirRX.HRManagement.Server.DismissalGoRoundAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSecurityResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.RespGoRoundAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartBlock38(DirRX.HRManagement.Server.DismissalGoRoundAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetITResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.RespGoRoundAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartBlock39(DirRX.HRManagement.Server.DismissalGoRoundAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSupplyResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.RespGoRoundAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual bool Decision35Result()
    {
      _obj.ReGoRound = true;
      return DismissalGoRoundAssignments.GetAll(a => Equals(a.Task, _obj) && a.Result == DirRX.HRManagement.DismissalGoRoundAssignment.Result.ClaimDebts).Any();
    }
    
    public virtual void StartBlock32(DirRX.HRManagement.Server.GoRoundNoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = DismissalTasks.Resources.EmployeeGoRoundAssignmentSubject;
    }

    public virtual void StartBlock29(DirRX.HRManagement.Server.DismissalGoRoundAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSecurityResponsible(businessUnit);
      e.Block.Performers.Add(Functions.DismissalTask.GetGoRoundRecipient(_obj, perfomer));
      e.Block.Subject = DismissalTasks.Resources.RespGoRoundControlAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartBlock40(DirRX.HRManagement.Server.DismissalGoRoundAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetITResponsible(businessUnit);
      e.Block.Performers.Add(Functions.DismissalTask.GetGoRoundRecipient(_obj, perfomer));
      e.Block.Subject = DismissalTasks.Resources.RespGoRoundControlAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartBlock41(DirRX.HRManagement.Server.DismissalGoRoundAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSupplyResponsible(businessUnit);
      e.Block.Performers.Add(Functions.DismissalTask.GetGoRoundRecipient(_obj, perfomer));
      e.Block.Subject = DismissalTasks.Resources.RespGoRoundControlAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    #endregion

    #region Подготовка и осуществление увольнения.
    
    public virtual void StartBlock10(DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.DismissEmployeeAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment10(DirRX.HRManagement.IDismissalPrepareDocsAssignment assignment, DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      assignment.Deadline = Calendar.EndOfWorkingDay(_obj.DismissalDate.Value);
    }
    
    public virtual void StartBlock66(DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.DismissEmployeeAssignmentSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartAssignment66(DirRX.HRManagement.IDismissalPrepareDocsAssignment assignment, DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      assignment.Deadline = Calendar.EndOfWorkingDay(_obj.DismissalDate.Value);
    }
    
    public virtual void StartBlock22(DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      var emplName = _obj.Employee.Name;
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetAccounter(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.DismissEmployeeAssignmentSubjectFormat(_obj.Employee.Name);
    }
        
    public virtual bool Decision65Result()
    {
      return Functions.Module.IsAllPaperStaffDoc(_obj.Employee.Department.BusinessUnit);
    }

    #endregion

    #region Согласование заявления.
    public virtual void StartBlock58(Sungero.Workflow.Server.NoticeArguments e)
    {
      var performers = Functions.Module.GetAbortationNoticePerformers(_obj);
      foreach (var performer in performers)
        e.Block.Performers.Add(performer);
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = DismissalTasks.Resources.DeniedNoticeSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartBlock4(DirRX.HRManagement.Server.DismissalStatementApprovalAssignmentArguments e)
    {
      e.Block.Performers.Add(Functions.Module.GetManager(_obj.Employee));
      if (DismissalSignDocsAssignments.GetAll(a => Equals(a.Task, _obj) && a.Result == DirRX.HRManagement.DismissalSignDocsAssignment.Result.ReApprove).Any())
        e.Block.Subject = DismissalTasks.Resources.ReApproveAssignmentSubjectFormat(_obj.Employee.Name);
      else
        e.Block.Subject = DismissalTasks.Resources.ApprovalAssignmentSubjectFormat(_obj.Employee.Name);
      e.Block.DismissalDate = _obj.DismissalDate;
    }
    
    public virtual void CompleteAssignment4(DirRX.HRManagement.IDismissalStatementApprovalAssignment assignment, DirRX.HRManagement.Server.DismissalStatementApprovalAssignmentArguments e)
    {
      if (assignment.Result == DirRX.HRManagement.DismissalStatementApprovalAssignment.Result.Refuse)
        _obj.State.Properties.DismissalDate.IsRequired = false;
      _obj.DismissalDate = assignment.DismissalDate;
    }
    
    public virtual void StartBlock5(DirRX.HRManagement.Server.DismissalStatementApprovalAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.ApprovalAssignmentSubjectFormat(_obj.Employee.Name);
      e.Block.DismissalDate = _obj.DismissalDate;
    }
  
    public virtual void StartBlock53(DirRX.HRManagement.Server.DismissalSignDocsAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = DismissalTasks.Resources.SignStatementAssignmentSubject;
      _obj.Statement = Functions.DismissalTask.CreateDismissalStatement(_obj);  
    }
    
    public virtual void CompleteAssignment53(DirRX.HRManagement.IDismissalSignDocsAssignment assignment, DirRX.HRManagement.Server.DismissalSignDocsAssignmentArguments e)
    {
      if (assignment.Result == DirRX.HRManagement.DismissalSignDocsAssignment.Result.ReApprove)
      {
        var statement = Functions.DismissalTask.GetEmployeeStatement(_obj);
        _obj.Statement = null;
        var lockInfo = Locks.GetLockInfo(statement);
        if (!lockInfo.IsLockedByOther)
          HRDocuments.Delete(statement);
      }
    }
    
    public virtual void StartBlock11(DirRX.HRManagement.Server.DismissalPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = DismissalTasks.Resources.DismissalDocsReWorkAssignmentSubjectFormat(_obj.Employee.Name);
    }

    #endregion

  }
}