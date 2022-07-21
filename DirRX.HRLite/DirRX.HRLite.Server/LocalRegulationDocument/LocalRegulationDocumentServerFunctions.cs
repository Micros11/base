using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.LocalRegulationDocument;

namespace DirRX.HRLite.Server
{
  partial class LocalRegulationDocumentFunctions
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
  }
}