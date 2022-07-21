using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.BusinessUnit;

namespace DirRX.EssPlatformSolution
{
  partial class BusinessUnitSharedHandlers
  {

    public virtual void UseESSDirRXChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue == true)
      {
        if (string.IsNullOrEmpty(_obj.SmsToOldPhoneDirRX))
          _obj.SmsToOldPhoneDirRX = DirRX.EssPlatformSolution.BusinessUnits.Resources.SmsToOldPhoneText;
        if (string.IsNullOrEmpty(_obj.SmsToNewPhoneDirRX))
          _obj.SmsToNewPhoneDirRX = DirRX.EssPlatformSolution.BusinessUnits.Resources.SmsToNewPhoneText;
        if (string.IsNullOrEmpty(_obj.SmsAfterCloseDirRX))
          _obj.SmsAfterCloseDirRX = DirRX.EssPlatformSolution.BusinessUnits.Resources.SmsAfterCloseText;
      }
      else
      {
        _obj.SmsToOldPhoneDirRX = string.Empty;
        _obj.SmsToNewPhoneDirRX = string.Empty;
        _obj.SmsAfterCloseDirRX = string.Empty;
      }
    }
  }
}