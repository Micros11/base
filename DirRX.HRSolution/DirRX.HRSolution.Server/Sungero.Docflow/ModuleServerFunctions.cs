using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;

namespace DirRX.HRSolution.Module.Docflow.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Получить подписи неформализованного документа.
    /// </summary>
    /// <param name="document">Неформализованный документ.</param>
    /// <returns>Список подписей.</returns>
    public override List<Sungero.Domain.Shared.ISignature> GetDocumentSignature(IOfficialDocument document)
    {
      var documentTypes = new List<string> {
        "270b7392-3114-4759-849e-ce44b9bddb0a", // BilaterialDocument
        "38686a2c-ec27-4453-9c0b-f540d11854c8", // LocalRegulationDocument
        "ded6a81c-e3cf-4c83-9dfb-3c8b455ecec1", // PersonnelDocument
        "48eb5e1d-a0bd-4fca-ae78-54dff8c193a9"  // StatementDocument
      };
      
      if (documentTypes.Contains(document.DocumentKind.DocumentType.DocumentTypeGuid))
      {
        var version = this.GetExportedDocumentVersion(document);
        return Signatures.Get(version).Where(s => s.IsValid && s.SignCertificate != null).ToList();
      }
      else
        return base.GetDocumentSignature(document);
    }

  }
}