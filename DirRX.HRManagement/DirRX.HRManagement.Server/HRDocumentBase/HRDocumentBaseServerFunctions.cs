using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRDocumentBase;

namespace DirRX.HRManagement.Server
{
  partial class HRDocumentBaseFunctions
  {
    /// <summary>
    /// Возвращает список подписывающих.
    /// </summary>
    /// <returns>Список тех, кто имеет право подписи.</returns>
    [Remote(IsPure = true)]
    public override List<Sungero.Docflow.Structures.SignatureSetting.Signatory> GetSignatories()
    {
      return base.GetSignatories();
    }
    
  }
}