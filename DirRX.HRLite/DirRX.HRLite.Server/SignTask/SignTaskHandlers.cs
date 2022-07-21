using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignTask;

namespace DirRX.HRLite
{
  partial class SignTaskApproversApproverPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ApproversApproverFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(a => Sungero.Company.Employees.Is(a) || a.Sid == Sungero.Docflow.Constants.Module.RoleGuid.DepartmentManagersRole);
    }
  }

  partial class SignTaskSignatoryPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SignatoryFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var document = BilaterialDocuments.As(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault());
      if (document != null)
      {
        var signatories = Functions.BilaterialDocument.GetSignatories(document).Select(s => s.EmployeeId).Distinct().ToList();
        return query.Where(s => signatories.Contains(s.Id));
      }
      else
        return query;
    }
  }

  partial class SignTaskServerHandlers
  {

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;
      
      HRManagement.PublicFunctions.Module.Remote.SendAbortationNotices(_obj, DirRX.HRLite.SignTasks.Resources.AbortNoticeFormat(_obj.DisplayValue), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      // Проверить наличие тел документа на подписание и его приложений.
      var document = _obj.DocumentGroup.BilaterialDocuments.FirstOrDefault();
      if (document.Versions.Count == 0)
        throw AppliedCodeException.Create(SignTasks.Resources.EmptyDocumentError);
      
      var addendas = _obj.AddendaGroup.BilaterialDocuments;
      foreach (var addenda in addendas)
        if (addenda.Versions.Count == 0)
          throw AppliedCodeException.Create(SignTasks.Resources.EmptyAddendaErrorFormat(addenda.Name));
      
      // Проверить полномочия руководителя на подпись документов и приложений.
      if (!Functions.BilaterialDocument.GetSignatories(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault()).Select(s => s.EmployeeId).Contains(_obj.Signatory.Id))
        throw AppliedCodeException.Create(DirRX.HRLite.Resources.ErrorNotSignRightsFormat(_obj.Signatory.DisplayValue));
      foreach (var addendaDocument in _obj.AddendaGroup.BilaterialDocuments)
      {
        if (!Functions.BilaterialDocument.GetSignatories(addendaDocument).Select(s => s.EmployeeId).Contains(_obj.Signatory.Id))
          throw AppliedCodeException.Create(DirRX.HRLite.Resources.ErrorNotSignRightsFormat(_obj.Signatory.DisplayValue));
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      _obj.NotifyAuthor = false;
    }
  }

}