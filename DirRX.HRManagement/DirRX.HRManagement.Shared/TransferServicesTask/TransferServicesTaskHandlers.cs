using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferServicesTask;

namespace DirRX.HRManagement
{
  partial class TransferServicesTaskSharedHandlers
  {

    public virtual void IsTransferChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      var employeeName = _obj.Employee == null ? string.Empty : _obj.Employee.Name;
      _obj.Subject = e.NewValue.Value ? TransferServicesTasks.Resources.TransferServiceTaskSubjectFormat(employeeName) :
          TransferServicesTasks.Resources.ChangeWorkConditionServiceTaskSubjectFormat(employeeName);
      
    }

    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.TransferServicesTaskEmployeeChangedEventArgs e)
    {
      var employeeName = e.NewValue == null ? string.Empty : e.NewValue.Name;
      if (_obj.IsTransfer.HasValue)
        _obj.Subject = _obj.IsTransfer.Value ? TransferServicesTasks.Resources.TransferServiceTaskSubjectFormat(employeeName) :
          TransferServicesTasks.Resources.ChangeWorkConditionServiceTaskSubjectFormat(employeeName);
    }

  }
}