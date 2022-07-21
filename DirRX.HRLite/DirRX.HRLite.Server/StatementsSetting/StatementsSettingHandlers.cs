using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementsSetting;

namespace DirRX.HRLite
{
  partial class StatementsSettingTemplateRXPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> TemplateRXFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var allStatementTemplates = Sungero.Docflow.DocumentTemplates.GetAll().ToList().Where(t => t.DocumentType != null && HRLite.Functions.Module.IsStatement(t.DocumentType.Value)).ToList();
      return query.Where(t => allStatementTemplates.Contains(t) && t.Status == Sungero.Docflow.DocumentTemplate.Status.Active);
    }
  }

  partial class StatementsSettingServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (_obj.Status == StatementsSetting.Status.Active && !_obj.Performers.Any())
        e.AddError(HRLite.Resources.NeedAnyPerformers);
    }
  }

  partial class StatementsSettingCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      base.CreatingFrom(e);
      e.Without(_info.Properties.DocumentKind);
    }
  }

  partial class StatementsSettingTemplatePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> TemplateFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var allStatementTemplates = Sungero.Docflow.DocumentTemplates.GetAll().ToList().Where(t => t.DocumentType != null && HRLite.Functions.Module.IsStatement(t.DocumentType.Value)).ToList();
      return query.Where(t => allStatementTemplates.Contains(t) && t.Status == Sungero.Docflow.DocumentTemplate.Status.Active);
    }
  }

  partial class StatementsSettingDocumentKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // уже использованные виды документов (за исключением текущей записи)
      var usedDocumentKind = StatementsSettings.GetAll().Where(ss => ss.Status == StatementsSetting.Status.Active && ss.Id != _obj.Id).Select(ss => ss.DocumentKind).ToList();
      
      // доступные для выбора виды документов: действующие, относящиеся к типу документа StatementDocument и не входящие в список использованных
      var availableDocumentKinds = Sungero.Docflow.DocumentKinds.GetAll().ToList().Where(dk => !usedDocumentKind.Contains(dk) && 
                                                                                         HRLite.Functions.Module.IsStatement(dk.DocumentType) &&
                                                                                         dk.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
      return query.Where(k => availableDocumentKinds.Contains(k));
    }
  }
}