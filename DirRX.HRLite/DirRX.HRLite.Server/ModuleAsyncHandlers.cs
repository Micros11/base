using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;

namespace DirRX.HRLite.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void SendConsentToKEDO(DirRX.HRLite.Server.AsyncHandlerInvokeArgs.SendConsentToKEDOInvokeArgs args)
    {
      args.Retry = false;
      Logger.DebugFormat("SendConsentToKEDO started, iteration: {0}, employeeIds: {1}", args.RetryIteration, args.employeeIds);
      var employeeIds = args.employeeIds.Split(',');
      
      var employees = HRSolution.Employees.GetAll(w => employeeIds.Contains(w.ToString()));
      var employeesForSendingConsent = employees.Where(w => w.ConsentDirRX == HRSolution.Employee.ConsentDirRX.NotSent);
      var employeesWithSignedConsent = employees.Where(w => w.ConsentDirRX == HRSolution.Employee.ConsentDirRX.Signed);
      var employeesWithSentConsent = employees.Where(w => w.ConsentDirRX == HRSolution.Employee.ConsentDirRX.Sent);
      var author = Users.Get(args.authorId);
      
      int totalEmployeesProcessed = employeeIds.Count();
      int consentSignedEmployees = employeesWithSignedConsent.Count();
      int consentSendedEmployees = employeesWithSentConsent.Count();
      int sendedTasks = 0;
      int errorsCount = 0;
      
      foreach (var employee in employeesForSendingConsent)
      {
        try
        {
          var task = HRLite.ConsentTasks.Create();
          task.Author = author;
          task.Employee = employee;
          task.Start();
          sendedTasks++;
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("SendConsentToKEDO. EmployeeId: {0}. {1}. {2}", employee.Id, ex.Message, ex.StackTrace);
          errorsCount++;
        }
      }
      
      var message = string.Empty;
      message = Resources.ConsentSendingResultMessageFormat(totalEmployeesProcessed, sendedTasks, consentSendedEmployees, consentSignedEmployees);
      
      if (errorsCount > 0)
      {
        message += Environment.NewLine;
        message += Resources.ConsentSendingResultWithErrorsFormat(errorsCount);
      }
      
      PublicFunctions.Module.SendNoticeAboutConsentSending(author, message);
    }

    public virtual void ChangeUserConsentStatus(DirRX.HRLite.Server.AsyncHandlerInvokeArgs.ChangeUserConsentStatusInvokeArgs args)
    {
      Logger.DebugFormat("DirRX.HRLite.AsyncHandlers.ChangeUserConsentStatus started, iteration: {0}", args.RetryIteration);
      int employeeId = args.EmployeeId;
      var employee = DirRX.HRSolution.Employees.Get(employeeId);
      var partTimeEmployees = HRSolution.Employees.GetAll(e => e.Status == Sungero.Company.Employee.Status.Active && e.Person.Equals(employee.Person) && e.Department.BusinessUnit.Equals(employee.Department.BusinessUnit));
      var status = new Sungero.Core.Enumeration(args.ConsentStatus);
      var oldStatus = new Sungero.Core.Enumeration(args.OldConsentStatus);
      
      foreach (var partTimeEmployee in partTimeEmployees)
      {
        try
        {
          if (partTimeEmployee.ConsentDirRX == oldStatus)
          {
            partTimeEmployee.ConsentDirRX = status;
            partTimeEmployee.Save();
          }
        }
        catch
        {
          Logger.ErrorFormat("DirRX.HRLite.AsyncHandlers.ChangeUserConsentStatus: could not update Consent status. Employee id: {0}.", partTimeEmployee.Id);
          args.Retry = true;
        }
      }
      
    }

    public virtual void CreateSubstitutionsForPerson(DirRX.HRLite.Server.AsyncHandlerInvokeArgs.CreateSubstitutionsForPersonInvokeArgs args)
    {
      int personId = args.personId;
      args.Retry = false;
      
      Logger.DebugFormat("HRLite.AsyncHandlers.CreateSubstitutionsForPerson started, iteration: {0}, personId: {1}", args.RetryIteration, personId);
      
      if (args.RetryIteration > Constants.Module.CreateSubstitutionsForEmployeeRetryCount)
      {
        Logger.ErrorFormat("HRLite.AsyncHandlers.CreateSubstitutionsForPerson didn`t create substitutions, iteration: {0}, personId: {1}", args.RetryIteration, personId);
        args.Retry = false;
        return;
      }
      
      var employees = HRSolution.Employees.GetAll(w => w.Status == HRSolution.Employee.Status.Active && w.Person.Id == personId);
      var substitute = HRSolution.Employees.Null;
      if (employees.Count() > 1)
      {
        var employeesWithLogins = employees.Where(w => w.Login != null);
        if (employeesWithLogins.Any())
        {
          if (employeesWithLogins.Count() > 1)
            substitute = employeesWithLogins.Where(e => e.Id == employeesWithLogins.Select(p => p.Id).Min()).FirstOrDefault();
          
          if (substitute == null)
            substitute = employeesWithLogins.FirstOrDefault();
        }
        else
        {
          substitute = employees.Where(e => e.Id == employees.Select(p => p.Id).Min()).FirstOrDefault();
        }
        
        try
        {
          var substitutions = Substitutions.GetAll(w => Equals(w.Substitute, substitute));
          var isNeedSendNotice = false;
          var employeesForSubstitutions = employees.Where(w => w.BusinessUnitDirRX != null && !Equals(w, substitute)).ToList();
          foreach (var employee in employeesForSubstitutions)
          {
            var oldSubstitutions = substitutions.Where(w => w.User.Id == employee.Id).Where(w => !w.EndDate.HasValue || w.EndDate.Value >= Calendar.Today);
            if (oldSubstitutions.Count() > 0)
            {
              foreach (var oldSubstitution in oldSubstitutions)
              {
                if (oldSubstitution.Substitute != substitute)
                {
                  Substitutions.Delete(oldSubstitution);
                  
                  var substitution = Substitutions.Create();
                  substitution.Substitute = substitute;
                  substitution.User = employee;
                  substitution.Save();
                  isNeedSendNotice = true;
                }
              }
            }
            else
            {
              var substitution = Substitutions.Create();
              substitution.Substitute = substitute;
              substitution.User = employee;
              substitution.Save();
              isNeedSendNotice = true;
            }
          }
          
          if (isNeedSendNotice)
            Functions.Module.StartAutoSubstitutionNotice(substitute, employeesForSubstitutions);
        }
        catch
        {
          Logger.ErrorFormat("HRLite.AsyncHandlers.CreateSubstitutionsForPerson: could not create substitutions. personId: {0}.", personId);
          args.Retry = true;
        }
      }
    }

    public virtual void AsyncSendNoticeWithAttachment(DirRX.HRLite.Server.AsyncHandlerInvokeArgs.AsyncSendNoticeWithAttachmentInvokeArgs args)
    {
      Logger.DebugFormat("HRLite.AsyncHandlers.AsyncSendNoticeWithAttachment started, iteration: {0}", args.RetryIteration);
      PublicFunctions.Module.SendNoticeWithAttachment(args.userid, args.result, args.docid);
      
      // После  допустимого количества попыток отправить уведомление в логи пишем ошибку и заканчиваем попытки.
      if (args.RetryIteration > Constants.Module.SendNoticePossibleRetryCount)
      {
        Logger.ErrorFormat(DirRX.HRLite.Resources.AsyncSendNoticeMaxRetryCount, Constants.Module.ImportOrgstructurePossibleRetryCount);
        args.Retry = false;
        return;
      }
      
      Logger.DebugFormat("HRLite.AsyncHandlers.AsyncSendNoticeWithAttachment ended, iteration: {0}", args.RetryIteration);
    }

    public virtual void ImportOrganizationStructure(DirRX.HRLite.Server.AsyncHandlerInvokeArgs.ImportOrganizationStructureInvokeArgs args)
    {
      Logger.DebugFormat("HRLite.AsyncHandlers.ImportOrganizationStructure started, iteration: {0}", args.RetryIteration);
      
      // После  допустимого количества попыток импортировать оргстуктуру в логи пишем ошибку и заканчиваем попытки.
      if (args.RetryIteration > Constants.Module.ImportOrgstructurePossibleRetryCount)
      {
        Logger.ErrorFormat(DirRX.HRLite.Resources.ImportOrgstructureMaxRetryCount, Constants.Module.ImportOrgstructurePossibleRetryCount);
        args.Retry = false;
        return;
      }
      
      var doc = Sungero.Docflow.SimpleDocuments.Get(args.docId);
      var importFilePath = Path.Combine(Path.GetTempPath(), string.Concat(Guid.NewGuid().ToString(), ".", doc.AssociatedApplication.Extension));
      doc.Export(importFilePath);
      Logger.DebugFormat("Import org structure from doc id = {0}, filename = {1}", args.docId, importFilePath);
      var result = PublicFunctions.Module.ImportOrganizationalStructure(importFilePath, args.docId);
      Logger.DebugFormat("Import org structure result: {0}", result);
      doc.Save();
      
      // Асинхронный обработчик для отправки уведомления об импорте оргстуктуры. Без него повторно вызывается событие BeforeSave Employee валится ошибка.
      var asyncHandler = AsyncHandlers.AsyncSendNoticeWithAttachment.Create();
      asyncHandler.result = result;
      asyncHandler.docid = args.docId;
      asyncHandler.userid = args.userId;
      asyncHandler.ExecuteAsync();
      Logger.DebugFormat("HRLite.AsyncHandlers.ImportOrganizationStructure ended, iteration: {0}", args.RetryIteration);
    }

    public virtual void RefreshAcquaintanceListInDocumentPublicBody(DirRX.HRLite.Server.AsyncHandlerInvokeArgs.RefreshAcquaintanceListInDocumentPublicBodyInvokeArgs args)
    {
      var document = InternalDocumentBases.Get(args.documentId);
      if (!Functions.Module.AddAcquaintanceListAndSignatureMarkInPublicBody(document, args.versionId))
        args.Retry = true;
    }
    
  }
}