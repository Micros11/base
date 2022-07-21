using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.AcquaintanceDeadlineNotice;

namespace DirRX.HRLite.Client
{
  partial class AcquaintanceDeadlineNoticeActions
  {
    public virtual void AcquaintanceReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.HRAcquaintanceTask.GetAcquaintanceControlReport(HRAcquaintanceTasks.As(_obj.Task)).Open();
    }

    public virtual bool CanAcquaintanceReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Отчет по ознакомлению может сформировать Специалист по работе с персоналом для задачи в работе или 
      // завершенной после выполнения задания на подписание.
      var hrSpecialist = Functions.Module.Remote.GetRole(Constants.Module.Roles.HRSpecialist);
      return (_obj.Task.Status.Value == HRAcquaintanceTask.Status.InProcess || _obj.Task.Status.Value == HRAcquaintanceTask.Status.Completed) &&
        HRAcquaintanceAssignments.GetAll(a => a.Task.Equals(_obj.Task)).Any() && Users.Current.IncludedIn(hrSpecialist);
    }

  }

}