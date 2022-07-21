using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceTask;

namespace DirRX.HRLite.Server
{
  partial class HRAcquaintanceTaskFunctions
  {
    /// <summary>
    /// Получить участников ознакомления.
    /// </summary>
    /// <returns>Участники ознакомления.</returns>
    [Remote(IsPure = true), Public]
    public virtual List<Sungero.Company.IEmployee> GetParticipants()
    {
      var recipients = _obj.Performers.Select(x => x.Performer).ToList();
      var excludedRecipients = _obj.ExcludedPerformers.Select(x => x.ExcludedPerformer).ToList();
      return GetParticipants(recipients, excludedRecipients);
    }
    
    /// <summary>
    /// Получить участников ознакомления.
    /// </summary>
    /// <param name="recipients">Список исполнителей.</param>
    /// <param name="excludedRecipients">Список исключаемых исполнителей.</param>
    /// <returns>Участники ознакомления.</returns>
    public static List<Sungero.Company.IEmployee> GetParticipants(List<IRecipient> recipients, List<IRecipient> excludedRecipients)
    {
      var performers = DirRX.HRLite.PublicFunctions.Module.GetNonSystemActiveRecipients(recipients);
      var excludedPerformers = DirRX.HRLite.PublicFunctions.Module.GetNonSystemActiveRecipients(excludedRecipients);
      return performers.Except(excludedPerformers).ToList();
    }
  }
}