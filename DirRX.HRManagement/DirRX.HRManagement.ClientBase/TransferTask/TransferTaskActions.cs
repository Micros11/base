using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferTask;

namespace DirRX.HRManagement.Client
{
  partial class TransferTaskActions
  {
    public virtual void GenerateEmployeeDocuments(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.Order == null)
      {
        _obj.AdditionalAgreement = Functions.TransferTask.Remote.CreateTransferHRDocument(_obj);
        _obj.Order = Functions.TransferTask.Remote.CreateTransferOrder(_obj, _obj.AdditionalAgreement);
      }
      else
      {
        Functions.TransferTask.Remote.ReCreateEmployeeHRDocuments(_obj);
        Dialogs.NotifyMessage(TransferPrepareDocsAssignments.Resources.DocumentsReform);
      }
    }

    public virtual bool CanGenerateEmployeeDocuments(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Действие Сформировать документы доступно только СОК, при этом должны быть заполнены обязательные поля
      var isEnabled = _obj.IsHRStarted.Value && _obj.TransferDate != null && _obj.Employee != null;
      if (isEnabled && _obj.IsTransfer.Value)
        isEnabled = _obj.Department != null && _obj.JobTitle != null;
      return isEnabled;
    }

    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = Functions.Module.GetReasonBeforeAbort();
      if (abortReason != string.Empty)
      {
        _obj.AbortingReason = abortReason;
        _obj.Save();
        base.Abort(e);
      }
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanAbort(e);
    }

  }
}