using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using Sungero.Workflow;
using DirRX.HRManagement.TransferTask;

namespace DirRX.HRManagement.Server
{
  partial class TransferTaskRouteHandlers
  {

    #region Доработка инициатором.
    
    public virtual void CompleteAssignment94(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromAssignmentToTask(_obj, assignment);
      Functions.TransferTask.FillTransferDataToDocs(_obj);
    }

    public virtual void StartAssignment94(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromTaskToAssignment(_obj, assignment);
    }
    
    public virtual void StartBlock94(DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      if (_obj.IsHRStarted.Value || _obj.IsManagerStarted.Value)
        e.Block.Subject = DirRX.HRManagement.TransferTasks.Resources.TransferManagerReWorkSubjectFormat(_obj.Employee.Name);
      else
        e.Block.Subject = DirRX.HRManagement.TransferTasks.Resources.TransferReWorkSubject;
    }
    
    #endregion
    
    #region Преобразование в Pdf.
    
    public virtual void Script74Execute()
    {
      // Получить приказ в Pdf с подстановкой штампов
      var order = _obj.Order;
      // Задать списки для сотрудника и якорей подписей
      var employees = new List<IEmployee>() { order.Employee };
      var employeeAnchors = new List<string>() { HRManagement.Resources.EmployeeAnchor };
      // Задать списки для подписантов со стороны организации и якорей подписей
      var ourSignatories = new List<IEmployee>() { order.OurSignatory };
      var ourSignatoryAnchors = new List<string>() { HRManagement.Resources.ManagerAnchor };
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(order, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
      
      // Доп. соглашение преобразовать только в том случае, если оно не предполагается в бумажном виде
      if (!Functions.Module.IsOnlyBilStaffDocPaper(_obj.Employee.Department.BusinessUnit))
      {
        // Получить доп. соглашение в Pdf с подстановкой штампов
        var additionalAgreement = _obj.AdditionalAgreement;
        PublicFunctions.Module.ConvertToPdfWithSignatureMarks(additionalAgreement, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
      }
    }
    
    #endregion
    
    #region Согласование изменений условий труда.
    
    public virtual bool Decision101Result()
    {
      return !_obj.IsManagerStarted.Value && !_obj.IsHRStarted.Value;
    }

    public virtual void StartBlock82(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.OldDepartment.Manager);
      if (_obj.IsTransfer.Value)
        e.Block.Performers.Add(_obj.Department.Manager);
      e.Block.Subject = DirRX.HRManagement.TransferTasks.Resources.TransferApprovednoticeFormat(_obj.Employee.Name);
    }

    public virtual void StartBlock90(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = TransferTasks.Resources.TransferTaskStartedNotice;
    }

    public virtual bool Decision88Result()
    {
      return _obj.SignImmediately.Value;
    }

    public virtual bool Decision84Result()
    {
      return _obj.IsTransfer.Value;
    }

    public virtual bool Decision85Result()
    {
      return _obj.IsManagerStarted.Value;
    }

    public virtual bool Decision77Result()
    {
      return _obj.IsHRStarted.Value;
    }
    
    public virtual void StartBlock78(DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = TransferTasks.Resources.EmployeeStatemantAssignmentSubject;
    }
    
    public virtual void StartAssignment78(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromTaskToAssignment(_obj, assignment);
    }
    
    public virtual void StartBlock53(Sungero.Workflow.Server.NoticeArguments e)
    {
      // Заполнить исполнителей блока
      var performers = new List<IUser>();
      // Исполнители всех заданий, кроме отказавшего или прекратившего
      var taskPerformers = Assignments.GetAll(a => Equals(a.Task, _obj) && a.Result != DirRX.HRManagement.TransferStatementApprovalAssignment.Result.Deny &&
                                              a.Result != DirRX.HRManagement.TransferStatementApprovalAssignment.Result.Abort).Select(a => a.Performer);
      if (taskPerformers.Any())
        performers = performers.Concat(taskPerformers).ToList();
      // Все, получавшие уведомления
      taskPerformers = Sungero.Workflow.Notices.GetAll(n => Equals(n.Task, _obj)).Select(n => n.Performer);
      if (taskPerformers.Any())
        performers = performers.Concat(taskPerformers).ToList();
      // Автор, если не он прекратил задачу
      if (!performers.Contains(_obj.Author) &&
          !TransferStatementApprovalAssignments.GetAll(a => Equals(a.Task, _obj) && Equals(a.Performer, _obj.Author) &&
                                                       a.Result == DirRX.HRManagement.TransferStatementApprovalAssignment.Result.Abort).Any())
        performers.Add(_obj.Author);
      foreach (var performer in performers)
        e.Block.Performers.Add(performer);

      // Сотрудник отказался на стадии согласования или прекратил задачу на стадии доработки
      if (TransferStatementApprovalAssignments.GetAll(a => Equals(a.Task, _obj) &&
                                                      ((a.BlockUid == Constants.TransferTask.EmployeeStetementBlockId &&
                                                        a.Result == DirRX.HRManagement.TransferStatementApprovalAssignment.Result.Deny) ||
                                                       (a.BlockUid == Constants.TransferTask.InitiatorReWorkBlockId &&
                                                        a.Result == DirRX.HRManagement.TransferStatementApprovalAssignment.Result.Abort &&
                                                        !_obj.IsHRStarted.Value && !_obj.IsManagerStarted.Value))).Any())
        e.Block.Subject = TransferTasks.Resources.EmployeeDeniedNoticeSubjectFormat(_obj.Employee.Name);
      // Прочие отказы и прекращения
      else
        e.Block.Subject = TransferTasks.Resources.DeniedNoticeSubjectFormat(_obj.Employee.Name);
    }
    
    public virtual void StartBlock3(DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      var employee = Sungero.Company.Employees.As(_obj.Author);
      if (employee != null && employee.Department != null && employee.Department.Manager != null)
      {
        e.Block.Performers.Add(employee.Department.Manager);
        if (TransferStatementApprovalAssignments.GetAll(a => Equals(a.Task, _obj) && a.BlockUid == Constants.TransferTask.ApprovalOldManagerAssignmentBlockId && a.Id != _obj.Id).Any())
          e.Block.Subject = TransferTasks.Resources.ReApprovalAssignmentSubjectFormat(_obj.Employee.Name);
        else
          e.Block.Subject = TransferTasks.Resources.ApprovalAssignmentSubjectFormat(_obj.Employee.Name);
      }
    }

    public virtual void StartAssignment3(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromTaskToAssignment(_obj, assignment);
    }

    public virtual void CompleteAssignment3(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromAssignmentToTask(_obj, assignment);
      Functions.TransferTask.FillTransferDataToDocs(_obj);
    }

    public virtual void StartBlock4(DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      if (_obj.Department != null && _obj.Department.Manager != null)
      {
        e.Block.Performers.Add(_obj.Department.Manager);
        e.Block.Subject = TransferTasks.Resources.NewManagerApprovalAssignmentSubjectFormat(_obj.Employee.Name);
      }
    }

    public virtual void StartAssignment4(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromTaskToAssignment(_obj, assignment);
    }
    
    public virtual void CompleteAssignment4(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromAssignmentToTask(_obj, assignment);
    }
    
    public virtual void StartBlock80(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.OldDepartment.Manager);
      if (_obj.IsTransfer.Value)
        e.Block.Performers.Add(_obj.Department.Manager);
      // Если в блок пришли с доработки, уведомляем о том, что документы доработаны.
      if (TransferStatementApprovalAssignments.GetAll(a => Equals(a.Task, _obj) && a.Result == DirRX.HRManagement.TransferStatementApprovalAssignment.Result.ReWork).Any())
        e.Block.Subject = TransferTasks.Resources.ManagersReworkNoticeFormat(_obj.Employee.Name);
      // Иначе уведомляем о начале процесса ИУТ.
      else
        e.Block.Subject = TransferTasks.Resources.TranferTaskStartedManagersNoticeFormat(_obj.Employee.Name);
    }
    
