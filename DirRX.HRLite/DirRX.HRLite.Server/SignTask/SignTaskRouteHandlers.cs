using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using DirRX.HRLite.SignTask;

namespace DirRX.HRLite.Server
{
  partial class SignTaskRouteHandlers
  {

    public virtual void StartBlock26(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      var monitoringPeriod = DirRX.HRManagement.PublicFunctions.Module.Remote.GetDocflowParamValue(Constants.Module.ParamKey.SignMonitoringTimeKey);
      if (string.IsNullOrEmpty(monitoringPeriod))
        e.Block.Period = TimeSpan.FromMinutes(Constants.SignTask.DefaultMonitoringPeriod);
      else
        e.Block.Period = TimeSpan.FromMinutes(double.Parse(monitoringPeriod));
    }

    #region Вычисление очередности согласования.
    public virtual bool Decision34Result()
    {
      return _obj.SignQueue.Value.Equals(HRLite.SignTask.SignQueue.WorkerFirst);
    }

    public virtual bool Decision28Result()
    {
      return _obj.SignQueue.Value.Equals(HRLite.SignTask.SignQueue.WorkerFirst);
    }

    public virtual bool Decision35Result()
    {
      return _obj.SignQueue.Value.Equals(HRLite.SignTask.SignQueue.WorkerFirst);
    }
    #endregion

    #region Согласование документа.
    public virtual bool Decision27Result()
    {
      return _obj.Approvers.Count > 0;
    }

    public virtual bool Monitoring26Result()
    {
      var documentApprovalTask = HRManagement.DocumentApprovalTasks.GetAll(t => t.ParentTask.Equals(_obj)).OrderByDescending(d => d.Created).FirstOrDefault();
      var documentApproved = documentApprovalTask.Status.Value == Sungero.Workflow.Task.Status.Completed;
      var documentApprovalAborted = documentApprovalTask.Status.Value == Sungero.Workflow.Task.Status.Aborted;
      
      if (documentApprovalAborted)
      {
        _obj.AbortingReason = documentApprovalTask.AbortingReason;
        _obj.Abort();
      }
      
      return documentApproved;
    }

    public virtual void Script25Execute()
    {
      DirRX.HRManagement.PublicFunctions.Module.Remote.CreateAndStartDocumentApprovalSubtask(_obj);
    }
    #endregion

    #region Конвертация в Pdf/a.
    public virtual bool Decision48Result()
    {
      var documents = _obj.DocumentGroup.BilaterialDocuments.ToList();
      var addendas = _obj.AddendaGroup.BilaterialDocuments;
      
      if (addendas != null)
        documents.AddRange(addendas);
      
      var allValid = true;
      if (documents.Any())
      {
        foreach (var document in documents)
        {
          if (document != null && document.Versions.Any())
          {
            var version = document.Versions.LastOrDefault();
            if (!PublicFunctions.Module.ValidatePdfAFormat(version.Body.Read(), version.AssociatedApplication.Extension))            
              allValid = false;            
            else            
              _obj.ConvertErrorMessage = PublicFunctions.Module.ClearConverError(_obj.ConvertErrorMessage, document.Id);
            
          }
        }
      }
      
      return allValid;
    }
    
    public virtual void StartBlock23(DirRX.HRLite.Server.SignReworkAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = SignTasks.Resources.AuthorConvertErrorsReworkSubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
    }
    
    public virtual bool Decision22Result()
    {
      return string.IsNullOrEmpty(_obj.ConvertErrorMessage);
    }
    
    public virtual void Script21Execute()
    {
      var errorInfoList = new List<string>();
      foreach (var doc in _obj.AddendaGroup.BilaterialDocuments.Concat(_obj.DocumentGroup.BilaterialDocuments))
      {
        var message = Functions.Module.ConvertToPdfa(doc).ErrorMessage;
        message = Functions.Module.GetTextWithLowerCaseFirstSymbol(message);
        if (!string.IsNullOrEmpty(message))
          errorInfoList.Add(DirRX.HRLite.SignTasks.Resources.DocumentNameWithErrorFormat(doc.Name, doc.Id, message));
      }
      if (errorInfoList.Count > 0)
      {
        var convertErrorMessage = string.Join(Environment.NewLine, errorInfoList);
        _obj.ConvertErrorMessage = convertErrorMessage;
      }
    }
    #endregion
    
    #region Связка между сотрудниками и документами.
    public virtual void Script20Execute()
    {
      var documents = _obj.DocumentGroup.BilaterialDocuments.Select(d => InternalDocumentBases.As(d)).Concat(_obj.AddendaGroup.BilaterialDocuments.Select(d => InternalDocumentBases.As(d))).ToList();
      var document = _obj.DocumentGroup.BilaterialDocuments.FirstOrDefault();
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(document.Employees.Select(p => p.Employee).Distinct().ToList(), documents, HRLite.EmployeeDocumentLink.Status.Active);
    }
    #endregion
    
