using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignTask;

namespace DirRX.HRLite.Shared
{
  partial class SignTaskFunctions
  {

    /// <summary>
    /// Рассчитать ожидаемый срок и сформировать информационное сообщение.
    /// </summary>
    /// <returns>Сообщение для вывода на экран.</returns>
    public string GenerateExpectedTaskDeadlineInfo()
    {
      return SignTasks.Resources.ExpectedTaskDeadlineInfoFormat(Calendar.Today.AddWorkingDays((_obj.EmployeeDeadline == null ? 0 : _obj.EmployeeDeadline.Value) +
                                                                                              (_obj.SignatoryDeadline == null ? 0 : _obj.SignatoryDeadline.Value) +
                                                                                              (_obj.ApproveDeadline == null ? 0 : _obj.ApproveDeadline.Value)).ToString("dd.MM.yyyy"));
    }
    
    /// <summary>
    /// Проверить, заполнены ли все необходимые сроки заданий на карточке задачи.
    /// </summary>
    /// <returns>True, если заполнены все необходимые сроки, иначе false.</returns>
    public bool IsAllDeadlinesFilled()
    {
      return _obj.SignatoryDeadline != null && _obj.EmployeeDeadline != null &&
        ((_obj.Approvers.Count > 0 && _obj.ApproveDeadline != null  && _obj.ApprovalQueue != null) || _obj.Approvers.Count == 0);
    }
    
  }
}