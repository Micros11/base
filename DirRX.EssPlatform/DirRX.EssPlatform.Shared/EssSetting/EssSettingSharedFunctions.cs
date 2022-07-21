using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatform.EssSetting;

namespace DirRX.EssPlatform.Shared
{
  partial class EssSettingFunctions
  {
    /// <summary>
    /// Получить настройки взаимодействия с Личным кабинетом.
    /// </summary>
    /// <returns>Настройки.</returns>
    [Public]
    public static IEssSetting GetSettings()
    {
      return EssSettings.GetAllCached().SingleOrDefault();
    }
    
    /// <summary>
    /// Установить доступность свойств карточки в зависимости от текущего состояния полей.
    /// </summary>
    public virtual void SwitchPropertiesAvailability()
    {
      var isEnabled = _obj.IsUsedIdentityService.HasValue && _obj.IsUsedIdentityService.Value;
      
      _obj.State.Properties.IdentityServiceAddress.IsEnabled = isEnabled;
      _obj.State.Properties.IdentityServiceLogin.IsEnabled = isEnabled;
      _obj.State.Properties.MessageBrokerAddress.IsEnabled = isEnabled;
      _obj.State.Properties.ESSAddress.IsEnabled = isEnabled;
      _obj.State.Properties.AgreementUrl.IsEnabled = isEnabled;
      
      _obj.State.Properties.IdentityServiceAddress.IsRequired = isEnabled;
      _obj.State.Properties.IdentityServiceLogin.IsRequired = isEnabled;
      _obj.State.Properties.MessageBrokerAddress.IsRequired = isEnabled;
      _obj.State.Properties.ESSAddress.IsRequired = isEnabled;
      _obj.State.Properties.AgreementUrl.IsRequired = isEnabled;
    }
  }
}