    #endregion

    #region Оформление и подписание приказа и доп. соглашения.

    public virtual bool Decision65Result()
    {
      return Functions.Module.IsOnlyBilStaffDocPaper(_obj.OldDepartment.BusinessUnit) && Functions.Module.IsEmployeeUsingESS(_obj.Employee);
    }

    public virtual bool Decision64Result()
    {
      return Functions.Module.IsAllElectronicStaffDoc(_obj.OldDepartment.BusinessUnit) && Functions.Module.IsEmployeeUsingESS(_obj.Employee);
    }

    public virtual void StartBlock5(DirRX.HRManagement.Server.TransferPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      if (_obj.IsHRStarted.Value ||
          TransferPrepareDocsAssignments.GetAll(a => Equals(a.Task, _obj) && a.BlockUid == Constants.TransferTask.TransferPrepareDocsAssignmentBlockId && a.Id != _obj.Id).Any())
        e.Block.Subject = TransferTasks.Resources.ReWorkPrepareDocAssignmentSubjectFormat(_obj.Employee.Name);
      else
        e.Block.Subject = TransferTasks.Resources.CheckDocAssignmentSubjectFormat(_obj.Employee.Name);
      e.Block.Department = _obj.Department;
      e.Block.JobTitle = _obj.JobTitle;
      e.Block.TransferDate = _obj.TransferDate;
      e.Block.OldDepartment = _obj.OldDepartment;
      e.Block.Employee = _obj.Employee;
      
      // Сформировать и вложить документы.
      if (_obj.AdditionalAgreement == null && _obj.Order == null)
      {
        e.Block.AdditionalAgreement = Functions.TransferTask.CreateTransferHRDocument(_obj);
        e.Block.Order = Functions.TransferTask.CreateTransferOrder(_obj, e.Block.AdditionalAgreement);
      }
      else
      {
        e.Block.AdditionalAgreement = _obj.AdditionalAgreement;
        e.Block.Order = _obj.Order;
      }
    }

