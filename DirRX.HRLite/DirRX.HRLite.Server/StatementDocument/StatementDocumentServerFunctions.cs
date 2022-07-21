using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementDocument;

namespace DirRX.HRLite.Server
{
  partial class StatementDocumentFunctions
  {
    /// <summary>
    /// Получить список всех видов документа доступных для типа "Заявление сотрудника".
    /// </summary>
    [Remote]
    public static List<Sungero.Docflow.IDocumentKind> GetStatementDocumentKinds()
    {
      return HRLite.StatementsSettings.GetAll().Where(s => s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).Select(d => d.DocumentKind).ToList();
    }
    
    /// <summary>
    /// Получить список всех подписей документа.
    /// </summary>
    /// <param name="versionId">Ид версии документа.</param>
    /// <returns>Коллекция подписей документа.</returns>
    public System.Collections.Generic.IEnumerable<Sungero.Domain.Shared.ISignature> GetDocumentSignatures(int versionId)
    {
      var version = _obj.Versions.FirstOrDefault(x => x.Id == versionId);
      if (version != null)
      {
        var versionSignatures = Signatures.Get(version)
          .Where(s => s.SignatureType == SignatureType.Approval)
          .GroupBy(s => s.SignatoryFullName).Select(s => s.OrderBy(d => d.SigningDate).FirstOrDefault())
          .OrderBy(d => d.SigningDate);
        return versionSignatures;
      }
      return Enumerable.Empty<Sungero.Domain.Shared.ISignature>();
    }
    
    /// <summary>
    /// Получить список отметок об ЭП в формате html.
    /// </summary>
    /// <param name="versionId">Ид версии документа.</param>
    /// <returns>Список отметок об ЭП.</returns>
    public virtual List<string> GetDocumentHtmlStamps(int versionId)
    {
      var signatures = this.GetDocumentSignatures(versionId);
      var htmlStamps = new List<string>();
      var htmlStamp = string.Empty;
      foreach (var signature in signatures)
      {
        if (signature.SignCertificate != null)
          htmlStamp = Functions.Module.GetSignatureMarkForCertificateAsHtml(signature);
        else
          htmlStamp = Functions.Module.GetSignatureMarkForSimpleSignatureAsHtml(signature);
        htmlStamps.Add(htmlStamp);
      }
      
      return htmlStamps;
    }
    
    /// <summary>
    /// Сконвертировать документ в ПДФ и поставить штамп.
    /// </summary>
    /// <param name="versionId">Версия документа.</param>
    /// <returns>Результат преобразования документа в PDF.</returns>
    public override Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult ConvertToPdfAndAddSignatureMark(int versionId)
    {
      var info = Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult.Create();
      info.HasErrors = false;
      var version = _obj.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        info.HasErrors = true;
        info.HasConvertionError = true;
        info.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(versionId);
        return info;
      }
      
      var documentWithStampFileName = PublicFunctions.Module.Remote.GetPdfaDocumentInLocalPdfFileWithStamp(_obj, versionId);
      var errorsList = PublicFunctions.Module.Remote.UploadPdfWithStampToDocumentVersion(documentWithStampFileName, version, _obj);

      if (errorsList.Count > 0)
      {
        Logger.Error(string.Join(Environment.NewLine, errorsList));
        info.HasErrors = true;
        info.HasConvertionError = false;
        info.HasLockError = false;
        info.ErrorMessage = string.Join(Environment.NewLine, errorsList);
      }
      return info;
    }
  }
}