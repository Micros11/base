using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignTask;

namespace DirRX.HRLite.Server
{
  partial class SignTaskFunctions
  {
    /// <summary>
    /// Получить сотрудников на подписание.
    /// </summary>
    /// <returns>Сотрудники на подписание.</returns>
    [Remote(IsPure = true), Public]
    public virtual List<Sungero.Company.IEmployee> GetParticipants()
    {
      return BilaterialDocuments.As(_obj.DocumentGroup.BilaterialDocuments.FirstOrDefault()).Employees.Select(t => t.Employee).ToList();
    }
    
    /// <summary>
    /// Проставить на документы штампы электронной подписи.
    /// </summary>
    public virtual void AddSignatureMarksToDocuments()
    {
      var documents = _obj.DocumentGroup.BilaterialDocuments.Concat(_obj.AddendaGroup.BilaterialDocuments);
      foreach (var document in documents)
        Functions.BilaterialDocument.ConvertToPdfAndAddSignatureMark(document, document.LastVersion.Id);
    }
  }
}