using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

using DirRX.HRLite.HRAcquaintanceTask;

namespace DirRX.HRLite.Server
{
  partial class HRAcquaintanceTaskRouteHandlers
  {

    public virtual void StartBlock71(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      var monitoringPeriod = DirRX.HRManagement.PublicFunctions.Module.Remote.GetDocflowParamValue(Constants.Module.ParamKey.AcquaintanceMonitoringTimeKey);
      if (string.IsNullOrEmpty(monitoringPeriod))
        e.Block.Period = TimeSpan.FromMinutes(Constants.HRAcquaintanceTask.DefaultMonitoringPeriod);
      else
        e.Block.Period = TimeSpan.FromMinutes(double.Parse(monitoringPeriod));
    }
    #region Согласование документа.
    public virtual bool Monitoring71Result()
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

    public virtual void Script70Execute()
    {
      DirRX.HRManagement.PublicFunctions.Module.Remote.CreateAndStartDocumentApprovalSubtask(_obj);
    }

    public virtual bool Decision69Result()
    {
      return _obj.Approvers.Count > 0;
    }
    #endregion

    #region Финальное обновление Листов ознакомления.
    public virtual void Script68Execute()
    {
      // Обновить листы ознакомления в документах задачи.
      foreach (var document in _obj.AddendaGroup.InternalDocumentBases.Concat(_obj.DocumentGroup.InternalDocumentBases))
      {
        var versionId = document.Versions.Select(vd => vd.Id).Where(vd => _obj.DocumentsVersionIDs.Select(vt => vt.VersionID).Contains(vd)).FirstOrDefault();
        var asyncHandler = HRLite.AsyncHandlers.RefreshAcquaintanceListInDocumentPublicBody.Create();
        asyncHandler.documentId = document.Id;
        asyncHandler.versionId = versionId;
        asyncHandler.ExecuteAsync();
      }
    }
    #endregion
    
