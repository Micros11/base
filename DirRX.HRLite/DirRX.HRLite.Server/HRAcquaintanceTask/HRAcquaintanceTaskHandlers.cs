using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceTask;

namespace DirRX.HRLite
{
  partial class HRAcquaintanceTaskApproversApproverPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ApproversApproverFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(a => Sungero.Company.Employees.Is(a) || a.Sid == Sungero.Docflow.Constants.Module.RoleGuid.DepartmentManagersRole);
    }
  }

  partial class HRAcquaintanceTaskServerHandlers
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
      var document = _obj.DocumentGroup.InternalDocumentBases.FirstOrDefault();
      if (document.Versions.Count == 0)
        throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.EmptyDocumentError);
      
      var addendas = _obj.AddendaGroup.InternalDocumentBases;
      foreach (var addenda in addendas)
        if (addenda.Versions.Count == 0)
          throw AppliedCodeException.Create(HRAcquaintanceTasks.Resources.EmptyAddendaErrorFormat(addenda.Name));
      
      // Проверить полномочия руководителя на подпись документов и приложений.
      // Если подписанта нет - не проверяем права на подпись.
      if (_obj.Signatory != null)
      {
        if (!HRLite.PublicFunctions.Module.IsSignRight(document.BusinessUnit, document.DocumentKind, _obj.Signatory))
          throw AppliedCodeException.Create(DirRX.HRLite.Resources.ErrorNotSignRightsFormat(_obj.Signatory.DisplayValue));
        foreach (var addendaDocument in addendas)
        {
          if (!HRLite.PublicFunctions.Module.IsSignRight(addendaDocument.BusinessUnit, addendaDocument.DocumentKind, _obj.Signatory))
            throw AppliedCodeException.Create(DirRX.HRLite.Resources.ErrorNotSignRightsFormat(_obj.Signatory.DisplayValue));
        }
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      _obj.NotifyAuthor = false;
    }
  }

  partial class HRAcquaintanceTaskExcludedPerformersExcludedPerformerPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ExcludedPerformersExcludedPerformerFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Sungero.RecordManagement.PublicFunctions.Module.ObserversFiltering(query);
    }
  }

  partial class HRAcquaintanceTaskPerformersPerformerPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> PerformersPerformerFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return (IQueryable<T>)Sungero.RecordManagement.PublicFunctions.Module.ObserversFiltering(query);
    }
  }

  partial class HRAcquaintanceTaskSignatoryPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SignatoryFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var document = PersonnelDocuments.As(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault());
      if (document != null)
      {
        var signatories = Functions.PersonnelDocument.GetSignatories(document).Select(s => s.EmployeeId).Distinct().ToList();
        return query.Where(s => signatories.Contains(s.Id));
      }
      else
        return query;
    }
  }

}