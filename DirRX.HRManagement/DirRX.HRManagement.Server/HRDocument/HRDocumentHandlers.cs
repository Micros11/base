using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.Company;
using Sungero.Domain.Shared;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRDocument;

namespace DirRX.HRManagement
{
  partial class HRDocumentFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (_filter == null)
        return query;
      
      // Фильтр по виду документа.
      if (_filter.DocumentKind != null)
        query = query.Where(d => Equals(d.DocumentKind, _filter.DocumentKind));

      // Фильтр по кандидату.
      if (_filter.Employee != null)
        query = query.Where(d => Equals(d.Employee, _filter.Employee));

      // Фильтр "Подразделение".
      if (_filter.Department != null)
        query = query.Where(c => Equals(c.Department, _filter.Department));

      // Фильтр по интервалу времени
      var periodBegin = Calendar.UserToday.AddDays(-7);
      var periodEnd = Calendar.UserToday.EndOfDay();
      
      if (_filter.LastWeek)
        periodBegin = Calendar.UserToday.AddDays(-7);
      
      if (_filter.LastMonth)
        periodBegin = Calendar.UserToday.AddDays(-30);
      
      if (_filter.Last90Days)
        periodBegin = Calendar.UserToday.AddDays(-90);
      
      if (_filter.ManualPeriod)
      {
        periodBegin = _filter.DateRangeFrom ?? Calendar.SqlMinValue;
        periodEnd = _filter.DateRangeTo ?? Calendar.SqlMaxValue;
      }
      
      query = query.Where(j => j.Created.Between(periodBegin, periodEnd));
                                
      return query;
    }
  }
  
  partial class HRDocumentServerHandlers
  {

    public override void BeforeSigning(Sungero.Domain.BeforeSigningEventArgs e)
    {
      if (e.Signature.SignatureType == SignatureType.Approval && e.Signature.Signatory != null && _obj.OurSignatory == null)
      {
        // Заполнить статус согласования "Подписан".
        Functions.HRDocument.SetInternalApprovalStateToSigned(_obj);
        
        // Заполнить подписывающего в карточке документа.
        // TODO решить, чем заполнять при подписании из ЛК
        Functions.HRDocument.SetDocumentSignatory(_obj, Employees.Current == null ? _obj.Employee : Employees.Current);
      }
      
      #region Из base.BeforeSigning
      // Если подписание выполняется в рамках агента - генерировать заглушку не надо.
      bool jobRunned;
      if (e.Params.TryGetValue(Sungero.ExchangeCore.PublicConstants.BoxBase.JobRunned, out jobRunned) && jobRunned)
        return;
      
      var versionId = (e.Signature as IInternalSignature).SignedEntityProperties
        .Select(p => p.ChildEntityId).Single();
      var info = Sungero.Exchange.PublicFunctions.ExchangeDocumentInfo.Remote.GetExDocumentInfoFromVersion(_obj, versionId.Value);
      
      var version = _obj.Versions.Single(v => v.Id == versionId);
      if (e.Signature.SignatureType == SignatureType.Approval &&
          info != null &&
          !Signatures.Get(version).Any(s => s.SignatureType == SignatureType.Approval && s.Id != info.SenderSignId))
      {
        Sungero.Docflow.PublicFunctions.Module.GenerateTempPublicBodyForExchangeDocument(_obj, version.Id);
        Sungero.Exchange.PublicFunctions.Module.EnqueueXmlToPdfBodyConverter(_obj, version.Id, _obj.ExchangeState);
      }
      #endregion
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      _obj.DocumentDate = _obj.RegistrationDate.HasValue ? _obj.RegistrationDate : _obj.InitiationDate;
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.Employee = _obj.PreparedBy;
      _obj.InitiationDate = Calendar.Now;
    }
  }
}