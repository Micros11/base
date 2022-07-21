using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.InformingTask;

namespace DirRX.HRLite.Server
{
  partial class InformingTaskFunctions
  {

    /// <summary>
    /// Получить участников уведомления.
    /// </summary>
    /// <returns>Участники уведомления.</returns>
    [Remote(IsPure = true), Public]
    public virtual List<Sungero.Company.IEmployee> GetRecipients()
    {
      var recipients = _obj.Recipients.Select(x => x.Recipient).ToList();
      return DirRX.HRLite.PublicFunctions.Module.GetNonSystemActiveRecipients(recipients);
    }
        
  }
}