    public virtual void CompleteAssignment5(DirRX.HRManagement.ITransferPrepareDocsAssignment assignment, DirRX.HRManagement.Server.TransferPrepareDocsAssignmentArguments e)
    {
      _obj.AdditionalAgreement = assignment.AdditionalAgreement;
      _obj.Order = assignment.Order;
    }

    public virtual void StartBlock7(DirRX.HRManagement.Server.TransferSignDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.SignAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartBlock8(DirRX.HRManagement.Server.TransferSignDocsAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = TransferTasks.Resources.SignEmployeeAssignmentSubject;
    }
    
    public virtual void StartBlock32(DirRX.HRManagement.Server.TransferPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      var addAgreement = Functions.TransferTask.GetAdditionalAgreement(_obj);
      var order = Functions.TransferTask.GetEmployeeOrder(_obj);
      e.Block.Subject = TransferTasks.Resources.ReWorkDocsAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartBlock67(DirRX.HRManagement.Server.TransferPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.SignAllPaperDocAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartBlock68(DirRX.HRManagement.Server.TransferPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.SignPaperAddAgreementAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void Script98Execute()
    {
      Functions.Module.CreateAndStartTransferServicesSubtask(_obj);
    }

    #endregion
    
    #region Блоки версии V1, для обеспечения совместимости
    
    public virtual void StartBlock18(DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.ApprovalAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment18(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromTaskToAssignment(_obj, assignment);
    }

    public virtual void CompleteAssignment18(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromAssignmentToTask(_obj, assignment);
    }

    public virtual void StartBlock34(DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.ApprovalAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment34(DirRX.HRManagement.ITransferStatementApprovalAssignment assignment, DirRX.HRManagement.Server.TransferStatementApprovalAssignmentArguments e)
    {
      Functions.TransferTask.FillTransferDataFromTaskToAssignment(_obj, assignment);
    }
    
    public virtual void StartBlock52(DirRX.HRManagement.Server.TransferSignDocsAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = TransferTasks.Resources.SignStatementAssignmentSubject;
      _obj.Statement = Functions.TransferTask.CreateTransferStatement(_obj);
    }

    public virtual void CompleteAssignment52(DirRX.HRManagement.ITransferSignDocsAssignment assignment, DirRX.HRManagement.Server.TransferSignDocsAssignmentArguments e)
    {
      if (assignment.Result == DirRX.HRManagement.TransferSignDocsAssignment.Result.ReApprove)
      {
        var statement = Functions.TransferTask.GetEmployeeStatement(_obj);
        _obj.Statement = null;
        var lockInfo = Locks.GetLockInfo(statement);
        if (!lockInfo.IsLockedByOther)
          HRDocuments.Delete(statement);
      }
    }
    
    public virtual void StartBlock21(DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSupplyResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment21(DirRX.HRManagement.ITransferAssignment assignment, DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }

    public virtual void StartBlock22(DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSecurityResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment22(DirRX.HRManagement.ITransferAssignment assignment, DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }

    public virtual void StartBlock23(DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetITResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment23(DirRX.HRManagement.ITransferAssignment assignment, DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }
    
    public virtual void StartBlock24(DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetAccounter(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment24(DirRX.HRManagement.ITransferAssignment assignment, DirRX.HRManagement.Server.TransferAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }
    
    #endregion

  }
}