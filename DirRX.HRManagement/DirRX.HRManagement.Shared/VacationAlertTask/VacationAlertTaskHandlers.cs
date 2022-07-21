using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationAlertTask;

namespace DirRX.HRManagement
{

  partial class VacationAlertTaskSharedHandlers
  {

    public virtual void NotificationChanged(DirRX.HRManagement.Shared.VacationAlertTaskNotificationChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        _obj.VacationNoticeAttachmentGroup.HRDocumentBases.Clear();
        _obj.VacationNoticeAttachmentGroup.HRDocumentBases.Add(e.NewValue);
      }
    }
  }
}