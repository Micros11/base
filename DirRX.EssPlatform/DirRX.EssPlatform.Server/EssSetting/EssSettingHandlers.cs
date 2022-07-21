using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatform.EssSetting;

namespace DirRX.EssPlatform
{
  partial class EssSettingServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Если не подключена настройка "Подключить сервисы личного кабинета", то последующие проверки выполняться не будут.
      if (_obj.IsUsedIdentityService != true)
        return;
      
      // Сначала проверить соответствие адреса ЛК формату URL, затем - корректность подключения,
      // проверять только для непустой строки.
      if (!string.IsNullOrEmpty(_obj.ESSAddress))
      {
        if (System.Uri.IsWellFormedUriString(_obj.ESSAddress, UriKind.Absolute))
        {
          var essConnectionErrors = DirRX.EssPlatform.PublicFunctions.Module.Remote.CheckESSConnection(_obj.ESSAddress);
          if (!string.IsNullOrEmpty(essConnectionErrors))
          {
            e.AddError(Resources.ESSCheckConnectionErrorFormat(essConnectionErrors));
          }
        }
        else
        {
          e.AddError(DirRX.EssPlatform.EssSettings.Resources.ESSUrlIsInvalid);
        }
      }
      
      // Сначала проверить соответствие адреса Idintity Service формату URL, затем - корректность подключения,
      // проверять только для непустой строки.
      if (!string.IsNullOrEmpty(_obj.IdentityServiceAddress))
      {
        if (System.Uri.IsWellFormedUriString(_obj.IdentityServiceAddress, UriKind.Absolute))
        {
          var loginResult = Functions.EssSetting.LoginIds(_obj, _obj.IdentityServicePassword, true);
          
          if (!string.IsNullOrEmpty(loginResult.Error))
          {
            e.AddError(loginResult.Error);
          }
        }
        else
        {
          e.AddError(DirRX.EssPlatform.EssSettings.Resources.IdsUrlInvalid);
        }
      }
      
      // Сначала проверить соответствие адреса MessageBroker формату URL, затем - корректность подключения,
      // проверять только для непустой строки.
      if (!string.IsNullOrEmpty(_obj.MessageBrokerAddress))
      {
        if (System.Uri.IsWellFormedUriString(_obj.MessageBrokerAddress, UriKind.Absolute))
        {
          var connectionErrors = DirRX.EssPlatform.PublicFunctions.Module.Remote.CheckMessageBrokerConnection(_obj.MessageBrokerAddress);
          if (!string.IsNullOrEmpty(connectionErrors))
          {
            e.AddError(Resources.MessageBrokerCheckConnectionErrorFormat(connectionErrors));
          }
        }
        else
        {
          e.AddError(DirRX.EssPlatform.EssSettings.Resources.MBUrlIsInvalid);
        }
      }
      
      // Проверить соответствие адреса Соглашения об ЭДО формату URL
      if (!string.IsNullOrEmpty(_obj.AgreementUrl))
      {
        if (!System.Uri.IsWellFormedUriString(_obj.AgreementUrl, UriKind.Absolute))
        {
          e.AddError(DirRX.EssPlatform.EssSettings.Resources.AgreementUrlIsInvalid);
        }
      }
    }

    public override void BeforeDelete(Sungero.Domain.BeforeDeleteEventArgs e)
    {
      throw AppliedCodeException.Create(DirRX.EssPlatform.EssSettings.Resources.EssSettingsCannotBeDeleted);
    }
  }

}