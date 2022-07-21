using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.Employee;

namespace DirRX.EssPlatformSolution.Server
{
  partial class EmployeeFunctions
  {

    /// <summary>
    /// Заполнить свойства ЛК.
    /// </summary>
    [Remote]
    public virtual void FillPersonalOfficeProperties()
    {
      if (_obj.Person != null)
      {
        var mainEmployee = EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Person.Id, null);
        
        if (mainEmployee != null)
        {
          _obj.PersonalAccountStatusDirRX = mainEmployee.PersonalAccountStatusDirRX;
          _obj.PersonalPhoneDirRX = mainEmployee.PersonalPhoneDirRX;
          _obj.MessagesEmailDirRX = mainEmployee.MessagesEmailDirRX;
          _obj.SmsNotifDirRX = mainEmployee.SmsNotifDirRX;
          _obj.ViberNotifDirRX = mainEmployee.ViberNotifDirRX;
          _obj.EmailNotifDirRX = mainEmployee.EmailNotifDirRX;
        }
      }
    }

  }
}