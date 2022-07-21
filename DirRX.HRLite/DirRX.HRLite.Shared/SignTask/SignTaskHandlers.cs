using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignTask;

namespace DirRX.HRLite
{

  partial class SignTaskSharedHandlers
  {

    public virtual void ApproversChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      if (_obj.Approvers.Count == 0)
      {
        _obj.ApproveDeadline = null;
        _obj.ApprovalQueue = null;
      }
    }

    public virtual void SignatoryChanged(DirRX.HRLite.Shared.SignTaskSignatoryChangedEventArgs e)
    {
      if (e.NewValue == null)
        _obj.SignatoryDeadline = null;
      _obj.State.Properties.SignatoryDeadline.IsRequired = e.NewValue != null;
      _obj.State.Properties.SignatoryDeadline.IsEnabled = e.NewValue != null;
    }

    public virtual void DocumentGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      
      // Очистить поля, которые подтягиваются из настройки.
      _obj.Approvers.Clear();
      _obj.ApproveDeadline = null;
      _obj.ApprovalQueue = null;
      _obj.SignatoryDeadline = null;
      _obj.EmployeeDeadline = null;
      _obj.SignQueue = null;
      _obj.ActiveText = string.Empty;
      
      // Очистить данные из документа и приложения к документу.
      _obj.Signatory = null;
      _obj.Employees.Clear();
      Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(_obj.AddendaGroup, null);
    }

    public virtual void DocumentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      var document = BilaterialDocuments.As(e.Attachment);
      _obj.Subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(SignTasks.Resources.SignTaskSubject, document.Name);
      
      // Заполнить поле На подпись.
      if (_obj.Signatory == null)
      {
        var allowedSignatories = Functions.BilaterialDocument.Remote.GetSignatories(document);
        var ourSignatory = document.OurSignatory;
        if (ourSignatory != null && allowedSignatories.Any(s => Equals(s.EmployeeId, ourSignatory.Id)))
          _obj.Signatory = ourSignatory;
        else
          _obj.Signatory = Functions.BilaterialDocument.GetDefaultSignatory(document, allowedSignatories);
      }
      
      // Заполнить поле Сотрудники.
      foreach (var employees in document.Employees)
        _obj.Employees.AddNew().Employee = DirRX.HRSolution.Employees.As(employees.Employee);
      
      // Найти настройку задачи и заполнить поля из нее.
      var setting = HRManagement.PublicFunctions.Module.Remote.GetTaskSetting(HRLite.SignTasks.Info.Name, document.DocumentKind, document.BusinessUnit);
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
        _obj.SignQueue = setting.SignQueue;
        
        // Текст.
        _obj.ActiveText = setting.TaskText;
      }
      
      // Добавить приложения.
      Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(_obj.AddendaGroup, document);
    }

  }
}