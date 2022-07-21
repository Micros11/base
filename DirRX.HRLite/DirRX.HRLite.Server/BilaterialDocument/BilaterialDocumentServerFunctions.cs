using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.BilaterialDocument;

namespace DirRX.HRLite.Server
{
  partial class BilaterialDocumentFunctions
  {
    /// <summary>
    /// Возвращает список подписывающих по правилу.
    /// </summary>
    /// <returns>Список тех, кто имеет право подписи.</returns>
    [Remote(IsPure = true)]
    public override List<Sungero.Docflow.Structures.SignatureSetting.Signatory> GetSignatories()
    {
      return base.GetSignatories();
    }
    
    /// <summary>
    /// Получить список всех подписей документа.
    /// </summary>
    /// <param name="versionId">Ид версии документа.</param>
    /// <returns>Коллекция подписей документа.</returns>
    public System.Collections.Generic.IEnumerable<Sungero.Domain.Shared.ISignature> GetDocumentSignatures(int versionId)
    {
      var version = _obj.Versions.FirstOrDefault(x => x.Id == versionId);
      var versionSignatures = Signatures.Get(version)
        .Where(s => s.SignatureType == SignatureType.Approval)
        .GroupBy(s => s.SignatoryFullName).Select(s => s.OrderBy(d => d.SigningDate).FirstOrDefault())
        .OrderBy(d => d.SigningDate);
      return versionSignatures;
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
    
    public override Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult ConvertToPdfAndAddSignatureMark(int versionId)
    {
      var info = Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult.Create();
      info.HasErrors = true;
      var version = _obj.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        info.HasConvertionError = true;
        info.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(versionId);
        return info;
      }
      
      var pdfDocumentStream = new System.IO.MemoryStream();
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          var pdfConverter = new Sungero.AsposeExtensions.Converter();
          var extension = version.BodyAssociatedApplication.Extension;
          // Сконвертировать в pdf документ.
          var pdfDocument = new Aspose.Pdf.Document(inputStream);
          pdfDocument.RemovePdfaCompliance();
          // Добавить новый лист для штампов.
          pdfDocument.Pages.Add();
          var pagesCount = pdfDocument.Pages.Count;
          // Координаты отсчитываются от нижнего левого угла.
          var horizontalCoord = 200;
          var verticalCoord = pdfDocument.Pages[pagesCount].Rect.Height - 50;
          var htmlStamps = this.GetDocumentHtmlStamps(versionId);
          // Отметка об эп проставляется только на последних страницах.
          var pages = new int[] { pagesCount };
          foreach (var htmlStamp in htmlStamps)
          {
            var pdfStamp = pdfConverter.CreateMarkFromHtml(htmlStamp);
            pdfStamp.XIndent = horizontalCoord;
            // Отступ сверху на высоту штампа.
            pdfStamp.YIndent = verticalCoord - pdfStamp.PdfPage.PageInfo.Height;
            pdfConverter.AddStampToDocument(pdfDocument, pdfStamp, pages);
            verticalCoord = verticalCoord - pdfStamp.PdfPage.PageInfo.Height - 12;
            // Если штампы не помещаются на лист, добавить новый.
            if (verticalCoord <= pdfStamp.PdfPage.PageInfo.Height)
            {
              pdfDocument.Pages.Add();
              pagesCount = pagesCount + 1;
              verticalCoord = pdfDocument.Pages[pagesCount].Rect.Height - 50;
              pages = new int[] { pagesCount };
            }
          }
          pdfDocument.Save(pdfDocumentStream);
        }
        catch (Exception e)
        {
          if (e is Sungero.AsposeExtensions.PdfConvertException)
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(_obj.Id), e.InnerException);
          else
            Logger.Error(string.Format("{0} {1}", Sungero.Docflow.Resources.PdfConvertErrorFormat(_obj.Id), e.Message));
          
          info.HasConvertionError = true;
          info.HasLockError = false;
          info.ErrorMessage = e.Message;
        }
      }
      
      if (!string.IsNullOrWhiteSpace(info.ErrorMessage))
        return info;
      
      version.PublicBody.Write(pdfDocumentStream);
      version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
      pdfDocumentStream.Close();

      try
      {
        _obj.Save();
        info.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = false;
        info.HasLockError = true;
        info.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = true;
        info.HasLockError = false;
        info.ErrorMessage = e.Message;
      }
      
      return info;
    }
  }
}