using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatform.EssSetting;

namespace DirRX.EssPlatform
{
  partial class EssSettingSharedHandlers
  {

    public virtual void IsUsedIdentityServiceChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      // Если отключена настройка "Подключить сервисы личного кабинета" то, очистить значение полей.
      if (!_obj.IsUsedIdentityService.Value)
      {
        _obj.IdentityServiceAddress = string.Empty;
        _obj.IdentityServiceLogin = string.Empty;
        _obj.MessageBrokerAddress = string.Empty;
        _obj.ESSAddress = string.Empty;
        _obj.AgreementUrl = string.Empty;
      }
      Functions.EssSetting.SwitchPropertiesAvailability(_obj);
    }

    public virtual void IdentityServiceLoginChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      _obj.IdentityServicePassword = string.Empty;
      
      if (e.NewValue == e.OldValue || e.NewValue == null)
        return;
      
      var trimmedLogin = e.NewValue.Trim();
      if (e.NewValue == trimmedLogin)
        return;
      
      _obj.IdentityServiceLogin = trimmedLogin;
    }

    public virtual void IdentityServiceAddressChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue || e.NewValue == null)
        return;
      
      var trimmedIdsUrl = e.NewValue.Trim();
      if (e.NewValue == trimmedIdsUrl)
        return;
      
      _obj.IdentityServiceAddress = trimmedIdsUrl;
    }
    
    public virtual void MessageBrokerAddressChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue || e.NewValue == null)
        return;
      
      var trimmedMBUrl = e.NewValue.Trim();
      if (e.NewValue == trimmedMBUrl)
        return;
      
      _obj.MessageBrokerAddress = trimmedMBUrl;
    }

  }
}