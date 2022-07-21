using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRDocumentBase;

namespace DirRX.HRManagement.Client
{

  partial class HRDocumentBaseActions
  {
    public override void ExportDocument(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var docsForExport = Functions.Module.Remote.PrepareExportHRDocuments(new List<Sungero.Docflow.IOfficialDocument>() { _obj }, false);
        var exportRezult = Functions.Module.Remote.PrepareBodiesAndSignsDocuments(docsForExport);
        var zip = Functions.Module.Remote.CreateZipFromZipModel(exportRezult.ZipModels, exportRezult.ExportedDocuments);
        zip.Export(); 
      }
      catch (Exception ex)
      {
        var addErrorMessage = Resources.ExportDialog_Error_Client_NoReason;
        Logger.Error(addErrorMessage, ex);
        e.AddError(addErrorMessage);
        return;
      }
    }

    public override bool CanExportDocument(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanExportDocument(e);
    }

    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);
    }

  }

}