    #region Конвертация в PDF/a.
    public virtual bool Decision74Result()
    {
      var documents = _obj.DocumentGroup.InternalDocumentBases.ToList();
      var addendas = _obj.AddendaGroup.InternalDocumentBases;
      
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
    
    public virtual void StartBlock62(DirRX.HRLite.Server.ReworkPersDocAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = HRAcquaintanceTasks.Resources.AuthorConvertErrorsReworkSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
    }
    
    public virtual bool Decision60Result()
    {
      return string.IsNullOrEmpty(_obj.ConvertErrorMessage);
    }
    
    public virtual void Script61Execute()
    {
      var errorInfoList = new List<string>();
      foreach (var doc in _obj.AddendaGroup.InternalDocumentBases.Concat(_obj.DocumentGroup.InternalDocumentBases))
      {
        var message = Functions.Module.ConvertToPdfa(doc).ErrorMessage;
        message = Functions.Module.GetTextWithLowerCaseFirstSymbol(message);
        if (!string.IsNullOrEmpty(message))
          errorInfoList.Add(DirRX.HRLite.HRAcquaintanceTasks.Resources.DocumentNameWithErrorFormat(doc.Name, doc.Id, message));
      }
      if (errorInfoList.Count > 0)
      {
        var convertErrorMessage = string.Join(Environment.NewLine, errorInfoList);
        _obj.ConvertErrorMessage = convertErrorMessage;
      }
    }
    #endregion

    #region Связать документы с сотрудниками, запомнить версии для построения ЛО.
    public virtual void Script57Execute()
    {
      var documents = _obj.DocumentGroup.InternalDocumentBases.ToList().Concat(_obj.AddendaGroup.InternalDocumentBases.ToList()).ToList();
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(_obj.Performers.Select(p => p.Performer).Distinct().ToList(), documents, HRLite.EmployeeDocumentLink.Status.Active);
      // Сохранить список ИД версий документа и приложений, подписанных в текущей задаче. Далее по этим версиям будут формироваться/обновляться листы ознакомления.
      // ЛО строится по всем задачам, в которых идет ознакомление с этими версиями.
      // Версии документов сохранить в случае, если не указан подписант. Иначе сохранить их после конвертации в Pdf.
      if (_obj.Signatory == null)
      {
        foreach (var document in documents)
        {
          var newID = _obj.DocumentsVersionIDs.AddNew();
          newID.VersionID = document.LastVersion.Id;
        }
      }
    }
    #endregion
    
    #region Обработка сотрудников, не подключенных к личному кабинету.
    public virtual bool Decision50Result()
    {
      var employees = Functions.HRAcquaintanceTask.GetParticipants(_obj);
      
      return EssPlatform.PublicFunctions.Module.Remote.HasEmployeeNotInESS(employees) || !Functions.Module.CheckEmployeeConsent(employees);
    }

    public virtual void StartBlock49(DirRX.HRLite.Server.AcquaintanceNotESSEmployeeAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = HRAcquaintanceTasks.Resources.AcquaintanceNotESSEmployeeSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
      e.Block.RelativeDeadlineDays = _obj.EmployeeDeadline.Value;
    }
    #endregion

    #region Уведомление об отказе в подписании руководителем.
    public virtual void StartBlock6(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = HRAcquaintanceTasks.Resources.RefuseNoticeSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
    }
    #endregion

    #region Подписание подписантом.
    public virtual bool Decision59Result()
    {
      return _obj.Signatory != null;
    }
    
    public virtual void CompleteAssignment3(DirRX.HRLite.ISignPersDocAssignment assignment, DirRX.HRLite.Server.SignPersDocAssignmentArguments e)
    {
      // Обновить листы ознакомления в документах задачи.
      foreach (var document in _obj.AddendaGroup.InternalDocumentBases.Concat(_obj.DocumentGroup.InternalDocumentBases))
      {
        var versionId = document.Versions.Select(vd => vd.Id).Where(vd => _obj.DocumentsVersionIDs.Select(vt => vt.VersionID).Contains(vd)).FirstOrDefault();
        Functions.Module.AddAcquaintanceListAndSignatureMarkInPublicBody(document, versionId);
      }
    }
    
    public virtual void StartBlock3(DirRX.HRLite.Server.SignPersDocAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Signatory);
      e.Block.Subject = HRAcquaintanceTasks.Resources.SignatorySubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
      if (_obj.SignatoryDeadline != null)
        e.Block.RelativeDeadlineDays = _obj.SignatoryDeadline.Value;
      // Сохранить список ИД версий документа и приложений, подписанных в текущей задаче. Далее по этим версиям будут формироваться/обновляться листы ознакомления.
      // ЛО строится по всем задачам, в которых идет ознакомление с этими версиями.
      var documents = _obj.DocumentGroup.InternalDocumentBases.ToList().Concat(_obj.AddendaGroup.InternalDocumentBases.ToList()).ToList();
      foreach (var document in documents)
      {
        var newID = _obj.DocumentsVersionIDs.AddNew();
        newID.VersionID = document.LastVersion.Id;
      }
    }
    #endregion
    
    #region Доработка инициатором.
    public virtual void StartBlock4(DirRX.HRLite.Server.ReworkPersDocAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = HRAcquaintanceTasks.Resources.AuthorReworkSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
    }
    #endregion
    
    #region Ознакомление сотрудниками.
    public virtual bool Decision64Result()
    {
      var employees = Functions.HRAcquaintanceTask.GetParticipants(_obj);
      
      return EssPlatform.PublicFunctions.Module.GetEmployeesWithPersonalAccountRegistred(employees).Any() && Functions.Module.CheckEmployeeConsent(employees);
    }
    
    public virtual void StartAssignment5(DirRX.HRLite.IHRAcquaintanceAssignment assignment, DirRX.HRLite.Server.HRAcquaintanceAssignmentArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
      {
        var task = HRAcquaintanceTasks.As(_obj);
        var documents = task.DocumentGroup.InternalDocumentBases.Concat(task.AddendaGroup.InternalDocumentBases).Select(d => Sungero.Content.ElectronicDocuments.As(d)).ToList();
        // FIXME Отключение "колокола"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, assignment.Subject, assignment.ActiveText, "AcquaintanceTask", "AcquaintanceAssignment", documents);
      }
    }
    
