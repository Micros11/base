using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringTask;

namespace DirRX.HRManagement
{
  partial class HiringTaskSharedHandlers
  {

    public virtual void PersonChanged(DirRX.HRManagement.Shared.HiringTaskPersonChangedEventArgs e)
    {
       if (e.NewValue != e.OldValue && e.NewValue != null)
        _obj.Subject = HiringTasks.Resources.HiringTaskSubjectFormat(e.NewValue.Name);
    }

    public virtual void AspirantChanged(DirRX.HRManagement.Shared.HiringTaskAspirantChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
        _obj.Subject = HiringTasks.Resources.HiringTaskSubjectFormat(e.NewValue.Name);
    }
  }

}