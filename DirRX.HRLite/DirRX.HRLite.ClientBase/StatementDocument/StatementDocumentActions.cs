using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementDocument;

namespace DirRX.HRLite.Client
{
  partial class StatementDocumentCollectionActions
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

}