    public virtual void StartBlock5(DirRX.HRLite.Server.HRAcquaintanceAssignmentArguments e)
    {
      e.Block.ShowInSelfServiceOffice = true;
      e.Block.Subject = HRAcquaintanceTasks.Resources.EmployeeAcquaintanceSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
      e.Block.RelativeDeadlineDays = _obj.EmployeeDeadline.Value;

      // Если информация об обработке отказа непустая, значит задание приходит повторно. В исполнителей включить только тех сотрудников, которых указал инициатор.
      if (_obj.RefusalResult.Count != 0)
      {
        var reworkPerformers = _obj.RefusalResult.Where(a => a.Result == HRLite.HRAcquaintanceTaskRefusalResult.Result.SendToRework);
        foreach (var reworkPerformer in reworkPerformers)
        {
          e.Block.Performers.Add(reworkPerformer.Performer);
          EssPlatform.PublicFunctions.Module.SendNewNotification(Sungero.Company.Employees.As(reworkPerformer.Performer), false);
        }
        _obj.RefusalResult.Clear();
      }
      else
      {
        var recipients = EssPlatform.PublicFunctions.Module.GetEmployeesWithPersonalAccountRegistred(Functions.HRAcquaintanceTask.GetParticipants(_obj))
          .Where(emp => Functions.Module.CheckEmployeeConsent(DirRX.HRSolution.Employees.As(emp)));
        foreach (var recipient in recipients)
        {
          e.Block.Performers.Add(recipient);
          EssPlatform.PublicFunctions.Module.SendNewNotification(recipient, false);
        }
      }
    }
    
    public virtual void CompleteAssignment5(DirRX.HRLite.IHRAcquaintanceAssignment assignment, DirRX.HRLite.Server.HRAcquaintanceAssignmentArguments e)
    {
      if (assignment.Result == HRLite.HRAcquaintanceAssignment.Result.Refuse)
      {
        var refResult = _obj.RefusalResult.AddNew();
        refResult.Performer = assignment.Performer;
      }
      
      // Обновить листы ознакомления в документах задачи.
      foreach (var document in _obj.AddendaGroup.InternalDocumentBases.Concat(_obj.DocumentGroup.InternalDocumentBases))
      {
        var versionId = document.Versions.Select(vd => vd.Id).Where(vd => _obj.DocumentsVersionIDs.Select(vt => vt.VersionID).Contains(vd)).FirstOrDefault();
        Functions.Module.AddAcquaintanceListAndSignatureMarkInPublicBody(document, versionId);
      }
    }
    #endregion
    
    #region Обработка отказа в подписании.
    public virtual void StartBlock7(DirRX.HRLite.Server.RefusalAcqAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = HRAcquaintanceTasks.Resources.RefusalSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
    }
    
    public virtual void StartAssignment7(DirRX.HRLite.IRefusalAcqAssignment assignment, DirRX.HRLite.Server.RefusalAcqAssignmentArguments e)
    {
      foreach (var res in _obj.RefusalResult)
      {
        var assignRes = assignment.RefusalResult.AddNew();
        assignRes.Performer = res.Performer;
        assignRes.Result = res.Result;
      }
    }
    
    public virtual void CompleteAssignment7(DirRX.HRLite.IRefusalAcqAssignment assignment, DirRX.HRLite.Server.RefusalAcqAssignmentArguments e)
    {
      _obj.RefusalResult.Clear();
      foreach (var res in assignment.RefusalResult)
      {
        var taskRes = _obj.RefusalResult.AddNew();
        taskRes.Performer = res.Performer;
        taskRes.Result = res.Result;
      }
    }

    public virtual void StartBlock13(DirRX.HRLite.Server.PrepareRefuseActAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = HRAcquaintanceTasks.Resources.PrepareRefuseActSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
    }
    
    public virtual bool Decision44Result()
    {
      return _obj.RefusalResult.Any(a => a.Result == HRLite.HRAcquaintanceTaskRefusalResult.Result.SendToRework);
    }
    
    public virtual bool Decision45Result()
    {
      return _obj.RefusalResult.Any(a => a.Result == HRLite.HRAcquaintanceTaskRefusalResult.Result.CreateAct);
    }
    
    #endregion
  }
}