using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;

namespace DirRX.HRLite.Shared
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Возвращает список подписывающих по правилу.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Список тех, кто имеет право подписи.</returns>
    public List<Sungero.Docflow.Structures.SignatureSetting.Signatory> GetSignatories(IOfficialDocument document)
    {
      if (BilaterialDocuments.Is(document))
        return Functions.BilaterialDocument.Remote.GetSignatories(BilaterialDocuments.As(document));  
      else if (PersonnelDocuments.Is(document))
        return Functions.PersonnelDocument.Remote.GetSignatories(PersonnelDocuments.As(document));
      else if (LocalRegulationDocuments.Is(document))
        return Functions.LocalRegulationDocument.Remote.GetSignatories(LocalRegulationDocuments.As(document));
      return null;
    }
    
    /// <summary>
    /// Пполучить подписывающего по умолчанию.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="signatories">Список подписывающих с приоритетом.</param>
    /// <returns>Подписывающий по умолчанию.</returns>
    public Sungero.Company.IEmployee GetDefaultSignatory(IOfficialDocument document, List<Sungero.Docflow.Structures.SignatureSetting.Signatory> signatories)
    {
      if (BilaterialDocuments.Is(document))
        return Functions.BilaterialDocument.GetDefaultSignatory(BilaterialDocuments.As(document), signatories);
      else if (PersonnelDocuments.Is(document))
        return Functions.PersonnelDocument.GetDefaultSignatory(PersonnelDocuments.As(document), signatories);
      else if (LocalRegulationDocuments.Is(document))
        return Functions.LocalRegulationDocument.GetDefaultSignatory(LocalRegulationDocuments.As(document), signatories);
      return null;
    }

  }
}