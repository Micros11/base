using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TaskSetting;

namespace DirRX.HRManagement
{

  partial class TaskSettingSharedHandlers
  {

    public virtual void ApproversChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      if (_obj.Approvers.Count == 0)
      {
        _obj.ApproveDeadlineDays = null;
        _obj.ApprovalQueue = null;
      }
    }

    public virtual void TaskTypeChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(e.NewValue) || !_obj.TaskType.Equals(HRLite.SignTasks.Info.Name))
      {
        _obj.SignQueue = null;
        _obj.State.Properties.SignQueue.IsEnabled = false;
        _obj.State.Properties.SignQueue.IsRequired = false;
      }
      else
      {
        _obj.State.Properties.SignQueue.IsEnabled = true;
        _obj.State.Properties.SignQueue.IsRequired = true;
      }
      
    }
  }

}