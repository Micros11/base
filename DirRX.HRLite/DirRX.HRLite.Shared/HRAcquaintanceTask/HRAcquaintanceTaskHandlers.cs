using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceTask;

namespace DirRX.HRLite
{

  partial class HRAcquaintanceTaskSharedHandlers
  {

    public virtual void ApproversChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      if (_obj.Approvers.Count == 0)
      {
        _obj.ApproveDeadline = null;
        _obj.ApprovalQueue = null;
      }
    }
    
    public virtual void AddendaGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      // Проверить тип приложения на соответствие типу документа на подписание.
      if (_obj.DocumentGroup.InternalDocumentBases.Any())
      {
        if ((PersonnelDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()) && !PersonnelDocuments.Is(e.Attachment)) ||
            (LocalRegulationDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()) && !LocalRegulationDocuments.Is(e.Attachment)) ||
            (DirRX.HRManagement.HRDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()) && !DirRX.HRManagement.HRDocuments.Is(e.Attachment)))
          throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.AddendaTypeError);
        
        if (DirRX.HRManagement.HRDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()))
        {
          var docKind = DirRX.HRManagement.HRDocuments.Get(e.Attachment.Id).DocumentKind;
          var docKindGuid = DirRX.HRManagement.PublicFunctions.Module.Remote.GetDocumentKindGuid(docKind);
          if (docKindGuid != DirRX.HRManagement.PublicConstants.Module.DocumentKind.VacationScheduleKind)
            throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.AddendaKindError);
        }       
      }
    }

    public virtual void SignatoryChanged(DirRX.HRLite.Shared.HRAcquaintanceTaskSignatoryChangedEventArgs e)
    {
      if (e.NewValue == null)
        _obj.SignatoryDeadline = null;
      _obj.State.Properties.SignatoryDeadline.IsRequired = e.NewValue != null;
      _obj.State.Properties.SignatoryDeadline.IsEnabled = e.NewValue != null;
    }

    public virtual void DocumentGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      _obj.Signatory = null;
      
      // Очистить поля, которые подтягиваются из настройки.
      _obj.Approvers.Clear();
      _obj.ApproveDeadline = null;
      _obj.ApprovalQueue = null;
      _obj.SignatoryDeadline = null;
      _obj.EmployeeDeadline = null;
      _obj.ActiveText = string.Empty;
      
      // Очистка приложений.
      Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(_obj.AddendaGroup, null);
      // Удаление приказа о введении ЛНА из вложения.
      if (LocalRegulationDocuments.Is(e.Attachment))
      {
        var document = LocalRegulationDocuments.As(e.Attachment);
        if (document.LeadingDocument != null && _obj.AddendaGroup.InternalDocumentBases.Contains(document.LeadingDocument))
          _obj.AddendaGroup.InternalDocumentBases.Remove(document.LeadingDocument);
      }
    }

    public virtual void DocumentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      // Если задача создается вручную - в качестве вложения должен быть Кадровый документ или ЛНА.
      if (!PersonnelDocuments.Is(e.Attachment) && !LocalRegulationDocuments.Is(e.Attachment) && !DirRX.HRManagement.HRDocuments.Is(e.Attachment))
        throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.AttachmentError);
      
      if (DirRX.HRManagement.HRDocuments.Is(e.Attachment))
      {
        var docKind = DirRX.HRManagement.HRDocuments.Get(e.Attachment.Id).DocumentKind;
        var docKindGuid = DirRX.HRManagement.PublicFunctions.Module.Remote.GetDocumentKindGuid(docKind);
        if (docKindGuid != DirRX.HRManagement.PublicConstants.Module.DocumentKind.VacationScheduleKind)
          throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.AttachmentError);
      }
      
      var document = Sungero.Docflow.InternalDocumentBases.As(e.Attachment);
      var subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(HRAcquaintanceTasks.Resources.AcquaintanceTaskSubject, document.Name);
      _obj.Subject = subject.Length > 250 ? subject.Substring(0, 250) : subject;
      
      // Заполнить поле На подпись для Кадрового документа или ЛНА.
      if (_obj.Signatory == null && (PersonnelDocuments.Is(e.Attachment) || LocalRegulationDocuments.Is(e.Attachment)))
      {
        var allowedSignatories = Functions.Module.GetSignatories(document);
        var ourSignatory = document.OurSignatory;
        if (ourSignatory != null && allowedSignatories.Any(s => Equals(s.EmployeeId, ourSignatory.Id)))
          _obj.Signatory = ourSignatory;
        else
          _obj.Signatory = Functions.Module.GetDefaultSignatory(document, allowedSignatories);
      }
      
      // Найти настройку задачи и заполнить поля из нее.
      var setting = HRManagement.PublicFunctions.Module.Remote.GetTaskSetting(HRLite.HRAcquaintanceTasks.Info.Name, document.DocumentKind, document.BusinessUnit);
      if (setting != null)
      {
        // Согласование.
        foreach (var approver in setting.Approvers.Select(a => a.Approver))
          _obj.Approvers.AddNew().Approver = approver;
        if (setting.ApproveDeadlineDays.HasValue)
          _obj.ApproveDeadline = setting.ApproveDeadlineDays.Value;
         if (setting.ApprovalQueue.HasValue)
          _obj.ApprovalQueue = setting.ApprovalQueue.Value;
        
        // Подписание.
        _obj.SignatoryDeadline = setting.SignerDealineDays.Value;
        _obj.EmployeeDeadline = setting.EmployeeDealineDays.Value;
        
        // Текст.
        _obj.ActiveText = setting.TaskText;
      }
      
      // Добавить приложения кадрового документа.
      if (PersonnelDocuments.Is(e.Attachment))
        Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(_obj.AddendaGroup, document);
      // Добавить приказ о введении ЛНА во вложение.
      else if (LocalRegulationDocuments.Is(e.Attachment))
      {
        if (document.LeadingDocument != null && !_obj.AddendaGroup.InternalDocumentBases.Contains(document.LeadingDocument))
          _obj.AddendaGroup.All.Add(document.LeadingDocument);
      }
    }
    
    public virtual void AddendaGroupCreated(Sungero.Workflow.Interfaces.AttachmentCreatedEventArgs e)
    {
      // Проверить тип приложения на соответствие типу документа на подписание.
      if (_obj.DocumentGroup.InternalDocumentBases.Any())
      {
        if ((PersonnelDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()) && !PersonnelDocuments.Is(e.Attachment)) ||
            (LocalRegulationDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()) && !LocalRegulationDocuments.Is(e.Attachment)) ||
            (DirRX.HRManagement.HRDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()) && !DirRX.HRManagement.HRDocuments.Is(e.Attachment)))
          throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.AddendaTypeError);
        
        if (DirRX.HRManagement.HRDocuments.Is(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault()))
        {
          var docKind = DirRX.HRManagement.HRDocuments.Get(e.Attachment.Id).DocumentKind;
          var docKindGuid = DirRX.HRManagement.PublicFunctions.Module.Remote.GetDocumentKindGuid(docKind);
          if (docKindGuid != DirRX.HRManagement.PublicConstants.Module.DocumentKind.VacationScheduleKind)
            throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.AddendaKindError);
        }
      }
    }

    public virtual void DocumentGroupCreated(Sungero.Workflow.Interfaces.AttachmentCreatedEventArgs e)
    {
      if (!PersonnelDocuments.Is(e.Attachment) && !LocalRegulationDocuments.Is(e.Attachment))
        throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.AttachmentError);
    }
  }
}