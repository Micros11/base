using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using DirRX.HRLite.StatementTask;

namespace DirRX.HRLite.Server
{
  partial class StatementTaskRouteHandlers
  {

    public virtual void Script9Execute()
    {
      var statement = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
      var versionId = statement.LastVersion.Id;
      Functions.StatementDocument.ConvertToPdfAndAddSignatureMark(statement, versionId);
    }
    public virtual void Script8Execute()
    {
      var documents = _obj.DocumentGroup.StatementDocuments.Select(d => InternalDocumentBases.As(d)).Concat(_obj.AddendaGroup.InternalDocumentBases).ToList();
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(Sungero.Company.Employees.As(_obj.Author), documents, HRLite.EmployeeDocumentLink.Status.Active);
    }
    
    public virtual void StartNotice5(DirRX.HRLite.IStatementNotice notice, DirRX.HRLite.Server.StatementNoticeArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
      {
        var document = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
        // FIXME Отключение "колоколов"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, notice.Subject, notice.ActiveText, "StatementTask", "Notice", document);
      }
    }

    public virtual void StartBlock5(DirRX.HRLite.Server.StatementNoticeArguments e)
    {
      e.Block.ShowInSelfServiceOffice = true;
      e.Block.Performers.Add(_obj.Author);
      EssPlatform.PublicFunctions.Module.SendNewNotification(Sungero.Company.Employees.As(_obj.Author), true);
      e.Block.Subject = StatementTasks.Resources.PerformedSubjectFormat(_obj.DocumentGroup.StatementDocuments.FirstOrDefault().Name);
      var documents = _obj.DocumentGroup.StatementDocuments.Select(d => InternalDocumentBases.As(d)).Concat(_obj.AddendaGroup.InternalDocumentBases).ToList();
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(Sungero.Company.Employees.As(_obj.Author), documents, HRLite.EmployeeDocumentLink.Status.Signed);
    }

    public virtual void StartNotice6(DirRX.HRLite.IStatementNotice notice, DirRX.HRLite.Server.StatementNoticeArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
      {
        var document = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
        // FIXME Отключение "колоколов"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, notice.Subject, notice.ActiveText, "StatementTask", "Notice", document);
      }
    }

    public virtual void StartBlock6(DirRX.HRLite.Server.StatementNoticeArguments e)
    {
      e.Block.ShowInSelfServiceOffice = true;
      e.Block.Performers.Add(_obj.Author);
      EssPlatform.PublicFunctions.Module.SendNewNotification(Sungero.Company.Employees.As(_obj.Author), true);
      e.Block.Subject = StatementTasks.Resources.RefusedSubjectFormat(_obj.DocumentGroup.StatementDocuments.FirstOrDefault().Name);
      var documents = _obj.DocumentGroup.StatementDocuments.Select(d => InternalDocumentBases.As(d)).Concat(_obj.AddendaGroup.InternalDocumentBases).ToList();
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(Sungero.Company.Employees.As(_obj.Author), documents, HRLite.EmployeeDocumentLink.Status.Closed);
    }
    
    public virtual void StartBlock4(DirRX.HRLite.Server.StatementPerformAssignmentArguments e)
    {
      var document = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
      // Исполнителей по заявлению выбрать по виду из НОР заявителя.
      var performers = Functions.Module.GetStatementPerformers(document.DocumentKind, _obj.Author);
      foreach (var performer in performers)
        e.Block.Performers.Add(performer);
      var statementSetting = Functions.Module.GetStatementSetting(document.DocumentKind);
      e.Block.IsParallel = statementSetting.PerformStartOrder == HRLite.StatementsSetting.PerformStartOrder.Parallel;
      e.Block.Subject = StatementTasks.Resources.PerformSubjectFormat(document.Name);
    }

    public virtual void StartBlock3(DirRX.HRLite.Server.StatementApproveAssignmentArguments e)
    {
      var document = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
      // Согласующих по заявлению выбрать по виду из НОР заявителя.
      var performers = Functions.Module.GetStatementApprovers(document.DocumentKind, _obj.Author);
      foreach (var performer in performers)
        e.Block.Performers.Add(performer);
      var statementSetting = Functions.Module.GetStatementSetting(document.DocumentKind);
      e.Block.IsParallel = statementSetting.ApproveStartOrder == HRLite.StatementsSetting.ApproveStartOrder.Parallel;
      e.Block.Subject = StatementTasks.Resources.ApproveSubjectFormat(document.Name);
    }

  }
}