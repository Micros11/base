using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.HROrder;
using Sungero.Domain.Shared;

namespace DirRX.HRManagement
{
  partial class HROrderServerHandlers
  {
    public override void BeforeSigning(Sungero.Domain.BeforeSigningEventArgs e)
    {
      if (e.Signature.SignatureType == SignatureType.Approval && e.Signature.Signatory != null && _obj.OurSignatory == null)
      {
        // Заполнить статус согласования "Подписан".
        Functions.HROrder.SetInternalApprovalStateToSigned(_obj);
        
        // Заполнить подписывающего в карточке документа.
        // TODO решить, чем заполнять при подписании из ЛК
        Functions.HROrder.SetDocumentSignatory(_obj, Employees.Current == null ? _obj.Employee : Employees.Current);
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
  }

  partial class HROrderDepartmentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> DepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = query.Where(d => d.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
      if (_obj.BusinessUnit != null)
        query = query.Where(d => Equals(d.BusinessUnit, _obj.BusinessUnit));
      return query;
    }
  }
}