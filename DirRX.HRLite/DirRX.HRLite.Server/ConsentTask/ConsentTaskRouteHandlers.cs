using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.HRLite.ConsentTask;

namespace DirRX.HRLite.Server
{
  partial class ConsentTaskRouteHandlers
  {
    
    #region Уведомления об итогах подписания.
    public virtual void StartBlock21(DirRX.HRLite.Server.ConsentNoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = ConsentTasks.Resources.NotSignedNoticeSubjectFormat(_obj.AgreementGroup.StatementDocuments.FirstOrDefault().Name);
    }

    public virtual void StartBlock20(DirRX.HRLite.Server.ConsentNoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = ConsentTasks.Resources.SignedNoticeSubjectFormat(_obj.AgreementGroup.StatementDocuments.FirstOrDefault().Name);
    }
    #endregion

    #region Смена статусов.
    public virtual void Script17Execute()
    {
      Functions.Module.ChangeConsentStatusAsync(_obj.Employee, HRSolution.Employee.ConsentDirRX.Sent.ToString());
    }
    
    public virtual void Script19Execute()
    {
      Functions.Module.ChangeConsentStatusAsync(_obj.Employee, HRSolution.Employee.ConsentDirRX.NotSent.ToString());
    }

    public virtual void Script18Execute()
    {
      Functions.Module.ChangeConsentStatusAsync(_obj.Employee, HRSolution.Employee.ConsentDirRX.Signed.ToString());
    }
    #endregion
    
    #region Конвертация в Pdf/a.
    public virtual void StartBlock9(DirRX.HRLite.Server.ConsentConvertAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = ConsentTasks.Resources.AuthorConvertErrorsReworkSubjectFormat(_obj.AgreementGroup.StatementDocuments.FirstOrDefault().Name);
    }

    public virtual bool Decision10Result()
    {
      var document = _obj.AgreementGroup.StatementDocuments.FirstOrDefault();
      if (document != null && document.Versions.Any())
      {
        var version = document.Versions.LastOrDefault();
        if (!PublicFunctions.Module.ValidatePdfAFormat(version.Body.Read(), version.AssociatedApplication.Extension))
          return false;
        else
          _obj.ConvertErrorMessage = PublicFunctions.Module.ClearConverError(_obj.ConvertErrorMessage, document.Id);
      }
      return true;
    }

    public virtual bool Decision8Result()
    {
      return string.IsNullOrEmpty(_obj.ConvertErrorMessage);
    }

    public virtual void Script7Execute()
    {
      var busunessUnit = DirRX.HRSolution.BusinessUnits.As(_obj.Employee.Department.BusinessUnit);
      // Вложить положение о КЭДО.
      _obj.RegulationGroup.LocalRegulationDocuments.Add(busunessUnit.RegulationDocument);
      
      // Создать из шаблона и вложить Согласие на КЭДО.
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.DocumentManagementAgreementKind);
      var template = busunessUnit.ConsentTemplate;
      var document = StatementDocuments.CreateFrom(template);
      document.Author = _obj.Employee;
      document.DocumentKind = docKind;
      document.BusinessUnit = busunessUnit;
      document.LifeCycleState = HRLite.StatementDocument.LifeCycleState.Active;
      document.AccessRights.Grant(_obj.Author, DefaultAccessRightsTypes.FullAccess);
      document.Save();
      _obj.AgreementGroup.StatementDocuments.Add(document);
      
      // Создать связку сотрудник-документ.
      var documentAsInternal = Sungero.Docflow.InternalDocumentBases.As(document);
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(_obj.Employee, new List<Sungero.Docflow.IInternalDocumentBase>() {documentAsInternal},
                                                          HRLite.EmployeeDocumentLink.Status.Active);
      
      // Сконвертировать.
      var message = Functions.Module.ConvertToPdfa(documentAsInternal).ErrorMessage;
      message = Functions.Module.GetTextWithLowerCaseFirstSymbol(message);
      if (!string.IsNullOrEmpty(message))
        _obj.ConvertErrorMessage = message;
    }
    #endregion

    #region Подписание сотрудником.
    public virtual void StartBlock4(DirRX.HRLite.Server.ConsentAssignmentArguments e)
    {
      e.Block.Subject = DirRX.HRLite.ConsentTasks.Resources.SignConsentSubjectFormat(_obj.AgreementGroup.StatementDocuments.FirstOrDefault().Name);
      e.Block.Performers.Add(_obj.Employee);
      e.Block.ShowInSelfServiceOffice = true;
    }
    
    public virtual void CompleteAssignment4(DirRX.HRLite.IConsentAssignment assignment, DirRX.HRLite.Server.ConsentAssignmentArguments e)
    {
      var document = _obj.AgreementGroup.StatementDocuments.FirstOrDefault();
      if (assignment.Result == HRLite.ConsentAssignment.Result.Sign)
        Functions.StatementDocument.ConvertToPdfAndAddSignatureMark(document, document.LastVersion.Id);
    }
    #endregion

  }
}