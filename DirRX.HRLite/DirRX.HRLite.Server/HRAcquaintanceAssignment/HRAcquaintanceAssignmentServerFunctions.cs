using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceAssignment;

namespace DirRX.HRLite.Server
{
  partial class HRAcquaintanceAssignmentFunctions
  {
    /// <summary>
    /// Проверить, можно ли выполнить задание. Задание можно выполнить, если установлен контекст ЛК или пользователь включен в роль
    /// Сотрудники, которые могут выполнять задания в веб-доступе или пользователь включен в роль для отладки.
    /// </summary>
    /// <returns>True - если задание выполнить можно, false - если нельзя.</returns>
    [Remote]
    public bool IsPossiblePerformAssignment()
    {
      var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
      return isFromEssAccount || Functions.Module.IsUserInTestRole() || Functions.Module.IsCurrentUserInWebAccessEmployees();
    }
  }
}