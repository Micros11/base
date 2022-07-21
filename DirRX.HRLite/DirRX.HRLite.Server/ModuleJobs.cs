using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRLite.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Отправить уведомления о приближении срока заданий на подписание за 1 день до срока.
    /// </summary>
    public virtual void SendSignTaskDeadlineNotice()
    {
      Functions.Module.SendSignTaskDeadlineNotice(1);
    }

    /// <summary>
    /// Отправить уведомления о приближении срока заданий на ознакомление с кадровыми документами и ЛНА за 1 день до срока.
    /// </summary>
    public virtual void SendAcquaintanceTaskDeadlineNotice()
    {
      Functions.Module.SendAcquaintanceTaskDeadlineNotice(1);
    }

  }
}