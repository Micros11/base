using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.Aspirant;

namespace DirRX.HRManagement.Shared
{
  partial class AspirantFunctions
  {

    /// <summary>
    /// Обновить ФИО сотрудника.
    /// </summary>
    /// <param name="person">Персона.</param>
    public void UpdateName(Sungero.Parties.IPerson person)
    {
      if (person != null && !Equals(person.Name, _obj.Name))
        _obj.Name = person.Name;
    }

  }
}