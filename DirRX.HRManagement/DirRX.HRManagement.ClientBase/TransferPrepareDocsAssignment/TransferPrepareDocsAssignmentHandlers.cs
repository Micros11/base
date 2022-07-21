using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferPrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class TransferPrepareDocsAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      // Настроить видимость полей только для блока подготовки документов.
      var isVisible = _obj.BlockUid == Constants.TransferTask.TransferPrepareDocsAssignmentBlockId;
      _obj.State.Properties.TransferDate.IsVisible = isVisible;
      _obj.State.Properties.Employee.IsVisible = isVisible;
      _obj.State.Properties.OldDepartment.IsVisible = isVisible;
      // Поля доступны, только если стартовал задачу сам СОК.
      var task = TransferTasks.As(_obj.Task);
      var isEnabled = isVisible && task.IsHRStarted.Value;
      _obj.State.Properties.TransferDate.IsEnabled = isEnabled;
      _obj.State.Properties.Employee.IsEnabled = isEnabled;
      _obj.State.Properties.OldDepartment.IsEnabled = isEnabled;
      
      // Настроить видимость полей, касающихся перевода только для блока подготовки документов в процессе перевода.
      var isVisibleTransferInfo = isVisible && task.IsTransfer.Value;
      _obj.State.Properties.Department.IsVisible = isVisibleTransferInfo;
      _obj.State.Properties.JobTitle.IsVisible = isVisibleTransferInfo;
      // Поля доступны, только если стартовал задачу сам СОК.
      var isEnabledTransferInfo = isVisibleTransferInfo && task.IsHRStarted.Value;
      _obj.State.Properties.Department.IsEnabled = isEnabledTransferInfo;
      _obj.State.Properties.JobTitle.IsEnabled = isEnabledTransferInfo;
    }
  }

}