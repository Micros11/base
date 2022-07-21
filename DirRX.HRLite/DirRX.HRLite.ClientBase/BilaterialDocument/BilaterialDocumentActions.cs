using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.BilaterialDocument;
using Sungero.Docflow.OfficialDocument;

namespace DirRX.HRLite.Client
{
  partial class BilaterialDocumentCollectionActions
  {

    public virtual bool CanExportHRDocument(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      if (e.FormType == Sungero.Domain.Shared.FormType.Collection)
        return true;
      return _objs.Any() && _objs.All(d => !d.State.IsChanged);
    }

    public virtual void ExportHRDocument(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var documents = new List<Sungero.Docflow.IOfficialDocument>(_objs);
      Sungero.Docflow.Client.ModuleFunctions.ExportDocumentDialog(documents);
    }
  }

  partial class BilaterialDocumentActions
  {

    public virtual void SendToSign(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = SignTasks.Create();
      task.DocumentGroup.BilaterialDocuments.Add(_obj);
      task.ShowModal();
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanSendToSign(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsChanged && _obj.Versions.Count > 0;
    }
  }

}