    #region Подписание сотрудниками, не подключенными к ЛК.
    public virtual void StartBlock18(DirRX.HRLite.Server.SignNotESSEmployeeAssigmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = DirRX.HRLite.SignTasks.Resources.SignNotESSEmployeeSubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
      e.Block.RelativeDeadlineDays = _obj.EmployeeDeadline.Value;
    }
    
    public virtual bool Decision15Result()
    {
      var employees = DirRX.HRLite.PublicFunctions.SignTask.Remote.GetParticipants(_obj);
      
      return EssPlatform.PublicFunctions.Module.Remote.HasEmployeeNotInESS(employees) || !Functions.Module.CheckEmployeeConsent(employees);
    }

    public virtual void StartBlock37(Sungero.Workflow.Server.NoticeArguments e)
    {
      var document = BilaterialDocuments.As(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault());
      var performers = EssPlatform.PublicFunctions.Module.GetEmployeesWithPersonalAccountRegistred(document.Employees.Select(t => t.Employee).ToList());
      
      foreach (var performer in performers)
        e.Block.Performers.Add(performer);
      e.Block.Subject = SignTasks.Resources.SignNoticeSubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
    }
    
    public virtual void StartBlock12(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = SignTasks.Resources.SignNoticeSubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
    }
    
    public virtual void StartBlock11(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = SignTasks.Resources.RefuseNoticeSubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
    }
    #endregion
    
    #region Доработка документа инициатором.
    public virtual void StartBlock10(DirRX.HRLite.Server.SignReworkAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = SignTasks.Resources.AuthorReworkSubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
    }
    #endregion
    
    #region Подписание документа сотрудником.
    public virtual bool Decision47Result()
    {
      // Документ подписан, если никто не выполнил задание на подписание с результатом Отказать.
      return !EmployeeSignAssignments.GetAll(a => Equals(a.Task, _obj) && a.Status == Sungero.Workflow.Assignment.Status.Completed && a.Result == HRLite.EmployeeSignAssignment.Result.Refuse)
        .Where(a => a.TaskStartId == _obj.StartId).Any();
    }
    
    public virtual void StartAssignment6(DirRX.HRLite.IEmployeeSignAssignment assignment, DirRX.HRLite.Server.EmployeeSignAssignmentArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
      {
        var task = SignTasks.As(_obj);
        var documents = task.DocumentGroup.BilaterialDocuments.Concat(task.AddendaGroup.BilaterialDocuments).Select(d => Sungero.Content.ElectronicDocuments.As(d)).ToList();
        // FIXME Отключение "колоколов"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, assignment.ActiveText, "SigntTask", "EmployeeSignAssignment", documents);
      }
    }
    
    public virtual void CompleteAssignment6(DirRX.HRLite.IEmployeeSignAssignment assignment, DirRX.HRLite.Server.EmployeeSignAssignmentArguments e)
    {
      if (assignment.Result == HRLite.EmployeeSignAssignment.Result.Sign)
        Functions.SignTask.AddSignatureMarksToDocuments(_obj);
    }
    
    public virtual void StartBlock6(DirRX.HRLite.Server.EmployeeSignAssignmentArguments e)
    {
      e.Block.ShowInSelfServiceOffice = true;
      var document = BilaterialDocuments.As(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault());
      var performers = EssPlatform.PublicFunctions.Module.GetEmployeesWithPersonalAccountRegistred(document.Employees.Select(t => t.Employee).ToList());
      
      foreach (var performer in performers)
      {
        e.Block.Performers.Add(performer);
        EssPlatform.PublicFunctions.Module.SendNewNotification(performer, false);
      }
      e.Block.Subject = SignTasks.Resources.EmployeeSignSubjectFormat(document.Name);
      e.Block.RelativeDeadlineDays = _obj.EmployeeDeadline.Value;
    }
    #endregion
    
    #region Подписание документа руководителем.
    public virtual void CompleteAssignment4(DirRX.HRLite.ISignAssignment assignment, DirRX.HRLite.Server.SignAssignmentArguments e)
    {
      if (assignment.Result == HRLite.SignAssignment.Result.Sign)
        Functions.SignTask.AddSignatureMarksToDocuments(_obj);
    }
    
    public virtual void StartBlock4(DirRX.HRLite.Server.SignAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Signatory);
      e.Block.Subject = SignTasks.Resources.SignatorySubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
      e.Block.RelativeDeadlineDays = _obj.SignatoryDeadline.Value;
    }

    public virtual void StartBlock38(DirRX.HRLite.Server.RefuseManagerSignNoticeArguments e)
    {
      e.Block.ShowInSelfServiceOffice = true;
      
      var document = BilaterialDocuments.As(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault());
      var performers = EssPlatform.PublicFunctions.Module.GetEmployeesWithPersonalAccountRegistred(document.Employees.Select(t => t.Employee).ToList());
      
      foreach (var performer in performers)
        e.Block.Performers.Add(performer);
      e.Block.Subject = SignTasks.Resources.RefuseNoticeSubjectFormat(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault().Name);
    }
    #endregion
  }
}