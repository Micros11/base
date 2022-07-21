using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferTask;

namespace DirRX.HRManagement
{
  partial class TransferTaskClientHandlers
  {

    public virtual void IsTransferValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      // При изменении значения поля "Перевод в другое подразделение" вручную, если ранее были сформированы документы,
      // удалить их из задачи и поставить в очередь на удаление
      if (_obj.Order != null)
      {
        Functions.Module.Remote.EnqueueDocumentDeleteJob(Sungero.Docflow.OfficialDocuments.As(_obj.Order));
        Functions.Module.Remote.EnqueueDocumentDeleteJob(Sungero.Docflow.OfficialDocuments.As(_obj.AdditionalAgreement));
        _obj.HRDocumentAttachmentGroup.HRDocumentBases.Clear();
        _obj.HRDocumentAttachmentGroup.HROrders.Clear();
        _obj.Order = null;
        _obj.AdditionalAgreement = null;
      }
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var errors = string.Empty;
      if (!e.Params.TryGetValue("Errors", out errors) || string.IsNullOrEmpty(errors))
        return;
      e.AddError(errors);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      PublicFunctions.Module.Remote.CheckPersonnelModuleLicense();
      Functions.TransferTask.SetEnabledProperties(_obj);
    }
  }
}